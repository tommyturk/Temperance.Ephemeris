using Temperance.Ephemeris.Models.Financials;

namespace Temperance.Ephemeris.Repositories.Financials.Interfaces
{
    public interface ISecurityMasterRepsotory
    {
        Task<DateTime?> GetSecurityIpoDate(string symbol);
        Task<List<SecurityMaster>> GetAll();
    }
}
