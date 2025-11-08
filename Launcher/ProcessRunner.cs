using System;
using System.Diagnostics;
using System.IO;  

namespace Launcher
{
    public static class ProcessRunner
    {


        /// <summary>
        /// На вход метод получает папку с exe файлом лаунчера. Ставит там метку старта и после начинается цикл в 6 итераций, чтобы найти папку с именем Modules.
        /// Как только мы его находим, цикл возвращает полный путь к корню решения, где, собственно и лежит Modules.(Ну, или где бы он не находился.)
        /// </summary>
        private static string? TryFindSolutionRoot(string startDir)
        {
            var dir = new DirectoryInfo(startDir);

            for (int i = 0; i < 6 && dir != null; i++)
            {
                var modules = new DirectoryInfo(Path.Combine(dir.FullName, "Modules"));
                if (modules.Exists)
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            return null;
        }
        private static string? TryFindExe(string startDir, string modulePath)
        {
            var dir = new DirectoryInfo(startDir);
            var next = Path.Combine(dir.FullName, "Modules", modulePath);
            return Path.GetFullPath(next);
        }
        public static Process StartFor(ModuleDescriptor descriptor, string baseDir)
        {

            string? exeFullPath, solutionRoot;

            solutionRoot = TryFindSolutionRoot(baseDir);
            exeFullPath = TryFindExe(solutionRoot, descriptor.RelativeExePath); // сюда запишем полный путь к exe.

            if (File.Exists(exeFullPath)) // проверяем наличие файла.
            {
                var psi = new ProcessStartInfo // инструкция к запуску.
                {
                    FileName = exeFullPath,
                    /// <summary>
                    /// В общем, здесь мы устанавливаем рабочую папку процесса. Консольные программы читают и пишут файлы рядом с собой и могут использовать относительные путь.
                    /// Нам такой радости не надо и пусть лучше мы установим рабочее пространство на такой случай, иначе поведение программы может быть плачевным.
                    /// В приоритет ставим папку модуля, но если не удалось найти, то назначаем папку лаунчера.
                    /// <summary>
                    WorkingDirectory = Path.GetDirectoryName(exeFullPath) ?? baseDir,   
                    UseShellExecute = true
                };

                var p = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };

                if (!p.Start())
                {
                    throw new InvalidOperationException("Процесс не был запущен по неизвестной причине.");
                }

                return p;
            }

            if (solutionRoot is not null)
            {
                string csproj = Path.Combine(solutionRoot, "Modules", descriptor.Id, $"{descriptor.Id}.csproj");
                if (File.Exists(csproj))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{csproj}\"",
                        WorkingDirectory = Path.GetDirectoryName(csproj) ?? solutionRoot,
                        UseShellExecute = true
                    };

                    var p = new Process
                    {
                        StartInfo = psi,
                        EnableRaisingEvents = true
                    };
                    if (!p.Start())
                    {
                        throw new InvalidOperationException("Процесс `dotnet run` не был запущен.");
                    }
                    return p;
                }
            }
            throw new FileNotFoundException(
                $"Не найден исполняемый файл модуля. \nОжидался путь: {exeFullPath}\n" +
                $"Также не найден проект для dev-запуска (Modules\\{descriptor.Id}\\{descriptor.Id}.csproj).");
        }

    }
}
