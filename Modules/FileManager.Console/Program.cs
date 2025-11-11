using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class FileManager
{
    private static string currentPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private static string clipboardPath = null;
    private static bool isCut = false;

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "Консольный файловый менеджер";
        Console.CursorVisible = false;

        if (args.Length > 0 && Directory.Exists(args[0]))
            currentPath = args[0];

        while (true)
        {
            try
            {
                DisplayDirectory();
            }
            catch (Exception ex)
            {
                SafeShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
            }
        }
    }

    static void DisplayDirectory()
    {
        Console.Clear();
        DrawHeader();

        SetColor(ConsoleColor.Cyan);
        Console.WriteLine(" Текущая папка:");
        SetColor(ConsoleColor.Yellow);
        Console.WriteLine($" {currentPath}\n");
        ResetColor();

        var entries = GetDirectoryEntries();
        int selectedIndex = 0;
        int scrollOffset = 0;
        int maxVisibleLines = Console.WindowHeight - 13; // Заголовок + путь + подсказки

        if (maxVisibleLines < 5) maxVisibleLines = 5;

        while (true)
        {
            DrawFileList(entries, selectedIndex, scrollOffset, maxVisibleLines);
            DrawHotkeys(entries.Count, selectedIndex + 1);

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow && selectedIndex > 0)
            {
                selectedIndex--;
                if (selectedIndex < scrollOffset) scrollOffset = selectedIndex;
            }
            else if (key == ConsoleKey.DownArrow && selectedIndex < entries.Count - 1)
            {
                selectedIndex++;
                if (selectedIndex >= scrollOffset + maxVisibleLines) scrollOffset = selectedIndex - maxVisibleLines + 1;
            }
            else if (key == ConsoleKey.Enter) { OpenSelected(entries, selectedIndex); break; }
            else if (key == ConsoleKey.Backspace) { GoUp(); break; }
            else if (key == ConsoleKey.C) Copy(entries[selectedIndex]);
            else if (key == ConsoleKey.V) { Paste(); break; }
            else if (key == ConsoleKey.X) Cut(entries[selectedIndex]);
            else if (key == ConsoleKey.D) { Delete(entries[selectedIndex]); break; }
            else if (key == ConsoleKey.N) { CreateFolder(); break; }
            else if (key == ConsoleKey.F) { CreateFile(); break; }
            else if (key == ConsoleKey.R) { Rename(entries[selectedIndex]); break; }
            else if (key == ConsoleKey.Q) Environment.Exit(0);
        }
    }

    static void DrawHeader()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                       КОНСОЛЬНЫЙ ФАЙЛОВЫЙ МЕНЕДЖЕР                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");
    }

    static void DrawFileList(List<(string Name, bool IsDirectory, string FullPath)> entries, int selected, int scroll, int maxLines)
    {
        int startY = 8;
        int endY = Math.Min(startY + maxLines, Console.WindowHeight - 5);

        // Очистка области
        for (int y = startY; y < endY; y++)
        {
            SafeSetCursor(2, y);
            Console.Write(new string(' ', 76));
        }

        int visibleIndex = 0;
        for (int i = scroll; i < entries.Count && visibleIndex < maxLines; i++, visibleIndex++)
        {
            int screenY = startY + visibleIndex;
            if (screenY >= Console.WindowHeight - 5) break;

            SafeSetCursor(2, screenY);

            var entry = entries[i];
            string icon = entry.IsDirectory ? "[Папка]" : "[Файл]";
            string name = entry.Name.Length > 50 ? entry.Name.Substring(0, 47) + "..." : entry.Name;

            if (i == selected)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("→ ");
            }
            else
            {
                Console.Write("  ");
            }

            if (entry.IsDirectory)
                SetColor(ConsoleColor.Blue);
            else if (entry.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                SetColor(ConsoleColor.Green);
            else if (entry.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                         entry.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                SetColor(ConsoleColor.Magenta);
            else
                SetColor(ConsoleColor.Gray);

            Console.Write($"{icon} {name}");
            ResetColor();
            Console.BackgroundColor = ConsoleColor.Black;
        }

        // Скроллбар
        if (entries.Count > maxLines)
        {
            int barY = startY + (selected * maxLines / entries.Count);
            if (barY >= startY && barY < endY)
            {
                SafeSetCursor(0, barY);
                SetColor(ConsoleColor.DarkGray);
                Console.Write("█");
                ResetColor();
            }
        }
    }

    static void DrawHotkeys(int total, int current)
    {
        int y = Console.WindowHeight - 4;
        SafeSetCursor(0, y);
        Console.Write(new string(' ', Console.WindowWidth));
        SafeSetCursor(0, y);
        SetColor(ConsoleColor.DarkGray);
        Console.Write($"[{current}/{total}] ↑↓ Навигация | Enter Открыть | Backspace ↑ | C Копировать | V Вставить | X Вырезать | D Удалить | N Папка | F Файл | R Переименовать | Q Выход");
        ResetColor();
    }

    static void SafeSetCursor(int x, int y)
    {
        try
        {
            if (y >= 0 && y < Console.WindowHeight && x >= 0 && x < Console.WindowWidth)
                Console.SetCursorPosition(x, y);
        }
        catch { }
    }

    static void SafeShowMessage(string msg, ConsoleColor color)
    {
        try
        {
            int y = Console.WindowHeight - 2;
            if (y < 0) y = 0;
            SafeSetCursor(0, y);
            Console.Write(new string(' ', Console.WindowWidth));
            SafeSetCursor(0, y);
            SetColor(color);
            Console.WriteLine(msg);
            ResetColor();
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey(true);
        }
        catch { }
    }

    // === Остальные методы (без изменений) ===
    static List<(string Name, bool IsDirectory, string FullPath)> GetDirectoryEntries()
    {
        var list = new List<(string, bool, string)>();
        try
        {
            var parent = Directory.GetParent(currentPath);
            if (parent != null)
                list.Add(("..", true, parent.FullName));

            var dirs = Directory.GetDirectories(currentPath)
                .Select(d => (Path.GetFileName(d), true, d))
                .OrderBy(x => x.Item1);

            var files = Directory.GetFiles(currentPath)
                .Select(f => (Path.GetFileName(f), false, f))
                .OrderBy(x => x.Item1);

            list.AddRange(dirs);
            list.AddRange(files);
        }
        catch { }
        return list;
    }

    static void OpenSelected(List<(string Name, bool IsDirectory, string FullPath)> entries, int index)
    {
        var selected = entries[index];
        if (selected.IsDirectory)
            currentPath = selected.FullPath;
        else
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = selected.FullPath, UseShellExecute = true }); }
            catch { }
    }

    static void GoUp()
    {
        var parent = Directory.GetParent(currentPath);
        if (parent != null) currentPath = parent.FullName;
    }

    static void Copy((string Name, bool IsDirectory, string FullPath) entry)
    {
        clipboardPath = entry.FullPath;
        isCut = false;
        SafeShowMessage($"Скопировано: {entry.Name}", ConsoleColor.Green);
    }

    static void Cut((string Name, bool IsDirectory, string FullPath) entry)
    {
        clipboardPath = entry.FullPath;
        isCut = true;
        SafeShowMessage($"Вырезано: {entry.Name}", ConsoleColor.Yellow);
    }

    static void Paste()
    {
        if (clipboardPath == null) return;
        try
        {
            string dest = Path.Combine(currentPath, Path.GetFileName(clipboardPath));
            if (File.Exists(clipboardPath))
            {
                File.Copy(clipboardPath, dest, true);
                if (isCut) File.Delete(clipboardPath);
            }
            else if (Directory.Exists(clipboardPath))
            {
                CopyDirectory(clipboardPath, dest);
                if (isCut) Directory.Delete(clipboardPath, true);
            }
            if (isCut) clipboardPath = null;
            SafeShowMessage(isCut ? "Перемещено" : "Вставлено", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            SafeShowMessage("Ошибка: " + ex.Message, ConsoleColor.Red);
        }
    }

    static void Delete((string Name, bool IsDirectory, string FullPath) entry)
    {
        Console.Clear();
        SetColor(ConsoleColor.Red);
        Console.WriteLine($"Удалить \"{entry.Name}\"? (Y/N)");
        ResetColor();
        if (Console.ReadKey(true).Key == ConsoleKey.Y)
        {
            try
            {
                if (entry.IsDirectory)
                    Directory.Delete(entry.FullPath, true);
                else
                    File.Delete(entry.FullPath);
                SafeShowMessage("Удалено", ConsoleColor.Magenta);
            }
            catch (Exception ex)
            {
                SafeShowMessage("Ошибка: " + ex.Message, ConsoleColor.Red);
            }
        }
    }

    static void CreateFolder()
    {
        Console.Clear();
        Console.Write("Имя новой папки: ");
        string name = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(currentPath, name));
                SafeShowMessage("Папка создана", ConsoleColor.Green);
            }
            catch (Exception ex) { SafeShowMessage("Ошибка: " + ex.Message, ConsoleColor.Red); }
        }
    }

    static void CreateFile()
    {
        Console.Clear();
        Console.Write("Имя нового файла: ");
        string name = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            try
            {
                File.Create(Path.Combine(currentPath, name)).Close();
                SafeShowMessage("Файл создана", ConsoleColor.Green);
            }
            catch (Exception ex) { SafeShowMessage("Ошибка: " + ex.Message, ConsoleColor.Red); }
        }
    }

    static void Rename((string Name, bool IsDirectory, string FullPath) entry)
    {
        Console.Clear();
        Console.Write($"Новое имя для \"{entry.Name}\": ");
        string newName = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(newName) && newName != entry.Name)
        {
            try
            {
                string newPath = Path.Combine(Path.GetDirectoryName(entry.FullPath), newName);
                if (entry.IsDirectory)
                    Directory.Move(entry.FullPath, newPath);
                else
                    File.Move(entry.FullPath, newPath);
                SafeShowMessage("Переименовано", ConsoleColor.Cyan);
            }
            catch (Exception ex) { SafeShowMessage("Ошибка: " + ex.Message, ConsoleColor.Red); }
        }
    }

    static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
        foreach (var dir in Directory.GetDirectories(source))
            CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
    }

    static void SetColor(ConsoleColor color) => Console.ForegroundColor = color;
    static void ResetColor() => Console.ResetColor();
}