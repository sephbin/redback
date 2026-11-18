using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Redback.Components.Core.Jinja
{
    // ==================================================================== filters

    internal static class Filters
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private delegate object FilterFn(Context ctx, object v, Args a);

        private static readonly Dictionary<string, FilterFn> Table = new Dictionary<string, FilterFn>
        {
            ["abs"] = (c, v, a) => v is double d ? Math.Abs(d) : (object)Math.Abs(Py.ToLong(v)),
            ["attr"] = (c, v, a) => Attr(v, a.Str(0, "name", "")),
            ["batch"] = (c, v, a) => Batch(v, a),
            ["capitalize"] = (c, v, a) => Capitalize(Py.Str(v)),
            ["center"] = (c, v, a) => Center(Py.Str(v), (int)a.Long(0, "width", 80), ' '),
            ["count"] = (c, v, a) => Length(v),
            ["d"] = (c, v, a) => Default(v, a),
            ["default"] = (c, v, a) => Default(v, a),
            ["dictsort"] = (c, v, a) => DictSort(v, a),
            ["e"] = (c, v, a) => Escape(Py.Str(v)),
            ["escape"] = (c, v, a) => Escape(Py.Str(v)),
            ["filesizeformat"] = (c, v, a) => FileSize(Py.ToDouble(v), a.Bool(0, "binary")),
            ["first"] = (c, v, a) => Py.Iterate(v).Cast<object>().DefaultIfEmpty(new Undefined("first", "No first item, sequence was empty.")).First(),
            ["float"] = (c, v, a) => ToFloat(v, a),
            ["forceescape"] = (c, v, a) => Escape(Py.Str(v)),
            ["format"] = (c, v, a) => Formatting.Percent(Py.Str(v), a.Kw.Count > 0 ? (object)a.Kw : new PyTuple(a.Pos)),
            ["groupby"] = (c, v, a) => GroupBy(v, a),
            ["indent"] = (c, v, a) => Indent(Py.Str(v), a),
            ["int"] = (c, v, a) => ToInt(v, a),
            ["items"] = (c, v, a) => v is Undefined ? new List<object>() : Methods.Invoke(AsDict(v, "items"), "items", a),
            ["join"] = (c, v, a) => string.Join(a.Str(0, "d", ""), Py.Iterate(v).Select(x => Py.Str(Py.GetAttrPath(x, a.Get(1, "attribute"))))),
            ["last"] = (c, v, a) => Py.Iterate(v).Cast<object>().DefaultIfEmpty(new Undefined("last", "No last item, sequence was empty.")).Last(),
            ["length"] = (c, v, a) => Length(v),
            ["list"] = (c, v, a) => Py.Iterate(v).ToList(),
            ["lower"] = (c, v, a) => Py.Str(v).ToLowerInvariant(),
            ["map"] = (c, v, a) => Map(c, v, a),
            ["max"] = (c, v, a) => MinMax(v, a, true),
            ["min"] = (c, v, a) => MinMax(v, a, false),
            ["pprint"] = (c, v, a) => Py.Repr(v),
            ["reject"] = (c, v, a) => Select(c, v, a, false, false),
            ["rejectattr"] = (c, v, a) => Select(c, v, a, false, true),
            ["replace"] = (c, v, a) => Replace(Py.Str(v), a.Str(0, "old", ""), a.Str(1, "new", ""), a.Long(2, "count", -1)),
            ["reverse"] = (c, v, a) => v is string s ? new string(s.Reverse().ToArray()) : (object)Py.Iterate(v).Reverse().ToList(),
            ["round"] = (c, v, a) => Round(v, a),
            ["safe"] = (c, v, a) => Py.Str(v),
            ["select"] = (c, v, a) => Select(c, v, a, true, false),
            ["selectattr"] = (c, v, a) => Select(c, v, a, true, true),
            ["slice"] = (c, v, a) => SliceFilter(v, a),
            ["sort"] = (c, v, a) => Sort(v, a),
            ["string"] = (c, v, a) => Py.Str(v),
            ["striptags"] = (c, v, a) => StripTags(Py.Str(v)),
            ["sum"] = (c, v, a) => Sum(v, a),
            ["title"] = (c, v, a) => Title(Py.Str(v)),
            ["tojson"] = (c, v, a) => Py.ToJson(v, a.Has(0, "indent") && a.Get(0, "indent") != null ? (int?)Py.ToLong(a.Get(0, "indent")) : null),
            ["trim"] = (c, v, a) => a.Get(0, "chars") is string ch ? Py.Str(v).Trim(ch.ToCharArray()) : Py.Str(v).Trim(),
            ["truncate"] = (c, v, a) => Truncate(Py.Str(v), a),
            ["unique"] = (c, v, a) => Unique(v, a),
            ["upper"] = (c, v, a) => Py.Str(v).ToUpperInvariant(),
            ["urlencode"] = (c, v, a) => UrlEncode(v),
            ["wordcount"] = (c, v, a) => (long)Regex.Matches(Py.Str(v), @"\w+").Count,
            ["wordwrap"] = (c, v, a) => WordWrap(Py.Str(v), a),
        };

        public static bool Exists(string name) => Table.ContainsKey(name);

        public static object Apply(Context ctx, string name, object v, Args a)
        {
            if (!Table.TryGetValue(name, out var fn)) throw new TemplateError($"No filter named '{name}'");
            return fn(ctx, v, a);
        }

        // ---------------------------------------------------------------- helpers

        private static Dictionary<string, object> AsDict(object v, string filter)
        {
            if (v is Dictionary<string, object> d) return d;
            throw new TemplateError($"Can only use '{filter}' on a mapping, got '{Py.TypeName(v)}'");
        }

        /// <summary>attr() only looks up attributes (never dict keys), exactly like Jinja's getattr-based filter.</summary>
        private static object Attr(object v, string name)
        {
            if (v is LoopContext || v is Namespace || v is GroupTuple) return Py.GetAttr(v, name);
            if (Methods.Has(v, name)) return new BoundMethod(v, name);
            return new Undefined(name, $"'{Py.TypeName(v)} object' has no attribute '{name}'");
        }

        private static object Length(object v)
        {
            switch (v)
            {
                case Undefined _: return 0L;
                case string s: return (long)s.Length;
                case ICollection c: return (long)c.Count;
            }
            throw new TemplateError($"object of type '{Py.TypeName(v)}' has no len()");
        }

        private static object Default(object v, Args a)
        {
            bool boolean = a.Bool(1, "boolean");
            if (v is Undefined || (boolean && !Py.Truthy(v))) return a.Get(0, "default_value", "");
            return v;
        }

        public static string Escape(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                                                 .Replace("\"", "&#34;").Replace("'", "&#39;");

        public static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();

        public static string Title(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool start = true;
            foreach (char c in s)
            {
                if (char.IsWhiteSpace(c) || c == '-' || c == '(' || c == '{' || c == '[' || c == '<')
                {
                    sb.Append(c);
                    start = true;
                }
                else
                {
                    sb.Append(start ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
                    start = false;
                }
            }
            return sb.ToString();
        }

        public static string Center(string s, int width, char fill)
        {
            int marg = width - s.Length;
            if (marg <= 0) return s;
            int left = marg / 2 + (marg & width & 1);
            return new string(fill, left) + s + new string(fill, marg - left);
        }

        public static string Replace(string s, string oldS, string newS, long count)
        {
            if (count < 0) return oldS.Length == 0 ? InsertBetween(s, newS) : s.Replace(oldS, newS);
            var sb = new StringBuilder();
            int pos = 0;
            for (long n = 0; n < count; n++)
            {
                int idx = s.IndexOf(oldS, pos, StringComparison.Ordinal);
                if (idx < 0 || oldS.Length == 0) break;
                sb.Append(s, pos, idx - pos).Append(newS);
                pos = idx + oldS.Length;
            }
            sb.Append(s.Substring(pos));
            return sb.ToString();
        }

        private static string InsertBetween(string s, string ins)
        {
            var sb = new StringBuilder(ins);
            foreach (char c in s) sb.Append(c).Append(ins);
            return sb.ToString();
        }

        public static List<string> SplitLines(string s)
        {
            var lines = s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();
            if (lines.Count > 0 && lines[lines.Count - 1].Length == 0) lines.RemoveAt(lines.Count - 1);
            return lines;
        }

        private static object Indent(string s, Args a)
        {
            object w = a.Get(0, "width", 4L);
            string ind = w is string ws ? ws : new string(' ', (int)Py.ToLong(w));
            bool first = a.Bool(1, "first");
            bool blank = a.Bool(2, "blank");
            s += "\n";
            var lines = SplitLines(s);
            string rv;
            if (blank) rv = string.Join("\n" + ind, lines);
            else
            {
                rv = lines.Count > 0 ? lines[0] : "";
                if (lines.Count > 1)
                    rv += "\n" + string.Join("\n", lines.Skip(1).Select(l => l.Length > 0 ? ind + l : l));
            }
            return first ? ind + rv : rv;
        }

        private static object Truncate(string s, Args a)
        {
            int length = (int)a.Long(0, "length", 255);
            bool killwords = a.Bool(1, "killwords");
            string end = a.Str(2, "end", "...");
            int leeway = (int)a.Long(3, "leeway", 5);
            if (s.Length <= length + leeway) return s;
            int cut = Math.Max(0, length - end.Length);
            if (killwords) return s.Substring(0, cut) + end;
            string res = s.Substring(0, cut);
            int sp = res.LastIndexOf(' ');
            if (sp >= 0) res = res.Substring(0, sp);
            return res + end;
        }

        private static object WordWrap(string s, Args a)
        {
            int width = (int)a.Long(0, "width", 79);
            bool breakLong = a.Bool(1, "break_long_words", true);
            string wrap = a.Str(2, "wrapstring", "\n");
            var paras = s.Replace("\r\n", "\n").Split('\n');
            var outLines = new List<string>();
            foreach (var para in paras)
            {
                var words = para.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var line = new StringBuilder();
                foreach (var word0 in words)
                {
                    string word = word0;
                    while (breakLong && word.Length > width)
                    {
                        if (line.Length > 0) { outLines.Add(line.ToString()); line.Clear(); }
                        outLines.Add(word.Substring(0, width));
                        word = word.Substring(width);
                    }
                    if (line.Length > 0 && line.Length + 1 + word.Length > width) { outLines.Add(line.ToString()); line.Clear(); }
                    if (line.Length > 0) line.Append(' ');
                    line.Append(word);
                }
                outLines.Add(line.ToString());
            }
            return string.Join(wrap, outLines);
        }

        private static object StripTags(string s)
        {
            s = Regex.Replace(s, @"<!--.*?-->|<[^>]*>", "", RegexOptions.Singleline);
            s = System.Net.WebUtility.HtmlDecode(s);
            return string.Join(" ", s.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        }

        private static object FileSize(double bytes, bool binary)
        {
            double b = binary ? 1024 : 1000;
            string[] pre = binary ? new[] { "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" }
                                  : new[] { "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            if (bytes == 1) return "1 Byte";
            if (bytes < b) return $"{(long)bytes} Bytes";
            for (int i = 0; i < pre.Length; i++)
            {
                double unit = Math.Pow(b, i + 2);
                if (bytes < unit || i == pre.Length - 1)
                    return (b * bytes / unit).ToString("0.0", Inv) + " " + pre[i];
            }
            return "";
        }

        private static object ToInt(object v, Args a)
        {
            object def = a.Get(0, "default", 0L);
            long bas = a.Long(1, "base", 10);
            switch (v)
            {
                case long _: return v;
                case int i: return (long)i;
                case bool b: return b ? 1L : 0L;
                case double d: return double.IsNaN(d) || double.IsInfinity(d) ? def : (long)Math.Truncate(d);
                case string s:
                    s = s.Trim().Replace("_", "");
                    try
                    {
                        if (bas == 10)
                        {
                            if (long.TryParse(s, NumberStyles.Integer, Inv, out long r)) return r;
                            if (double.TryParse(s, NumberStyles.Float, Inv, out double dd)) return (long)Math.Truncate(dd);
                            return def;
                        }
                        if (bas == 2 || bas == 8 || bas == 16)
                        {
                            string t = s.ToLowerInvariant();
                            if (t.StartsWith("0x") || t.StartsWith("0o") || t.StartsWith("0b")) t = t.Substring(2);
                            return Convert.ToInt64(t, (int)bas);
                        }
                    }
                    catch { }
                    return def;
            }
            return def;
        }

        private static object ToFloat(object v, Args a)
        {
            object def = a.Get(0, "default", 0.0);
            if (Py.IsNumber(v)) return Py.ToDouble(v);
            if (v is string s && double.TryParse(s.Trim(), NumberStyles.Float, Inv, out double r)) return r;
            return def;
        }

        private static object Round(object v, Args a)
        {
            double d = Py.ToDouble(v);
            int prec = (int)a.Long(0, "precision", 0);
            string method = a.Str(1, "method", "common");
            double f = Math.Pow(10, prec);
            switch (method)
            {
                case "common":
                    if (Py.IsInt(v) && !(v is bool)) return Py.ToLong(v); // Python round(int, n) keeps the int
                    return prec >= 0 && prec <= 15 ? Math.Round(d, prec, MidpointRounding.ToEven) : Math.Round(d * f) / f;
                case "ceil": return Math.Ceiling(d * f) / f;
                case "floor": return Math.Floor(d * f) / f;
            }
            throw new TemplateError("method must be 'common', 'ceil' or 'floor'");
        }

        private static object Batch(object v, Args a)
        {
            int n = (int)a.Long(0, "linecount", 1);
            if (n <= 0) throw new TemplateError("batch size must be positive");
            bool fill = a.Has(1, "fill_with");
            object fillWith = a.Get(1, "fill_with");
            var result = new List<object>();
            List<object> tmp = new List<object>();
            foreach (var item in Py.Iterate(v))
            {
                if (tmp.Count == n) { result.Add(tmp); tmp = new List<object>(); }
                tmp.Add(item);
            }
            if (tmp.Count > 0)
            {
                if (fill && tmp.Count < n) while (tmp.Count < n) tmp.Add(fillWith);
                result.Add(tmp);
            }
            return result;
        }

        private static object SliceFilter(object v, Args a)
        {
            var seq = Py.Iterate(v).ToList();
            int slices = (int)a.Long(0, "slices", 1);
            if (slices <= 0) throw new TemplateError("slices must be positive");
            bool fill = a.Has(1, "fill_with") && a.Get(1, "fill_with") != null;
            object fillWith = a.Get(1, "fill_with");
            int per = seq.Count / slices, extra = seq.Count % slices, offset = 0;
            var result = new List<object>();
            for (int i = 0; i < slices; i++)
            {
                int start = offset + i * per;
                if (i < extra) offset++;
                int end = offset + (i + 1) * per;
                var tmp = seq.GetRange(start, end - start);
                if (fill && i >= extra) tmp.Add(fillWith);
                result.Add(tmp);
            }
            return result;
        }

        private static object SortKey(object x, bool caseSensitive) => !caseSensitive && x is string s ? s.ToLowerInvariant() : x;

        private sealed class PyComparer : IComparer<object>
        {
            public static readonly PyComparer Instance = new PyComparer();
            public int Compare(object a, object b) => Py.Compare(a, b);
        }

        private static object Sort(object v, Args a)
        {
            bool reverse = a.Bool(0, "reverse");
            bool cs = a.Bool(1, "case_sensitive");
            object attr = a.Get(2, "attribute");
            var items = Py.Iterate(v).ToList();
            Func<object, object> key = x => SortKey(Py.GetAttrPath(x, attr), cs);
            return (reverse ? items.OrderByDescending(key, PyComparer.Instance) : items.OrderBy(key, PyComparer.Instance)).ToList();
        }

        private static object DictSort(object v, Args a)
        {
            var d = AsDict(v, "dictsort");
            bool cs = a.Bool(0, "case_sensitive");
            string by = a.Str(1, "by", "key");
            bool reverse = a.Bool(2, "reverse");
            if (by != "key" && by != "value") throw new TemplateError("You can only sort by either 'key' or 'value'");
            var items = d.Select(kv => new PyTuple(new[] { kv.Key, kv.Value })).ToList();
            Func<PyTuple, object> key = t => SortKey(by == "key" ? t[0] : t[1], cs);
            return (reverse ? items.OrderByDescending(key, PyComparer.Instance) : items.OrderBy(key, PyComparer.Instance)).Cast<object>().ToList();
        }

        private static object GroupBy(object v, Args a)
        {
            object attr = a.Get(0, "attribute");
            object def = a.Get(1, "default");
            bool cs = a.Bool(2, "case_sensitive");
            Func<object, object> raw = x =>
            {
                var r = Py.GetAttrPath(x, attr);
                return r is Undefined && a.Has(1, "default") ? def : r;
            };
            var items = Py.Iterate(v).OrderBy(x => SortKey(raw(x), cs), PyComparer.Instance).ToList();
            var result = new List<object>();
            object curKey = null;
            List<object> cur = null;
            object curGrouper = null;
            foreach (var it in items)
            {
                var rk = raw(it);
                var k = SortKey(rk, cs);
                if (cur == null || !Py.Eq(k, curKey))
                {
                    if (cur != null) result.Add(new GroupTuple(curGrouper, cur));
                    cur = new List<object>();
                    curKey = k;
                    curGrouper = rk;
                }
                cur.Add(it);
            }
            if (cur != null) result.Add(new GroupTuple(curGrouper, cur));
            return result;
        }

        private static object Unique(object v, Args a)
        {
            bool cs = a.Bool(0, "case_sensitive");
            object attr = a.Get(1, "attribute");
            var seen = new List<object>();
            var result = new List<object>();
            foreach (var it in Py.Iterate(v))
            {
                var k = SortKey(Py.GetAttrPath(it, attr), cs);
                if (seen.Any(s => Py.Eq(s, k))) continue;
                seen.Add(k);
                result.Add(it);
            }
            return result;
        }

        private static object MinMax(object v, Args a, bool max)
        {
            bool cs = a.Bool(0, "case_sensitive");
            object attr = a.Get(1, "attribute");
            var items = Py.Iterate(v).ToList();
            if (items.Count == 0) return new Undefined(max ? "max" : "min", "No aggregated item, sequence was empty.");
            object best = items[0];
            object bestKey = SortKey(Py.GetAttrPath(best, attr), cs);
            foreach (var it in items.Skip(1))
            {
                var k = SortKey(Py.GetAttrPath(it, attr), cs);
                int c = Py.Compare(k, bestKey);
                if (max ? c > 0 : c < 0) { best = it; bestKey = k; }
            }
            return best;
        }

        private static object Sum(object v, Args a)
        {
            object attr = a.Get(0, "attribute");
            object total = a.Get(1, "start", 0L);
            foreach (var it in Py.Iterate(v)) total = Py.Binary("+", total, Py.GetAttrPath(it, attr));
            return total;
        }

        private static object Map(Context ctx, object v, Args a)
        {
            var items = Py.Iterate(v).ToList();
            if (a.Kw.ContainsKey("attribute"))
            {
                object attr = a.Kw["attribute"];
                bool hasDef = a.Kw.TryGetValue("default", out var def);
                return items.Select(x =>
                {
                    var r = Py.GetAttrPath(x, attr);
                    return r is Undefined && hasDef ? def : r;
                }).ToList();
            }
            if (a.Pos.Count == 0) throw new TemplateError("map requires a filter name or attribute=");
            string name = Py.Str(a.Pos[0]);
            if (!Exists(name)) throw new TemplateError($"No filter named '{name}'");
            var rest = new Args(a.Pos.Skip(1).ToList(), a.Kw);
            return items.Select(x => Apply(ctx, name, x, rest)).ToList();
        }

        private static object Select(Context ctx, object v, Args a, bool keep, bool byAttr)
        {
            int off = 0;
            object attr = null;
            if (byAttr)
            {
                if (a.Pos.Count == 0) throw new TemplateError("Missing parameter for attribute name");
                attr = a.Pos[0];
                off = 1;
            }
            Func<object, bool> pred;
            if (a.Pos.Count > off)
            {
                string test = Py.Str(a.Pos[off]);
                if (!Tests.Exists(test)) throw new TemplateError($"No test named '{test}'");
                var rest = new Args(a.Pos.Skip(off + 1).ToList(), a.Kw);
                pred = x => Tests.Apply(test, x, rest);
            }
            else pred = Py.Truthy;
            return Py.Iterate(v).Where(x => pred(byAttr ? Py.GetAttrPath(x, attr) : x) == keep).ToList();
        }

        private static object UrlEncode(object v)
        {
            string Enc(object o) => Uri.EscapeDataString(Py.Str(o)).Replace("%2F", "/");
            string Qs(object o) => Uri.EscapeDataString(Py.Str(o)).Replace("%20", "+");
            switch (v)
            {
                case string s: return Enc(s);
                case Dictionary<string, object> d: return string.Join("&", d.Select(kv => Qs(kv.Key) + "=" + Qs(kv.Value)));
                case IList l:
                    return string.Join("&", l.Cast<object>().Select(x =>
                        x is IList p && p.Count == 2 ? Qs(p[0]) + "=" + Qs(p[1]) : Qs(x)));
            }
            return Enc(v);
        }
    }

    // ==================================================================== tests

    internal static class Tests
    {
        private delegate bool TestFn(object v, Args a);

        private static readonly Dictionary<string, TestFn> Table = new Dictionary<string, TestFn>
        {
            ["defined"] = (v, a) => !(v is Undefined),
            ["undefined"] = (v, a) => v is Undefined,
            ["none"] = (v, a) => v == null,
            ["boolean"] = (v, a) => v is bool,
            ["true"] = (v, a) => v is bool b && b,
            ["false"] = (v, a) => v is bool b && !b,
            ["number"] = (v, a) => Py.IsNumber(v),
            ["integer"] = (v, a) => v is long || v is int,
            ["float"] = (v, a) => v is double,
            ["string"] = (v, a) => v is string,
            ["mapping"] = (v, a) => Py.IsMapping(v),
            ["iterable"] = (v, a) => v is string || v is IList || v is Dictionary<string, object> || v is Undefined,
            ["sequence"] = (v, a) => v is string || v is IList || v is Dictionary<string, object>,
            ["callable"] = (v, a) => v is IJinjaCallable || v is Undefined, // Python's Undefined defines __call__
            ["even"] = (v, a) => Py.ToLong(v) % 2 == 0,
            ["odd"] = (v, a) => Py.ToLong(v) % 2 != 0,
            ["divisibleby"] = (v, a) => Py.ToLong(v) % Py.ToLong(a.Get(0, "num", 1L)) == 0,
            ["eq"] = (v, a) => Py.Eq(v, a.Get(0, "other")),
            ["equalto"] = (v, a) => Py.Eq(v, a.Get(0, "other")),
            ["=="] = (v, a) => Py.Eq(v, a.Get(0, "other")),
            ["ne"] = (v, a) => !Py.Eq(v, a.Get(0, "other")),
            ["!="] = (v, a) => !Py.Eq(v, a.Get(0, "other")),
            ["lt"] = (v, a) => Py.Compare(v, a.Get(0, "other")) < 0,
            ["lessthan"] = (v, a) => Py.Compare(v, a.Get(0, "other")) < 0,
            ["<"] = (v, a) => Py.Compare(v, a.Get(0, "other")) < 0,
            ["le"] = (v, a) => Py.Compare(v, a.Get(0, "other")) <= 0,
            ["<="] = (v, a) => Py.Compare(v, a.Get(0, "other")) <= 0,
            ["gt"] = (v, a) => Py.Compare(v, a.Get(0, "other")) > 0,
            ["greaterthan"] = (v, a) => Py.Compare(v, a.Get(0, "other")) > 0,
            [">"] = (v, a) => Py.Compare(v, a.Get(0, "other")) > 0,
            ["ge"] = (v, a) => Py.Compare(v, a.Get(0, "other")) >= 0,
            [">="] = (v, a) => Py.Compare(v, a.Get(0, "other")) >= 0,
            ["in"] = (v, a) => Py.Contains(a.Get(0, "seq"), v),
            ["lower"] = (v, a) => v is string s && s == s.ToLowerInvariant(),
            ["upper"] = (v, a) => v is string s && s == s.ToUpperInvariant(),
            ["sameas"] = (v, a) =>
            {
                var o = a.Get(0, "other");
                if (v == null || o == null || v is bool || o is bool) return Py.Eq(v, o) && (v?.GetType() == o?.GetType());
                return ReferenceEquals(v, o);
            },
            ["escaped"] = (v, a) => false,
            ["filter"] = (v, a) => v is string s && Filters.Exists(s),
            ["test"] = (v, a) => v is string s && Exists(s),
        };

        public static bool Exists(string name) => Table.ContainsKey(name);

        public static bool Apply(string name, object v, Args a)
        {
            if (!Table.TryGetValue(name, out var fn)) throw new TemplateError($"No test named '{name}'");
            return fn(v, a);
        }
    }

    // ==================================================================== printf-style and str.format formatting

    internal static class Formatting
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Python's "fmt % args".</summary>
        public static string Percent(string fmt, object args)
        {
            List<object> values;
            Dictionary<string, object> map = args as Dictionary<string, object>;
            if (args is PyTuple t) values = t.ToList();
            else values = new List<object> { args };

            var sb = new StringBuilder();
            int vi = 0, i = 0, n = fmt.Length;
            bool usedMap = false;
            while (i < n)
            {
                char c = fmt[i++];
                if (c != '%') { sb.Append(c); continue; }
                if (i >= n) throw new TemplateError("incomplete format");
                if (fmt[i] == '%') { sb.Append('%'); i++; continue; }

                object value = null;
                bool haveValue = false;
                if (fmt[i] == '(')
                {
                    int close = fmt.IndexOf(')', i);
                    if (close < 0) throw new TemplateError("incomplete format key");
                    string key = fmt.Substring(i + 1, close - i - 1);
                    if (map == null) throw new TemplateError("format requires a mapping");
                    value = map.TryGetValue(key, out var mv) ? mv : throw new TemplateError($"KeyError: {Py.Quote(key)}");
                    haveValue = true;
                    usedMap = true;
                    i = close + 1;
                }

                string flags = "";
                while (i < n && "-+ #0".IndexOf(fmt[i]) >= 0) flags += fmt[i++];
                int width = -1, prec = -1;
                if (i < n && fmt[i] == '*') { width = (int)Py.ToLong(Next(values, ref vi)); i++; }
                else { int st = i; while (i < n && char.IsDigit(fmt[i])) i++; if (i > st) width = int.Parse(fmt.Substring(st, i - st), Inv); }
                if (i < n && fmt[i] == '.')
                {
                    i++;
                    if (i < n && fmt[i] == '*') { prec = (int)Py.ToLong(Next(values, ref vi)); i++; }
                    else { int st = i; while (i < n && char.IsDigit(fmt[i])) i++; prec = i > st ? int.Parse(fmt.Substring(st, i - st), Inv) : 0; }
                }
                while (i < n && "hlL".IndexOf(fmt[i]) >= 0) i++;
                if (i >= n) throw new TemplateError("incomplete format");
                char conv = fmt[i++];
                if (!haveValue) value = map != null && !(args is PyTuple) ? args : Next(values, ref vi);

                sb.Append(FormatOne(value, flags, width, prec, conv));
            }
            if (!usedMap && map == null && vi < values.Count)
                throw new TemplateError("not all arguments converted during string formatting");
            return sb.ToString();
        }

        private static object Next(List<object> values, ref int vi)
        {
            if (vi >= values.Count) throw new TemplateError("not enough arguments for format string");
            return values[vi++];
        }

        private static string FormatOne(object v, string flags, int width, int prec, char conv)
        {
            bool left = flags.Contains('-'), zero = flags.Contains('0') && !left;
            string sign = flags.Contains('+') ? "+" : flags.Contains(' ') ? " " : "";
            string body;
            bool numeric = true;
            bool neg = false;
            switch (conv)
            {
                case 's':
                case 'r':
                case 'a':
                    body = conv == 's' ? Py.Str(v) : Py.Repr(v);
                    if (prec >= 0 && body.Length > prec) body = body.Substring(0, prec);
                    numeric = false;
                    break;
                case 'd':
                case 'i':
                case 'u':
                {
                    long l = Py.ToLong(Py.IsNumber(v) ? v : (object)Py.ToDouble(v));
                    neg = l < 0;
                    body = Math.Abs(l).ToString(Inv);
                    if (prec > body.Length) body = body.PadLeft(prec, '0');
                    break;
                }
                case 'f':
                case 'F':
                {
                    double d = Py.ToDouble(v);
                    neg = d < 0 || (d == 0 && 1 / d < 0);
                    body = Math.Abs(d).ToString("F" + (prec < 0 ? 6 : prec), Inv);
                    break;
                }
                case 'e':
                case 'E':
                {
                    double d = Py.ToDouble(v);
                    neg = d < 0;
                    body = Exp(Math.Abs(d), prec < 0 ? 6 : prec);
                    if (conv == 'E') body = body.ToUpperInvariant();
                    break;
                }
                case 'g':
                case 'G':
                {
                    double d = Py.ToDouble(v);
                    neg = d < 0;
                    body = General(Math.Abs(d), prec < 0 ? 6 : prec);
                    if (conv == 'G') body = body.ToUpperInvariant();
                    break;
                }
                case 'x':
                case 'X':
                case 'o':
                {
                    long l = Py.ToLong(v);
                    neg = l < 0;
                    body = Convert.ToString(Math.Abs(l), conv == 'o' ? 8 : 16);
                    if (conv == 'X') body = body.ToUpperInvariant();
                    if (flags.Contains('#')) body = (conv == 'o' ? "0o" : conv == 'x' ? "0x" : "0X") + body;
                    break;
                }
                case 'c':
                    body = v is string cs ? cs : ((char)Py.ToLong(v)).ToString();
                    numeric = false;
                    break;
                default:
                    throw new TemplateError($"unsupported format character '{conv}'");
            }
            if (numeric)
            {
                string sg = neg ? "-" : sign;
                if (zero && width > 0 && sg.Length + body.Length < width) body = body.PadLeft(width - sg.Length, '0');
                body = sg + body;
            }
            if (width > 0 && body.Length < width) body = left ? body.PadRight(width) : body.PadLeft(width);
            return body;
        }

        private static string Exp(double d, int prec)
        {
            string s = d.ToString((prec > 0 ? "0." + new string('0', prec) : "0") + "e+00", Inv);
            return s;
        }

        private static string General(double d, int prec)
        {
            if (prec == 0) prec = 1;
            if (d == 0) return "0";
            int exp = (int)Math.Floor(Math.Log10(d));
            if (exp < -4 || exp >= prec)
            {
                string m = Exp(d, prec - 1);
                int e = m.IndexOf('e');
                string mant = m.Substring(0, e);
                if (mant.Contains('.')) mant = mant.TrimEnd('0').TrimEnd('.');
                return mant + m.Substring(e);
            }
            string f = d.ToString("F" + Math.Max(0, prec - 1 - exp), Inv);
            if (f.Contains('.')) f = f.TrimEnd('0').TrimEnd('.');
            return f;
        }

        /// <summary>Python's "fmt".format(*args, **kwargs).</summary>
        public static string Braces(string fmt, List<object> args, Dictionary<string, object> kw)
        {
            var sb = new StringBuilder();
            int auto = 0, i = 0, n = fmt.Length;
            while (i < n)
            {
                char c = fmt[i];
                if (c == '{')
                {
                    if (i + 1 < n && fmt[i + 1] == '{') { sb.Append('{'); i += 2; continue; }
                    int close = fmt.IndexOf('}', i);
                    if (close < 0) throw new TemplateError("Single '{' encountered in format string");
                    string field = fmt.Substring(i + 1, close - i - 1);
                    i = close + 1;

                    string spec = "";
                    char conv = '\0';
                    int colon = field.IndexOf(':');
                    if (colon >= 0) { spec = field.Substring(colon + 1); field = field.Substring(0, colon); }
                    int bang = field.IndexOf('!');
                    if (bang >= 0) { conv = field.Length > bang + 1 ? field[bang + 1] : 's'; field = field.Substring(0, bang); }

                    string head = field, rest = "";
                    int dot = field.IndexOfAny(new[] { '.', '[' });
                    if (dot >= 0) { head = field.Substring(0, dot); rest = field.Substring(dot); }

                    object v;
                    if (head.Length == 0)
                    {
                        if (auto >= args.Count) throw new TemplateError("Replacement index out of range for positional args");
                        v = args[auto++];
                    }
                    else if (int.TryParse(head, NumberStyles.None, Inv, out int idx))
                    {
                        if (idx >= args.Count) throw new TemplateError($"Replacement index {idx} out of range for positional args");
                        v = args[idx];
                    }
                    else if (!kw.TryGetValue(head, out v)) throw new TemplateError($"KeyError: {Py.Quote(head)}");

                    foreach (Match m in Regex.Matches(rest, @"\.(\w+)|\[([^\]]+)\]"))
                    {
                        if (m.Groups[1].Success) v = Py.GetAttr(v, m.Groups[1].Value);
                        else
                        {
                            string k = m.Groups[2].Value;
                            v = Py.GetItem(v, long.TryParse(k, out long lk) ? (object)lk : k);
                        }
                    }

                    if (conv == 'r' || conv == 'a') v = Py.Repr(v);
                    else if (conv == 's') v = Py.Str(v);
                    sb.Append(Spec(v, spec));
                    continue;
                }
                if (c == '}')
                {
                    if (i + 1 < n && fmt[i + 1] == '}') { sb.Append('}'); i += 2; continue; }
                    throw new TemplateError("Single '}' encountered in format string");
                }
                sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        private static readonly Regex SpecRx = new Regex(@"^(?:(.)?([<>=^]))?([+\- ])?(#)?(0)?(\d+)?([,_])?(?:\.(\d+))?([bcdeEfFgGnosxX%])?$");

        /// <summary>Python format-spec mini-language.</summary>
        public static string Spec(object v, string spec)
        {
            if (string.IsNullOrEmpty(spec)) return Py.Str(v);
            var m = SpecRx.Match(spec);
            if (!m.Success) throw new TemplateError($"Invalid format specifier '{spec}'");
            char fill = m.Groups[1].Success ? m.Groups[1].Value[0] : ' ';
            char align = m.Groups[2].Success ? m.Groups[2].Value[0] : '\0';
            string sign = m.Groups[3].Success ? m.Groups[3].Value : "-";
            bool alt = m.Groups[4].Success;
            if (m.Groups[5].Success && !m.Groups[2].Success) { fill = '0'; align = '='; }
            int width = m.Groups[6].Success ? int.Parse(m.Groups[6].Value, Inv) : 0;
            string group = m.Groups[7].Success ? m.Groups[7].Value : "";
            int prec = m.Groups[8].Success ? int.Parse(m.Groups[8].Value, Inv) : -1;
            char type = m.Groups[9].Success ? m.Groups[9].Value[0] : '\0';

            string body;
            bool neg = false, numeric = false;
            if (type == 's' || (type == '\0' && !Py.IsNumber(v)))
            {
                body = Py.Str(v);
                if (prec >= 0 && body.Length > prec) body = body.Substring(0, prec);
            }
            else
            {
                numeric = true;
                if (type == '\0') type = Py.IsInt(v) && !(v is bool) ? 'd' : (prec >= 0 ? 'g' : 'r');
                switch (type)
                {
                    case 'd':
                    case 'n':
                    {
                        long l = Py.ToLong(v);
                        neg = l < 0;
                        body = Group(Math.Abs(l).ToString(Inv), group);
                        break;
                    }
                    case 'b':
                    case 'o':
                    case 'x':
                    case 'X':
                    {
                        long l = Py.ToLong(v);
                        neg = l < 0;
                        body = Convert.ToString(Math.Abs(l), type == 'b' ? 2 : type == 'o' ? 8 : 16);
                        if (type == 'X') body = body.ToUpperInvariant();
                        if (alt) body = "0" + (type == 'X' ? 'X' : type) + body;
                        break;
                    }
                    case 'c':
                        body = ((char)Py.ToLong(v)).ToString();
                        numeric = false;
                        break;
                    case 'r':
                    {
                        double d = Py.ToDouble(v);
                        neg = d < 0;
                        body = Py.FloatStr(Math.Abs(d));
                        break;
                    }
                    case '%':
                    {
                        double d = Py.ToDouble(v) * 100;
                        neg = d < 0;
                        body = GroupFloat(Math.Abs(d).ToString("F" + (prec < 0 ? 6 : prec), Inv), group) + "%";
                        break;
                    }
                    case 'e':
                    case 'E':
                    {
                        double d = Py.ToDouble(v);
                        neg = d < 0;
                        body = Exp(Math.Abs(d), prec < 0 ? 6 : prec);
                        if (type == 'E') body = body.ToUpperInvariant();
                        break;
                    }
                    case 'g':
                    case 'G':
                    {
                        double d = Py.ToDouble(v);
                        neg = d < 0;
                        body = General(Math.Abs(d), prec < 0 ? 6 : prec);
                        if (type == 'G') body = body.ToUpperInvariant();
                        break;
                    }
                    default: // f / F
                    {
                        double d = Py.ToDouble(v);
                        neg = d < 0;
                        body = GroupFloat(Math.Abs(d).ToString("F" + (prec < 0 ? 6 : prec), Inv), group);
                        break;
                    }
                }
            }

            string sg = numeric ? (neg ? "-" : sign == "-" ? "" : sign) : "";
            if (align == '\0') align = numeric ? '>' : '<';
            int pad = width - sg.Length - body.Length;
            if (pad <= 0) return sg + body;
            switch (align)
            {
                case '<': return sg + body + new string(fill, pad);
                case '^': return new string(fill, pad / 2) + sg + body + new string(fill, pad - pad / 2);
                case '=': return sg + new string(fill, pad) + body;
                default: return new string(fill, pad) + sg + body;
            }
        }

        private static string Group(string digits, string sep)
        {
            if (sep.Length == 0 || digits.Length <= 3) return digits;
            var sb = new StringBuilder();
            int first = digits.Length % 3;
            if (first > 0) sb.Append(digits, 0, first);
            for (int i = first; i < digits.Length; i += 3)
            {
                if (sb.Length > 0) sb.Append(sep);
                sb.Append(digits, i, 3);
            }
            return sb.ToString();
        }

        private static string GroupFloat(string num, string sep)
        {
            int dot = num.IndexOf('.');
            return dot < 0 ? Group(num, sep) : Group(num.Substring(0, dot), sep) + num.Substring(dot);
        }
    }
}
