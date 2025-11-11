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
                    relativeExePath: @"FileManager.Console\bin\Debug\net8.0\FileManager.Console.exe"),

                new ModuleDescriptor(
                    id: "Encryptor.Console",
                    name: "Шифровщик/дешифровщик (заготовка)",
                    description: "Пустой модуль для демонстрации подключения.",
                    relativeExePath: @"Encryptor.Console\bin\Debug\net8.0\Encryptor.Console.exe"),

                new ModuleDescriptor(
                    id: "Sync.Console",
                    name: "Синхронизатор (заготовка)",
                    description: "Пустой модуль для демонстрации подключения.",
                    relativeExePath: @"Sync.Console\bin\Debug\net8.0\Sync.Console.exe")
            };
        }
    }
}
