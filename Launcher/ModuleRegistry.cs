using System.Collections.Generic;
using System.Reflection;

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
                    solutionPathProj : @"Task2\ColorMixer.WinForms\ColorMixer.WinForms.csproj"),

                new ModuleDescriptor(
                    id: "link-shortener",
                    name: "Task3: Сокращатель ссылок",
                    description: "GUI имитация сокращателя ссылок с хранением в JSON.",
                    relativeExePath: @"Task3\LinkShortener.WinForms\bin\Debug\net8.0-windows\LinkShortener.WinForms.exe",
                    solutionPathProj: @"Task3\LinkShortener.WinForms\LinkShortener.WinForms.csproj"),

                new ModuleDescriptor(
                    id: "clients-db",
                    name: "Task3: База клиентов",
                    description: "GUI-\"Excel\" для работы с базой клиентов в JSON.",
                    relativeExePath: @"Task3\ClientsDb.WinForms\bin\Debug\net8.0-windows\ClientsDb.WinForms.exe",
                    solutionPathProj: @"Task3\ClientsDb.WinForms\ClientsDb.WinForms.csproj"),

                new ModuleDescriptor(
                    id: "function-svg-plotter",
                    name: "Task3: График y = f(x)",
                    description: "Построение графика на отрезке -10..10 и сохранение в SVG.",
                    relativeExePath: @"Task3\FunctionSvgPlotter.WinForms\bin\Debug\net8.0-windows\FunctionSvgPlotter.WinForms.exe",
                    solutionPathProj: @"Task3\FunctionSvgPlotter.WinForms\FunctionSvgPlotter.WinForms.csproj"),

                new ModuleDescriptor(
                    id: "Task4.MazeGenerator.WinForms",
                    name: "Task4: Генератор лабиринтов",
                    description: "Генерирует случайный проходимый лабиринт (0 – дорога, 1 – стена, 5 – вход, 6 – выход).",
                    relativeExePath: @"Task4\MazeGenerator.WinForms\bin\Debug\net8.0-windows\MazeGenerator.WinForms.exe",
                    solutionPathProj: @"Task4\MazeGenerator.WinForms\MazeGenerator.WinForms.csproj"),

                new ModuleDescriptor(
                    id: "Task4.MazePathFinder.WinForms",
                    name: "Task4: Поиск пути в лабиринте",
                    description: "Принимает двумерный массив и строит маршрут от входа до выхода в виде команд движения.",
                    relativeExePath: @"Task4\MazePathFinder.WinForms\bin\Debug\net8.0-windows\MazePathFinder.WinForms.exe",
                    solutionPathProj: @"Task4\MazePathFinder.WinForms\MazePathFinder.WinForms.csproj"),

                new ModuleDescriptor(
                    id: "Task4.MazePathValidator.WinForms",
                    name: "Task4: Проверка маршрута",
                    description: "Проверяет, приводит ли введённый набор команд к выходу из лабиринта.",
                    relativeExePath: @"Task4\MazePathValidator.WinForms\bin\Debug\net8.0-windows\MazePathValidator.WinForms.exe",
                    solutionPathProj: @"Task4\MazePathValidator.WinForms\MazePathValidator.WinForms.csproj")
            };
        }
    }
}
