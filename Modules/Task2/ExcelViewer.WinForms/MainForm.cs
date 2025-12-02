using System;
using System.IO;
using System.Windows.Forms;
using Task2.Core;

namespace ExcelViewer.WinForms
{
    public class MainForm : Form
    {
        private readonly IExcelFileScanner _scanner;
        private readonly ISpreadsheetReader _reader;

        private TextBox _txtFolder;
        private Button _btnBrowse;
        private Button _btnScan;

        private FlowLayoutPanel _filesPanel;
        private Label _infoLabel;

        public MainForm()
        {
            Text = "Просмотрщик Excel-файлов (Task2)";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            _scanner = new ExcelFileScanner();
            _reader = new ClosedXmlSpreadsheetReader();

            InitializeControls();
        }

        private void InitializeControls()
        {
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };

            _txtFolder = new TextBox
            {
                Left = 8,
                Top = 18,
                Width = 500
            };

            _btnBrowse = new Button
            {
                Text = "Обзор...",
                Left = _txtFolder.Right + 8,
                Top = 16,
                AutoSize = true
            };
            _btnBrowse.Click += OnBrowseClick;

            _btnScan = new Button
            {
                Text = "Сканировать",
                Left = _btnBrowse.Right + 8,
                Top = 16,
                AutoSize = true
            };
            _btnScan.Click += (_, _) => LoadExcelFiles();

            topPanel.Controls.Add(_txtFolder);
            topPanel.Controls.Add(_btnBrowse);
            topPanel.Controls.Add(_btnScan);

            _infoLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(8),
                Text = "Укажите путь к папке с Excel-файлами (.xlsx, .xls) и нажмите «Сканировать»."
            };

            _filesPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            Controls.Add(_filesPanel);
            Controls.Add(_infoLabel);
            Controls.Add(topPanel);

            // По умолчанию — папка рядом с exe
            _txtFolder.Text = Application.StartupPath;
        }

        private void OnBrowseClick(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Выберите папку с Excel-файлами"
            };

            if (!string.IsNullOrWhiteSpace(_txtFolder.Text) && Directory.Exists(_txtFolder.Text))
            {
                dialog.SelectedPath = _txtFolder.Text;
            }

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _txtFolder.Text = dialog.SelectedPath;
                LoadExcelFiles();
            }
        }

        private void LoadExcelFiles()
        {
            _filesPanel.Controls.Clear();

            string folder = _txtFolder.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                var label = new Label
                {
                    Text = "Указанная папка не существует.",
                    AutoSize = true,
                    Padding = new Padding(8)
                };
                _filesPanel.Controls.Add(label);
                return;
            }

            var files = _scanner.ScanExcelFiles(folder);

            if (files.Count == 0)
            {
                var label = new Label
                {
                    Text = "В папке Excel-файлы не найдены.",
                    AutoSize = true,
                    Padding = new Padding(8)
                };
                _filesPanel.Controls.Add(label);
                return;
            }

            foreach (var file in files)
            {
                var button = new Button
                {
                    Text = Path.GetFileName(file),
                    Tag = file,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(8),
                    Margin = new Padding(4)
                };

                button.Click += OnFileButtonClick;
                _filesPanel.Controls.Add(button);
            }
        }

        private void OnFileButtonClick(object sender, EventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            if (button.Tag is not string filePath)
            {
                return;
            }

            try
            {
                var model = _reader.ReadSpreadsheet(filePath);

                if (model.Worksheets.Count == 0)
                {
                    MessageBox.Show(this,
                        "В книге нет данных (UsedRange пуст).",
                        "Информация",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using (var form = new SpreadsheetForm(model))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Ошибка при чтении файла: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
