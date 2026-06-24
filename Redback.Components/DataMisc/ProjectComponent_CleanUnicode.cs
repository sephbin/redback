using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataMisc
{
    public class CleanUnicodeComponent : RedbackGHBase
    {
        public CleanUnicodeComponent() : base("CleanUnicode", "CleanUnicode",
            "Converts Unicode text to ASCII-safe equivalent", "2-Data - Misc") { }

        public override Guid ComponentGuid => new Guid("9667a866-274c-47d8-ac65-638dd571d06e");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Text", "T", "Text to clean", GH_ParamAccess.item);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Result", "R", "ASCII-safe text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = null;
            DA.GetData(0, ref text);
            if (text == null) { DA.SetData(0, null); return; }
            DA.SetData(0, SafeAscii(text));
        }

        private static readonly Dictionary<char, string> _lut = new Dictionary<char, string>
        {
            { '¢', "cents"  }, { '£', "pounds" }, { '¥', "yen"    },
            { '§', "section"}, { '©', "(C)"    }, { '®', "(R)"    },
            { '°', "deg"    }, { 'ß', "s"      }, { 'ç', "c"      },
            { 'é', "e"      }, { 'ñ', "n"      }, { 'ö', "o"      },
            { '–', "-"      }, { '—', "-"      }, { '•', "-"      },
            { '€', "euros"  }, { '™', "TM"     },
            { '‘', "'"      }, { '’', "'"      },
            { '“', "\""     }, { '”', "\""     },
        };

        private static string SafeAscii(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (_lut.TryGetValue(c, out string rep))
                    sb.Append(rep);
                else
                    sb.Append(c);
            }

            string normalized = sb.ToString().Normalize(NormalizationForm.FormKD);

            var result = new StringBuilder(normalized.Length);
            foreach (char c in normalized)
                if (c < 128) result.Append(c);

            return result.ToString();
        }
    }
}
