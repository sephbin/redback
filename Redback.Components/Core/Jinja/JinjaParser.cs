using System;
using System.Collections.Generic;

namespace Redback.Components.Core.Jinja
{
    /// <summary>Turns lexer segments into a tree of statement nodes and expression nodes.</summary>
    internal sealed class Parser
    {
        private static readonly HashSet<string> TestArgStop = new HashSet<string> { "else", "or", "and", "if", "in", "not", "is" };

        private readonly List<Segment> _segs;
        private int _si;

        public Parser(List<Segment> segs) { _segs = segs; }

        public List<Node> ParseTemplate() => ParseBody(null, out _, out _);

        private List<Node> ParseBody(string[] ends, out string endTag, out TokenStream endStream)
        {
            StackGuard.Check();
            var nodes = new List<Node>();
            while (_si < _segs.Count)
            {
                var seg = _segs[_si++];
                switch (seg.Kind)
                {
                    case SegKind.Text:
                        nodes.Add(new TextNode(seg.Content) { Line = seg.Line });
                        break;

                    case SegKind.Output:
                    {
                        var ts = new TokenStream(seg.Content, seg.Line);
                        if (ts.AtEnd) throw ts.Error("Empty expression in '{{ }}'");
                        var e = ParseTuple(ts, true);
                        ts.ExpectEnd();
                        nodes.Add(new OutputNode(e) { Line = seg.Line });
                        break;
                    }

                    case SegKind.Tag:
                    {
                        var ts = new TokenStream(seg.Content, seg.Line);
                        if (ts.Peek.Type != TT.Name) throw ts.Error("Expected a statement name after '{%'");
                        string head = ts.Next().Text;
                        if (ends != null && Array.IndexOf(ends, head) >= 0)
                        {
                            endTag = head;
                            endStream = ts;
                            return nodes;
                        }
                        var node = ParseStatement(head, ts);
                        node.Line = seg.Line;
                        nodes.Add(node);
                        break;
                    }
                }
            }
            if (ends != null)
            {
                int line = _segs.Count > 0 ? _segs[_segs.Count - 1].Line : 0;
                throw new TemplateError($"Unexpected end of template; expected '{string.Join("' or '", ends)}'", line);
            }
            endTag = null;
            endStream = null;
            return nodes;
        }

        private List<Node> ParseBlockUntil(string end)
        {
            var body = ParseBody(new[] { end }, out _, out _);
            return body;
        }

        private Node ParseStatement(string head, TokenStream ts)
        {
            switch (head)
            {
                case "if": return ParseIf(ts);
                case "for": return ParseFor(ts);
                case "set": return ParseSet(ts);
                case "macro": return ParseMacro(ts);
                case "call": return ParseCallBlock(ts);
                case "filter": return ParseFilterBlock(ts);
                case "with": return ParseWith(ts);
                case "block":
                    ts.ExpectName();
                    ts.SkipName("scoped");
                    ts.ExpectEnd();
                    return new BlockNode(ParseBlockUntil("endblock"));
                case "break": ts.ExpectEnd(); return new BreakNode();
                case "continue": ts.ExpectEnd(); return new ContinueNode();
                case "include":
                case "extends":
                case "import":
                case "from":
                    throw ts.Error($"'{head}' is not supported: this component renders a single template with no template loader");
                default:
                    if (head.StartsWith("end") || head == "else" || head == "elif")
                        throw ts.Error($"Unexpected '{head}': there is no open block for it to close");
                    throw ts.Error($"Unknown statement '{head}'");
            }
        }

        private Node ParseIf(TokenStream ts)
        {
            var node = new IfNode();
            var cond = ParseExpr(ts);
            ts.ExpectEnd();
            while (true)
            {
                var body = ParseBody(new[] { "elif", "else", "endif" }, out string tag, out var ets);
                node.Branches.Add(new KeyValuePair<Expr, List<Node>>(cond, body));
                if (tag == "elif")
                {
                    cond = ParseExpr(ets);
                    ets.ExpectEnd();
                    continue;
                }
                if (tag == "else")
                {
                    ets.ExpectEnd();
                    node.Else = ParseBody(new[] { "endif" }, out _, out var e2);
                    e2.ExpectEnd();
                }
                else ets.ExpectEnd();
                return node;
            }
        }

        private Node ParseFor(TokenStream ts)
        {
            var node = new ForNode();
            bool paren = ts.SkipOp("(");
            node.Targets.Add(ts.ExpectName());
            while (ts.SkipOp(",")) node.Targets.Add(ts.ExpectName());
            if (paren) ts.ExpectOp(")");
            ts.ExpectKeyword("in");
            node.Iter = ParseTuple(ts, false);
            if (ts.SkipName("if")) node.Filter = ParseExpr(ts);
            if (ts.IsName("recursive")) throw ts.Error("Recursive for-loops are not supported");
            ts.ExpectEnd();
            node.Body = ParseBody(new[] { "else", "endfor" }, out string tag, out var ets);
            ets.ExpectEnd();
            if (tag == "else")
            {
                node.Else = ParseBody(new[] { "endfor" }, out _, out var e2);
                e2.ExpectEnd();
            }
            return node;
        }

        private Node ParseSet(TokenStream ts)
        {
            string first = ts.ExpectName();
            if (ts.SkipOp("."))
            {
                string attr = ts.ExpectName();
                ts.ExpectOp("=");
                var v = ParseTuple(ts, true);
                ts.ExpectEnd();
                return new SetAttrNode { Name = first, Attr = attr, Value = v };
            }

            var targets = new List<string> { first };
            while (ts.SkipOp(",")) targets.Add(ts.ExpectName());

            if (ts.SkipOp("="))
            {
                var v = ParseTuple(ts, true);
                ts.ExpectEnd();
                return new SetNode { Targets = targets, Value = v };
            }

            if (targets.Count != 1) throw ts.Error("Block assignment ({% set x %}...{% endset %}) takes a single name");
            Expr filter = null;
            if (ts.SkipOp("|")) filter = ParseFilterChain(ts, new PlaceholderExpr());
            ts.ExpectEnd();
            var body = ParseBody(new[] { "endset" }, out _, out var ets);
            ets.ExpectEnd();
            return new SetBlockNode { Name = first, Body = body, Filter = filter };
        }

        private List<MacroParam> ParseParams(TokenStream ts)
        {
            var ps = new List<MacroParam>();
            ts.ExpectOp("(");
            while (!ts.IsOp(")"))
            {
                if (ps.Count > 0) ts.ExpectOp(",");
                if (ts.IsOp(")")) break;
                var p = new MacroParam { Name = ts.ExpectName() };
                if (ts.SkipOp("=")) p.Default = ParseExpr(ts);
                ps.Add(p);
            }
            ts.ExpectOp(")");
            return ps;
        }

        private Node ParseMacro(TokenStream ts)
        {
            var node = new MacroNode { Name = ts.ExpectName() };
            node.Params = ts.IsOp("(") ? ParseParams(ts) : new List<MacroParam>();
            ts.ExpectEnd();
            node.Body = ParseBody(new[] { "endmacro" }, out _, out _);
            return node;
        }

        private Node ParseCallBlock(TokenStream ts)
        {
            var node = new CallBlockNode();
            node.Params = ts.IsOp("(") ? ParseParams(ts) : new List<MacroParam>();
            var call = ParseExpr(ts) as CallExpr;
            if (call == null) throw ts.Error("Expected a macro call after 'call'");
            node.Call = call;
            ts.ExpectEnd();
            node.Body = ParseBody(new[] { "endcall" }, out _, out _);
            return node;
        }

        private Node ParseFilterBlock(TokenStream ts)
        {
            var filter = ParseFilterChain(ts, new PlaceholderExpr());
            ts.ExpectEnd();
            var body = ParseBody(new[] { "endfilter" }, out _, out _);
            return new FilterBlockNode { Filter = filter, Body = body };
        }

        private Node ParseWith(TokenStream ts)
        {
            var node = new WithNode();
            while (!ts.AtEnd)
            {
                if (node.Assignments.Count > 0) ts.ExpectOp(",");
                string name = ts.ExpectName();
                ts.ExpectOp("=");
                node.Assignments.Add(new KeyValuePair<string, Expr>(name, ParseExpr(ts)));
            }
            node.Body = ParseBody(new[] { "endwith" }, out _, out _);
            return node;
        }

        // ---------------------------------------------------------------- expressions

        private Expr ParseTuple(TokenStream ts, bool withCond)
        {
            var first = ParseExpr(ts, withCond);
            if (!ts.IsOp(",")) return first;
            var items = new List<Expr> { first };
            while (ts.SkipOp(","))
            {
                if (ts.AtEnd || ts.IsOp(")") || ts.IsOp("=") || ts.IsName("if")) break;
                items.Add(ParseExpr(ts, withCond));
            }
            return new ListExpr(items, true);
        }

        private Expr ParseExpr(TokenStream ts, bool withCond = true)
        {
            StackGuard.Check();
            var e = ParseOr(ts);
            if (!withCond) return e;
            while (ts.IsName("if"))
            {
                ts.Next();
                var cond = ParseOr(ts);
                Expr els = null;
                if (ts.SkipName("else")) els = ParseExpr(ts);
                e = new CondExpr(cond, e, els);
            }
            return e;
        }

        private Expr ParseOr(TokenStream ts)
        {
            var l = ParseAnd(ts);
            while (ts.SkipName("or")) l = new BinaryExpr("or", l, ParseAnd(ts));
            return l;
        }

        private Expr ParseAnd(TokenStream ts)
        {
            var l = ParseNot(ts);
            while (ts.SkipName("and")) l = new BinaryExpr("and", l, ParseNot(ts));
            return l;
        }

        private Expr ParseNot(TokenStream ts)
        {
            if (ts.SkipName("not")) return new UnaryExpr("not", ParseNot(ts));
            return ParseCompare(ts);
        }

        private Expr ParseCompare(TokenStream ts)
        {
            var first = ParseMath1(ts);
            List<KeyValuePair<string, Expr>> ops = null;
            while (true)
            {
                string op = null;
                var t = ts.Peek;
                if (t.Type == TT.Op && (t.Text == "==" || t.Text == "!=" || t.Text == "<" || t.Text == "<=" || t.Text == ">" || t.Text == ">="))
                { op = t.Text; ts.Next(); }
                else if (ts.IsName("in")) { op = "in"; ts.Next(); }
                else if (ts.IsName("not") && ts.Look(1).Type == TT.Name && ts.Look(1).Text == "in")
                { op = "notin"; ts.Next(); ts.Next(); }
                if (op == null) break;
                (ops ??= new List<KeyValuePair<string, Expr>>()).Add(new KeyValuePair<string, Expr>(op, ParseMath1(ts)));
            }
            return ops == null ? first : new CompareExpr(first, ops);
        }

        private Expr ParseMath1(TokenStream ts)
        {
            var l = ParseConcat(ts);
            while (ts.IsOp("+") || ts.IsOp("-"))
            {
                string op = ts.Next().Text;
                l = new BinaryExpr(op, l, ParseConcat(ts));
            }
            return l;
        }

        private Expr ParseConcat(TokenStream ts)
        {
            var l = ParseMath2(ts);
            while (ts.SkipOp("~")) l = new BinaryExpr("~", l, ParseMath2(ts));
            return l;
        }

        private Expr ParseMath2(TokenStream ts)
        {
            var l = ParsePow(ts);
            while (ts.IsOp("*") || ts.IsOp("/") || ts.IsOp("//") || ts.IsOp("%"))
            {
                string op = ts.Next().Text;
                l = new BinaryExpr(op, l, ParsePow(ts));
            }
            return l;
        }

        private Expr ParsePow(TokenStream ts)
        {
            var l = ParseUnary(ts, true);
            while (ts.SkipOp("**")) l = new BinaryExpr("**", l, ParseUnary(ts, true));
            return l;
        }

        private Expr ParseUnary(TokenStream ts, bool withFilter)
        {
            Expr node;
            if (ts.SkipOp("-")) node = new UnaryExpr("neg", ParseUnary(ts, false));
            else if (ts.SkipOp("+")) node = new UnaryExpr("pos", ParseUnary(ts, false));
            else node = ParsePrimary(ts);
            node = ParsePostfix(ts, node);
            if (withFilter) node = ParseFilterExpr(ts, node);
            return node;
        }

        private Expr ParsePrimary(TokenStream ts)
        {
            var t = ts.Next();
            switch (t.Type)
            {
                case TT.Name:
                    switch (t.Text)
                    {
                        case "true": case "True": return new LiteralExpr(true);
                        case "false": case "False": return new LiteralExpr(false);
                        case "none": case "None": return new LiteralExpr(null);
                    }
                    return new NameExpr(t.Text);
                case TT.Str:
                {
                    string s = (string)t.Value;
                    while (ts.Peek.Type == TT.Str) s += (string)ts.Next().Value;
                    return new LiteralExpr(s);
                }
                case TT.Int:
                case TT.Float:
                    return new LiteralExpr(t.Value);
                case TT.Op:
                    if (t.Text == "(")
                    {
                        if (ts.SkipOp(")")) return new ListExpr(new List<Expr>(), true);
                        var e = ParseExpr(ts);
                        if (ts.IsOp(","))
                        {
                            var items = new List<Expr> { e };
                            while (ts.SkipOp(","))
                            {
                                if (ts.IsOp(")")) break;
                                items.Add(ParseExpr(ts));
                            }
                            e = new ListExpr(items, true);
                        }
                        ts.ExpectOp(")");
                        return e;
                    }
                    if (t.Text == "[")
                    {
                        var items = new List<Expr>();
                        while (!ts.IsOp("]"))
                        {
                            if (items.Count > 0) ts.ExpectOp(",");
                            if (ts.IsOp("]")) break;
                            items.Add(ParseExpr(ts));
                        }
                        ts.ExpectOp("]");
                        return new ListExpr(items, false);
                    }
                    if (t.Text == "{")
                    {
                        var pairs = new List<KeyValuePair<Expr, Expr>>();
                        while (!ts.IsOp("}"))
                        {
                            if (pairs.Count > 0) ts.ExpectOp(",");
                            if (ts.IsOp("}")) break;
                            var k = ParseExpr(ts);
                            ts.ExpectOp(":");
                            pairs.Add(new KeyValuePair<Expr, Expr>(k, ParseExpr(ts)));
                        }
                        ts.ExpectOp("}");
                        return new DictExpr(pairs);
                    }
                    break;
            }
            throw ts.Error($"Unexpected {t}");
        }

        private Expr ParsePostfix(TokenStream ts, Expr node)
        {
            while (true)
            {
                if (ts.SkipOp("."))
                {
                    var t = ts.Next();
                    if (t.Type == TT.Name) node = new GetAttrExpr(node, t.Text);
                    else if (t.Type == TT.Int) node = new GetItemExpr(node, new LiteralExpr(t.Value));
                    else throw ts.Error($"Expected attribute name after '.', got {t}");
                }
                else if (ts.SkipOp("["))
                {
                    Expr a = null, b = null, c = null;
                    bool slice = false;
                    if (!ts.IsOp(":")) a = ParseExpr(ts);
                    if (ts.SkipOp(":"))
                    {
                        slice = true;
                        if (!ts.IsOp("]") && !ts.IsOp(":")) b = ParseExpr(ts);
                        if (ts.SkipOp(":") && !ts.IsOp("]")) c = ParseExpr(ts);
                    }
                    ts.ExpectOp("]");
                    node = slice ? new SliceExpr(node, a, b, c) : (Expr)new GetItemExpr(node, a);
                }
                else if (ts.IsOp("("))
                {
                    var call = new CallExpr { Fn = node };
                    ParseCallArgs(ts, call.Args, call.Kwargs);
                    node = call;
                }
                else return node;
            }
        }

        private void ParseCallArgs(TokenStream ts, List<Expr> args, List<KeyValuePair<string, Expr>> kwargs)
        {
            ts.ExpectOp("(");
            bool first = true;
            while (!ts.IsOp(")"))
            {
                if (!first) ts.ExpectOp(",");
                first = false;
                if (ts.IsOp(")")) break;
                if (ts.IsOp("*") || ts.IsOp("**")) throw ts.Error("*args / **kwargs call syntax is not supported");
                if (ts.Peek.Type == TT.Name && ts.Look(1).Type == TT.Op && ts.Look(1).Text == "=")
                {
                    string name = ts.Next().Text;
                    ts.Next();
                    kwargs.Add(new KeyValuePair<string, Expr>(name, ParseExpr(ts)));
                }
                else
                {
                    if (kwargs.Count > 0) throw ts.Error("Positional argument follows keyword argument");
                    args.Add(ParseExpr(ts));
                }
            }
            ts.ExpectOp(")");
        }

        private Expr ParseFilterExpr(TokenStream ts, Expr node)
        {
            while (true)
            {
                if (ts.SkipOp("|")) node = ParseOneFilter(ts, node);
                else if (ts.IsName("is")) node = ParseTest(ts, node);
                else return node;
            }
        }

        /// <summary>Parses "name(args) | name2 ..." (no leading pipe) applied to <paramref name="target"/>.</summary>
        private Expr ParseFilterChain(TokenStream ts, Expr target)
        {
            var node = ParseOneFilter(ts, target);
            while (ts.SkipOp("|")) node = ParseOneFilter(ts, node);
            return node;
        }

        private Expr ParseOneFilter(TokenStream ts, Expr target)
        {
            string name = ts.ExpectName();
            if (!Filters.Exists(name)) throw ts.Error($"No filter named '{name}'");
            var f = new FilterExpr { Target = target, Name = name };
            if (ts.IsOp("(")) ParseCallArgs(ts, f.Args, f.Kwargs);
            return f;
        }

        private Expr ParseTest(TokenStream ts, Expr target)
        {
            ts.ExpectKeyword("is");
            bool negate = ts.SkipName("not");
            string name;
            if (ts.Peek.Type == TT.Name) name = ts.Next().Text;
            else if (ts.Peek.Type == TT.Op && (ts.Peek.Text == "==" || ts.Peek.Text == "!=" || ts.Peek.Text == "<" ||
                                               ts.Peek.Text == "<=" || ts.Peek.Text == ">" || ts.Peek.Text == ">="))
                name = ts.Next().Text;
            else throw ts.Error($"Expected a test name after 'is', got {ts.Peek}");

            // 'none', 'true' and 'false' parse as literals elsewhere but are test names here.
            if (!Tests.Exists(name)) throw ts.Error($"No test named '{name}'");
            var node = new TestExpr { Target = target, Name = name, Negate = negate };

            if (ts.IsOp("("))
            {
                ParseCallArgs(ts, node.Args, node.Kwargs);
            }
            else
            {
                var p = ts.Peek;
                bool startsArg = p.Type == TT.Str || p.Type == TT.Int || p.Type == TT.Float ||
                                 (p.Type == TT.Name && !TestArgStop.Contains(p.Text)) ||
                                 (p.Type == TT.Op && (p.Text == "[" || p.Text == "{"));
                if (startsArg) node.Args.Add(ParsePostfix(ts, ParsePrimary(ts)));
            }
            return node;
        }
    }
}
