using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Task2.Core;

namespace LinePlotter.WinForms
{
    public class MainForm : Form
    {
        private readonly ILinearFunctionService _functionService;
        private readonly IChartRenderer _chartRenderer;

        private TextBox _txtK;
        private TextBox _txtB;
        private TextBox _txtXRange;   // x – диапазон по оси X

        private Button _btnPlot;
        private Button _btnSave;

        private PictureBox _pictureBox;

        private System.Collections.Generic.IReadOnlyList<PointF> _lastPoints;

        public MainForm()
        {
            Text = "Построение y = kx + b (Task2)";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            _functionService = new LinearFunctionServer();
            _chartRenderer = new SimpleCharRenderer();

            InitializeControls();
        }

        private void InitializeControls()
        {
            var panelTop = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                ColumnCount = 8,
                RowCount = 1
            };

            panelTop.ColumnStyles.Clear();
            for (int i = 0; i < 8; i++)
            {
                panelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 8));
            }

            panelTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _txtK = new TextBox { Text = "1", Dock = DockStyle.Fill };
            _txtB = new TextBox { Text = "0", Dock = DockStyle.Fill };
            _txtXRange = new TextBox { Text = "10", Dock = DockStyle.Fill }; // начальный диапазон [-10;10]

            _btnPlot = new Button { Text = "Построить", Dock = DockStyle.Fill };
            _btnPlot.Click += OnPlotClick;

            _btnSave = new Button { Text = "Сохранить...", Dock = DockStyle.Fill };
            _btnSave.Click += OnSaveClick;

            panelTop.Controls.Add(new Label
            {
                Text = "k:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, 0);
            panelTop.Controls.Add(_txtK, 1, 0);

            panelTop.Controls.Add(new Label
            {
                Text = "b:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 2, 0);
            panelTop.Controls.Add(_txtB, 3, 0);

            panelTop.Controls.Add(new Label
            {
                Text = "x:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 4, 0);
            panelTop.Controls.Add(_txtXRange, 5, 0);

            panelTop.Controls.Add(_btnPlot, 6, 0);
            panelTop.Controls.Add(_btnSave, 7, 0);

            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Controls.Add(_pictureBox);
            Controls.Add(panelTop);
        }

        private bool TryReadInputs(out double k, out double b, out double xRange)
        {
            k = b = xRange = 0;

            if (!double.TryParse(_txtK.Text, out k))
            {
                MessageBox.Show(this, "Некорректное значение k.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(_txtB.Text, out b))
            {
                MessageBox.Show(this, "Некорректное значение b.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(_txtXRange.Text, out xRange))
            {
                MessageBox.Show(this, "Некорректное значение x.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (xRange <= 0)
            {
                MessageBox.Show(this, "x должен быть больше 0 (диапазон [-x; x]).", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void OnPlotClick(object sender, EventArgs e)
        {
            if (!TryReadInputs(out double k, out double b, out double xRange))
            {
                return;
            }

            try
            {
                double fromX = -xRange;
                double toX = xRange;
                const int steps = 200; // фиксированное число точек

                _lastPoints = _functionService.BuildPoints(k, b, fromX, toX, steps);

                using var ms = new MemoryStream();
                var size = _pictureBox.ClientSize;
                if (size.Width <= 0 || size.Height <= 0)
                {
                    size = new Size(800, 400);
                }

                _chartRenderer.RenderToImage(_lastPoints, size, ms, ChartImageFormat.Png);
                ms.Position = 0;

                var oldImage = _pictureBox.Image;
                _pictureBox.Image = Image.FromStream(ms);
                oldImage?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Ошибка построения графика: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (_lastPoints == null || _lastPoints.Count == 0)
            {
                MessageBox.Show(this,
                    "Сначала постройте график.",
                    "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|Bitmap (*.bmp)|*.bmp",
                FileName = "line.png"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var format = ChartImageFormat.Png;

            if (dialog.FilterIndex == 2)
            {
                format = ChartImageFormat.Jpeg;
            }
            else if (dialog.FilterIndex == 3)
            {
                format = ChartImageFormat.Bmp;
            }

            try
            {
                using var fs = File.Create(dialog.FileName);
                var size = new Size(800, 400);
                _chartRenderer.RenderToImage(_lastPoints, size, fs, format);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Ошибка сохранения файла: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
