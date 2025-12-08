namespace Task3.Core.Charting
{
    public sealed class ChartSettings
    {
        public float Width { get; set; } = 800f;

        public float Height { get; set; } = 600f;

        public float MarginLeft { get; set; } = 40f;

        public float MarginRight { get; set; } = 20f;

        public float MarginTop { get; set; } = 20f;

        public float MarginBottom { get; set; } = 40f;

        public float XMin { get; set; } = -10f;

        public float XMax { get; set; } = 10f;

        public float YMin { get; set; } = -10f;

        public float YMax { get; set; } = 10f;

        public float GridStep { get; set; } = 1f;

        public bool ShowGrid { get; set; } = true;

        public bool ShowAxes { get; set; } = true;
    }
}
