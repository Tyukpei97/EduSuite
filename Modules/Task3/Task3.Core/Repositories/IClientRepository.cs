using Task3.Core.Models;

namespace Task3.Core.Repositories
{
    public interface IClientRepository
    {
        IReadOnlyList<Client> GetAll();

        void SaveAll(IEnumerable<Client> clients);
    }
}
