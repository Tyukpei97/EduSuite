using System;
using System.Drawing;
using System.Windows.Forms;
using Task4.Core;

namespace MazePathFinder.WinForms
{
    public class MainForm : Form
    {
        private TextBox mazeInputTextBox;
        private Button findPathButton;
        private TextBox resultTextBox;
        private Label infoLabel;

        public MainForm()
        {
            Text = "Task4: Поиск пути в лабиринте";
            Width = 900;
            Height = 600;
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
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            Controls.Add(layout);

            infoLabel = new Label
            {
                AutoSize = true,
                Text = "Вставьте текстовый вид лабиринта (0, 1, 5, 6), затем нажмите \"Найти путь\"."
            };
            layout.Controls.Add(infoLabel, 0, 0);

            mazeInputTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10.0f)
            };
            layout.Controls.Add(mazeInputTextBox, 0, 1);

            findPathButton = new Button
            {
                Text = "Найти путь",
                Dock = DockStyle.Top,
                Height = 32
            };
            findPathButton.Click += FindPathButtonOnClick;

            resultTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            var bottomPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            bottomPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            bottomPanel.Controls.Add(findPathButton, 0, 0);
            bottomPanel.Controls.Add(resultTextBox, 0, 1);

            layout.Controls.Add(bottomPanel, 0, 2);
        }

        private void FindPathButtonOnClick(object? sender, EventArgs e)
        {
            try
            {
                string mazeText = mazeInputTextBox.Text;

                MazeGrid maze = MazeTextSerializer.ParseFromText(mazeText);
                var directions = MazePathFinderService.FindPath(maze);

                string formatted = MazePathFormatter.FormatDirections(directions);
                resultTextBox.Text = formatted;
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "Ошибка при поиске пути: " + exception.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
