using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Redback.Components.Core.Jinja
{
    // ==================================================================== value types

    /// <summary>Jinja's default Undefined: prints as empty, is falsy, iterates as empty, errors on attribute access.</summary>
    internal sealed class Undefined
    {
        public readonly string Name;
        private readonly string _hint;
        public Undefined(string name, string hint = null) { Name = name; _hint = hint; }
        public string Message => _hint ?? $"'{Name}' is undefined";
        public void Fail() => throw new TemplateError(Message);
        public override string ToString() => "";
    }

    /// <summary>A list that prints like a Python tuple.</summary>
    internal class PyTuple : List<object>
    {
        public PyTuple() { }
        public PyTuple(IEnumerable<object> items) : base(items) { }
    }

    /// <summary>Result row of the groupby filter: unpackable as (grouper, list) and has .grouper/.list.</summary>
    internal sealed class GroupTuple : PyTuple
    {
        public GroupTuple(object grouper, List<object> list) : base(new object[] { grouper, list }) { }
    }

    /// <summary>namespace() object: the only thing {% set ns.attr = ... %} may assign to.</summary>
    internal sealed class Namespace : Dictionary<string, object> { }

    internal sealed class LoopContext
    {
        private readonly List<object> _items;
        public int Index0;
        private object _lastChanged = Missing;
        private static readonly object Missing = new object();

        public LoopContext(List<object> items) { _items = items; }

        public object Get(string name)
        {
            int n = _items.Count;
            switch (name)
            {
                case "index": return (long)(Index0 + 1);
                case "index0": return (long)Index0;
                case "revindex": return (long)(n - Index0);
                case "revindex0": return (long)(n - Index0 - 1);
                case "first": return Index0 == 0;
                case "last": return Index0 == n - 1;
                case "length": return (long)n;
                case "depth": return 1L;
                case "depth0": return 0L;
                case "previtem": return Index0 > 0 ? _items[Index0 - 1] : new Undefined("previtem", "there is no previous item");
                case "nextitem": return Index0 < n - 1 ? _items[Index0 + 1] : new Undefined("nextitem", "there is no next item");
                case "cycle":
                case "changed":
                    return new BoundMethod(this, name);
            }
            return new Undefined(name, $"'loop' has no attribute '{name}'");
        }

        public object Cycle(Args a)
        {
            if (a.Pos.Count == 0) throw new TemplateError("loop.cycle() needs at least one argument");
            return a.Pos[Index0 % a.Pos.Count];
        }

        public object Changed(Args a)
        {
            var val = new PyTuple(a.Pos);
            if (_lastChanged != Missing && Py.Eq(_lastChanged, val)) return false;
            _lastChanged = val;
            return true;
        }
    }

    /// <summary>Positional + keyword arguments for filters, tests and functions.</summary>
    internal sealed class Args
    {
        public readonly List<object> Pos;
        public readonly Dictionary<string, object> Kw;

        public Args(List<object> pos, Dictionary<string, object> kw) { Pos = pos; Kw = kw ?? new Dictionary<string, object>(); }

        public object Get(int i, string name, object def = null)
        {
            if (i >= 0 && i < Pos.Count) return Pos[i];
            if (name != null && Kw.TryGetValue(name, out var v)) return v;
            return def;
        }

        public bool Has(int i, string name) => (i >= 0 && i < Pos.Count) || (name != null && Kw.ContainsKey(name));
        public bool Bool(int i, string name, bool def = false) => Has(i, name) ? Py.Truthy(Get(i, name)) : def;
        public long Long(int i, string name, long def) => Has(i, name) && Get(i, name) != null ? Py.ToLong(Get(i, name)) : def;
        public string Str(int i, string name, string def) => Has(i, name) && Get(i, name) != null ? Py.Str(Get(i, name)) : def;
    }

    // ==================================================================== Python semantics

    internal static class Py
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        // ---------------------------------------------------------------- conversion from JSON

        public static object FromJson(JsonElement e)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.Object:
                    var d = new Dictionary<string, object>();
                    foreach (var p in e.EnumerateObject()) d[p.Name] = FromJson(p.Value);
                    return d;
                case JsonValueKind.Array:
                    var l = new List<object>();
                    foreach (var x in e.EnumerateArray()) l.Add(FromJson(x));
                    return l;
                case JsonValueKind.String: return e.GetString();
                case JsonValueKind.Number:
                    string raw = e.GetRawText();
                    if (raw.IndexOfAny(new[] { '.', 'e', 'E' }) < 0 && e.TryGetInt64(out long lv)) return lv;
                    return e.GetDouble();
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                default: return null;
            }
        }

        // ---------------------------------------------------------------- type helpers

        public static bool IsInt(object o) => o is long || o is int || o is bool;
        public static bool IsNumber(object o) => o is long || o is int || o is double || o is bool;
        public static bool IsMapping(object o) => o is Dictionary<string, object>;

        public static string TypeName(object o)
        {
            switch (o)
            {
                case null: return "NoneType";
                case Undefined _: return "Undefined";
                case bool _: return "bool";
                case long _: case int _: return "int";
                case double _: return "float";
                case string _: return "str";
                case PyTuple _: return "tuple";
                case IList _: return "list";
                case Namespace _: return "Namespace";
                case Dictionary<string, object> _: return "dict";
                case Macro _: return "Macro";
                case IJinjaCallable _: return "function";
                case LoopContext _: return "LoopContext";
                default: return o.GetType().Name;
            }
        }

        public static long ToLong(object o)
        {
            switch (o)
            {
                case long l: return l;
                case int i: return i;
                case bool b: return b ? 1 : 0;
                case double d: return (long)Math.Truncate(d);
                case string s when long.TryParse(s.Trim(), NumberStyles.Integer, Inv, out long r): return r;
                case Undefined u: u.Fail(); return 0;
            }
            throw new TemplateError($"expected an integer, got '{TypeName(o)}'");
        }

        public static double ToDouble(object o)
        {
            switch (o)
            {
                case double d: return d;
                case long l: return l;
                case int i: return i;
                case bool b: return b ? 1 : 0;
                case string s when double.TryParse(s.Trim(), NumberStyles.Float, Inv, out double r): return r;
                case Undefined u: u.Fail(); return 0;
            }
            throw new TemplateError($"expected a number, got '{TypeName(o)}'");
        }

        public static bool Truthy(object o)
        {
            switch (o)
            {
                case null: return false;
                case Undefined _: return false;
                case bool b: return b;
                case long l: return l != 0;
                case int i: return i != 0;
                case double d: return d != 0;
                case string s: return s.Length > 0;
                case ICollection c: return c.Count > 0;
                default: return true;
            }
        }

        // ---------------------------------------------------------------- string conversion

        /// <summary>Python str() — what {{ value }} prints.</summary>
        public static string Str(object o)
        {
            switch (o)
            {
                case null: return "None";
                case Undefined _: return "";
                case string s: return s;
                case bool b: return b ? "True" : "False";
                case long l: return l.ToString(Inv);
                case int i: return i.ToString(Inv);
                case double d: return FloatStr(d);
                case IList _:
                case Dictionary<string, object> _:
                    return Repr(o);
                case Macro m: return $"<Macro '{m.Name}'>";
                default: return o.ToString();
            }
        }

        /// <summary>Python repr().</summary>
        public static string Repr(object o)
        {
            switch (o)
            {
                case string s: return Quote(s);
                case Namespace ns:
                    return "<Namespace " + string.Join(", ", ns.Select(kv => $"{kv.Key}={Repr(kv.Value)}")) + ">";
                case Dictionary<string, object> d:
                    return "{" + string.Join(", ", d.Select(kv => Quote(kv.Key) + ": " + Repr(kv.Value))) + "}";
                case PyTuple t:
                    return t.Count == 1 ? "(" + Repr(t[0]) + ",)" : "(" + string.Join(", ", t.Select(Repr)) + ")";
                case IList l:
                    return "[" + string.Join(", ", l.Cast<object>().Select(Repr)) + "]";
                case Undefined _: return "Undefined";
                default: return Str(o);
            }
        }

        public static string Quote(string s)
        {
            char q = s.IndexOf('\'') >= 0 && s.IndexOf('"') < 0 ? '"' : '\'';
            var sb = new StringBuilder();
            sb.Append(q);
            foreach (char c in s)
            {
                if (c == q || c == '\\') sb.Append('\\').Append(c);
                else if (c == '\n') sb.Append("\\n");
                else if (c == '\r') sb.Append("\\r");
                else if (c == '\t') sb.Append("\\t");
                else if (c < 0x20 || c == 0x7f) sb.Append("\\x").Append(((int)c).ToString("x2"));
                else sb.Append(c);
            }
            sb.Append(q);
            return sb.ToString();
        }

        public static string FloatStr(double d)
        {
            if (double.IsNaN(d)) return "nan";
            if (double.IsPositiveInfinity(d)) return "inf";
            if (double.IsNegativeInfinity(d)) return "-inf";
            if (d == Math.Floor(d) && Math.Abs(d) < 1e16)
            {
                string ip = ((long)d).ToString(Inv);
                if (d == 0 && 1 / d < 0) ip = "-0";
                return ip + ".0";
            }
            // Shortest round-trip digits (Python repr). net48's "R" format can emit 17 digits where 15-16 suffice.
            string sci = null;
            for (int p = 15; p <= 17; p++)
            {
                sci = d.ToString("E" + (p - 1), Inv);
                if (double.Parse(sci, NumberStyles.Float, Inv) == d) break;
            }
            bool neg = sci[0] == '-';
            if (neg) sci = sci.Substring(1);
            int ei = sci.IndexOf('E');
            int exp = int.Parse(sci.Substring(ei + 1), Inv);
            string digits = sci.Substring(0, ei).Replace(".", "").TrimEnd('0');
            if (digits.Length == 0) digits = "0";

            string r;
            if (exp >= -4 && exp < 16)
            {
                if (exp >= 0)
                    r = digits.Length <= exp + 1
                        ? digits + new string('0', exp + 1 - digits.Length) + ".0"
                        : digits.Substring(0, exp + 1) + "." + digits.Substring(exp + 1);
                else
                    r = "0." + new string('0', -exp - 1) + digits;
            }
            else
            {
                r = digits.Substring(0, 1) + (digits.Length > 1 ? "." + digits.Substring(1) : "") +
                    "e" + (exp < 0 ? "-" : "+") + Math.Abs(exp).ToString("00", Inv);
            }
            return neg ? "-" + r : r;
        }

        // ---------------------------------------------------------------- equality / ordering

        public static bool Eq(object a, object b)
        {
            if (a is Undefined && b is Undefined) return true;
            if (a is Undefined || b is Undefined) return false;
            if (a == null || b == null) return a == null && b == null;
            if (IsNumber(a) && IsNumber(b))
            {
                if (IsInt(a) && IsInt(b)) return ToLong(a) == ToLong(b);
                return ToDouble(a) == ToDouble(b);
            }
            if (a is string sa && b is string sb) return string.Equals(sa, sb, StringComparison.Ordinal);
            if (a is IList la && b is IList lb)
            {
                if (la.Count != lb.Count) return false;
                for (int i = 0; i < la.Count; i++) if (!Eq(la[i], lb[i])) return false;
                return true;
            }
            if (a is Dictionary<string, object> da && b is Dictionary<string, object> db)
            {
                if (da.Count != db.Count) return false;
                foreach (var kv in da)
                    if (!db.TryGetValue(kv.Key, out var bv) || !Eq(kv.Value, bv)) return false;
                return true;
            }
            return Equals(a, b);
        }

        public static int Compare(object a, object b)
        {
            if (a is Undefined ua) ua.Fail();
            if (b is Undefined ub) ub.Fail();
            if (IsNumber(a) && IsNumber(b))
            {
                if (IsInt(a) && IsInt(b)) return ToLong(a).CompareTo(ToLong(b));
                return ToDouble(a).CompareTo(ToDouble(b));
            }
            if (a is string sa && b is string sb) return string.CompareOrdinal(sa, sb);
            if (a is IList la && b is IList lb)
            {
                for (int i = 0; i < Math.Min(la.Count, lb.Count); i++)
                {
                    int c = Compare(la[i], lb[i]);
                    if (c != 0) return c;
                }
                return la.Count.CompareTo(lb.Count);
            }
            throw new TemplateError($"'<' not supported between instances of '{TypeName(a)}' and '{TypeName(b)}'");
        }

        public static bool CompareOp(string op, object a, object b)
        {
            switch (op)
            {
                case "==": return Eq(a, b);
                case "!=": return !Eq(a, b);
                case "<": return Compare(a, b) < 0;
                case "<=": return Compare(a, b) <= 0;
                case ">": return Compare(a, b) > 0;
                case ">=": return Compare(a, b) >= 0;
                case "in": return Contains(b, a);
                case "notin": return !Contains(b, a);
            }
            throw new TemplateError($"Unknown comparison '{op}'");
        }

        public static bool Contains(object container, object item)
        {
            switch (container)
            {
                case Undefined _: return false;
                case string s:
                    if (!(item is string sub)) throw new TemplateError($"'in <string>' requires string as left operand, not {TypeName(item)}");
                    return s.IndexOf(sub, StringComparison.Ordinal) >= 0;
                case Dictionary<string, object> d: return item is string k && d.ContainsKey(k);
                case IList l:
                    foreach (var x in l) if (Eq(x, item)) return true;
                    return false;
            }
            throw new TemplateError($"argument of type '{TypeName(container)}' is not iterable");
        }

        // ---------------------------------------------------------------- iteration

        public static IEnumerable<object> Iterate(object o)
        {
            switch (o)
            {
                case Undefined _: return Enumerable.Empty<object>();
                case string s: return s.Select(c => (object)c.ToString());
                case Dictionary<string, object> d: return d.Keys.Cast<object>();
                case IList l: return l.Cast<object>();
            }
            throw new TemplateError($"'{TypeName(o)}' object is not iterable");
        }

        // ---------------------------------------------------------------- attribute / item access

        public static object GetAttr(object obj, string name)
        {
            switch (obj)
            {
                case Undefined u: u.Fail(); return null;
                case LoopContext lc: return lc.Get(name);
                case GroupTuple g when name == "grouper": return g[0];
                case GroupTuple g when name == "list": return g[1];
                case Namespace ns:
                    return ns.TryGetValue(name, out var nv) ? nv : new Undefined(name, $"'Namespace' object has no attribute '{name}'");
                case Dictionary<string, object> d:
                    // Jinja tries Python attributes first for '.', so dict methods shadow keys of the same name.
                    if (Methods.Has(obj, name)) return new BoundMethod(obj, name);
                    return d.TryGetValue(name, out var v) ? v : new Undefined(name, $"'dict object' has no attribute '{name}'");
            }
            if (obj != null && Methods.Has(obj, name)) return new BoundMethod(obj, name);
            return new Undefined(name, $"'{TypeName(obj)} object' has no attribute '{name}'");
        }

        public static object GetItem(object obj, object key)
        {
            switch (obj)
            {
                case Undefined u: u.Fail(); return null;
                case Namespace _: return GetAttr(obj, Str(key));
                case Dictionary<string, object> d:
                {
                    string k = Str(key);
                    if (d.TryGetValue(k, out var v)) return v;
                    if (key is string && Methods.Has(obj, k)) return new BoundMethod(obj, k);
                    return new Undefined(k, $"'dict object' has no attribute {Quote(k)}");
                }
                case IList l when IsInt(key):
                {
                    long i = ToLong(key);
                    if (i < 0) i += l.Count;
                    return i >= 0 && i < l.Count ? l[(int)i] : new Undefined(Str(key), $"list index {key} out of range");
                }
                case string s when IsInt(key):
                {
                    long i = ToLong(key);
                    if (i < 0) i += s.Length;
                    return i >= 0 && i < s.Length ? s[(int)i].ToString() : new Undefined(Str(key), $"string index {key} out of range");
                }
            }
            if (key is string sk) return GetAttr(obj, sk);
            return new Undefined(Str(key), $"'{TypeName(obj)} object' has no element {Repr(key)}");
        }

        /// <summary>Resolves "a.b.0" style attribute paths used by filters such as sort(attribute=...).</summary>
        public static object GetAttrPath(object obj, object path)
        {
            if (path == null) return obj;
            if (IsInt(path)) return GetItem(obj, path);
            foreach (var part in Str(path).Split('.'))
            {
                object key = long.TryParse(part, NumberStyles.Integer, Inv, out long li) ? (object)li : part;
                obj = GetItem(obj, key);
                if (obj is Undefined) return obj;
            }
            return obj;
        }

        public static object Slice(object obj, object start, object stop, object step)
        {
            if (obj is Undefined u) u.Fail();
            int len;
            if (obj is string s) len = s.Length;
            else if (obj is IList l) len = l.Count;
            else throw new TemplateError($"'{TypeName(obj)}' object is not subscriptable");

            long st = step == null ? 1 : ToLong(step);
            if (st == 0) throw new TemplateError("slice step cannot be zero");

            long Norm(object v, long dflt, long lo, long hi)
            {
                if (v == null) return dflt;
                long x = ToLong(v);
                if (x < 0) x += len;
                return Math.Max(lo, Math.Min(hi, x));
            }

            long a, b;
            if (st > 0) { a = Norm(start, 0, 0, len); b = Norm(stop, len, 0, len); }
            else { a = Norm(start, len - 1, -1, len - 1); b = Norm(stop, -1, -1, len - 1); }

            var idx = new List<int>();
            for (long i = a; st > 0 ? i < b : i > b; i += st) idx.Add((int)i);

            if (obj is string str) return new string(idx.Select(i => str[i]).ToArray());
            var list = (IList)obj;
            var items = idx.Select(i => list[i]);
            return obj is PyTuple ? new PyTuple(items) : (object)new List<object>(items);
        }

        public static object EvalWithBlockValue(Context ctx, Expr e, object value)
        {
            var saved = ctx.BlockValue;
            ctx.BlockValue = value;
            try { return e.Eval(ctx); }
            finally { ctx.BlockValue = saved; }
        }

        // ---------------------------------------------------------------- arithmetic

        public static object Neg(object v)
        {
            switch (v)
            {
                case long l: return -l;
                case int i: return -(long)i;
                case bool b: return b ? -1L : 0L;
                case double d: return -d;
                case Undefined u: u.Fail(); break;
            }
            throw new TemplateError($"bad operand type for unary -: '{TypeName(v)}'");
        }

        public static object Pos(object v)
        {
            if (IsNumber(v)) return v is double ? v : (object)ToLong(v);
            if (v is Undefined u) u.Fail();
            throw new TemplateError($"bad operand type for unary +: '{TypeName(v)}'");
        }

        public static object Binary(string op, object a, object b)
        {
            if (op == "~") return Str(a) + Str(b);
            if (a is Undefined ua) ua.Fail();
            if (b is Undefined ub) ub.Fail();

            if (IsNumber(a) && IsNumber(b))
            {
                bool ints = IsInt(a) && IsInt(b);
                switch (op)
                {
                    case "+": return ints ? (object)(ToLong(a) + ToLong(b)) : ToDouble(a) + ToDouble(b);
                    case "-": return ints ? (object)(ToLong(a) - ToLong(b)) : ToDouble(a) - ToDouble(b);
                    case "*": return ints ? (object)(ToLong(a) * ToLong(b)) : ToDouble(a) * ToDouble(b);
                    case "/":
                        if (ToDouble(b) == 0) throw new TemplateError("division by zero");
                        return ToDouble(a) / ToDouble(b);
                    case "//":
                        if (ToDouble(b) == 0) throw new TemplateError("integer division or modulo by zero");
                        if (ints)
                        {
                            long x = ToLong(a), y = ToLong(b);
                            long q = x / y;
                            if ((x % y != 0) && ((x < 0) != (y < 0))) q--;
                            return q;
                        }
                        return Math.Floor(ToDouble(a) / ToDouble(b));
                    case "%":
                        if (ToDouble(b) == 0) throw new TemplateError("integer division or modulo by zero");
                        if (ints)
                        {
                            long x = ToLong(a), y = ToLong(b);
                            long r = x % y;
                            if (r != 0 && ((r < 0) != (y < 0))) r += y;
                            return r;
                        }
                        else
                        {
                            double x = ToDouble(a), y = ToDouble(b);
                            double r = x % y;
                            if (r != 0 && ((r < 0) != (y < 0))) r += y;
                            return r;
                        }
                    case "**":
                        if (ints && ToLong(b) >= 0)
                        {
                            long bs = ToLong(a), ex = ToLong(b), res = 1;
                            while (ex > 0) { if ((ex & 1) == 1) res *= bs; bs *= bs; ex >>= 1; }
                            return res;
                        }
                        return Math.Pow(ToDouble(a), ToDouble(b));
                }
            }

            switch (op)
            {
                case "+":
                    if (a is string s1 && b is string s2) return s1 + s2;
                    if (a is IList l1 && b is IList l2)
                    {
                        var items = l1.Cast<object>().Concat(l2.Cast<object>());
                        return a is PyTuple ? new PyTuple(items) : (object)new List<object>(items);
                    }
                    break;
                case "*":
                    if (a is string rs && IsInt(b)) return Repeat(rs, ToLong(b));
                    if (b is string rs2 && IsInt(a)) return Repeat(rs2, ToLong(a));
                    if (a is IList rl && IsInt(b)) return RepeatList(rl, ToLong(b));
                    if (b is IList rl2 && IsInt(a)) return RepeatList(rl2, ToLong(a));
                    break;
                case "%":
                    if (a is string fmt) return Formatting.Percent(fmt, b);
                    break;
            }
            throw new TemplateError($"unsupported operand type(s) for {op}: '{TypeName(a)}' and '{TypeName(b)}'");
        }

        private static string Repeat(string s, long n)
        {
            var sb = new StringBuilder();
            for (long i = 0; i < n; i++) sb.Append(s);
            return sb.ToString();
        }

        private static List<object> RepeatList(IList l, long n)
        {
            var r = new List<object>();
            for (long i = 0; i < n; i++) r.AddRange(l.Cast<object>());
            return r;
        }

        // ---------------------------------------------------------------- JSON output (tojson)

        public static string ToJson(object o, int? indent)
        {
            var sb = new StringBuilder();
            WriteJson(sb, o, indent, 0);
            return sb.ToString()
                .Replace("<", "\\u003c").Replace(">", "\\u003e")
                .Replace("&", "\\u0026").Replace("'", "\\u0027");
        }

        private static void WriteJson(StringBuilder sb, object o, int? indent, int level)
        {
            switch (o)
            {
                case null:
                case Undefined _:
                    sb.Append("null"); return;
                case bool b: sb.Append(b ? "true" : "false"); return;
                case long _:
                case int _:
                    sb.Append(Str(o)); return;
                case double d:
                    sb.Append(double.IsNaN(d) ? "NaN" : double.IsPositiveInfinity(d) ? "Infinity" : double.IsNegativeInfinity(d) ? "-Infinity" : FloatStr(d));
                    return;
                case string s: WriteJsonString(sb, s); return;
                case Dictionary<string, object> d:
                {
                    if (d.Count == 0) { sb.Append("{}"); return; }
                    sb.Append('{');
                    bool first = true;
                    foreach (var kv in d.OrderBy(k => k.Key, StringComparer.Ordinal))
                    {
                        if (!first) sb.Append(indent.HasValue ? "," : ", ");
                        first = false;
                        NewLine(sb, indent, level + 1);
                        WriteJsonString(sb, kv.Key);
                        sb.Append(": ");
                        WriteJson(sb, kv.Value, indent, level + 1);
                    }
                    NewLine(sb, indent, level);
                    sb.Append('}');
                    return;
                }
                case IList l:
                {
                    if (l.Count == 0) { sb.Append("[]"); return; }
                    sb.Append('[');
                    for (int i = 0; i < l.Count; i++)
                    {
                        if (i > 0) sb.Append(indent.HasValue ? "," : ", ");
                        NewLine(sb, indent, level + 1);
                        WriteJson(sb, l[i], indent, level + 1);
                    }
                    NewLine(sb, indent, level);
                    sb.Append(']');
                    return;
                }
            }
            throw new TemplateError($"Object of type {TypeName(o)} is not JSON serializable");
        }

        private static void NewLine(StringBuilder sb, int? indent, int level)
        {
            if (!indent.HasValue) return;
            sb.Append('\n').Append(' ', indent.Value * level);
        }

        private static void WriteJsonString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (c < 0x20 || c > 0x7e) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }
    }

    // ==================================================================== methods on values (str.upper(), dict.items(), ...)

    internal static class Methods
    {
        private static readonly HashSet<string> DictMethods = new HashSet<string> { "items", "keys", "values", "get" };
        private static readonly HashSet<string> ListMethods = new HashSet<string> { "index", "count" };
        private static readonly HashSet<string> StrMethods = new HashSet<string>
        {
            "upper", "lower", "strip", "lstrip", "rstrip", "split", "splitlines", "startswith", "endswith",
            "replace", "title", "capitalize", "format", "join", "count", "find", "index", "isdigit", "isalpha",
            "isalnum", "isspace", "islower", "isupper", "zfill", "center", "ljust", "rjust", "swapcase"
        };

        public static bool Has(object obj, string name)
        {
            switch (obj)
            {
                case Namespace _: return false;
                case Dictionary<string, object> _: return DictMethods.Contains(name);
                case string _: return StrMethods.Contains(name);
                case IList _: return ListMethods.Contains(name);
            }
            return false;
        }

        public static object Invoke(object self, string name, Args a)
        {
            switch (self)
            {
                case LoopContext lc: return name == "cycle" ? lc.Cycle(a) : lc.Changed(a);
                case Dictionary<string, object> d: return DictMethod(d, name, a);
                case string s: return StrMethod(s, name, a);
                case IList l:
                    var item = a.Get(0, null);
                    if (name == "count") return (long)l.Cast<object>().Count(x => Py.Eq(x, item));
                    for (int i = 0; i < l.Count; i++) if (Py.Eq(l[i], item)) return (long)i;
                    throw new TemplateError($"{Py.Repr(item)} is not in list");
            }
            throw new TemplateError($"'{Py.TypeName(self)}' has no method '{name}'");
        }

        private static object DictMethod(Dictionary<string, object> d, string name, Args a)
        {
            switch (name)
            {
                case "items": return d.Select(kv => (object)new PyTuple(new[] { kv.Key, kv.Value })).ToList();
                case "keys": return d.Keys.Cast<object>().ToList();
                case "values": return d.Values.ToList();
                default: // get
                    return d.TryGetValue(Py.Str(a.Get(0, "key")), out var v) ? v : a.Get(1, "default");
            }
        }

        private static object StrMethod(string s, string name, Args a)
        {
            switch (name)
            {
                case "upper": return s.ToUpperInvariant();
                case "lower": return s.ToLowerInvariant();
                case "swapcase": return new string(s.Select(c => char.IsUpper(c) ? char.ToLowerInvariant(c) : char.ToUpperInvariant(c)).ToArray());
                case "strip": return a.Get(0, null) is string cs ? s.Trim(cs.ToCharArray()) : s.Trim();
                case "lstrip": return a.Get(0, null) is string cl ? s.TrimStart(cl.ToCharArray()) : s.TrimStart();
                case "rstrip": return a.Get(0, null) is string cr ? s.TrimEnd(cr.ToCharArray()) : s.TrimEnd();
                case "split": return Split(s, a.Get(0, "sep") as string, a.Long(1, "maxsplit", -1));
                case "splitlines": return Filters.SplitLines(s).Cast<object>().ToList();
                case "startswith": return Prefixes(a.Get(0, null)).Any(p => s.StartsWith(p, StringComparison.Ordinal));
                case "endswith": return Prefixes(a.Get(0, null)).Any(p => s.EndsWith(p, StringComparison.Ordinal));
                case "replace": return Filters.Replace(s, a.Str(0, "old", ""), a.Str(1, "new", ""), a.Long(2, "count", -1));
                case "title": return Filters.Title(s);
                case "capitalize": return Filters.Capitalize(s);
                case "format": return Formatting.Braces(s, a.Pos, a.Kw);
                case "join": return string.Join(s, Py.Iterate(a.Get(0, null)).Select(Py.Str));
                case "count": return (long)CountOccurrences(s, a.Str(0, null, ""));
                case "find": return (long)s.IndexOf(a.Str(0, null, ""), StringComparison.Ordinal);
                case "index":
                    int ix = s.IndexOf(a.Str(0, null, ""), StringComparison.Ordinal);
                    if (ix < 0) throw new TemplateError("substring not found");
                    return (long)ix;
                case "isdigit": return s.Length > 0 && s.All(char.IsDigit);
                case "isalpha": return s.Length > 0 && s.All(char.IsLetter);
                case "isalnum": return s.Length > 0 && s.All(char.IsLetterOrDigit);
                case "isspace": return s.Length > 0 && s.All(char.IsWhiteSpace);
                case "islower": return s.Any(char.IsLetter) && s == s.ToLowerInvariant();
                case "isupper": return s.Any(char.IsLetter) && s == s.ToUpperInvariant();
                case "zfill":
                {
                    int w = (int)a.Long(0, "width", 0);
                    if (s.Length >= w) return s;
                    bool sign = s.Length > 0 && (s[0] == '-' || s[0] == '+');
                    return sign ? s[0] + new string('0', w - s.Length) + s.Substring(1) : new string('0', w - s.Length) + s;
                }
                case "center": return Filters.Center(s, (int)a.Long(0, "width", 0), a.Str(1, "fillchar", " ")[0]);
                case "ljust": return s.PadRight((int)a.Long(0, "width", 0), a.Str(1, "fillchar", " ")[0]);
                case "rjust": return s.PadLeft((int)a.Long(0, "width", 0), a.Str(1, "fillchar", " ")[0]);
            }
            throw new TemplateError($"'str' object has no attribute '{name}'");
        }

        private static IEnumerable<string> Prefixes(object o) => o is IList l ? l.Cast<object>().Select(Py.Str) : new[] { Py.Str(o) };

        private static int CountOccurrences(string s, string sub)
        {
            if (sub.Length == 0) return s.Length + 1;
            int n = 0, i = 0;
            while ((i = s.IndexOf(sub, i, StringComparison.Ordinal)) >= 0) { n++; i += sub.Length; }
            return n;
        }

        public static List<object> Split(string s, string sep, long maxsplit)
        {
            var result = new List<object>();
            if (sep == null)
            {
                // Python whitespace split: runs of whitespace, no empty strings.
                int i = 0, n = s.Length;
                while (i < n)
                {
                    while (i < n && char.IsWhiteSpace(s[i])) i++;
                    if (i >= n) break;
                    if (maxsplit >= 0 && result.Count == maxsplit) { result.Add(s.Substring(i).TrimEnd()); break; }
                    int st = i;
                    while (i < n && !char.IsWhiteSpace(s[i])) i++;
                    result.Add(s.Substring(st, i - st));
                }
                return result;
            }
            if (sep.Length == 0) throw new TemplateError("empty separator");
            int pos = 0;
            while (true)
            {
                int idx = maxsplit >= 0 && result.Count == maxsplit ? -1 : s.IndexOf(sep, pos, StringComparison.Ordinal);
                if (idx < 0) { result.Add(s.Substring(pos)); break; }
                result.Add(s.Substring(pos, idx - pos));
                pos = idx + sep.Length;
            }
            return result;
        }
    }

    // ==================================================================== global functions

    internal static class Globals
    {
        public static readonly Dictionary<string, object> Table = new Dictionary<string, object>
        {
            ["range"] = new BuiltinFunction((ctx, a) =>
            {
                long start = 0, stop, step = 1;
                if (a.Pos.Count == 1) stop = Py.ToLong(a.Pos[0]);
                else if (a.Pos.Count >= 2)
                {
                    start = Py.ToLong(a.Pos[0]);
                    stop = Py.ToLong(a.Pos[1]);
                    if (a.Pos.Count > 2) step = Py.ToLong(a.Pos[2]);
                }
                else throw new TemplateError("range expected at least 1 argument");
                if (step == 0) throw new TemplateError("range() arg 3 must not be zero");
                var r = new List<object>();
                for (long i = start; step > 0 ? i < stop : i > stop; i += step)
                {
                    r.Add(i);
                    if (r.Count > 100000) throw new TemplateError("range too big (max 100000 items)");
                }
                return r;
            }),
            ["dict"] = new BuiltinFunction((ctx, a) =>
            {
                var d = new Dictionary<string, object>();
                if (a.Pos.Count > 0 && a.Pos[0] is Dictionary<string, object> src)
                    foreach (var kv in src) d[kv.Key] = kv.Value;
                foreach (var kv in a.Kw) d[kv.Key] = kv.Value;
                return d;
            }),
            ["namespace"] = new BuiltinFunction((ctx, a) =>
            {
                var ns = new Namespace();
                if (a.Pos.Count > 0 && a.Pos[0] is Dictionary<string, object> src)
                    foreach (var kv in src) ns[kv.Key] = kv.Value;
                foreach (var kv in a.Kw) ns[kv.Key] = kv.Value;
                return ns;
            }),
            ["joiner"] = new BuiltinFunction((ctx, a) =>
            {
                string sep = a.Str(0, "sep", ", ");
                bool used = false;
                return new BuiltinFunction((c2, a2) =>
                {
                    if (!used) { used = true; return ""; }
                    return sep;
                });
            }),
        };
    }
}
