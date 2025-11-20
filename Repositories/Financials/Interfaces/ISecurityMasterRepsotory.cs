namespace Temperance.Ephemeris.Repositories.Financials.Interfaces
{
    public interface ISecurityMasterRepsotory
    {
        Task<DateTime?> GetSecurityIpoDate(string symbol);
    }
}
