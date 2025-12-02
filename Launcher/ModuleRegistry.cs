using System.Collections.Generic;

/// <summary>
/// Вот помнишь наш шаблон, аля, паспорт модулей. Вот этот файл буквально возвращает список таких паспортов, чтобы программа их читала и выводила в лаунчере.
/// Функция LoadModules() как раз и берёт эту коллекцию и выгружает инфу.
/// </summary>

namespace Launcher
{
    public static class ModuleRegistry
    {
        public static IReadOnlyList<ModuleDescriptor> GetAll()
        {
            return new List<ModuleDescriptor>
            {
                new ModuleDescriptor(
                    id: "FileManager.Console",
                    name: "Консольный файловый менеджер",
                    description: "Команды: help, exit/quit, pwd, ls, cd, mkdir, touch.",
                    relativeExePath: @"FileManager.Console\bin\Debug\net8.0\FileManager.Console.exe",
                    solutionPathProj: @"FileManager.Console\FileManager.Console.csproj"),

                new ModuleDescriptor(
                    id: "Encryptor.Console",
                    name: "Шифровщик/дешифровщик",
                    description: "Доступные шифры: " +
                    "Шифр Цезаря, " +
                    "Шифр Виженера, " +
                    "XOR-шифрование, " +
                    "Base64 (кодирование/декодирование), " +
                    "Шифр замены символов, " +
                    "Шифр перестановки",
                    relativeExePath: @"Encryptor.Console\bin\Debug\net8.0\Encryptor.Console.exe",
                    solutionPathProj: @"Encryptor.Console\Encryptor.Console.csproj"),

                new ModuleDescriptor(
                    id: "Sync.Console",
                    name: "Синхронизатор",
                    description: "Готовый модуль для демонстрации.",
                    relativeExePath: @"Sync.Console\bin\Debug\net8.0\Sync.Console.exe", 
                    solutionPathProj : @"Sync.Console\Sync.Console.csproj"),

                new ModuleDescriptor(
                    id: "ExcelViewer.WinForms",
                    name: "Task2: Просмотр Excel",
                    description:
                        "WinForms-приложение для поиска и просмотра Excel-файлов (.xlsx/.xls) в выбранной папке. " +
                        "Поддерживается выбор папки, список найденных файлов, просмотр всех листов книги с отображением таблиц.",
                    relativeExePath: @"Task2\ExcelViewer.WinForms\bin\Debug\net8.0-windows\ExcelViewer.WinForms.exe", 
                    solutionPathProj : @"Task2\ExcelViewer.WinForms\ExcelViewer.WinForms.csproj"),

                new ModuleDescriptor(
                    id: "LinePlotter.WinForms",
                    name: "Task2: График линейной функции",
                    description:
                        "Построение графика функции y = kx + b по заданным параметрам. " +
                        "Генерирует набор точек, рисует график и позволяет сохранить его в PNG/JPEG/BMP.",
                    relativeExePath: @"Task2\LinePlotter.WinForms\bin\Debug\net8.0-windows\LinePlotter.WinForms.exe",
                    solutionPathProj : @"Task2\LinePlotter.WinForms\LinePlotter.WinForms.csproj"),

                new ModuleDescriptor(
                    id: "ColorMixer.WinForms",
                    name: "Task2: Смешивание цветов",
                    description:
                        "Смешивает произвольное количество HEX-цветов. " +
                        "Позволяет добавлять/удалять цвета, валидирует HEX и показывает результирующий цвет.",
                    relativeExePath: @"Task2\ColorMixer.WinForms\bin\Debug\net8.0-windows\ColorMixer.WinForms.exe",
                    solutionPathProj : @"Task2\ColorMixer.WinForms\ColorMixer.WinForms.csproj")
            };
        }
    }
}
