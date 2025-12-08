using System.Globalization;
using System.Text;
using System.Drawing;

namespace Task3.Core.Charting
{
    public sealed class SimpleSvgChartRenderer : ISvgChartRenderer
    {
        public string RenderSvg(IEnumerable<PointF> points, ChartSettings settings)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            float width = settings.Width;
            float height = settings.Height;

            float plotLeft = settings.MarginLeft;
            float plotRight = width - settings.MarginRight;
            float plotTop = settings.MarginTop;
            float plotBottom = height - settings.MarginBottom;

            float plotWidth = plotRight - plotLeft;
            float plotHeight = plotBottom - plotTop;

            float xRange = settings.XMax - settings.XMin;
            if (xRange <= 0f)
            {
                xRange = 1f;
            }

            float yRange = settings.YMax - settings.YMin;
            if (yRange <= 0f)
            {
                yRange = 1f;
            }

            float sx = plotWidth / xRange;
            float sy = plotHeight / yRange;

            // --- адаптивный шаг сетки ---
            float xStep = settings.GridStep <= 0f ? xRange : settings.GridStep;
            float yStep = settings.GridStep <= 0f ? yRange : settings.GridStep;

            const int maxLines = 25;

            if (xStep > 0f && xRange / xStep > maxLines)
            {
                float factor = (float)Math.Ceiling(xRange / (maxLines * xStep));
                if (factor < 1f)
                {
                    factor = 1f;
                }

                xStep *= factor;
            }

            if (yStep > 0f && yRange / yStep > maxLines)
            {
                float factor = (float)Math.Ceiling(yRange / (maxLines * yStep));
                if (factor < 1f)
                {
                    factor = 1f;
                }

                yStep *= factor;
            }

            var sb = new StringBuilder();

            sb.AppendLine(
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width.ToString(CultureInfo.InvariantCulture)}\" height=\"{height.ToString(CultureInfo.InvariantCulture)}\" viewBox=\"0 0 {width.ToString(CultureInfo.InvariantCulture)} {height.ToString(CultureInfo.InvariantCulture)}\">");

            sb.AppendLine("<rect x=\"0\" y=\"0\" width=\"100%\" height=\"100%\" fill=\"white\"/>");

            // ---- сетка ----
            if (settings.ShowGrid && xStep > 0f && yStep > 0f)
            {
                float xStart = (float)Math.Ceiling(settings.XMin / xStep) * xStep;
                for (float x = xStart; x <= settings.XMax; x += xStep)
                {
                    float sxPos = plotLeft + (x - settings.XMin) * sx;

                    sb.AppendLine(
                        $"<line x1=\"{sxPos.ToString(CultureInfo.InvariantCulture)}\" y1=\"{plotTop.ToString(CultureInfo.InvariantCulture)}\" x2=\"{sxPos.ToString(CultureInfo.InvariantCulture)}\" y2=\"{plotBottom.ToString(CultureInfo.InvariantCulture)}\" stroke=\"#dddddd\" stroke-width=\"1\"/>");
                }

                float yStart = (float)Math.Ceiling(settings.YMin / yStep) * yStep;
                for (float y = yStart; y <= settings.YMax; y += yStep)
                {
                    float syPos = plotBottom - (y - settings.YMin) * sy;

                    sb.AppendLine(
                        $"<line x1=\"{plotLeft.ToString(CultureInfo.InvariantCulture)}\" y1=\"{syPos.ToString(CultureInfo.InvariantCulture)}\" x2=\"{plotRight.ToString(CultureInfo.InvariantCulture)}\" y2=\"{syPos.ToString(CultureInfo.InvariantCulture)}\" stroke=\"#dddddd\" stroke-width=\"1\"/>");
                }
            }

            // ---- оси ----
            if (settings.ShowAxes)
            {
                if (settings.XMin <= 0 && settings.XMax >= 0)
                {
                    float x0 = plotLeft + (0 - settings.XMin) * sx;

                    sb.AppendLine(
                        $"<line x1=\"{x0.ToString(CultureInfo.InvariantCulture)}\" y1=\"{plotTop.ToString(CultureInfo.InvariantCulture)}\" x2=\"{x0.ToString(CultureInfo.InvariantCulture)}\" y2=\"{plotBottom.ToString(CultureInfo.InvariantCulture)}\" stroke=\"black\" stroke-width=\"1.5\"/>");
                }

                if (settings.YMin <= 0 && settings.YMax >= 0)
                {
                    float y0 = plotBottom - (0 - settings.YMin) * sy;

                    sb.AppendLine(
                        $"<line x1=\"{plotLeft.ToString(CultureInfo.InvariantCulture)}\" y1=\"{y0.ToString(CultureInfo.InvariantCulture)}\" x2=\"{plotRight.ToString(CultureInfo.InvariantCulture)}\" y2=\"{y0.ToString(CultureInfo.InvariantCulture)}\" stroke=\"black\" stroke-width=\"1.5\"/>");
                }
            }

            // ---- полилиния с графиком ----
            var pointsBuilder = new StringBuilder();

            foreach (PointF p in points)
            {
                float sxPos = plotLeft + (p.X - settings.XMin) * sx;
                float syPos = plotBottom - (p.Y - settings.YMin) * sy;

                if (sxPos < plotLeft || sxPos > plotRight)
                {
                    continue;
                }

                if (syPos < plotTop || syPos > plotBottom)
                {
                    continue;
                }

                pointsBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0},{1} ", sxPos, syPos);
            }

            if (pointsBuilder.Length > 0)
            {
                sb.AppendLine(
                    $"<polyline fill=\"none\" stroke=\"red\" stroke-width=\"2\" points=\"{pointsBuilder.ToString().Trim()}\" />");
            }

            sb.AppendLine("</svg>");

            return sb.ToString();
        }

    }
}
