using Task3.Core.Models;
using Task3.Core.Services;

namespace Task3.Core.Repositories
{
    public sealed class JsonShortLinkRepository : IShortLinkRepository
    {
        private readonly string _filePath;
        private readonly JsonStorageService _storage;

        public JsonShortLinkRepository(string filePath, JsonStorageService storage)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public IReadOnlyList<ShortLinkRecord> GetAll()
        {
            return _storage.LoadList<ShortLinkRecord>(_filePath);
        }

        public void SaveAll(IEnumerable<ShortLinkRecord> records)
        {
            _storage.SaveList(_filePath, records);
        }
    }
}
