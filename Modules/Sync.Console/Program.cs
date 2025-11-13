using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

class FolderSynchronizer
{
    private static string sourcePath;
    private static string replicaPath;

    // Только флаг окончания начальной синхронизации
    private static volatile bool isInitialSyncDone = false;

    private const int MaxRetryAttempts = 5;
    private const int RetryDelayMs = 1000;

    // ---- Новое: трекинг внутренних изменений, чтобы не ловить собственные события ----
    private static readonly object InternalChangeLock = new();
    private static readonly HashSet<string> InternalChangePaths =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.WriteLine("Синхронизатор папок 1.7");

        const int maxAttempts = 3;
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            attempt++;

            Console.Write("Введите путь к первой папке: ");
            sourcePath = Console.ReadLine()?.Trim();

            Console.Write("Введите путь ко второй папке: ");
            replicaPath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(replicaPath))
            {
                Console.WriteLine("Ошибка: пути к папкам не могут быть пустыми.");

                if (attempt < maxAttempts)
                {
                    Console.WriteLine($"Попробуйте снова. Осталось попыток: {maxAttempts - attempt}");
                }
                else
                {
                    Console.WriteLine("Превышено количество попыток. Программа завершена.");
                }

                continue;
            }

            try
            {
                sourcePath = Path.GetFullPath(sourcePath);
                replicaPath = Path.GetFullPath(replicaPath);

                if (!Directory.Exists(sourcePath))
                {
                    Console.WriteLine($"Ошибка: папка не существует: {sourcePath}");

                    if (attempt < maxAttempts)
                    {
                        Console.WriteLine($"Попробуйте снова. Осталось попыток: {maxAttempts - attempt}");
                    }
                    else
                    {
                        Console.WriteLine("Превышено количество попыток. Программа завершена.");
                    }

                    continue;
                }

                if (!Directory.Exists(replicaPath))
                {
                    Console.WriteLine($"Ошибка: папка не существует: {replicaPath}");

                    if (attempt < maxAttempts)
                    {
                        Console.WriteLine($"Попробуйте снова. Осталось попыток: {maxAttempts - attempt}");
                    }
                    else
                    {
                        Console.WriteLine("Превышено количество попыток. Программа завершена.");
                    }

                    continue;
                }

                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке пути: {ex.Message}");

                if (attempt < maxAttempts)
                {
                    Console.WriteLine($"Попробуйте снова. Осталось попыток: {maxAttempts - attempt}");
                }
                else
                {
                    Console.WriteLine("Превышено количество попыток. Программа завершена.");
                }
            }
        }

        if (attempt >= maxAttempts &&
            (string.IsNullOrWhiteSpace(sourcePath)
             || !Directory.Exists(sourcePath)
             || !Directory.Exists(replicaPath)))
        {
            Console.WriteLine("Не удалось получить корректные пути.");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
            return;
        }

        try
        {
            Console.WriteLine($"Начальная синхронизация: {sourcePath} → {replicaPath}...");
            SynchronizeFolders(sourcePath, replicaPath);
            isInitialSyncDone = true;
            Console.WriteLine("Начальная синхронизация завершена.");

            using var sourceWatcher = new FileSystemWatcher(sourcePath);

            // Следим только за первой папкой, изменения копируем во вторую
            ConfigureWatcher(sourceWatcher, sourcePath, replicaPath);

            Console.WriteLine("Синхронизация запущена. Нажмите клавишу 'E' для завершения.");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.E)
                {
                    Console.WriteLine("\nВыход по нажатию 'E'. Программа завершена.");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }

    private static void ConfigureWatcher(FileSystemWatcher watcher, string source, string target)
    {
        watcher.NotifyFilter =
            NotifyFilters.FileName |
            NotifyFilters.DirectoryName |
            NotifyFilters.LastWrite |
            NotifyFilters.Size;

        watcher.IncludeSubdirectories = true;
        watcher.Filter = "*.*";

        watcher.Changed += (s, e) => HandleEvent(() => OnFileChanged(e.FullPath, source, target));
        watcher.Created += (s, e) => HandleEvent(() => OnFileCreated(e.FullPath, source, target));
        watcher.Deleted += (s, e) => HandleEvent(() => OnFileDeleted(e.FullPath, source, target));
        watcher.Renamed += (s, e) => HandleEvent(() => OnFileRenamed(e.OldFullPath, e.FullPath, source, target));

        watcher.EnableRaisingEvents = true;

        Console.WriteLine($"Отслеживание запущено для {source}");
    }

    private static void HandleEvent(Action action)
    {
        if (!isInitialSyncDone)
        {
            return;
        }

        try
        {
            action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки события: {ex.Message}");
        }
        finally
        {
            // Небольшая пауза, чтобы сгладить всплеск событий
            Thread.Sleep(100);
        }
    }

    // ----------------- Вспомогательные методы для внутренних изменений -----------------

    private static void MarkInternalChange(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string full = Path.GetFullPath(path);

        lock (InternalChangeLock)
        {
            InternalChangePaths.Add(full);
        }
    }

    private static bool IsInternalChange(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        string full = Path.GetFullPath(path);

        lock (InternalChangeLock)
        {
            return InternalChangePaths.Contains(full);
        }
    }

    private static void UnmarkInternalChange(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string full = Path.GetFullPath(path);

        lock (InternalChangeLock)
        {
            InternalChangePaths.Remove(full);
        }
    }

    private static bool ShouldIgnore(string fullPath)
    {
        // Игнорируем собственные записи
        if (IsInternalChange(fullPath))
        {
            return true;
        }

        // Игнорируем временные/служебные файлы Office
        string fileName = Path.GetFileName(fullPath);

        if (fileName.StartsWith("~$", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    // ----------------- Логика синхронизации -----------------

    private static void SynchronizeFolders(string source, string target)
    {
        try
        {
            Directory.CreateDirectory(target);

            // Копируем/обновляем файлы
            foreach (string sourceFile in Directory.GetFiles(source))
            {
                string relativePath = Path.GetRelativePath(source, sourceFile);
                string targetFile = Path.Combine(target, relativePath);

                CopyFileWithRetry(sourceFile, targetFile);
            }

            // Рекурсивно обрабатываем подкаталоги
            foreach (string sourceDir in Directory.GetDirectories(source))
            {
                string relativePath = Path.GetRelativePath(source, sourceDir);
                string targetDir = Path.Combine(target, relativePath);

                SynchronizeFolders(sourceDir, targetDir);
            }

            // Удаляем лишние файлы в целевой папке
            foreach (string targetFile in Directory.GetFiles(target))
            {
                string relativePath = Path.GetRelativePath(target, targetFile);
                string sourceFile = Path.Combine(source, relativePath);

                if (!File.Exists(sourceFile))
                {
                    DeleteFile(targetFile);
                }
            }

            // Удаляем лишние папки в целевой папке
            foreach (string targetDir in Directory.GetDirectories(target))
            {
                string relativePath = Path.GetRelativePath(target, targetDir);
                string sourceDir = Path.Combine(source, relativePath);

                if (!Directory.Exists(sourceDir))
                {
                    DeleteDirectory(targetDir);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка синхронизации {source} → {target}: {ex.Message}");
        }
    }

    private static void OnFileChanged(string fullPath, string source, string target)
    {
        if (ShouldIgnore(fullPath))
        {
            return;
        }

        string relativePath = Path.GetRelativePath(source, fullPath);
        string targetPath = Path.Combine(target, relativePath);

        if (File.Exists(fullPath))
        {
            CopyFileWithRetry(fullPath, targetPath);
        }
    }

    private static void OnFileCreated(string fullPath, string source, string target)
    {
        if (ShouldIgnore(fullPath))
        {
            return;
        }

        string relativePath = Path.GetRelativePath(source, fullPath);
        string targetPath = Path.Combine(target, relativePath);

        if (File.Exists(fullPath))
        {
            CopyFileWithRetry(fullPath, targetPath);
        }
        else if (Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(targetPath);
            Console.WriteLine($"Создана папка: {targetPath}");
        }
    }

    private static void OnFileDeleted(string fullPath, string source, string target)
    {
        if (ShouldIgnore(fullPath))
        {
            return;
        }

        string relativePath = Path.GetRelativePath(source, fullPath);
        string targetPath = Path.Combine(target, relativePath);

        if (File.Exists(targetPath))
        {
            DeleteFile(targetPath);
        }
        else if (Directory.Exists(targetPath))
        {
            DeleteDirectory(targetPath);
        }
    }

    private static void OnFileRenamed(string oldFullPath, string newFullPath, string source, string target)
    {
        // Если и старый, и новый — временные/внутренние, то просто игнорируем
        if (ShouldIgnore(oldFullPath) && ShouldIgnore(newFullPath))
        {
            return;
        }

        string oldRelativePath = Path.GetRelativePath(source, oldFullPath);
        string newRelativePath = Path.GetRelativePath(source, newFullPath);

        string oldTargetPath = Path.Combine(target, oldRelativePath);
        string newTargetPath = Path.Combine(target, newRelativePath);

        // Если был файл с предыдущим именем — удаляем
        if (File.Exists(oldTargetPath))
        {
            DeleteFile(oldTargetPath);
        }
        else if (Directory.Exists(oldTargetPath))
        {
            DeleteDirectory(oldTargetPath);
        }

        // Если новый объект — файл, копируем его в новое место
        if (File.Exists(newFullPath))
        {
            CopyFileWithRetry(newFullPath, newTargetPath);
        }
        else if (Directory.Exists(newFullPath))
        {
            Directory.CreateDirectory(newTargetPath);
            Console.WriteLine($"Переименована папка: {oldTargetPath} → {newTargetPath}");
        }
    }

    private static void CopyFileWithRetry(string sourceFile, string targetFile)
    {
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                string? dir = Path.GetDirectoryName(targetFile);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Помечаем файл как внутреннее изменение, чтобы не ловить свои события
                MarkInternalChange(targetFile);

                try
                {
                    using (FileStream sourceStream = File.Open(
                               sourceFile,
                               FileMode.Open,
                               FileAccess.Read,
                               FileShare.ReadWrite))
                    using (FileStream targetStream = File.Open(
                               targetFile,
                               FileMode.Create,
                               FileAccess.Write,
                               FileShare.None))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;

                        while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            targetStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
                finally
                {
                    // С небольшим запасом держим файл помеченным как внутренний
                    Thread.Sleep(50);
                    UnmarkInternalChange(targetFile);
                }

                Console.WriteLine($"Скопировано: {sourceFile} → {targetFile}");
                return;
            }
            catch (IOException ex) when (ex.Message.Contains("being used by another process"))
            {
                Console.WriteLine($"Файл {sourceFile} заблокирован, попытка {attempt}/{MaxRetryAttempts}...");

                if (attempt < MaxRetryAttempts)
                {
                    Thread.Sleep(RetryDelayMs);
                }
                else
                {
                    Console.WriteLine($"Ошибка копирования {sourceFile}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка копирования {sourceFile}: {ex.Message}");
                return;
            }
        }
    }

    private static void DeleteFile(string file)
    {
        try
        {
            File.Delete(file);
            Console.WriteLine($"Удалено: {file}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка удаления {file}: {ex.Message}");
        }
    }

    private static void DeleteDirectory(string dir)
    {
        try
        {
            Directory.Delete(dir, true);
            Console.WriteLine($"Удалена папка: {dir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка удаления папки {dir}: {ex.Message}");
        }
    }
}
