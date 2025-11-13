using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FileManager.ConsoleApp
{
    internal static class Program
    {
        private static string _currentDirectory = Directory.GetCurrentDirectory();

        private static readonly string DateFornat = "dd-MM-yyyy HH:mm";

        private static readonly Dictionary<string, Action<IReadOnlyList<string>>> Commands =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {"help", Help },
                {"exit", Exit },
                {"pwd", Pwd },
                {"ls", Ls },
                {"cd", Cd },
                {"mkdir", MkDir },
                {"touch", Touch }
            };

        // ------------------------- Команды -------------------------

        private static void Help(IReadOnlyList<string> _)
        {
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("  help                — показать эту справку");
            Console.WriteLine("  exit                — выход из программы");
            Console.WriteLine("  pwd                 — показать текущую директорию");
            Console.WriteLine("  ls [path]           — содержимое каталога (по умолчанию — текущая папка).");
            Console.WriteLine("                         path может быть относительным или абсолютным путём.");
            Console.WriteLine("  cd <path>           — смена директории (относительный или абсолютный путь)");
            Console.WriteLine("                         Просто \\ подразумевает переход к корню диска.");
            Console.WriteLine("                         Чтобы перейти в папку достаточно ввести имя директории(если вы в корне, то уже через \\folder).");
            Console.WriteLine("                         Чтобы переступить через директорую: folder\\folder_1.");
            Console.WriteLine("                         Чтобы открать папку внутри родительской директории: \\folder\\folder1.");
            Console.WriteLine("  mkdir <name>        — создать каталог");
            Console.WriteLine("  touch <file>        — создать пустой файл или обновить время изменения.");
            Console.WriteLine("                         Также, укажите тип файла.");
        }

        private static void Exit(IReadOnlyList<string> _)
        {
            Environment.Exit(0);
        }

        private static void Pwd(IReadOnlyList<string> _)
        {
            Console.WriteLine(_currentDirectory);
        }

        private static void Ls(IReadOnlyList<string> args)
        {
            string target = args.Count > 0 ? args[0] : ".";
            string full = ResolvePath(target);

            if (File.Exists(full))
            {
                var fi = new FileInfo(full);
                PrintFileLine(fi);
                return;
            }

            if (!Directory.Exists(full))
                throw new DirectoryNotFoundException($"Каталог не найден: {full}");

            var dir = new DirectoryInfo(full);

            DirectoryInfo[] subdirs;
            FileInfo[] files;

            try
            {
                subdirs = dir.GetDirectories();
                files = dir.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException("Недостаточно прав для чтения содержимого каталога.");
            }

            Array.Sort(subdirs, (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            Array.Sort(files, (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            Console.WriteLine(full);

            foreach (var d in subdirs)
                PrintDirLine(d);

            foreach (var f in files)
                PrintFileLine(f);
        }

        private static void Cd(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
                throw new ArgumentException("Укажите путь: cd <path>");

            string pathArg = Path.GetFullPath(args[0]);

            string full = ResolvePath(pathArg);

            if (!Directory.Exists(full))
                throw new DirectoryNotFoundException($"Каталог не найден: {full}");

            _currentDirectory = full;
        }

        private static void MkDir(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
                throw new ArgumentException("Укажите имя каталога: mkdir <name>");

            string full = ResolvePath(args[0]);

            Directory.CreateDirectory(full);
        }

        private static void Touch(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
                throw new ArgumentException("Укажите имя файла: touch <file>");

            string full = ResolvePath(args[0]);

            if (Directory.Exists(full))
                throw new IOException("Укажите каталог, а не файл");

            if (File.Exists(full))
                File.SetLastWriteTimeUtc(full, DateTime.UtcNow);
            else
                using (File.Create(full)) { }
        }

        // ------------------------- Утилиты -------------------------

        private static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                    sb.Append(c);
            }

            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens;
        }

        private static string ResolvePath(string path)
        {
            string baseDir = _currentDirectory;

            string combined = Path.IsPathRooted(path) ? path : Path.Combine(baseDir, path);

            return combined;
        }

        private static void PrintDirLine(DirectoryInfo d)
        {
            string ts = d.LastWriteTime.ToString(DateFornat, CultureInfo.InvariantCulture);

            Console.WriteLine($"<DIR> {ts} {d.Name}");
        }

        private static void PrintFileLine(FileInfo f)
        {
            string ts = f.LastWriteTime.ToString(DateFornat, CultureInfo.InvariantCulture);

            string size = f.Length.ToString("<FILE>", CultureInfo.InvariantCulture).PadLeft(10);

            Console.WriteLine($"{size} {ts}, {f.Name}");
        }

        private static void WriteError(string message)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка: {message}");
            Console.ForegroundColor = old;
        }

        private static void PrintWelcome()
        {
            Console.WriteLine("Крутейший консольный файловый менеджер");
            Console.WriteLine("Введите `help` для списка команд.");
            Console.WriteLine("Обязательно введите `help`, для примера ввода пути к файлам.");
        }


        // ------------------------- Main -------------------------

        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            PrintWelcome();

            while (true)
            {
                Console.WriteLine($"[FM] {_currentDirectory}> ");

                string? line = Console.ReadLine();

                if (line is null)
                    return;

                var tokens = Tokenize(line);

                if (tokens.Count == 0)
                    continue;

                string cmd = tokens[0];

                var argsList = tokens.Count > 1 ? tokens.GetRange(1, tokens.Count - 1) : new List<string>();

                if (Commands.TryGetValue(cmd, out var handler))
                {
                    try
                    {
                        handler(argsList);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                    WriteError($"Неизвестная команда: {cmd}. Введите 'help' для справки");
            }
        }

    }
}