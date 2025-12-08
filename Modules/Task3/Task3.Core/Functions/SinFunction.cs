namespace Task3.Core.Functions
{
    public sealed class SinFunction : IFunction
    {
        public SinFunction(double amplitude, double frequency)
        {
            Amplitude = amplitude;
            Frequency = frequency;
        }

        public double Amplitude { get; }

        public double Frequency { get; }

        public string DisplayName
        {
            get
            {
                return $"y = {Amplitude} * sin({Frequency}x)";
            }
        }

        public double Evaluate(double x)
        {
            return Amplitude * Math.Sin(Frequency * x);
        }
    }
}
