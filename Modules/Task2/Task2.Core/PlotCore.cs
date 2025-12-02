using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

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
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Реальные min/max по X и Y
                float rawMinX = points.Min(p => p.X);
                float rawMaxX = points.Max(p => p.X);
                float rawMinY = points.Min(p => p.Y);
                float rawMaxY = points.Max(p => p.Y);

                // Симметричный диапазон по X вокруг нуля
                float maxAbsX = Math.Max(Math.Abs(rawMinX), Math.Abs(rawMaxX));
                if (maxAbsX < 1f)
                    maxAbsX = 1f;
                float minX = -maxAbsX;
                float maxX = maxAbsX;

                // Симметричный диапазон по Y вокруг нуля
                float maxAbsY = Math.Max(Math.Abs(rawMinY), Math.Abs(rawMaxY));
                if (maxAbsY < 1f)
                    maxAbsY = 1f;
                float minY = -maxAbsY;
                float maxY = maxAbsY;

                float padding = 40f;

                Func<float, float> normX = x =>
                    padding + (x - minX) / (maxX - minX) * (size.Width - 2 * padding);

                Func<float, float> normY = y =>
                    size.Height - padding - (y - minY) / (maxY - minY) * (size.Height - 2 * padding);

                float left = padding;
                float right = size.Width - padding;
                float top = padding;
                float bottom = size.Height - padding;

                using (var gridPen = new Pen(Color.LightGray, 1f) { DashStyle = DashStyle.Dot })
                using (var borderPen = new Pen(Color.Gray, 1f))
                using (var axisPen = new Pen(Color.Black, 2f))
                using (var funcPen = new Pen(Color.DarkBlue, 2f))
                using (var font = new Font(FontFamily.GenericSansSerif, 9f))
                using (var textBrush = new SolidBrush(Color.Black))
                {
                    // Рамка области графика
                    g.DrawRectangle(borderPen, left, top, right - left, bottom - top);

                    // Сетка 10x10
                    const int gridLines = 10;
                    float stepX = (maxX - minX) / gridLines;
                    float stepY = (maxY - minY) / gridLines;

                    for (int i = 1; i < gridLines; i++)
                    {
                        float x = minX + i * stepX;
                        float sx = normX(x);
                        g.DrawLine(gridPen, sx, top, sx, bottom);

                        float y = minY + i * stepY;
                        float sy = normY(y);
                        g.DrawLine(gridPen, left, sy, right, sy);
                    }

                    // Оси по центру
                    float y0 = normY(0);
                    float x0 = normX(0);

                    // Ось X
                    g.DrawLine(axisPen, left, y0, right, y0);
                    g.DrawLine(axisPen, right, y0, right - 10, y0 - 5);
                    g.DrawLine(axisPen, right, y0, right - 10, y0 + 5);
                    g.DrawString("X", font, textBrush, right + 5, y0 - 10);

                    // Ось Y
                    g.DrawLine(axisPen, x0, bottom, x0, top);
                    g.DrawLine(axisPen, x0, top, x0 - 5, top + 10);
                    g.DrawLine(axisPen, x0, top, x0 + 5, top + 10);
                    g.DrawString("Y", font, textBrush, x0 + 5, top - 20);

                    // ---- Нумерация по X (под рамкой) ----
                    for (int i = 0; i <= gridLines; i++)
                    {
                        float xVal = minX + i * stepX;
                        float sx = normX(xVal);

                        string label = xVal.ToString("0.#");
                        SizeF labelSize = g.MeasureString(label, font);

                        float lx = sx - labelSize.Width / 2f;
                        float ly = bottom + 2f; // чуть ниже рамки

                        g.DrawString(label, font, textBrush, lx, ly);
                    }

                    // ---- Нумерация по Y (слева от рамки) ----
                    for (int j = 0; j <= gridLines; j++)
                    {
                        float yVal = minY + j * stepY;
                        float sy = normY(yVal);

                        string label = yVal.ToString("0.#");
                        SizeF labelSize = g.MeasureString(label, font);

                        float lx = left - labelSize.Width - 2f; // слева от рамки
                        float ly = sy - labelSize.Height / 2f;

                        g.DrawString(label, font, textBrush, lx, ly);
                    }

                    // ---- График функции ----
                    for (int i = 1; i < points.Count; i++)
                    {
                        var p1 = points[i - 1];
                        var p2 = points[i];

                        g.DrawLine(
                            funcPen,
                            normX(p1.X), normY(p1.Y),
                            normX(p2.X), normY(p2.Y));
                    }
                }

                switch (format)
                {
                    case ChartImageFormat.Png:
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ChartImageFormat.Jpeg:
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ChartImageFormat.Bmp:
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    default:
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }
            }
        }
    }

}
