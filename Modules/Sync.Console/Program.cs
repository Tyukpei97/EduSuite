using System;
using System.IO;
using System.Text;
using System.Threading;

class FolderSynchronizer
{
    private static string sourcePath;
    private static string replicaPath;
    private static volatile bool isInitialSyncDone = false;
    private static volatile bool isSyncing = false;
    private const int MaxRetryAttempts = 5;
    private const int RetryDelayMs = 1000;

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.WriteLine("Синхронизатор папок 1.6");

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
                    Console.WriteLine($"Попробуйте снова. Осталось попыток: {maxAttempts - attempt}");
                else
                    Console.WriteLine("Превышено количество попыток. Программа завершена.");
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
                        Console.WriteLine($"Попробуйте снова. Осталось попыток: {maxAttempts - attempt}");
                    else
                        Console.WriteLine("Превышено количество попыток. Программа завершена.");
                    continue;
                }

                if (!Directory.Exists(replicaPath))
                {
                    Console.WriteLine($"Ошибка: папка не существует: {replicaPath}");
                    if (attempt < maxAttempts)
                        Console.WriteLine($"Попробуйте снова. Осталось попыток: {maxAttempts - attempt}");
                    else
                        Console.WriteLine("Превышено количество попыток. Программа завершена.");
                    continue;
                }

                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке пути: {ex.Message}");
                if (attempt < maxAttempts)
                    Console.WriteLine($"Попробуйте снова. Осталось попыток: {maxAttempts - attempt}");
                else
                    Console.WriteLine("Превышено количество попыток. Программа завершена.");
            }
        }

        if (attempt >= maxAttempts && (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath) || !Directory.Exists(replicaPath)))
        {
            Console.WriteLine("Не удалось получить корректные пути. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
            return;
        }

        try
        {
            isSyncing = true;
            Console.WriteLine($"Начальная синхронизация: {sourcePath} → {replicaPath}...");
            SynchronizeFolders(sourcePath, replicaPath);
            isInitialSyncDone = true;
            isSyncing = false;
            Console.WriteLine("Начальная синхронизация завершена.");

            using var sourceWatcher = new FileSystemWatcher(sourcePath);
            using var replicaWatcher = new FileSystemWatcher(replicaPath);
            ConfigureWatcher(sourceWatcher, sourcePath, replicaPath);
            ConfigureWatcher(replicaWatcher, replicaPath, sourcePath);

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
        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size;
        watcher.IncludeSubdirectories = true;
        watcher.Changed += (s, e) => HandleEvent(() => OnFileChanged(e.FullPath, source, target));
        watcher.Created += (s, e) => HandleEvent(() => OnFileCreated(e.FullPath, source, target));
        watcher.Deleted += (s, e) => HandleEvent(() => OnFileDeleted(e.FullPath, source, target));
        watcher.Renamed += (s, e) => HandleEvent(() => OnFileRenamed(e.OldFullPath, e.FullPath, source, target));
        watcher.EnableRaisingEvents = true;
        Console.WriteLine($"Отслеживание запущено для {source}");
    }

    private static void HandleEvent(Action action)
    {
        if (isSyncing || !isInitialSyncDone) return;
        isSyncing = true;
        try
        {
            action();
        }
        finally
        {
            isSyncing = false;
            Thread.Sleep(500);
        }
    }

    private static void SynchronizeFolders(string source, string target)
    {
        try
        {
            Directory.CreateDirectory(target);
            foreach (string sourceFile in Directory.GetFiles(source))
            {
                string relativePath = Path.GetRelativePath(source, sourceFile);
                string targetFile = Path.Combine(target, relativePath);
                CopyFileWithRetry(sourceFile, targetFile);
            }
            foreach (string sourceDir in Directory.GetDirectories(source))
            {
                string relativePath = Path.GetRelativePath(source, sourceDir);
                string targetDir = Path.Combine(target, relativePath);
                SynchronizeFolders(sourceDir, targetDir);
            }
            foreach (string targetFile in Directory.GetFiles(target))
            {
                string relativePath = Path.GetRelativePath(target, targetFile);
                string sourceFile = Path.Combine(source, relativePath);
                if (!File.Exists(sourceFile))
                    DeleteFile(targetFile);
            }
            foreach (string targetDir in Directory.GetDirectories(target))
            {
                string relativePath = Path.GetRelativePath(target, targetDir);
                string sourceDir = Path.Combine(source, relativePath);
                if (!Directory.Exists(sourceDir))
                    DeleteDirectory(targetDir);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка синхронизации {source} → {target}: {ex.Message}");
        }
    }

    private static void OnFileChanged(string fullPath, string source, string target)
    {
        string relativePath = Path.GetRelativePath(source, fullPath);
        string targetPath = Path.Combine(target, relativePath);
        if (File.Exists(fullPath))
            CopyFileWithRetry(fullPath, targetPath);
    }

    private static void OnFileCreated(string fullPath, string source, string target)
    {
        string relativePath = Path.GetRelativePath(source, fullPath);
        string targetPath = Path.Combine(target, relativePath);
        if (File.Exists(fullPath))
            CopyFileWithRetry(fullPath, targetPath);
        else if (Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(targetPath);
            Console.WriteLine($"Создана папка: {targetPath}");
        }
    }

    private static void OnFileDeleted(string fullPath, string source, string target)
    {
        string relativePath = Path.GetRelativePath(source, fullPath);
        string targetPath = Path.Combine(target, relativePath);
        if (File.Exists(targetPath))
            DeleteFile(targetPath);
        else if (Directory.Exists(targetPath))
            DeleteDirectory(targetPath);
    }

    private static void OnFileRenamed(string oldFullPath, string newFullPath, string source, string target)
    {
        string oldRelativePath = Path.GetRelativePath(source, oldFullPath);
        string newRelativePath = Path.GetRelativePath(source, newFullPath);
        string oldTargetPath = Path.Combine(target, oldRelativePath);
        string newTargetPath = Path.Combine(target, newRelativePath);
        if (File.Exists(oldTargetPath))
        {
            DeleteFile(oldTargetPath);
            CopyFileWithRetry(newFullPath, newTargetPath);
        }
        else if (Directory.Exists(oldTargetPath))
        {
            DeleteDirectory(oldTargetPath);
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
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                using (FileStream sourceStream = File.Open(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream targetStream = File.Open(targetFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        targetStream.Write(buffer, 0, bytesRead);
                    }
                }
                Console.WriteLine($"Скопировано: {sourceFile} → {targetFile}");
                return;
            }
            catch (IOException ex) when (ex.Message.Contains("being used by another process"))
            {
                Console.WriteLine($"Файл {sourceFile} заблокирован, попытка {attempt}/{MaxRetryAttempts}...");
                if (attempt < MaxRetryAttempts)
                    Thread.Sleep(RetryDelayMs);
                else
                    Console.WriteLine($"Ошибка копирования {sourceFile}: {ex.Message}");
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