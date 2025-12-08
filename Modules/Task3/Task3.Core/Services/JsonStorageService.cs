using System.Text.Json;

namespace Task3.Core.Services
{
    public sealed class JsonStorageService
    {
        private readonly JsonSerializerOptions _options;

        public JsonStorageService()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        public List<T> LoadList<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<T>();
            }

            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }

            var result = JsonSerializer.Deserialize<List<T>>(json, _options);

            return result ?? new List<T>();
        }

        public void SaveList<T>(string filePath, IEnumerable<T> items)
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(items, _options);

            File.WriteAllText(filePath, json);
        }
    }
}
