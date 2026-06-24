using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataMisc
{
    public class EncodeIntegerComponent : RedbackGHBase
    {
        private const string DefaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-:+/";
        private const int DefaultLength = 5;

        public EncodeIntegerComponent() : base("Encode Integer", "Encode Integer",
            "Encodes an integer to a fixed-length string using a custom alphabet", "2-Data - Misc") { }

        public override Guid ComponentGuid => new Guid("9ae23301-ce66-4a26-aa2b-d0620a0a8b11");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddIntegerParameter("Number", "N", "Integer to encode", GH_ParamAccess.item);
            p[0].Optional = true;
            p.AddTextParameter("Alphabet", "A", "Alphabet characters (each item joined to form the alphabet string)", GH_ParamAccess.list);
            p[1].Optional = true;
            p.AddIntegerParameter("Length", "L", "Output string length", GH_ParamAccess.item, DefaultLength);
            p[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Encoded", "E", "Encoded string", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int number = 0;
            var alphabetParts = new List<string>();
            int length = DefaultLength;

            DA.GetData(0, ref number);
            DA.GetDataList(1, alphabetParts);
            DA.GetData(2, ref length);

            string alphabet = alphabetParts.Count > 0
                ? string.Concat(alphabetParts)
                : DefaultAlphabet;

            DA.SetData(0, BaseEncode(number, alphabet, length));
        }

        private static string BaseEncode(int number, string alphabet, int length)
        {
            string sign = "";
            if (number < 0) { sign = "-"; number = -number; }

            string encoded;
            if (number < alphabet.Length)
            {
                string padded = new string('0', length) + alphabet[number];
                encoded = padded.Substring(padded.Length - length);
            }
            else
            {
                var sb = new StringBuilder();
                int n = number;
                while (n != 0)
                {
                    n = Math.DivRem(n, alphabet.Length, out int rem);
                    sb.Insert(0, alphabet[rem]);
                }
                string padded = new string('0', length) + sb.ToString();
                encoded = padded.Substring(padded.Length - length);
            }
            return sign + encoded;
        }
    }
}
