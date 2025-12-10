using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Task3.Core.Charting;
using Task3.Core.Functions;

namespace FunctionSvgPlotter.WinForms
{
    public sealed class MainForm : Form
    {
        private ComboBox _cmbFunctionType = null!;

        private GroupBox _grpLinear = null!;
        private TextBox _txtLinearK = null!;
        private TextBox _txtLinearB = null!;

        private GroupBox _grpQuadratic = null!;
        private TextBox _txtQuadA = null!;
        private TextBox _txtQuadB = null!;
        private TextBox _txtQuadC = null!;

        private GroupBox _grpSin = null!;
        private TextBox _txtAmplitude = null!;
        private TextBox _txtFrequency = null!;

        private Button _btnPlot = null!;
        private Button _btnSaveSvg = null!;

        private PictureBox _pictureBox = null!;

        private readonly ISvgChartRenderer _svgRenderer;

        private List<PointF>? _currentPoints;
        private ChartSettings? _currentSettings;

        public MainForm()
        {
            Text = "График y = f(x) с сохранением в SVG";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            _svgRenderer = new SimpleSvgChartRenderer();

            InitializeUi();
        }

        private void InitializeUi()
        {
            _cmbFunctionType = new ComboBox
            {
                Left = 10,
                Top = 10,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cmbFunctionType.Items.Add("Линейная: y = kx + b");
            _cmbFunctionType.Items.Add("Квадратичная: y = ax² + bx + c");
            _cmbFunctionType.Items.Add("Синус: y = A * sin(ωx)");
            _cmbFunctionType.SelectedIndexChanged += CmbFunctionType_SelectedIndexChanged;

            _btnPlot = new Button
            {
                Text = "Построить",
                Left = 280,
                Top = 10,
                Width = 120
            };
            _btnPlot.Click += BtnPlot_Click;

            _btnSaveSvg = new Button
            {
                Text = "Сохранить SVG",
                Left = 410,
                Top = 10,
                Width = 140
            };
            _btnSaveSvg.Click += BtnSaveSvg_Click;

            InitializeLinearGroup();
            InitializeQuadraticGroup();
            InitializeSinGroup();

            _pictureBox = new PictureBox
            {
                Left = 10,
                Top = 150,
                Width = 960,
                Height = 450,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(_cmbFunctionType);
            Controls.Add(_btnPlot);
            Controls.Add(_btnSaveSvg);
            Controls.Add(_grpLinear);
            Controls.Add(_grpQuadratic);
            Controls.Add(_grpSin);
            Controls.Add(_pictureBox);

            _cmbFunctionType.SelectedIndex = 0;

            UpdateGroupsVisibility();
        }

        private void InitializeLinearGroup()
        {
            _grpLinear = new GroupBox
            {
                Text = "Параметры линейной функции",
                Left = 10,
                Top = 50,
                Width = 320,
                Height = 90
            };

            var lblK = new Label
            {
                Text = "k:",
                Left = 10,
                Top = 25,
                AutoSize = true
            };

            _txtLinearK = new TextBox
            {
                Left = 40,
                Top = 20,
                Width = 80,
                Text = "1"
            };

            var lblB = new Label
            {
                Text = "b:",
                Left = 140,
                Top = 25,
                AutoSize = true
            };

            _txtLinearB = new TextBox
            {
                Left = 170,
                Top = 20,
                Width = 80,
                Text = "0"
            };

            _grpLinear.Controls.Add(lblK);
            _grpLinear.Controls.Add(_txtLinearK);
            _grpLinear.Controls.Add(lblB);
            _grpLinear.Controls.Add(_txtLinearB);
        }

        private void InitializeQuadraticGroup()
        {
            _grpQuadratic = new GroupBox
            {
                Text = "Параметры квадратичной функции",
                Left = 10,
                Top = 50,
                Width = 450,
                Height = 90
            };

            var lblA = new Label
            {
                Text = "a:",
                Left = 10,
                Top = 25,
                AutoSize = true
            };

            _txtQuadA = new TextBox
            {
                Left = 40,
                Top = 20,
                Width = 80,
                Text = "1"
            };

            var lblB = new Label
            {
                Text = "b:",
                Left = 140,
                Top = 25,
                AutoSize = true
            };

            _txtQuadB = new TextBox
            {
                Left = 170,
                Top = 20,
                Width = 80,
                Text = "0"
            };

            var lblC = new Label
            {
                Text = "c:",
                Left = 270,
                Top = 25,
                AutoSize = true
            };

            _txtQuadC = new TextBox
            {
                Left = 300,
                Top = 20,
                Width = 80,
                Text = "0"
            };

            _grpQuadratic.Controls.Add(lblA);
            _grpQuadratic.Controls.Add(_txtQuadA);
            _grpQuadratic.Controls.Add(lblB);
            _grpQuadratic.Controls.Add(_txtQuadB);
            _grpQuadratic.Controls.Add(lblC);
            _grpQuadratic.Controls.Add(_txtQuadC);
        }

        private void InitializeSinGroup()
        {
            _grpSin = new GroupBox
            {
                Text = "Параметры синусоиды",
                Left = 10,
                Top = 50,
                Width = 380,
                Height = 90
            };

            var lblA = new Label
            {
                Text = "A (амплитуда):",
                Left = 10,
                Top = 25,
                AutoSize = true
            };

            _txtAmplitude = new TextBox
            {
                Left = 120,
                Top = 20,
                Width = 80,
                Text = "1"
            };

            var lblW = new Label
            {
                Text = "ω (частота):",
                Left = 210,
                Top = 25,
                AutoSize = true
            };

            _txtFrequency = new TextBox
            {
                Left = 290,
                Top = 20,
                Width = 60,
                Text = "1"
            };

            _grpSin.Controls.Add(lblA);
            _grpSin.Controls.Add(_txtAmplitude);
            _grpSin.Controls.Add(lblW);
            _grpSin.Controls.Add(_txtFrequency);
        }

        private void CmbFunctionType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateGroupsVisibility();
        }

        private void UpdateGroupsVisibility()
        {
            int index = _cmbFunctionType.SelectedIndex;

            _grpLinear.Visible = index == 0;
            _grpQuadratic.Visible = index == 1;
            _grpSin.Visible = index == 2;
        }

        private void BtnPlot_Click(object? sender, EventArgs e)
        {
            if (!TryCreateFunction(out IFunction? function))
            {
                return;
            }

            const double xMin = -10.0;
            const double xMax = 10.0;
            const int steps = 2000;

            double step = (xMax - xMin) / steps;

            var points = new List<PointF>(steps + 1);

            double minY = double.MaxValue;
            double maxY = double.MinValue;

            for (int i = 0; i <= steps; i++)
            {
                double x = xMin + step * i;
                double y = function.Evaluate(x);

                if (double.IsNaN(y) || double.IsInfinity(y))
                {
                    continue;
                }

                points.Add(new PointF((float)x, (float)y));

                if (y < minY)
                {
                    minY = y;
                }

                if (y > maxY)
                {
                    maxY = y;
                }
            }

            if (points.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "Не удалось построить точки для выбранной функции.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            double maxAbsY = Math.Max(Math.Abs(minY), Math.Abs(maxY));

            if (maxAbsY < 1.0)
            {
                maxAbsY = 1.0;
            }

            var settings = new ChartSettings
            {
                XMin = (float)xMin,
                XMax = (float)xMax,
                YMin = (float)(-maxAbsY),
                YMax = (float)maxAbsY,
                GridStep = 1f,
                ShowAxes = true,
                ShowGrid = true
            };

            if (_pictureBox.Width <= 0 || _pictureBox.Height <= 0)
            {
                settings.Width = 960;
                settings.Height = 450;
            }
            else
            {
                settings.Width = _pictureBox.Width;
                settings.Height = _pictureBox.Height;
            }

            var bmp = new Bitmap((int)settings.Width, (int)settings.Height);

            DrawChartToBitmap(bmp, points, settings);

            if (_pictureBox.Image != null)
            {
                _pictureBox.Image.Dispose();
            }

            _pictureBox.Image = bmp;

            _currentPoints = points;
            _currentSettings = settings;
        }

        private void BtnSaveSvg_Click(object? sender, EventArgs e)
        {
            if (_currentPoints == null || _currentSettings == null)
            {
                MessageBox.Show(
                    this,
                    "Сначала постройте график.",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title = "Сохранить как SVG",
                Filter = "SVG файлы|*.svg",
                FileName = "function.svg"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string svg = _svgRenderer.RenderSvg(_currentPoints, _currentSettings);

            try
            {
                File.WriteAllText(dlg.FileName, svg);
                MessageBox.Show(
                    this,
                    "Файл успешно сохранён.",
                    "Готово",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Не удалось сохранить SVG: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool TryCreateFunction(out IFunction? function)
        {
            function = null;

            int index = _cmbFunctionType.SelectedIndex;

            try
            {
                if (index == 0)
                {
                    if (!double.TryParse(_txtLinearK.Text.Trim(), out double k))
                    {
                        ShowInvalidParam("k");
                        return false;
                    }

                    if (!double.TryParse(_txtLinearB.Text.Trim(), out double b))
                    {
                        ShowInvalidParam("b");
                        return false;
                    }

                    function = new LinearFunction(k, b);
                }
                else if (index == 1)
                {
                    if (!double.TryParse(_txtQuadA.Text.Trim(), out double a))
                    {
                        ShowInvalidParam("a");
                        return false;
                    }

                    if (!double.TryParse(_txtQuadB.Text.Trim(), out double b))
                    {
                        ShowInvalidParam("b");
                        return false;
                    }

                    if (!double.TryParse(_txtQuadC.Text.Trim(), out double c))
                    {
                        ShowInvalidParam("c");
                        return false;
                    }

                    function = new QuadraticFunction(a, b, c);
                }
                else if (index == 2)
                {
                    if (!double.TryParse(_txtAmplitude.Text.Trim(), out double amplitude))
                    {
                        ShowInvalidParam("A");
                        return false;
                    }

                    if (!double.TryParse(_txtFrequency.Text.Trim(), out double frequency))
                    {
                        ShowInvalidParam("ω");
                        return false;
                    }

                    function = new SinFunction(amplitude, frequency);
                }
                else
                {
                    MessageBox.Show(
                        this,
                        "Неизвестный тип функции.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return false;
                }

                return function != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Ошибка при создании функции: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        private void ShowInvalidParam(string name)
        {
            MessageBox.Show(
                this,
                $"Неверное значение параметра {name}.",
                "Внимание",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void DrawChartToBitmap(Bitmap bitmap, IList<PointF> points, ChartSettings settings)
        {
            using Graphics g = Graphics.FromImage(bitmap);

            g.Clear(Color.White);

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

            using var gridPen = new Pen(Color.LightGray, 1f);
            using var axisPen = new Pen(Color.Black, 1.5f);
            using var funcPen = new Pen(Color.Red, 2f);
            using var font = new Font("Segoe UI", 8f);
            using var textBrush = new SolidBrush(Color.Black);

            if (settings.ShowGrid && xStep > 0f && yStep > 0f)
            {
                float xStart = (float)Math.Ceiling(settings.XMin / xStep) * xStep;
                for (float x = xStart; x <= settings.XMax; x += xStep)
                {
                    float sxPos = plotLeft + (x - settings.XMin) * sx;

                    g.DrawLine(gridPen, sxPos, plotTop, sxPos, plotBottom);

                    float yLabel = plotBottom + 2;
                    g.DrawString(x.ToString("0"), font, textBrush, sxPos - 8, yLabel);
                }

                float yStart = (float)Math.Ceiling(settings.YMin / yStep) * yStep;
                for (float y = yStart; y <= settings.YMax; y += yStep)
                {
                    float syPos = plotBottom - (y - settings.YMin) * sy;

                    g.DrawLine(gridPen, plotLeft, syPos, plotRight, syPos);

                    float xLabel = 2;
                    g.DrawString(y.ToString("0"), font, textBrush, xLabel, syPos - 6);
                }
            }

            if (settings.ShowAxes)
            {
                if (settings.XMin <= 0 && settings.XMax >= 0)
                {
                    float x0 = plotLeft + (0 - settings.XMin) * sx;
                    g.DrawLine(axisPen, x0, plotTop, x0, plotBottom);
                }

                if (settings.YMin <= 0 && settings.YMax >= 0)
                {
                    float y0 = plotBottom - (0 - settings.YMin) * sy;
                    g.DrawLine(axisPen, plotLeft, y0, plotRight, y0);
                }
            }

            if (points.Count < 2)
            {
                return;
            }

            PointF? prev = null;

            foreach (PointF p in points)
            {
                float sxPos = plotLeft + (p.X - settings.XMin) * sx;
                float syPos = plotBottom - (p.Y - settings.YMin) * sy;

                var current = new PointF(sxPos, syPos);

                if (prev.HasValue)
                {
                    g.DrawLine(funcPen, prev.Value, current);
                }

                prev = current;
            }
        }

    }
}
