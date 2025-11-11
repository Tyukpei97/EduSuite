using System;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Шифровщик/Дешифровшик 2.0");
            Console.WriteLine("\n1. Шифровать\n2. Дешифровать\n3. Выход");
            Console.Write("Ваш выбор: ");
            string choice = Console.ReadLine();
            if (choice == "3") break;
            if (choice != "1" && choice != "2") { Console.WriteLine("Неверный выбор."); continue; }

            bool encrypt = choice == "1";

            Console.WriteLine("\nШифры:\n1. Цезаря\n2. Виженера\n3. Замены\n4. Перестановки\n5. XOR\n6. Атбаш");
            Console.Write("Выбор шифра: ");
            string cipher = Console.ReadLine();

            Console.Write("Введите текст: ");
            string text = Console.ReadLine();
            string key = cipher != "6" ? GetInput("Ключ: ") : "";

            try
            {
                string result = cipher switch
                {
                    "1" => encrypt ? CaesarCipher.Encrypt(text, int.Parse(GetInput("Сдвиг: "))) : CaesarCipher.Decrypt(text, int.Parse(GetInput("Сдвиг: "))),
                    "2" => encrypt ? VigenereCipher.Encrypt(text, ValidateKey(key)) : VigenereCipher.Decrypt(text, ValidateKey(key)),
                    "3" => encrypt ? SubstitutionCipher.Encrypt(text, key) : SubstitutionCipher.Decrypt(text, key),
                    "4" => encrypt ? TranspositionCipher.Encrypt(text, key) : TranspositionCipher.Decrypt(text, key),
                    "5" => encrypt ? XORCipher.Encrypt(text, key) : XORCipher.Decrypt(text, key),
                    "6" => AtbashCipher.Transform(text),
                    _ => throw new Exception("Неверный шифр.")
                };
                Console.WriteLine($"Результат: {result}");
            }
            catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
        }
    }

    static string GetInput(string prompt) { Console.Write(prompt); return Console.ReadLine(); }

    static string ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new Exception("Ключ не может быть пустым.");
        if (!key.All(c => (c >= 'А' && c <= 'Я' || c == 'Ё') || (c >= 'A' && c <= 'Z'))) throw new Exception("Ключ должен содержать только кириллические или латинские буквы.");
        return key.ToUpper();
    }
}




// 1. Шифр Цезаря
public static class CaesarCipher
{
    private static readonly string cyrillicAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
    private static readonly string latinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Encrypt(string text, int shift)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return new string(text.Select(c =>
        {
            string alphabet = (c >= 'А' && c <= 'Я' || c == 'Ё' || c >= 'а' && c <= 'я' || c == 'ё') ? cyrillicAlphabet : latinAlphabet;
            int index = alphabet.IndexOf(char.ToUpper(c));
            if (index >= 0)
            {
                shift = shift % alphabet.Length;
                char newChar = alphabet[(index + shift) % alphabet.Length];
                return char.IsUpper(c) ? newChar : char.ToLower(newChar);
            }
            return c;
        }).ToArray());
    }

    public static string Decrypt(string text, int shift)
    {
        return Encrypt(text, -shift);
    }
}




// 2. Шифр Виженера
public static class VigenereCipher
{
    private static readonly string cyrillicAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
    private static readonly string latinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Encrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        StringBuilder result = new StringBuilder();
        for (int i = 0, j = 0; i < text.Length; i++)
        {
            string alphabet = (text[i] >= 'А' && text[i] <= 'Я' || text[i] == 'Ё' || text[i] >= 'а' && text[i] <= 'я' || text[i] == 'ё') ? cyrillicAlphabet : latinAlphabet;
            int index = alphabet.IndexOf(char.ToUpper(text[i]));
            if (index >= 0)
            {
                int shift = alphabet.IndexOf(key[j++ % key.Length]);
                if (shift < 0) shift = 0;
                char newChar = alphabet[(index + shift) % alphabet.Length];
                result.Append(char.IsUpper(text[i]) ? newChar : char.ToLower(newChar));
            }
            else result.Append(text[i]);
        }
        return result.ToString();
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        StringBuilder result = new StringBuilder();
        for (int i = 0, j = 0; i < text.Length; i++)
        {
            string alphabet = (text[i] >= 'А' && text[i] <= 'Я' || text[i] == 'Ё' || text[i] >= 'а' && text[i] <= 'я' || text[i] == 'ё') ? cyrillicAlphabet : latinAlphabet;
            int index = alphabet.IndexOf(char.ToUpper(text[i]));
            if (index >= 0)
            {
                int shift = alphabet.IndexOf(key[j++ % key.Length]);
                if (shift < 0) shift = 0;
                char newChar = alphabet[(index - shift + alphabet.Length) % alphabet.Length];
                result.Append(char.IsUpper(text[i]) ? newChar : char.ToLower(newChar));
            }
            else result.Append(text[i]);
        }
        return result.ToString();
    }
}




// 3. Шифр замены
public static class SubstitutionCipher
{
    private static readonly char[] cyrillicAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ".ToCharArray();
    private static readonly char[] latinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    private static (char[], char[]) CreateSubstitutionMap(string key)
    {
        if (string.IsNullOrEmpty(key)) return (cyrillicAlphabet, latinAlphabet);
        Random rand = new Random(key.GetHashCode());
        return (cyrillicAlphabet.OrderBy(_ => rand.Next()).ToArray(), latinAlphabet.OrderBy(_ => rand.Next()).ToArray());
    }

    public static string Encrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        var (cyrillicMap, latinMap) = CreateSubstitutionMap(key);
        return new string(text.Select(c =>
        {
            char[] alphabet = (c >= 'А' && c <= 'Я' || c == 'Ё' || c >= 'а' && c <= 'я' || c == 'ё') ? cyrillicAlphabet : latinAlphabet;
            char[] map = (c >= 'А' && c <= 'Я' || c == 'Ё' || c >= 'а' && c <= 'я' || c == 'ё') ? cyrillicMap : latinMap;
            int index = Array.IndexOf(alphabet, char.ToUpper(c));
            if (index >= 0)
                return char.IsUpper(c) ? map[index] : char.ToLower(map[index]);
            return c;
        }).ToArray());
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        var (cyrillicMap, latinMap) = CreateSubstitutionMap(key);
        char[] cyrillicReverse = new char[cyrillicAlphabet.Length];
        char[] latinReverse = new char[latinAlphabet.Length];
        for (int i = 0; i < cyrillicAlphabet.Length; i++)
            cyrillicReverse[Array.IndexOf(cyrillicMap, cyrillicAlphabet[i])] = cyrillicAlphabet[i];
        for (int i = 0; i < latinAlphabet.Length; i++)
            latinReverse[Array.IndexOf(latinMap, latinAlphabet[i])] = latinAlphabet[i];

        return new string(text.Select(c =>
        {
            char[] alphabet = (c >= 'А' && c <= 'Я' || c == 'Ё' || c >= 'а' && c <= 'я' || c == 'ё') ? cyrillicAlphabet : latinAlphabet;
            char[] map = (c >= 'А' && c <= 'Я' || c == 'Ё' || c >= 'а' && c <= 'я' || c == 'ё') ? cyrillicReverse : latinReverse;
            int index = Array.IndexOf(alphabet, char.ToUpper(c));
            if (index >= 0)
                return char.IsUpper(c) ? map[index] : char.ToLower(map[index]);
            return c;
        }).ToArray());
    }
}




// 4. Шифр перестановки
public static class TranspositionCipher
{
    public static string Encrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        int cols = key.Length, rows = (int)Math.Ceiling((double)text.Length / cols);
        char[,] grid = new char[rows, cols];
        for (int i = 0, r = 0; r < rows; r++)
            for (int c = 0; c < cols && i < text.Length; c++)
                grid[r, c] = text[i++];

        StringBuilder result = new StringBuilder();
        foreach (int col in key.Select((c, i) => (c, i)).OrderBy(x => x.c).Select(x => x.i))
            for (int r = 0; r < rows; r++)
                if (grid[r, col] != '\0')
                    result.Append(grid[r, col]);
        return result.ToString();
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        int cols = key.Length, rows = (int)Math.Ceiling((double)text.Length / cols);
        if (text.Length > rows * cols) throw new Exception("Длина текста превышает размер таблицы.");

        int[] colLengths = new int[cols];
        int remaining = text.Length;
        for (int i = 0; i < cols; i++)
        {
            colLengths[i] = remaining >= rows ? rows : remaining;
            remaining -= colLengths[i];
        }

        char[,] grid = new char[rows, cols];
        int textIndex = 0;
        foreach (int col in key.Select((c, i) => (c, i)).OrderBy(x => x.c).Select(x => x.i))
            for (int r = 0; r < colLengths[col] && textIndex < text.Length; r++)
                grid[r, col] = text[textIndex++];

        StringBuilder result = new StringBuilder();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (grid[r, c] != '\0')
                    result.Append(grid[r, c]);
        return result.ToString();
    }
}




// 5. Шифр XOR
public static class XORCipher
{
    public static string Encrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        return Convert.ToBase64String(bytes.Select((b, i) => (byte)(b ^ keyBytes[i % keyBytes.Length])).ToArray());
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        try
        {
            byte[] bytes = Convert.FromBase64String(text);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            return Encoding.UTF8.GetString(bytes.Select((b, i) => (byte)(b ^ keyBytes[i % keyBytes.Length])).ToArray());
        }
        catch { return "Ошибка декодирования."; }
    }
}




// 6. Шифр Атбаш
public static class AtbashCipher
{
    private static readonly char[] cyrillicAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ".ToCharArray();
    private static readonly char[] cyrillicReversed = cyrillicAlphabet.Reverse().ToArray();
    private static readonly char[] latinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] latinReversed = latinAlphabet.Reverse().ToArray();

    public static string Transform(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return new string(text.Select(c =>
        {
            char[] alphabet = (c >= 'А' && c <= 'Я' || c == 'Ё' || c >= 'а' && c <= 'я' || c == 'ё') ? cyrillicAlphabet : latinAlphabet;
            char[] reversed = (c >= 'А' && c <= 'Я' || c == 'Ё' || c >= 'а' && c <= 'я' || c == 'ё') ? cyrillicReversed : latinReversed;
            int index = Array.IndexOf(alphabet, char.ToUpper(c));
            if (index >= 0)
                return char.IsUpper(c) ? reversed[index] : char.ToLower(reversed[index]);
            return c;
        }).ToArray());
    }
}