namespace Task3.Core.Functions
{
    public sealed class LinearFunction : IFunction
    {
        public LinearFunction(double k, double b)
        {
            K = k;
            B = b;
        }

        public double K { get; }

        public double B { get; }

        public string DisplayName
        {
            get
            {
                return $"y = {K}x + {B}";
            }
        }

        public double Evaluate(double x)
        {
            return K * x + B;
        }
    }
}
