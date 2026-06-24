using GH_IO.Serialization;
using Grasshopper.Kernel.Types;

namespace Redback.Components.Grasshopper.Gradient
{
    public class RedbackGradientGoo : GH_Goo<RedbackGradient>
    {
        public override bool   IsValid         => Value != null && Value.Gradient != null;
        public override string TypeName        => "Redback Gradient";
        public override string TypeDescription => "A gradient object that can be evaluated at a value to return a colour.";

        public RedbackGradientGoo() { }
        public RedbackGradientGoo(RedbackGradient gradient) { Value = gradient; }

        public override IGH_Goo Duplicate() => new RedbackGradientGoo(Value?.Duplicate());

        public override string ToString()
            => IsValid ? $"Gradient [{Value.Lower:0.##}→{Value.Upper:0.##}]" : "Null Gradient";

        public override bool Write(GH_IWriter writer)
        {
            if (Value == null) return true;
            writer.SetDouble("Lower", Value.Lower);
            writer.SetDouble("Upper", Value.Upper);
            GradientIO.Write(writer.CreateChunk("Gradient"), Value.Gradient);
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            double lower = 0.0, upper = 1.0;
            reader.TryGetDouble("Lower", ref lower);
            reader.TryGetDouble("Upper", ref upper);
            var chunk    = reader.FindChunk("Gradient");
            var gradient = chunk != null ? GradientIO.Read(chunk) : RedbackGradient.CreateDefault().Gradient;
            Value = new RedbackGradient(gradient, lower, upper);
            return true;
        }
    }
}
