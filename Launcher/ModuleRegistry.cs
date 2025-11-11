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
                    description: "Консольный файловый менеджер (Задание 1)",
                    relativeExePath: @"FileManager.Console\bin\Debug\net8.0\FileManager.Console.exe"),

                new ModuleDescriptor(
                    id: "Encryptor.Console",
                    name: "Шифровщик/дешифровщик",
                    description: "Шифровщик/дешифровщик по 6 разным шифрам (Задание 1)",
                    relativeExePath: @"Encryptor.Console\bin\Debug\net8.0\Encryptor.Console.exe"),

                new ModuleDescriptor(
                    id: "Sync.Console",
                    name: "Синхронизатор",
                    description: "Синхронизатор двух папок (Удаление/Добавление файла) (Задание 1)",
                    relativeExePath: @"Sync.Console\bin\Debug\net8.0\Sync.Console.exe")


            };
        }
    }
}
