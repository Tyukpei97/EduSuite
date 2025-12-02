using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Task2.Core;

namespace ColorMixer.WinForms
{
    public class MainForm : Form
    {
        private readonly IColorParser _colorParser;
        private readonly IColorMixer _colorMixer;

        private FlowLayoutPanel _inputsPanel;
        private Button _btnAdd;
        private Button _btnMix;
        private Panel _resultPanel;
        private Label _resultLabel;

        public MainForm()
        {
            Text = "Смешивание HEX-цветов (Task2)";
            Width = 600;
            Height = 400;
            StartPosition = FormStartPosition.CenterScreen;

            _colorParser = new SimpleColorParser();
            _colorMixer = new AverageColorMixer();

            InitializeControls();
            AddColorInput("#FF0000");
            AddColorInput("#00FF00");
            AddColorInput("#0000FF");
        }

        private void InitializeControls()
        {
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };

            _btnAdd = new Button
            {
                Text = "Добавить цвет",
                AutoSize = true,
                Location = new Point(8, 10)
            };
            _btnAdd.Click += (_, _) => AddColorInput("#FFFFFF");

            _btnMix = new Button
            {
                Text = "Смешать",
                AutoSize = true,
                Location = new Point(140, 10)
            };
            _btnMix.Click += OnMixClick;

            topPanel.Controls.Add(_btnAdd);
            topPanel.Controls.Add(_btnMix);

            _inputsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 150,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(8)
            };

            _resultPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            _resultLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Результирующий цвет: -"
            };

            _resultPanel.Controls.Add(_resultLabel);

            Controls.Add(_resultPanel);
            Controls.Add(_inputsPanel);
            Controls.Add(topPanel);
        }

        private void AddColorInput(string defaultValue)
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 4)
            };

            var textBox = new TextBox
            {
                Width = 80,
                Text = defaultValue
            };

            var btnRemove = new Button
            {
                Text = "убрать",
                AutoSize = true,
                Margin = new Padding(4, 0, 0, 0)
            };
            btnRemove.Click += (_, _) =>
            {
                _inputsPanel.Controls.Remove(panel);
                panel.Dispose();
            };

            panel.Controls.Add(new Label { Text = "HEX:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) });
            panel.Controls.Add(textBox);
            panel.Controls.Add(btnRemove);

            _inputsPanel.Controls.Add(panel);
        }

        private void OnMixClick(object sender, EventArgs e)
        {
            var colors = new List<Color>();

            foreach (Control row in _inputsPanel.Controls)
            {
                if (row is not FlowLayoutPanel panel)
                    continue;

                foreach (Control ctrl in panel.Controls)
                {
                    if (ctrl is TextBox textBox)
                    {
                        string hex = textBox.Text;

                        if (string.IsNullOrWhiteSpace(hex))
                            continue;

                        try
                        {
                            var color = _colorParser.ParseHex(hex);
                            colors.Add(color);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this,
                                $"Ошибка в значении \"{hex}\": {ex.Message}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }

            if (colors.Count == 0)
            {
                MessageBox.Show(this,
                    "Введите хотя бы один корректный HEX-цвет.",
                    "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var mixed = _colorMixer.Mix(colors);
            string hexResult = _colorParser.ToHex(mixed);

            _resultPanel.BackColor = mixed;
            _resultLabel.Text = $"Результирующий цвет: {hexResult}";
        }
    }
}
