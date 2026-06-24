using System;
using System.Drawing;
using GH_IO.Serialization;
using GHGradient = Grasshopper.GUI.Gradient.GH_Gradient;

namespace Redback.Components.Grasshopper.Gradient
{
    // Helpers for manually serializing GH_Gradient (it has no public Write/Read).
    internal static class GradientIO
    {
        public static void Write(GH_IWriter writer, GHGradient gradient)
        {
            writer.SetBoolean("Linear", gradient.Linear);
            int count = gradient.GripCount;
            writer.SetInt32("Count", count);
            for (int i = 0; i < count; i++)
            {
                var grip  = gradient[i];
                var chunk = writer.CreateChunk("Grip", i);
                chunk.SetDouble("t",  grip.Parameter);
                chunk.SetInt32("CL", grip.ColourLeft.ToArgb());
                chunk.SetInt32("CR", grip.ColourRight.ToArgb());
            }
        }

        public static GHGradient Read(GH_IReader reader)
        {
            bool linear = false;
            reader.TryGetBoolean("Linear", ref linear);

            int count = 0;
            reader.TryGetInt32("Count", ref count);

            if (count == 0)
                return new GHGradient(new[] { 0.0, 1.0 }, new[] { Color.Black, Color.White });

            var positions = new double[count];
            var colors    = new Color[count];

            for (int i = 0; i < count; i++)
            {
                positions[i] = (double)i / Math.Max(1, count - 1);
                colors[i]    = Color.Black;

                var chunk = reader.FindChunk("Grip", i);
                if (chunk == null) continue;

                int clArgb = Color.Black.ToArgb();
                chunk.TryGetDouble("t",  ref positions[i]);
                chunk.TryGetInt32("CL", ref clArgb);
                colors[i] = Color.FromArgb(clArgb);
            }

            var gradient = new GHGradient(positions, colors);
            gradient.Linear = linear;
            return gradient;
        }
    }
}
