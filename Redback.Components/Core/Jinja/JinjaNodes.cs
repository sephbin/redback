using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redback.Components.Core.Jinja
{
    // ==================================================================== runtime context

    internal sealed class Context
    {
        public readonly List<Dictionary<string, object>> Frames;
        public object BlockValue;
        public int Depth;

        public Context(List<Dictionary<string, object>> frames) { Frames = frames; }

        public object Lookup(string name)
        {
            for (int i = Frames.Count - 1; i >= 0; i--)
                if (Frames[i].TryGetValue(name, out var v)) return v;
            if (Globals.Table.TryGetValue(name, out var g)) return g;
            return new Undefined(name);
        }

        public void Set(string name, object value) => Frames[Frames.Count - 1][name] = value;
        public void Push() => Frames.Add(new Dictionary<string, object>());
        public void Pop() => Frames.RemoveAt(Frames.Count - 1);
        public List<Dictionary<string, object>> Snapshot() => new List<Dictionary<string, object>>(Frames);
    }

    /// <summary>
    /// A StackOverflowException cannot be caught and would take down Rhino, so deep recursion
    /// (runaway macros, absurdly nested expressions) is turned into a normal template error first.
    /// </summary>
    internal static class StackGuard
    {
        public static void Check()
        {
            try { System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack(); }
            catch (InsufficientExecutionStackException) { throw new TemplateError("Template is nested or recursive too deeply"); }
        }
    }

    internal sealed class BreakSignal : Exception { }
    internal sealed class ContinueSignal : Exception { }

    // ==================================================================== statements

    internal abstract class Node
    {
        public int Line;
        public abstract void Render(Context ctx, StringBuilder sb);

        public static void RenderAll(List<Node> nodes, Context ctx, StringBuilder sb)
        {
            StackGuard.Check();
            foreach (var n in nodes)
            {
                // The filter tags template errors with the innermost line without catching them, so a
                // failure deep inside recursive macros unwinds once instead of rethrowing at every level.
                try { n.Render(ctx, sb); }
                catch (Exception ex) when (ShouldWrap(ex, n.Line)) { throw new TemplateError(ex.Message, n.Line); }
            }
        }

        private static bool ShouldWrap(Exception ex, int line)
        {
            if (ex is TemplateError te) { te.SetLineIfUnknown(line); return false; }
            return !(ex is BreakSignal || ex is ContinueSignal);
        }

        protected static string Capture(List<Node> body, Context ctx)
        {
            var sb = new StringBuilder();
            RenderAll(body, ctx, sb);
            return sb.ToString();
        }

        protected static void Bind(Context ctx, List<string> targets, object value)
        {
            if (targets.Count == 1) { ctx.Set(targets[0], value); return; }
            var items = Py.Iterate(value).ToList();
            if (items.Count != targets.Count)
                throw new TemplateError(items.Count < targets.Count
                    ? $"not enough values to unpack (expected {targets.Count}, got {items.Count})"
                    : $"too many values to unpack (expected {targets.Count})");
            for (int i = 0; i < targets.Count; i++) ctx.Set(targets[i], items[i]);
        }
    }

    internal sealed class TextNode : Node
    {
        private readonly string _text;
        public TextNode(string text) { _text = text; }
        public override void Render(Context ctx, StringBuilder sb) => sb.Append(_text);
    }

    internal sealed class OutputNode : Node
    {
        private readonly Expr _expr;
        public OutputNode(Expr e) { _expr = e; }
        public override void Render(Context ctx, StringBuilder sb) => sb.Append(Py.Str(_expr.Eval(ctx)));
    }

    internal sealed class BlockNode : Node
    {
        private readonly List<Node> _body;
        public BlockNode(List<Node> body) { _body = body; }
        public override void Render(Context ctx, StringBuilder sb) => RenderAll(_body, ctx, sb);
    }

    internal sealed class IfNode : Node
    {
        public readonly List<KeyValuePair<Expr, List<Node>>> Branches = new List<KeyValuePair<Expr, List<Node>>>();
        public List<Node> Else;

        public override void Render(Context ctx, StringBuilder sb)
        {
            foreach (var b in Branches)
            {
                if (Py.Truthy(b.Key.Eval(ctx))) { RenderAll(b.Value, ctx, sb); return; }
            }
            if (Else != null) RenderAll(Else, ctx, sb);
        }
    }

    internal sealed class ForNode : Node
    {
        public readonly List<string> Targets = new List<string>();
        public Expr Iter, Filter;
        public List<Node> Body, Else;

        public override void Render(Context ctx, StringBuilder sb)
        {
            var source = Py.Iterate(Iter.Eval(ctx)).ToList();
            ctx.Push();
            try
            {
                List<object> items;
                if (Filter == null) items = source;
                else
                {
                    items = new List<object>();
                    foreach (var it in source)
                    {
                        Bind(ctx, Targets, it);
                        if (Py.Truthy(Filter.Eval(ctx))) items.Add(it);
                    }
                }

                if (items.Count == 0)
                {
                    if (Else != null) RenderAll(Else, ctx, sb);
                    return;
                }

                var loop = new LoopContext(items);
                for (int i = 0; i < items.Count; i++)
                {
                    loop.Index0 = i;
                    ctx.Set("loop", loop);
                    Bind(ctx, Targets, items[i]);
                    try { RenderAll(Body, ctx, sb); }
                    catch (ContinueSignal) { }
                    catch (BreakSignal) { break; }
                }
            }
            finally { ctx.Pop(); }
        }
    }

    internal sealed class BreakNode : Node
    {
        public override void Render(Context ctx, StringBuilder sb) => throw new BreakSignal();
    }

    internal sealed class ContinueNode : Node
    {
        public override void Render(Context ctx, StringBuilder sb) => throw new ContinueSignal();
    }

    internal sealed class SetNode : Node
    {
        public List<string> Targets;
        public Expr Value;
        public override void Render(Context ctx, StringBuilder sb) => Bind(ctx, Targets, Value.Eval(ctx));
    }

    internal sealed class SetAttrNode : Node
    {
        public string Name, Attr;
        public Expr Value;

        public override void Render(Context ctx, StringBuilder sb)
        {
            if (!(ctx.Lookup(Name) is Namespace ns))
                throw new TemplateError($"Cannot assign attribute on '{Name}': only namespace() objects support attribute assignment");
            ns[Attr] = Value.Eval(ctx);
        }
    }

    internal sealed class SetBlockNode : Node
    {
        public string Name;
        public List<Node> Body;
        public Expr Filter;

        public override void Render(Context ctx, StringBuilder sb)
        {
            object v = Capture(Body, ctx);
            if (Filter != null) v = Py.EvalWithBlockValue(ctx, Filter, v);
            ctx.Set(Name, v);
        }
    }

    internal sealed class FilterBlockNode : Node
    {
        public Expr Filter;
        public List<Node> Body;

        public override void Render(Context ctx, StringBuilder sb)
            => sb.Append(Py.Str(Py.EvalWithBlockValue(ctx, Filter, Capture(Body, ctx))));
    }

    internal sealed class WithNode : Node
    {
        public readonly List<KeyValuePair<string, Expr>> Assignments = new List<KeyValuePair<string, Expr>>();
        public List<Node> Body;

        public override void Render(Context ctx, StringBuilder sb)
        {
            var values = Assignments.Select(a => a.Value.Eval(ctx)).ToList();
            ctx.Push();
            try
            {
                for (int i = 0; i < values.Count; i++) ctx.Set(Assignments[i].Key, values[i]);
                RenderAll(Body, ctx, sb);
            }
            finally { ctx.Pop(); }
        }
    }

    internal sealed class MacroParam
    {
        public string Name;
        public Expr Default;
    }

    internal sealed class MacroNode : Node
    {
        public string Name;
        public List<MacroParam> Params;
        public List<Node> Body;

        public override void Render(Context ctx, StringBuilder sb) => ctx.Set(Name, new Macro(Name, Params, Body, ctx.Snapshot()));
    }

    internal sealed class CallBlockNode : Node
    {
        public List<MacroParam> Params;
        public CallExpr Call;
        public List<Node> Body;

        public override void Render(Context ctx, StringBuilder sb)
        {
            var caller = new Macro("caller", Params, Body, ctx.Snapshot());
            sb.Append(Py.Str(Call.Invoke(ctx, caller)));
        }
    }

    // ==================================================================== callables

    internal interface IJinjaCallable
    {
        object Call(Context ctx, List<object> args, Dictionary<string, object> kwargs);
    }

    internal sealed class Macro : IJinjaCallable
    {
        private const int MaxDepth = 200;
        public readonly string Name;
        private readonly List<MacroParam> _params;
        private readonly List<Node> _body;
        private readonly List<Dictionary<string, object>> _closure;

        public Macro(string name, List<MacroParam> ps, List<Node> body, List<Dictionary<string, object>> closure)
        {
            Name = name; _params = ps; _body = body; _closure = closure;
        }

        public object Call(Context ctx, List<object> args, Dictionary<string, object> kwargs)
        {
            var frame = new Dictionary<string, object>();
            var frames = new List<Dictionary<string, object>>(_closure) { frame };
            var mctx = new Context(frames) { Depth = ctx.Depth + 1 };
            if (mctx.Depth > MaxDepth) throw new TemplateError($"Maximum macro recursion depth exceeded in '{Name}'");
            StackGuard.Check();

            var kw = new Dictionary<string, object>(kwargs);
            for (int i = 0; i < _params.Count; i++)
            {
                var p = _params[i];
                object v;
                if (i < args.Count)
                {
                    if (kw.ContainsKey(p.Name)) throw new TemplateError($"macro '{Name}' got multiple values for argument '{p.Name}'");
                    v = args[i];
                }
                else if (kw.TryGetValue(p.Name, out var kv)) { v = kv; kw.Remove(p.Name); }
                else if (p.Default != null) v = p.Default.Eval(mctx);
                else v = new Undefined(p.Name);
                frame[p.Name] = v;
            }

            if (kw.TryGetValue("caller", out var caller)) { frame["caller"] = caller; kw.Remove("caller"); }
            frame["varargs"] = new PyTuple(args.Skip(_params.Count));
            frame["kwargs"] = kw;

            var sb = new StringBuilder();
            Node.RenderAll(_body, mctx, sb);
            return sb.ToString();
        }
    }

    internal sealed class BuiltinFunction : IJinjaCallable
    {
        private readonly Func<Context, Args, object> _fn;
        public BuiltinFunction(Func<Context, Args, object> fn) { _fn = fn; }
        public object Call(Context ctx, List<object> args, Dictionary<string, object> kwargs) => _fn(ctx, new Args(args, kwargs));
    }

    internal sealed class BoundMethod : IJinjaCallable
    {
        public readonly object Self;
        public readonly string Name;
        public BoundMethod(object self, string name) { Self = self; Name = name; }
        public object Call(Context ctx, List<object> args, Dictionary<string, object> kwargs)
            => Methods.Invoke(Self, Name, new Args(args, kwargs));
    }

    // ==================================================================== expressions

    internal abstract class Expr
    {
        public abstract object Eval(Context ctx);
    }

    internal sealed class LiteralExpr : Expr
    {
        private readonly object _v;
        public LiteralExpr(object v) { _v = v; }
        public override object Eval(Context ctx) => _v;
    }

    internal sealed class PlaceholderExpr : Expr
    {
        public override object Eval(Context ctx) => ctx.BlockValue;
    }

    internal sealed class NameExpr : Expr
    {
        public readonly string Name;
        public NameExpr(string n) { Name = n; }
        public override object Eval(Context ctx) => ctx.Lookup(Name);
    }

    internal sealed class GetAttrExpr : Expr
    {
        private readonly Expr _target;
        private readonly string _name;
        public GetAttrExpr(Expr t, string n) { _target = t; _name = n; }
        public override object Eval(Context ctx) => Py.GetAttr(_target.Eval(ctx), _name);
    }

    internal sealed class GetItemExpr : Expr
    {
        private readonly Expr _target, _key;
        public GetItemExpr(Expr t, Expr k) { _target = t; _key = k; }
        public override object Eval(Context ctx) => Py.GetItem(_target.Eval(ctx), _key.Eval(ctx));
    }

    internal sealed class SliceExpr : Expr
    {
        private readonly Expr _target, _start, _stop, _step;
        public SliceExpr(Expr t, Expr a, Expr b, Expr c) { _target = t; _start = a; _stop = b; _step = c; }
        public override object Eval(Context ctx)
            => Py.Slice(_target.Eval(ctx), _start?.Eval(ctx), _stop?.Eval(ctx), _step?.Eval(ctx));
    }

    internal sealed class CallExpr : Expr
    {
        public Expr Fn;
        public readonly List<Expr> Args = new List<Expr>();
        public readonly List<KeyValuePair<string, Expr>> Kwargs = new List<KeyValuePair<string, Expr>>();

        public override object Eval(Context ctx) => Invoke(ctx, null);

        public object Invoke(Context ctx, object caller)
        {
            var fn = Fn.Eval(ctx);
            if (fn is Undefined u) u.Fail();
            if (!(fn is IJinjaCallable c)) throw new TemplateError($"'{Py.TypeName(fn)}' object is not callable");
            var args = Args.Select(a => a.Eval(ctx)).ToList();
            var kw = new Dictionary<string, object>();
            foreach (var k in Kwargs) kw[k.Key] = k.Value.Eval(ctx);
            if (caller != null) kw["caller"] = caller;
            return c.Call(ctx, args, kw);
        }
    }

    internal sealed class FilterExpr : Expr
    {
        public Expr Target;
        public string Name;
        public readonly List<Expr> Args = new List<Expr>();
        public readonly List<KeyValuePair<string, Expr>> Kwargs = new List<KeyValuePair<string, Expr>>();

        public override object Eval(Context ctx)
        {
            var v = Target.Eval(ctx);
            var kw = new Dictionary<string, object>();
            foreach (var k in Kwargs) kw[k.Key] = k.Value.Eval(ctx);
            return Filters.Apply(ctx, Name, v, new Args(Args.Select(a => a.Eval(ctx)).ToList(), kw));
        }
    }

    internal sealed class TestExpr : Expr
    {
        public Expr Target;
        public string Name;
        public bool Negate;
        public readonly List<Expr> Args = new List<Expr>();
        public readonly List<KeyValuePair<string, Expr>> Kwargs = new List<KeyValuePair<string, Expr>>();

        public override object Eval(Context ctx)
        {
            var v = Target.Eval(ctx);
            var kw = new Dictionary<string, object>();
            foreach (var k in Kwargs) kw[k.Key] = k.Value.Eval(ctx);
            bool r = Tests.Apply(Name, v, new Args(Args.Select(a => a.Eval(ctx)).ToList(), kw));
            return Negate ? !r : r;
        }
    }

    internal sealed class UnaryExpr : Expr
    {
        private readonly string _op;
        private readonly Expr _e;
        public UnaryExpr(string op, Expr e) { _op = op; _e = e; }

        public override object Eval(Context ctx)
        {
            var v = _e.Eval(ctx);
            switch (_op)
            {
                case "not": return !Py.Truthy(v);
                case "neg": return Py.Neg(v);
                default: return Py.Pos(v);
            }
        }
    }

    internal sealed class BinaryExpr : Expr
    {
        private readonly string _op;
        private readonly Expr _l, _r;
        public BinaryExpr(string op, Expr l, Expr r) { _op = op; _l = l; _r = r; }

        public override object Eval(Context ctx)
        {
            var l = _l.Eval(ctx);
            if (_op == "and") return Py.Truthy(l) ? _r.Eval(ctx) : l;
            if (_op == "or") return Py.Truthy(l) ? l : _r.Eval(ctx);
            return Py.Binary(_op, l, _r.Eval(ctx));
        }
    }

    internal sealed class CompareExpr : Expr
    {
        private readonly Expr _first;
        private readonly List<KeyValuePair<string, Expr>> _ops;
        public CompareExpr(Expr first, List<KeyValuePair<string, Expr>> ops) { _first = first; _ops = ops; }

        public override object Eval(Context ctx)
        {
            var left = _first.Eval(ctx);
            foreach (var op in _ops)
            {
                var right = op.Value.Eval(ctx);
                if (!Py.CompareOp(op.Key, left, right)) return false;
                left = right;
            }
            return true;
        }
    }

    internal sealed class CondExpr : Expr
    {
        private readonly Expr _cond, _then, _else;
        public CondExpr(Expr cond, Expr then, Expr els) { _cond = cond; _then = then; _else = els; }

        public override object Eval(Context ctx)
        {
            if (Py.Truthy(_cond.Eval(ctx))) return _then.Eval(ctx);
            return _else != null ? _else.Eval(ctx) : new Undefined("<conditional>", "the inline if-expression evaluated to false and has no else section");
        }
    }

    internal sealed class ListExpr : Expr
    {
        private readonly List<Expr> _items;
        private readonly bool _tuple;
        public ListExpr(List<Expr> items, bool tuple) { _items = items; _tuple = tuple; }

        public override object Eval(Context ctx)
        {
            var vals = _items.Select(i => i.Eval(ctx));
            return _tuple ? new PyTuple(vals) : new List<object>(vals);
        }
    }

    internal sealed class DictExpr : Expr
    {
        private readonly List<KeyValuePair<Expr, Expr>> _pairs;
        public DictExpr(List<KeyValuePair<Expr, Expr>> pairs) { _pairs = pairs; }

        public override object Eval(Context ctx)
        {
            var d = new Dictionary<string, object>();
            foreach (var p in _pairs) d[Py.Str(p.Key.Eval(ctx))] = p.Value.Eval(ctx);
            return d;
        }
    }
}
