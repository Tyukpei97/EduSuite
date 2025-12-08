using Task3.Core.Models;
using Task3.Core.Services;

namespace Task3.Core.Repositories
{
    public sealed class JsonClientRepository : IClientRepository
    {
        private readonly string _filePath;
        private readonly JsonStorageService _storage;

        public JsonClientRepository(string filePath, JsonStorageService storage)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public IReadOnlyList<Client> GetAll()
        {
            return _storage.LoadList<Client>(_filePath);
        }

        public void SaveAll(IEnumerable<Client> clients)
        {
            _storage.SaveList(_filePath, clients);
        }
    }
}
