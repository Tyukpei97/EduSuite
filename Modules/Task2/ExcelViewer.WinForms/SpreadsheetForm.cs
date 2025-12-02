using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Task2.Core;

namespace ExcelViewer.WinForms
{
    public sealed class SpreadsheetForm : Form
    {
        private readonly SpreadsheetModel _model;

        private ComboBox _cmbSheets;
        private DataGridView _grid;

        public SpreadsheetForm(SpreadsheetModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));

            InitializeComponents();
            LoadSheets();
        }

        private void InitializeComponents()
        {
            Text = $"Просмотр: {Path.GetFileName(_model.FilePath)}";
            Width = 1100;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;

            _cmbSheets = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbSheets.SelectedIndexChanged += CmbSheetsOnSelectedIndexChanged;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                RowHeadersVisible = false
            };

            Controls.Add(_grid);
            Controls.Add(_cmbSheets);
        }

        private void LoadSheets()
        {
            _cmbSheets.Items.Clear();

            foreach (var ws in _model.Worksheets)
            {
                _cmbSheets.Items.Add(ws.Name);
            }

            if (_cmbSheets.Items.Count > 0)
            {
                _cmbSheets.SelectedIndex = 0;
            }
        }

        private void CmbSheetsOnSelectedIndexChanged(object? sender, EventArgs e)
        {
            int index = _cmbSheets.SelectedIndex;

            if (index < 0 || index >= _model.Worksheets.Count)
            {
                return;
            }

            var ws = _model.Worksheets[index];
            ShowWorksheet(ws);
        }

        private void ShowWorksheet(WorksheetModel ws)
        {
            _grid.Columns.Clear();
            _grid.Rows.Clear();

            for (int i = 0; i < ws.Columns.Count; i++)
            {
                _grid.Columns.Add($"col{i + 1}", ws.Columns[i]);
            }

            foreach (var row in ws.Rows)
            {
                var values = new object[row.Count];

                for (int i = 0; i < row.Count; i++)
                {
                    values[i] = row[i] ?? string.Empty;
                }

                _grid.Rows.Add(values);   // передаём object[] с ячейками по столбцам
            }
        }
    }
}
