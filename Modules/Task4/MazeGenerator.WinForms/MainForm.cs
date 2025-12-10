using System;
using System.Drawing;
using System.Windows.Forms;
using Task4.Core;

namespace MazeGenerator.WinForms
{
    public class MainForm : Form
    {
        private NumericUpDown widthNumeric;
        private NumericUpDown heightNumeric;
        private Button generateButton;
        private Panel mazePanel;
        private TextBox mazeTextBox;
        private Label statusLabel;

        private MazeGrid? currentMaze;

        public MainForm()
        {
            Text = "Task4: Генератор лабиринтов";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            InitializeUi();
        }

        private void InitializeUi()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            Controls.Add(layout);

            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            layout.Controls.Add(topPanel, 0, 0);

            var widthLabel = new Label
            {
                AutoSize = true,
                Text = "Ширина (A):"
            };
            topPanel.Controls.Add(widthLabel);

            widthNumeric = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 200,
                Value = 20,
                Width = 60
            };
            topPanel.Controls.Add(widthNumeric);

            var heightLabel = new Label
            {
                AutoSize = true,
                Text = "Высота (B):"
            };
            topPanel.Controls.Add(heightLabel);

            heightNumeric = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 200,
                Value = 15,
                Width = 60
            };
            topPanel.Controls.Add(heightNumeric);

            generateButton = new Button
            {
                Text = "Сгенерировать лабиринт",
                AutoSize = true
            };
            generateButton.Click += GenerateButtonOnClick;
            topPanel.Controls.Add(generateButton);

            statusLabel = new Label
            {
                AutoSize = true,
                Text = "Укажите размеры и нажмите \"Сгенерировать\"."
            };
            topPanel.Controls.Add(statusLabel);

            mazePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };
            mazePanel.Paint += MazePanelOnPaint;
            layout.Controls.Add(mazePanel, 0, 1);

            mazeTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Font = new Font("Consolas", 10.0f)
            };
            layout.Controls.Add(mazeTextBox, 0, 2);
        }

        private void GenerateButtonOnClick(object? sender, EventArgs e)
        {
            try
            {
                int width = (int)widthNumeric.Value;
                int height = (int)heightNumeric.Value;

                currentMaze = MazeGeneratorService.Generate(width, height);

                mazeTextBox.Text = MazeTextSerializer.SerializeToText(currentMaze);
                statusLabel.Text = $"Лабиринт {currentMaze.Width}x{currentMaze.Height} сгенерирован.";

                mazePanel.Invalidate();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "Ошибка при генерации лабиринта: " + exception.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void MazePanelOnPaint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);

            if (currentMaze == null)
            {
                return;
            }

            DrawMaze(e.Graphics, currentMaze, mazePanel.ClientRectangle);
        }

        private static void DrawMaze(Graphics graphics, MazeGrid maze, Rectangle targetRectangle)
        {
            int width = maze.Width;
            int height = maze.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            float cellWidth = (float)targetRectangle.Width / width;
            float cellHeight = (float)targetRectangle.Height / height;
            float cellSize = Math.Min(cellWidth, cellHeight);

            float offsetX = targetRectangle.Left + (targetRectangle.Width - cellSize * width) / 2.0f;
            float offsetY = targetRectangle.Top + (targetRectangle.Height - cellSize * height) / 2.0f;

            using var wallBrush = new SolidBrush(Color.Black);
            using var roadBrush = new SolidBrush(Color.White);
            using var entranceBrush = new SolidBrush(Color.LimeGreen);
            using var exitBrush = new SolidBrush(Color.Red);
            using var borderPen = new Pen(Color.Gray, 1.0f);

            graphics.FillRectangle(wallBrush, targetRectangle);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int value = maze.GetCell(x, y);

                    Brush cellBrush = value switch
                    {
                        (int)MazeCellType.Road => roadBrush,
                        (int)MazeCellType.Entrance => entranceBrush,
                        (int)MazeCellType.Exit => exitBrush,
                        _ => wallBrush
                    };

                    float cellLeft = offsetX + x * cellSize;
                    float cellTop = offsetY + y * cellSize;

                    var cellRectangle = new RectangleF(
                        cellLeft,
                        cellTop,
                        cellSize,
                        cellSize);

                    graphics.FillRectangle(cellBrush, cellRectangle);
                    graphics.DrawRectangle(
                        borderPen,
                        cellRectangle.X,
                        cellRectangle.Y,
                        cellRectangle.Width,
                        cellRectangle.Height);
                }
            }
        }
    }
}
