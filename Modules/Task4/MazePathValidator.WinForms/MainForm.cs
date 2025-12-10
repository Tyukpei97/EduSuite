using System;
using System.Drawing;
using System.Windows.Forms;
using Task4.Core;

namespace MazePathValidator.WinForms
{
    public class MainForm : Form
    {
        private TextBox mazeInputTextBox;
        private TextBox commandsTextBox;
        private Button validateButton;
        private TextBox resultTextBox;

        public MainForm()
        {
            Text = "Task4: Проверка маршрута в лабиринте";
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
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            Controls.Add(layout);

            var infoLabel = new Label
            {
                AutoSize = true,
                Text = "Вставьте лабиринт слева (0, 1, 5, 6), а справа — команды движения (\"вверх, вниз, влево, вправо\")."
            };
            layout.Controls.Add(infoLabel, 0, 0);

            var centerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };

            centerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            centerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            mazeInputTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10.0f)
            };

            commandsTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            centerLayout.Controls.Add(mazeInputTextBox, 0, 0);
            centerLayout.Controls.Add(commandsTextBox, 1, 0);

            layout.Controls.Add(centerLayout, 0, 1);

            var bottomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            bottomLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            validateButton = new Button
            {
                Text = "Проверить маршрут",
                Dock = DockStyle.Top,
                Height = 32
            };
            validateButton.Click += ValidateButtonOnClick;

            resultTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            bottomLayout.Controls.Add(validateButton, 0, 0);
            bottomLayout.Controls.Add(resultTextBox, 0, 1);

            layout.Controls.Add(bottomLayout, 0, 2);
        }

        private void ValidateButtonOnClick(object? sender, EventArgs e)
        {
            try
            {
                string mazeText = mazeInputTextBox.Text;
                string commandsText = commandsTextBox.Text;

                MazeGrid maze = MazeTextSerializer.ParseFromText(mazeText);
                var directions = MazePathCommandsParser.ParseCommands(commandsText);

                MazePathValidationResult result = MazePathValidatorService.ValidatePath(maze, directions);

                resultTextBox.Text = result.Message;
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "Ошибка при проверке маршрута: " + exception.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
