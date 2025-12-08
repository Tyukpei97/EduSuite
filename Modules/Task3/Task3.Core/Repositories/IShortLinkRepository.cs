using Task3.Core.Models;

namespace Task3.Core.Repositories
{
    public interface IShortLinkRepository
    {
        IReadOnlyList<ShortLinkRecord> GetAll();

        void SaveAll(IEnumerable<ShortLinkRecord> records);
    }
}
