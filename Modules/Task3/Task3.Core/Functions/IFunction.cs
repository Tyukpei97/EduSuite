namespace Task3.Core.Functions
{
    public interface IFunction
    {
        string DisplayName { get; }

        double Evaluate(double x);
    }
}
