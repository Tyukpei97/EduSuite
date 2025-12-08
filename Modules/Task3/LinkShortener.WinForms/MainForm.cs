using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Task3.Core.Models;
using Task3.Core.Repositories;
using Task3.Core.Services;

namespace LinkShortener.WinForms
{
    public sealed class MainForm : Form
    {
        private readonly IShortLinkRepository _repository;
        private BindingList<ShortLinkRecord> _records;

        private TabControl _tabControl = null!;
        private TabPage _tabSave = null!;
        private TabPage _tabOpen = null!;

        private TextBox _txtFullUrl = null!;
        private TextBox _txtCustomCode = null!;
        private Button _btnSave = null!;
        private DataGridView _dgvLinks = null!;

        private TextBox _txtOpenCode = null!;
        private Button _btnOpen = null!;
        private Label _lblOpenStatus = null!;

        public MainForm()
        {
            Text = "Имитация сокращателя ссылок";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            string appDir = AppContext.BaseDirectory;
            string jsonPath = Path.Combine(appDir, "short_links.json");

            var storage = new JsonStorageService();
            _repository = new JsonShortLinkRepository(jsonPath, storage);

            _records = new BindingList<ShortLinkRecord>();

            InitializeUi();
            LoadDataSafe();
        }

        private void InitializeUi()
        {
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            _tabSave = new TabPage("Сохранить ссылку");
            _tabOpen = new TabPage("Открыть по коду");

            _tabControl.TabPages.Add(_tabSave);
            _tabControl.TabPages.Add(_tabOpen);

            InitializeSaveTab();
            InitializeOpenTab();

            Controls.Add(_tabControl);
        }

        private void InitializeSaveTab()
        {
            var lblFullUrl = new Label
            {
                Text = "Полная ссылка:",
                Left = 10,
                Top = 15,
                AutoSize = true
            };

            _txtFullUrl = new TextBox
            {
                Left = 120,
                Top = 10,
                Width = 500   
            };

            var lblCustomCode = new Label
            {
                Text = "Сокращённый код:",
                Left = 10,
                Top = 50,
                AutoSize = true
            };

            _txtCustomCode = new TextBox
            {
                Left = 125,
                Top = 45,
                Width = 200
            };

            _btnSave = new Button
            {
                Text = "Сохранить",
                Left = 330,   
                Top = 43,
                Width = 100
            };
            _btnSave.Click += BtnSave_Click;

            _dgvLinks = new DataGridView
            {
                Left = 10,
                Top = 90,
                Width = 840,
                Height = 430,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText
            };

            _dgvLinks.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(ShortLinkRecord.ShortCode),
                    HeaderText = "Код",
                    Width = 150
                });

            _dgvLinks.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(ShortLinkRecord.FullUrl),
                    HeaderText = "Полная ссылка",
                    Width = 650
                });

            // подписываемся на двойной клик для копирования кода
            _dgvLinks.CellDoubleClick += DgvLinks_CellDoubleClick;

            _tabSave.Controls.Add(lblFullUrl);
            _tabSave.Controls.Add(_txtFullUrl);
            _tabSave.Controls.Add(lblCustomCode);
            _tabSave.Controls.Add(_txtCustomCode);
            _tabSave.Controls.Add(_btnSave);
            _tabSave.Controls.Add(_dgvLinks);
        }


        private void InitializeOpenTab()
        {
            var lblCode = new Label
            {
                Text = "Код:",
                Left = 10,
                Top = 20,
                AutoSize = true
            };

            _txtOpenCode = new TextBox
            {
                Left = 60,
                Top = 15,
                Width = 200
            };

            _btnOpen = new Button
            {
                Text = "Открыть",
                Left = 270,
                Top = 13,
                Width = 100
            };
            _btnOpen.Click += BtnOpen_Click;

            _lblOpenStatus = new Label
            {
                Left = 10,
                Top = 60,
                AutoSize = true
            };

            _tabOpen.Controls.Add(lblCode);
            _tabOpen.Controls.Add(_txtOpenCode);
            _tabOpen.Controls.Add(_btnOpen);
            _tabOpen.Controls.Add(_lblOpenStatus);
        }

        private void LoadDataSafe()
        {
            try
            {
                var items = _repository
                    .GetAll()
                    .OrderBy(r => r.ShortCode)
                    .ToList();

                _records = new BindingList<ShortLinkRecord>(items);
                _dgvLinks.DataSource = _records;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Не удалось загрузить данные: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _records = new BindingList<ShortLinkRecord>();
                _dgvLinks.DataSource = _records;
            }
        }

        private void SaveDataSafe()
        {
            try
            {
                _repository.SaveAll(_records);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Не удалось сохранить данные: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string fullUrl = _txtFullUrl.Text.Trim();
            string code = _txtCustomCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullUrl))
            {
                MessageBox.Show(
                    this,
                    "Введите полную ссылку.",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!Uri.TryCreate(fullUrl, UriKind.Absolute, out _))
            {
                MessageBox.Show(
                    this,
                    "Неверный формат ссылки. Например: https://example.com",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show(
                    this,
                    "Введите сокращённый код (слово, букву, число и т.п.).",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (_records.Any(
                    r => string.Equals(r.ShortCode?.Trim(), code, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(
                    this,
                    "Такой код уже существует. Укажите другой.",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var record = new ShortLinkRecord
            {
                ShortCode = code,
                FullUrl = fullUrl,
                CreatedAt = DateTime.Now,
                OpenCount = 0
            };

            _records.Insert(0, record);
            SaveDataSafe();

            _txtFullUrl.Clear();
            _txtCustomCode.Clear();
        }

        private void DgvLinks_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            // Берём всегда первый столбец (код)
            var row = _dgvLinks.Rows[e.RowIndex];
            string? code = row.Cells[0].Value?.ToString();

            if (string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            try
            {
                Clipboard.SetText(code);
                MessageBox.Show(
                    this,
                    $"Код \"{code}\" скопирован в буфер обмена.",
                    "Копирование",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Не удалось скопировать код: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void BtnOpen_Click(object? sender, EventArgs e)
        {
            string code = _txtOpenCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                _lblOpenStatus.Text = "Введите код.";
                return;
            }

            // Ищем сначала в текущем списке
            ShortLinkRecord? record = _records
                .FirstOrDefault(
                    r => string.Equals(r.ShortCode?.Trim(), code, StringComparison.OrdinalIgnoreCase));

            // Если не нашли – пробуем перечитать файл
            if (record == null)
            {
                try
                {
                    var fromFile = _repository.GetAll();
                    record = fromFile
                        .FirstOrDefault(
                            r => string.Equals(r.ShortCode?.Trim(), code, StringComparison.OrdinalIgnoreCase));
                }
                catch (Exception ex)
                {
                    _lblOpenStatus.Text = "Ошибка чтения файла ссылок: " + ex.Message;
                    return;
                }
            }

            if (record == null)
            {
                _lblOpenStatus.Text = "Ссылка с таким кодом не найдена.";
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = record.FullUrl,
                    UseShellExecute = true
                };

                Process.Start(psi);
                _lblOpenStatus.Text = "Ссылка открыта.";
            }
            catch (Exception ex)
            {
                _lblOpenStatus.Text = "Не удалось открыть ссылку: " + ex.Message;
            }
        }
    }
}
