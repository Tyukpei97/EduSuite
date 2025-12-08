namespace Task3.Core.Functions
{
    public sealed class QuadraticFunction : IFunction
    {
        public QuadraticFunction(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }

        public double A { get; }

        public double B { get; }

        public double C { get; }

        public string DisplayName
        {
            get
            {
                return $"y = {A}x² + {B}x + {C}";
            }
        }

        public double Evaluate(double x)
        {
            return A * x * x + B * x + C;
        }
    }
}
