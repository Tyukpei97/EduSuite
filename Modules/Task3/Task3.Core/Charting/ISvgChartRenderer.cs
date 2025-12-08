using System.Collections.Generic;
using System.Drawing;

namespace Task3.Core.Charting
{
    public interface ISvgChartRenderer
    {
        string RenderSvg(IEnumerable<PointF> points, ChartSettings settings);
    }
}
