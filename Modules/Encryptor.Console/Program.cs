using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace FileEncrypter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Шифровщик/Дешифровщик текстовых файлов");
            Console.WriteLine("-------------------------------------");

            while (true)
            {
                Console.Write("Введите путь к файлу (или '0' для выхода): ");
                string filePath = Console.ReadLine();

                if (filePath == "0")
                {
                    Console.WriteLine("Программа завершена.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    Console.WriteLine("Путь не может быть пустым.");
                    continue;
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Файл не существует!");
                    continue;
                }

                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Шифровать");
                Console.WriteLine("2. Дешифровать");

                string action = Console.ReadLine();

                if (action != "1" && action != "2")
                {
                    Console.WriteLine("Неверный выбор действия!");
                    continue;
                }

                Console.WriteLine("Выберите тип шифрования:");
                Console.WriteLine("1. Шифр Цезаря");
                Console.WriteLine("2. Шифр Виженера");
                Console.WriteLine("3. XOR-шифрование");
                Console.WriteLine("4. Base64 (кодирование/декодирование)");
                Console.WriteLine("5. Шифр замены символов");
                Console.WriteLine("6. Шифр перестановки");

                string encryptionType = Console.ReadLine();

                string result = string.Empty;

                try
                {
                    // ВАЖНО: читаем файл ПЕРЕД шифровкой/дешифровкой,
                    // уже после того как пользователь выбрал действие и тип шифра.
                    string content;

                    try
                    {
                        content = File.ReadAllText(filePath, Encoding.UTF8);

                        if (string.IsNullOrEmpty(content))
                        {
                            Console.WriteLine("Файл пустой!");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка чтения файла: {ex.Message}");
                        continue;
                    }

                    switch (encryptionType)
                    {
                        case "1":
                            {
                                Console.Write("Введите сдвиг (целое число): ");
                                if (!int.TryParse(Console.ReadLine(), out int shift))
                                {
                                    Console.WriteLine("Неверный формат сдвига! Требуется целое число.");
                                    continue;
                                }

                                result = action == "1"
                                    ? CaesarCipher.Encrypt(content, shift)
                                    : CaesarCipher.Decrypt(content, shift);

                                break;
                            }

                        case "2":
                            {
                                Console.Write("Введите ключ (строка, только буквы): ");
                                string vigenereKey = Console.ReadLine();

                                if (string.IsNullOrEmpty(vigenereKey) ||
                                    !vigenereKey.All(char.IsLetter))
                                {
                                    Console.WriteLine("Ключ должен быть непустой строкой, содержащей только буквы!");
                                    continue;
                                }

                                result = action == "1"
                                    ? VigenereCipher.Encrypt(content, vigenereKey)
                                    : VigenereCipher.Decrypt(content, vigenereKey);

                                break;
                            }

                        case "3":
                            {
                                Console.Write("Введите ключ (строка): ");
                                string xorKey = Console.ReadLine();

                                if (string.IsNullOrEmpty(xorKey))
                                {
                                    Console.WriteLine("Ключ не может быть пустым!");
                                    continue;
                                }

                                result = XORCipher.EncryptDecrypt(content, xorKey);
                                break;
                            }

                        case "4":
                            {
                                result = action == "1"
                                    ? Base64Cipher.Encrypt(content)
                                    : Base64Cipher.Decrypt(content);

                                break;
                            }

                        case "5":
                            {
                                Console.Write("Введите ключ (26 уникальных букв): ");
                                string substitutionKey = Console.ReadLine();

                                if (string.IsNullOrEmpty(substitutionKey) ||
                                    substitutionKey.Length != 26 ||
                                    !substitutionKey.All(char.IsLetter) ||
                                    substitutionKey.ToLower().Distinct().Count() != 26)
                                {
                                    Console.WriteLine("Ключ должен содержать ровно 26 уникальных букв!");
                                    continue;
                                }

                                result = action == "1"
                                    ? SubstitutionCipher.Encrypt(content, substitutionKey)
                                    : SubstitutionCipher.Decrypt(content, substitutionKey);

                                break;
                            }

                        case "6":
                            {
                                Console.Write("Введите ключ (числа через пробел, например, 2 1 3): ");
                                string permutationKey = Console.ReadLine();

                                if (string.IsNullOrEmpty(permutationKey))
                                {
                                    Console.WriteLine("Ключ не может быть пустым!");
                                    continue;
                                }

                                try
                                {
                                    int[] permutation = Array.ConvertAll(
                                        permutationKey.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                                        int.Parse);

                                    if (permutation.Length == 0 ||
                                        permutation.Any(x => x < 1 || x > permutation.Length) ||
                                        permutation.Distinct().Count() != permutation.Length)
                                    {
                                        Console.WriteLine("Ключ должен содержать уникальные числа от 1 до длины ключа!");
                                        continue;
                                    }

                                    result = action == "1"
                                        ? PermutationCipher.Encrypt(content, permutation)
                                        : PermutationCipher.Decrypt(content, permutation);
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine("Ключ должен содержать только числа, разделенные пробелами!");
                                    continue;
                                }

                                break;
                            }

                        default:
                            {
                                Console.WriteLine("Неверный тип шифрования!");
                                continue;
                            }
                    }

                    try
                    {
                        File.WriteAllText(filePath, result, Encoding.UTF8);
                        Console.WriteLine("Операция выполнена успешно! Файл обновлен.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка записи в файл: {ex.Message}");
                        continue;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Хотите продолжить работу? (1 - да, 0 - выйти)");
                    string continueChoice = Console.ReadLine();

                    if (continueChoice == "0")
                    {
                        Console.WriteLine("Программа завершена.");
                        break;
                    }

                    Console.WriteLine("-------------------------------------");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    Console.WriteLine();
                    Console.WriteLine("Хотите продолжить работу? (1 - да, 0 - выйти)");

                    string continueChoice = Console.ReadLine();

                    if (continueChoice == "0")
                    {
                        Console.WriteLine("Программа завершена.");
                        break;
                    }

                    Console.WriteLine("-------------------------------------");
                }
            }
        }
    }

    // Шифр Цезаря
    static class CaesarCipher
    {
        public static string Encrypt(string text, int shift)
        {
            return Transform(text, shift);
        }

        public static string Decrypt(string text, int shift)
        {
            return Transform(text, -shift);
        }

        private static string Transform(string text, int shift)
        {
            char[] result = new char[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    char baseChar = char.IsUpper(text[i]) ? 'A' : 'a';
                    result[i] = (char)(baseChar +
                                       (text[i] - baseChar + shift + 26) % 26);
                }
                else
                {
                    result[i] = text[i];
                }
            }

            return new string(result);
        }
    }

    // Шифр Виженера
    static class VigenereCipher
    {
        public static string Encrypt(string text, string key)
        {
            return Transform(text, key, true);
        }

        public static string Decrypt(string text, string key)
        {
            return Transform(text, key, false);
        }

        private static string Transform(string text, string key, bool encrypt)
        {
            char[] result = new char[text.Length];
            key = key.ToUpper();
            int keyIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    char baseChar = char.IsUpper(text[i]) ? 'A' : 'a';
                    int keyShift = key[keyIndex % key.Length] - 'A';
                    int shift = encrypt ? keyShift : -keyShift;

                    result[i] = (char)(baseChar +
                                       (text[i] - baseChar + shift + 26) % 26);

                    keyIndex++;
                }
                else
                {
                    result[i] = text[i];
                }
            }

            return new string(result);
        }
    }

    // XOR-шифрование
    static class XORCipher
    {
        public static string EncryptDecrypt(string text, string key)
        {
            char[] result = new char[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                result[i] = (char)(text[i] ^ key[i % key.Length]);
            }

            return new string(result);
        }
    }

    // Base64
    static class Base64Cipher
    {
        public static string Encrypt(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        public static string Decrypt(string text)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(text);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                throw new Exception("Неверный формат Base64 для дешифрования!");
            }
        }
    }

    // Шифр замены символов
    static class SubstitutionCipher
    {
        public static string Encrypt(string text, string key)
        {
            Dictionary<char, char> mapping = CreateMapping(key, true);
            return Transform(text, mapping);
        }

        public static string Decrypt(string text, string key)
        {
            Dictionary<char, char> mapping = CreateMapping(key, false);
            return Transform(text, mapping);
        }

        private static Dictionary<char, char> CreateMapping(string key, bool encrypt)
        {
            Dictionary<char, char> mapping = new Dictionary<char, char>();
            string alphabet = "abcdefghijklmnopqrstuvwxyz";

            key = key.ToLower();

            for (int i = 0; i < 26; i++)
            {
                mapping[encrypt ? alphabet[i] : key[i]] =
                    encrypt ? key[i] : alphabet[i];
            }

            return mapping;
        }

        private static string Transform(string text, Dictionary<char, char> mapping)
        {
            char[] result = new char[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    char lowerChar = char.ToLower(text[i]);
                    result[i] = char.IsUpper(text[i])
                        ? char.ToUpper(mapping[lowerChar])
                        : mapping[lowerChar];
                }
                else
                {
                    result[i] = text[i];
                }
            }

            return new string(result);
        }
    }

    // Шифр перестановки
    static class PermutationCipher
    {
        public static string Encrypt(string text, int[] permutation)
        {
            return Transform(text, permutation);
        }

        public static string Decrypt(string text, int[] permutation)
        {
            int[] inverse = new int[permutation.Length];

            for (int i = 0; i < permutation.Length; i++)
            {
                inverse[permutation[i] - 1] = i + 1;
            }

            return Transform(text, inverse);
        }

        private static string Transform(string text, int[] permutation)
        {
            int blockSize = permutation.Length;
            char[] result = new char[text.Length];

            for (int i = 0; i < text.Length; i += blockSize)
            {
                int remaining = Math.Min(blockSize, text.Length - i);
                char[] block = new char[blockSize];

                Array.Fill(block, ' ');

                for (int j = 0; j < remaining; j++)
                {
                    block[j] = text[i + j];
                }

                for (int j = 0; j < blockSize; j++)
                {
                    int pos = permutation[j] - 1;

                    if (pos < blockSize && i + j < text.Length)
                    {
                        result[i + j] = block[pos];
                    }
                }
            }

            return new string(result);
        }
    }
}