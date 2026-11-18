using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Redback.Components.Core.Jinja
{
    /// <summary>Error raised while parsing or rendering a template. Line is 1-based (0 = unknown).</summary>
    public sealed class TemplateError : Exception
    {
        public int Line { get; private set; }
        public string RawMessage { get; }

        public TemplateError(string message, int line = 0) : base(message)
        {
            Line = line;
            RawMessage = message;
        }

        public override string Message => Line > 0 ? $"Line {Line}: {RawMessage}" : RawMessage;

        internal void SetLineIfUnknown(int line)
        {
            if (Line == 0) Line = line;
        }
    }

    internal enum SegKind { Text, Output, Tag }

    internal sealed class Segment
    {
        public readonly SegKind Kind;
        public readonly string Content;
        public readonly int Line;
        public Segment(SegKind kind, string content, int line) { Kind = kind; Content = content; Line = line; }
    }

    /// <summary>
    /// Splits template source into text / {{ output }} / {% tag %} segments, applying Jinja's
    /// whitespace-control rules ({%- -%} etc.), comment removal and {% raw %} blocks.
    /// </summary>
    internal static class Lexer
    {
        private static readonly Regex RawEnd = new Regex(@"\{%(-?)\s*endraw\s*(-?)%\}", RegexOptions.Compiled);

        public static List<Segment> Split(string src)
        {
            // Jinja normalises newlines and (keep_trailing_newline=False) drops one trailing newline.
            src = (src ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            if (src.EndsWith("\n")) src = src.Substring(0, src.Length - 1);

            var segs = new List<Segment>();
            int pos = 0;
            bool stripNext = false;

            int lineIdx = 0, lineNo = 1;
            int LineAt(int idx)
            {
                if (idx < lineIdx) { lineIdx = 0; lineNo = 1; }
                for (; lineIdx < idx && lineIdx < src.Length; lineIdx++)
                    if (src[lineIdx] == '\n') lineNo++;
                return lineNo;
            }

            while (pos < src.Length)
            {
                int open = FindOpen(src, pos);
                string text = open < 0 ? src.Substring(pos) : src.Substring(pos, open - pos);
                int textLine = LineAt(pos);
                if (stripNext) { text = text.TrimStart(); stripNext = false; }

                if (open < 0)
                {
                    AddText(segs, text, textLine);
                    break;
                }

                char kind = src[open + 1];
                int p = open + 2;
                if (p < src.Length && src[p] == '-') { text = text.TrimEnd(); p++; }
                else if (p < src.Length && src[p] == '+') p++;
                AddText(segs, text, textLine);

                int line = LineAt(open);

                if (kind == '#')
                {
                    int cclose = src.IndexOf("#}", p, StringComparison.Ordinal);
                    if (cclose < 0) throw new TemplateError("Missing end of comment tag '#}'", line);
                    stripNext = cclose > p && src[cclose - 1] == '-';
                    pos = cclose + 2;
                    continue;
                }

                int close = FindClose(src, p, kind == '{' ? '}' : '%', line);
                int contentEnd = close;
                if (contentEnd > p && src[contentEnd - 1] == '-') { stripNext = true; contentEnd--; }
                else if (contentEnd > p && src[contentEnd - 1] == '+') contentEnd--;
                string content = src.Substring(p, contentEnd - p);
                pos = close + 2;

                if (kind == '{')
                {
                    segs.Add(new Segment(SegKind.Output, content, line));
                }
                else if (content.Trim() == "raw")
                {
                    var m = RawEnd.Match(src, pos);
                    if (!m.Success) throw new TemplateError("Missing '{% endraw %}'", line);
                    string raw = src.Substring(pos, m.Index - pos);
                    if (stripNext) { raw = raw.TrimStart(); stripNext = false; }
                    if (m.Groups[1].Value == "-") raw = raw.TrimEnd();
                    AddText(segs, raw, LineAt(pos));
                    stripNext = m.Groups[2].Value == "-";
                    pos = m.Index + m.Length;
                }
                else
                {
                    segs.Add(new Segment(SegKind.Tag, content, line));
                }
            }
            return segs;
        }

        private static void AddText(List<Segment> segs, string text, int line)
        {
            if (text.Length > 0) segs.Add(new Segment(SegKind.Text, text, line));
        }

        private static int FindOpen(string src, int pos)
        {
            int i = pos;
            while (true)
            {
                int idx = src.IndexOf('{', i);
                if (idx < 0 || idx + 1 >= src.Length) return -1;
                char c = src[idx + 1];
                if (c == '{' || c == '%' || c == '#') return idx;
                i = idx + 1;
            }
        }

        /// <summary>Finds the closing "}}" or "%}" while skipping string literals and nested brackets.</summary>
        private static int FindClose(string src, int p, char closeCh, int line)
        {
            int depth = 0;
            char quote = '\0';
            for (int i = p; i < src.Length; i++)
            {
                char c = src[i];
                if (quote != '\0')
                {
                    if (c == '\\') { i++; continue; }
                    if (c == quote) quote = '\0';
                    continue;
                }
                if (c == '\'' || c == '"') { quote = c; continue; }
                if (c == '(' || c == '[' || c == '{') { depth++; continue; }
                if (depth == 0 && c == closeCh && i + 1 < src.Length && src[i + 1] == '}') return i;
                if (c == ')' || c == ']' || c == '}') { if (depth > 0) depth--; }
            }
            throw new TemplateError(closeCh == '}' ? "Missing end of print tag '}}'" : "Missing end of block tag '%}'", line);
        }
    }

    internal enum TT { Name, Int, Float, Str, Op, End }

    internal sealed class Token
    {
        public readonly TT Type;
        public readonly string Text;
        public readonly object Value;
        public Token(TT type, string text, object value = null) { Type = type; Text = text; Value = value; }
        public override string ToString() => Type == TT.End ? "end of statement" : $"'{Text}'";
    }

    internal sealed class TokenStream
    {
        private static readonly string[] Ops2 = { "**", "//", "==", "!=", "<=", ">=" };
        private const string Ops1 = "+-*/%~<>=()[]{},.:|";

        private readonly List<Token> _toks;
        private int _i;
        public readonly int Line;

        public TokenStream(string src, int line)
        {
            Line = line;
            _toks = Tokenize(src, line);
        }

        public Token Peek => _toks[_i];
        public Token Look(int n) => _toks[Math.Min(_i + n, _toks.Count - 1)];
        public Token Next() { var t = _toks[_i]; if (_i < _toks.Count - 1) _i++; return t; }
        public bool AtEnd => Peek.Type == TT.End;

        public bool IsOp(string op) => Peek.Type == TT.Op && Peek.Text == op;
        public bool IsName(string name) => Peek.Type == TT.Name && Peek.Text == name;

        public bool SkipOp(string op) { if (IsOp(op)) { Next(); return true; } return false; }
        public bool SkipName(string name) { if (IsName(name)) { Next(); return true; } return false; }

        public void ExpectOp(string op)
        {
            if (!IsOp(op)) throw Error($"Expected '{op}', got {Peek}");
            Next();
        }

        public void ExpectKeyword(string name)
        {
            if (!IsName(name)) throw Error($"Expected '{name}', got {Peek}");
            Next();
        }

        public string ExpectName()
        {
            if (Peek.Type != TT.Name) throw Error($"Expected a name, got {Peek}");
            return Next().Text;
        }

        public void ExpectEnd()
        {
            if (!AtEnd) throw Error($"Unexpected {Peek}");
        }

        public TemplateError Error(string msg) => new TemplateError(msg, Line);

        private static List<Token> Tokenize(string s, int line)
        {
            var toks = new List<Token>();
            int i = 0, n = s.Length;
            while (i < n)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }

                if (char.IsLetter(c) || c == '_')
                {
                    int st = i;
                    while (i < n && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
                    toks.Add(new Token(TT.Name, s.Substring(st, i - st)));
                    continue;
                }

                if (char.IsDigit(c))
                {
                    int st = i;
                    bool isFloat = false;
                    while (i < n && (char.IsDigit(s[i]) || s[i] == '_')) i++;
                    if (i + 1 < n && s[i] == '.' && char.IsDigit(s[i + 1]))
                    {
                        isFloat = true;
                        i++;
                        while (i < n && (char.IsDigit(s[i]) || s[i] == '_')) i++;
                    }
                    if (i < n && (s[i] == 'e' || s[i] == 'E'))
                    {
                        int save = i;
                        i++;
                        if (i < n && (s[i] == '+' || s[i] == '-')) i++;
                        if (i < n && char.IsDigit(s[i]))
                        {
                            isFloat = true;
                            while (i < n && char.IsDigit(s[i])) i++;
                        }
                        else i = save;
                    }
                    string txt = s.Substring(st, i - st);
                    string clean = txt.Replace("_", "");
                    if (isFloat)
                        toks.Add(new Token(TT.Float, txt, double.Parse(clean, NumberStyles.Float, CultureInfo.InvariantCulture)));
                    else if (long.TryParse(clean, NumberStyles.None, CultureInfo.InvariantCulture, out long lv))
                        toks.Add(new Token(TT.Int, txt, lv));
                    else
                        toks.Add(new Token(TT.Float, txt, double.Parse(clean, CultureInfo.InvariantCulture)));
                    continue;
                }

                if (c == '\'' || c == '"')
                {
                    char q = c;
                    i++;
                    var sb = new StringBuilder();
                    bool closed = false;
                    while (i < n)
                    {
                        char ch = s[i++];
                        if (ch == q) { closed = true; break; }
                        if (ch == '\\' && i < n)
                        {
                            char e = s[i++];
                            switch (e)
                            {
                                case 'n': sb.Append('\n'); break;
                                case 't': sb.Append('\t'); break;
                                case 'r': sb.Append('\r'); break;
                                case '0': sb.Append('\0'); break;
                                case '\\': sb.Append('\\'); break;
                                case '\'': sb.Append('\''); break;
                                case '"': sb.Append('"'); break;
                                case 'u':
                                    if (i + 4 <= n && int.TryParse(s.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int u4))
                                    { sb.Append((char)u4); i += 4; }
                                    else sb.Append("\\u");
                                    break;
                                case 'x':
                                    if (i + 2 <= n && int.TryParse(s.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int x2))
                                    { sb.Append((char)x2); i += 2; }
                                    else sb.Append("\\x");
                                    break;
                                default: sb.Append('\\').Append(e); break;
                            }
                        }
                        else sb.Append(ch);
                    }
                    if (!closed) throw new TemplateError("Unterminated string literal", line);
                    toks.Add(new Token(TT.Str, sb.ToString(), sb.ToString()));
                    continue;
                }

                if (i + 1 < n)
                {
                    string two = s.Substring(i, 2);
                    if (Array.IndexOf(Ops2, two) >= 0)
                    {
                        toks.Add(new Token(TT.Op, two));
                        i += 2;
                        continue;
                    }
                }
                if (Ops1.IndexOf(c) >= 0)
                {
                    toks.Add(new Token(TT.Op, c.ToString()));
                    i++;
                    continue;
                }
                throw new TemplateError($"Unexpected character '{c}'", line);
            }
            toks.Add(new Token(TT.End, ""));
            return toks;
        }
    }
}
