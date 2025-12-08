using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Task3.Core.Models;
using Task3.Core.Repositories;
using Task3.Core.Services;

namespace ClientsDb.WinForms
{
    public sealed class MainForm : Form
    {
        private readonly IClientRepository _repository;
        private BindingList<Client> _clients;

        private DataGridView _dgvClients = null!;

        private TextBox _txtFullName = null!;
        private TextBox _txtTotalSpent = null!;
        private TextBox _txtEmail = null!;
        private TextBox _txtPhone = null!;
        private TextBox _txtComment = null!;

        private Button _btnAdd = null!;
        private Button _btnUpdate = null!;
        private Button _btnDelete = null!;
        private Button _btnSortByName = null!;
        private Button _btnSortByTotal = null!;
        private Button _btnReload = null!;

        public MainForm()
        {
            Text = "База клиентов (JSON)";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            string appDir = AppContext.BaseDirectory;
            string jsonPath = Path.Combine(appDir, "clients.json");

            var storage = new JsonStorageService();
            _repository = new JsonClientRepository(jsonPath, storage);

            _clients = new BindingList<Client>();

            InitializeUi();
            LoadDataSafe();
        }

        private void InitializeUi()
        {
            _dgvClients = new DataGridView
            {
                Left = 10,
                Top = 10,
                Width = 650,
                Height = 530,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            _dgvClients.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(Client.FullName),
                    HeaderText = "ФИО",
                    Width = 200
                });

            _dgvClients.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(Client.TotalSpent),
                    HeaderText = "Потрачено",
                    Width = 100
                });

            _dgvClients.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(Client.Email),
                    HeaderText = "Email",
                    Width = 150
                });

            _dgvClients.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(Client.Phone),
                    HeaderText = "Телефон",
                    Width = 120
                });

            _dgvClients.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(Client.Comment),
                    HeaderText = "Комментарий",
                    Width = 200
                });

            _dgvClients.SelectionChanged += DgvClients_SelectionChanged;

            int left = 680;
            int top = 10;
            int labelWidth = 80;
            int inputWidth = 220;
            int verticalStep = 30;

            var lblFullName = new Label
            {
                Text = "ФИО:",
                Left = left,
                Top = top + 5,
                Width = labelWidth
            };

            _txtFullName = new TextBox
            {
                Left = left + labelWidth,
                Top = top,
                Width = inputWidth
            };

            top += verticalStep;

            var lblTotalSpent = new Label
            {
                Text = "Потрачено:",
                Left = left,
                Top = top + 5,
                Width = labelWidth
            };

            _txtTotalSpent = new TextBox
            {
                Left = left + labelWidth,
                Top = top,
                Width = inputWidth
            };

            top += verticalStep;

            var lblEmail = new Label
            {
                Text = "Email:",
                Left = left,
                Top = top + 5,
                Width = labelWidth
            };

            _txtEmail = new TextBox
            {
                Left = left + labelWidth,
                Top = top,
                Width = inputWidth
            };

            top += verticalStep;

            var lblPhone = new Label
            {
                Text = "Телефон:",
                Left = left,
                Top = top + 5,
                Width = labelWidth
            };

            _txtPhone = new TextBox
            {
                Left = left + labelWidth,
                Top = top,
                Width = inputWidth
            };

            top += verticalStep;

            var lblComment = new Label
            {
                Text = "Комментарий:",
                Left = left,
                Top = top + 5,
                Width = labelWidth
            };

            _txtComment = new TextBox
            {
                Left = left + labelWidth,
                Top = top,
                Width = inputWidth,
                Height = 80,
                Multiline = true
            };

            top += 100;

            _btnAdd = new Button
            {
                Text = "Добавить",
                Left = left,
                Top = top,
                Width = 100
            };
            _btnAdd.Click += BtnAdd_Click;

            _btnUpdate = new Button
            {
                Text = "Обновить",
                Left = left + 110,
                Top = top,
                Width = 100
            };
            _btnUpdate.Click += BtnUpdate_Click;

            _btnDelete = new Button
            {
                Text = "Удалить",
                Left = left + 220,
                Top = top,
                Width = 100
            };
            _btnDelete.Click += BtnDelete_Click;

            top += verticalStep + 10;

            _btnSortByName = new Button
            {
                Text = "Сортировать по ФИО",
                Left = left,
                Top = top,
                Width = 150
            };
            _btnSortByName.Click += BtnSortByName_Click;

            _btnSortByTotal = new Button
            {
                Text = "Сортировать по сумме",
                Left = left + 160,
                Top = top,
                Width = 160
            };
            _btnSortByTotal.Click += BtnSortByTotal_Click;

            _btnReload = new Button
            {
                Text = "Перечитать файл",
                Left = left,
                Top = top + verticalStep + 10,
                Width = 180
            };
            _btnReload.Click += BtnReload_Click;

            Controls.Add(_dgvClients);
            Controls.Add(lblFullName);
            Controls.Add(_txtFullName);
            Controls.Add(lblTotalSpent);
            Controls.Add(_txtTotalSpent);
            Controls.Add(lblEmail);
            Controls.Add(_txtEmail);
            Controls.Add(lblPhone);
            Controls.Add(_txtPhone);
            Controls.Add(lblComment);
            Controls.Add(_txtComment);
            Controls.Add(_btnAdd);
            Controls.Add(_btnUpdate);
            Controls.Add(_btnDelete);
            Controls.Add(_btnSortByName);
            Controls.Add(_btnSortByTotal);
            Controls.Add(_btnReload);
        }

        private void LoadDataSafe()
        {
            try
            {
                var items = _repository
                    .GetAll()
                    .OrderBy(c => c.FullName)
                    .ToList();

                _clients = new BindingList<Client>(items);
                _dgvClients.DataSource = _clients;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Не удалось загрузить базу клиентов: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _clients = new BindingList<Client>();
                _dgvClients.DataSource = _clients;
            }
        }

        private void SaveDataSafe()
        {
            try
            {
                _repository.SaveAll(_clients);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Не удалось сохранить базу клиентов: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DgvClients_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dgvClients.CurrentRow == null)
            {
                return;
            }

            if (_dgvClients.CurrentRow.DataBoundItem is Client client)
            {
                _txtFullName.Text = client.FullName;
                _txtTotalSpent.Text = client.TotalSpent.ToString();
                _txtEmail.Text = client.Email ?? string.Empty;
                _txtPhone.Text = client.Phone ?? string.Empty;
                _txtComment.Text = client.Comment ?? string.Empty;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!TryReadClientFromInputs(out Client client))
            {
                return;
            }

            client.Id = Guid.NewGuid();
            _clients.Add(client);
            SaveDataSafe();
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_dgvClients.CurrentRow == null)
            {
                MessageBox.Show(
                    this,
                    "Выберите клиента для обновления.",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (_dgvClients.CurrentRow.DataBoundItem is not Client existing)
            {
                return;
            }

            if (!TryReadClientFromInputs(out Client updated))
            {
                return;
            }

            existing.FullName = updated.FullName;
            existing.TotalSpent = updated.TotalSpent;
            existing.Email = updated.Email;
            existing.Phone = updated.Phone;
            existing.Comment = updated.Comment;

            _dgvClients.Refresh();
            SaveDataSafe();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_dgvClients.CurrentRow == null)
            {
                return;
            }

            if (_dgvClients.CurrentRow.DataBoundItem is not Client client)
            {
                return;
            }

            DialogResult result = MessageBox.Show(
                this,
                "Удалить выбранного клиента?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _clients.Remove(client);
            SaveDataSafe();
        }

        private void BtnSortByName_Click(object? sender, EventArgs e)
        {
            var sorted = _clients
                .OrderBy(c => c.FullName)
                .ToList();

            _clients = new BindingList<Client>(sorted);
            _dgvClients.DataSource = _clients;
        }

        private void BtnSortByTotal_Click(object? sender, EventArgs e)
        {
            var sorted = _clients
                .OrderByDescending(c => c.TotalSpent)
                .ToList();

            _clients = new BindingList<Client>(sorted);
            _dgvClients.DataSource = _clients;
        }

        private void BtnReload_Click(object? sender, EventArgs e)
        {
            LoadDataSafe();
        }

        private bool TryReadClientFromInputs(out Client client)
        {
            client = new Client();

            string fullName = _txtFullName.Text.Trim();
            string totalText = _txtTotalSpent.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show(
                    this,
                    "Введите ФИО.",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (!decimal.TryParse(totalText, out decimal totalSpent))
            {
                MessageBox.Show(
                    this,
                    "Неверное значение в поле: Потрачено.",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            client.FullName = fullName;
            client.TotalSpent = totalSpent;
            client.Email = string.IsNullOrWhiteSpace(_txtEmail.Text)
                ? null
                : _txtEmail.Text.Trim();
            client.Phone = string.IsNullOrWhiteSpace(_txtPhone.Text)
                ? null
                : _txtPhone.Text.Trim();
            client.Comment = string.IsNullOrWhiteSpace(_txtComment.Text)
                ? null
                : _txtComment.Text.Trim();

            return true;
        }
    }
}
