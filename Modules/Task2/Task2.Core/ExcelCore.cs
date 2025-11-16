using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace Task2.Core
{
    // ----------- МОДЕЛИ ------------

    public sealed class WorksheetModel
    {
        public string Name { get; }

        public IReadOnlyList<string> Columns { get; }

        public IReadOnlyList<IReadOnlyList<string>> Rows { get; }

        public WorksheetModel(
            string name,
            IReadOnlyList<string> columns,
            IReadOnlyList<IReadOnlyList<string>> rows)
        {
            Name = name;
            Columns = columns;
            Rows = rows;
        }
    }

    public sealed class SpreadsheetModel
    {
        public string FilePath { get; }

        public IReadOnlyList<WorksheetModel> Worksheets { get; }

        public SpreadsheetModel(string filePath, IReadOnlyList<WorksheetModel> worksheets)
        {
            FilePath = filePath;
            Worksheets = worksheets;
        }
    }

    // ----------- ИНТЕРФЕЙСЫ ------------

    public interface IExcelFileScanner
    {
        IReadOnlyList<string> ScanExcelFiles(string folderPath);
    }

    public interface ISpreadsheetReader
    {
        SpreadsheetModel ReadSpreadsheet(string filePath);
    }

    // ----------- РЕАЛИЗАЦИЯ СКАНЕРА ------------

    public class ExcelFileScanner : IExcelFileScanner
    {
        private static readonly string[] ExcelExtensions =
        {
            ".xlsx",
            ".xls"
        };

        public IReadOnlyList<string> ScanExcelFiles(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return Array.Empty<string>();


            var files = Directory
                .EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => ExcelExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .OrderBy(Path.GetFileName)
                .ToList();

            return files;
        }
    }

    // ----------- ЧТЕНИЕ EXCEL (ClosedXML) ------------

    public class ClosedXmlSpreadsheetReader : ISpreadsheetReader
    {
        public SpreadsheetModel ReadSpreadsheet(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден", filePath);

            var worksheets = new List<WorksheetModel>();

            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var ws in workbook.Worksheets)
                {
                    var usedRange = ws.RangeUsed();
                    if (usedRange == null)
                    {
                        continue;
                    }

                    int rowCount = usedRange.RowCount();
                    int colCount = usedRange.ColumnCount();

                    var columns = new List<string>();
                    var rows = new List<IReadOnlyList<string>>();

                    // Первая строка — заголовки
                    for (int c = 1; c <= colCount; c++)
                    {
                        string header = usedRange.Cell(1, c).GetString();
                        if (string.IsNullOrWhiteSpace(header))
                            header = $"Колонка {c}";


                        columns.Add(header);
                    }

                    // Остальные строки — данные
                    for (int r = 2; r <= rowCount; r++)
                    {
                        var row = new string[colCount];

                        for (int c = 1; c <= colCount; c++)
                        {
                            row[c - 1] = usedRange.Cell(r, c).GetString();
                        }

                        rows.Add(row);
                    }

                    worksheets.Add(new WorksheetModel(ws.Name, columns, rows));
                }
            }

            return new SpreadsheetModel(filePath, worksheets);
        }
    }
}
