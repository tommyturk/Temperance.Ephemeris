namespace Temperance.Ephemeris.Services.Financials.Interfaces
{
    public interface ISecurityMasterService
    {
        Task<DateTime?> GetSecurityIpoDate(string symbol);
    }
}
