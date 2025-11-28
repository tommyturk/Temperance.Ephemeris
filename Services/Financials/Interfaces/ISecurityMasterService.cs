using Temperance.Ephemeris.Models.Financials;

namespace Temperance.Ephemeris.Services.Financials.Interfaces
{
    public interface ISecurityMasterService
    {
        Task<DateTime?> GetSecurityIpoDate(string symbol);
        Task<List<SecurityMaster>> GetAll();
    }
}
