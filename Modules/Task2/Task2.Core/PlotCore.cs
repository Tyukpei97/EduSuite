using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;

namespace Task2.Core
{
    public enum ChartImageFormat
    {
        Png,
        Jpeg,
        Bmp
    }

    public interface ILinearFunctionService
    {
        IReadOnlyList<PointF> BuildPoints(double k, double b, double fromX, double toX, int steps);
    }

    public interface IChartRenderer
    {
        void RenderToImage(IReadOnlyList<PointF> points, Size size, Stream output, ChartImageFormat format);
    }

    public class LinearFunctionServer : ILinearFunctionService
    {
        public IReadOnlyList<PointF> BuildPoints (double k, double b, double fromX, double toX, int steps)
        {
            if (steps < 2)
                throw new ArgumentOutOfRangeException(nameof(steps));

            if (fromX > toX)
                (fromX, toX) = (toX, fromX);

            double step = (toX  - fromX) / (steps - 1);
            var points = new List<PointF>(steps);

            for (int i = 0; i < steps; i++)
            {
                double x = fromX + step * i;
                double y = k * x + b;
                points.Add(new PointF((float)x, (float)y));
            }

            return points;
        }
    }

    public class SimpleCharRenderer : IChartRenderer
    {
        public void RenderToImage(IReadOnlyList<PointF> points, Size size, Stream output, ChartImageFormat format)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Список точек пуст.", nameof(points));

            if (size.Width <= 0 || size.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Некорректный размер изображения.");

            using (var bitmap = new Bitmap(size.Width, size.Height))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                float minX = points.Min(p => p.X);
                float maxX = points.Max(p =>  p.X);
                float minY = points.Min(p => p.Y);
                float maxY = points.Max(p => p.Y);

                if (Math.Abs(maxX - minY) < 1e-6f)
                    maxX = minX + 1;

                if (Math.Abs(maxY - minY) < 1e-6f)
                    maxY = minY + 1;

                float padding = 20f;

                Func<float, float> normX = x =>
                    padding + (x - minX) / (maxX - minX) * (size.Width - 2 * padding);

                Func<float, float> normY = y => 
                    size.Height - padding - (y - minY) / (maxY - minY) * (size.Height - 2 * padding);

                using (var axisPen = new Pen(Color.Gray, 1))
                using (var linePen = new Pen(Color.Black, 2))
                {
                    g.DrawRectangle(axisPen, padding, padding, size.Width - 2 * padding, size.Height - 2 * padding);

                    for (int i = 1; i < points.Count; i++)
                    {
                        var p1 = points[i - 1];
                        var p2 = points[i];

                        g.DrawLine(linePen, normX(p1.X), normY(p1.Y), normX(p2.X), normY(p2.Y));
                    }
                }

                switch (format)
                {
                    case ChartImageFormat.Png:
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png); break;

                    case ChartImageFormat.Jpeg:
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg); break;

                    case ChartImageFormat.Bmp:
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Bmp); break;

                    default:
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png); break;
                }
            }
        }
    }


}
