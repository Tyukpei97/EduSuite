using System;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n1. Шифровать\n2. Дешифровать\n3. Выход");
            Console.Write("Выбор: ");
            string choice = Console.ReadLine();
            if (choice == "3") break;
            if (choice != "1" && choice != "2") { Console.WriteLine("Неверный выбор."); continue; }

            bool encrypt = choice == "1";

            Console.WriteLine("\nШифры:\n1. Цезаря\n2. Виженера\n3. Замены\n4. Перестановки\n5. XOR\n6. Атбаш (Латиница)");
            Console.Write("Выбор шифра: ");
            string cipher = Console.ReadLine();

            Console.Write("Текст: ");
            string text = Console.ReadLine();
            string key = cipher != "6" ? GetInput("Ключ: ") : "";

            try
            {
                string result = cipher switch
                {
                    "1" => encrypt ? Caesar.Encrypt(text, int.Parse(GetInput("Сдвиг: "))) : Caesar.Decrypt(text, int.Parse(GetInput("Сдвиг: "))),
                    "2" => encrypt ? Vigenere.Encrypt(text, key) : Vigenere.Decrypt(text, key),
                    "3" => encrypt ? Substitution.Encrypt(text, key) : Substitution.Decrypt(text, key),
                    "4" => encrypt ? Transposition.Encrypt(text, key) : Transposition.Decrypt(text, key),
                    "5" => encrypt ? XOR.Encrypt(text, key) : XOR.Decrypt(text, key),
                    "6" => Atbash.Transform(text),
                    _ => throw new Exception("Неверный шифр.")
                };
                Console.WriteLine($"Результат: {result}");
            }
            catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
        }
    }

    static string GetInput(string prompt) { Console.Write(prompt); return Console.ReadLine(); }
}

static class Caesar
{
    public static string Encrypt(string text, int shift)
    {
        if (string.IsNullOrEmpty(text)) return "";
        char[] result = text.Select(c => char.IsLetter(c)
            ? (char)((((char.ToUpper(c) - 'A') + shift) % 26) + (char.IsUpper(c) ? 'A' : 'a'))
            : c).ToArray();
        return new string(result);
    }

    public static string Decrypt(string text, int shift) => Encrypt(text, 26 - (shift % 26));
}

static class Vigenere
{
    public static string Encrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        key = key.ToUpper();
        char[] result = new char[text.Length];
        for (int i = 0, j = 0; i < text.Length; i++)
            if (char.IsLetter(text[i]))
            {
                char baseChar = char.IsUpper(text[i]) ? 'A' : 'a';
                result[i] = (char)((((text[i] - baseChar) + (key[j++ % key.Length] - 'A')) % 26) + baseChar);
            }
            else result[i] = text[i];
        return new string(result);
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        key = key.ToUpper();
        char[] result = new char[text.Length];
        for (int i = 0, j = 0; i < text.Length; i++)
            if (char.IsLetter(text[i]))
            {
                char baseChar = char.IsUpper(text[i]) ? 'A' : 'a';
                result[i] = (char)((((text[i] - baseChar) - (key[j++ % key.Length] - 'A') + 26) % 26) + baseChar);
            }
            else result[i] = text[i];
        return new string(result);
    }
}

static class Substitution
{
    private static readonly char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] shuffled = alphabet.OrderBy(_ => Guid.NewGuid()).ToArray();

    public static string Encrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        char[] result = text.Select(c => char.IsLetter(c)
            ? char.IsUpper(c) ? shuffled[Array.IndexOf(alphabet, char.ToUpper(c))] : char.ToLower(shuffled[Array.IndexOf(alphabet, char.ToUpper(c))])
            : c).ToArray();
        return new string(result);
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        char[] result = text.Select(c => char.IsLetter(c)
            ? char.IsUpper(c) ? alphabet[Array.IndexOf(shuffled, char.ToUpper(c))] : char.ToLower(alphabet[Array.IndexOf(shuffled, char.ToUpper(c))])
            : c).ToArray();
        return new string(result);
    }
}

static class Transposition
{
    public static string Encrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        int cols = key.Length, rows = (int)Math.Ceiling((double)text.Length / cols);
        char[,] grid = new char[rows, cols];
        int idx = 0;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols && idx < text.Length; c++)
                grid[r, c] = text[idx++];
        string result = "";
        foreach (char k in key.OrderBy(c => c))
            for (int r = 0; r < rows; r++)
                if (grid[r, key.IndexOf(k)] != '\0') result += grid[r, key.IndexOf(k)];
        return result;
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        int cols = key.Length, rows = (int)Math.Ceiling((double)text.Length / cols);
        char[,] grid = new char[rows, cols];
        int[] order = key.Select((c, i) => (c, i)).OrderBy(x => x.c).Select(x => x.i).ToArray();
        int idx = 0;
        foreach (int c in order)
            for (int r = 0; r < rows && idx < text.Length; r++)
                grid[r, c] = text[idx++];
        return string.Concat(Enumerable.Range(0, rows).SelectMany(r => Enumerable.Range(0, cols).Select(c => grid[r, c]).Where(c => c != '\0')));
    }
}

static class XOR
{
    public static string Encrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] result = bytes.Select((b, i) => (byte)(b ^ keyBytes[i % keyBytes.Length])).ToArray();
        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key)) return text;
        try
        {
            byte[] bytes = Convert.FromBase64String(text);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] result = bytes.Select((b, i) => (byte)(b ^ keyBytes[i % keyBytes.Length])).ToArray();
            return Encoding.UTF8.GetString(result);
        }
        catch { return "Ошибка декодирования."; }
    }
}

static class Atbash
{
    private static readonly char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] reversed = alphabet.Reverse().ToArray();

    public static string Transform(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        char[] result = text.Select(c => char.IsLetter(c)
            ? char.IsUpper(c) ? reversed[Array.IndexOf(alphabet, char.ToUpper(c))] : char.ToLower(reversed[Array.IndexOf(alphabet, char.ToUpper(c))])
            : c).ToArray();
        return new string(result);
    }
}