using System;
using System.Drawing;
using GHGradient = Grasshopper.GUI.Gradient.GH_Gradient;

namespace Redback.Components.Grasshopper.Gradient
{
    public class RedbackGradient
    {
        public GHGradient Gradient { get; }
        public double Lower { get; }
        public double Upper { get; }

        public RedbackGradient(GHGradient gradient, double lower, double upper)
        {
            Gradient = gradient ?? throw new ArgumentNullException(nameof(gradient));
            Lower = lower;
            Upper = upper;
        }

        // Evaluates the gradient at an external value in [Lower, Upper], mapping to [0, 1].
        public Color ColourAt(double value)
        {
            double range = Upper - Lower;
            double t = range == 0.0 ? 0.5 : (value - Lower) / range;
            t = Math.Max(0.0, Math.Min(1.0, t));
            return Gradient.ColourAt(t);
        }

        public RedbackGradient Duplicate()
            => new RedbackGradient(new GHGradient(Gradient), Lower, Upper);

        public static RedbackGradient CreateDefault()
        {
            var g = new GHGradient(new[] { 0.0, 1.0 }, new[] { Color.Black, Color.White });
            return new RedbackGradient(g, 0.0, 1.0);
        }
    }
}
