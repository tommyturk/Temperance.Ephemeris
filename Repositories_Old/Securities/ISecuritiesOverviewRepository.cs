namespace Temperance.Ephemeris.Repositories_Old.Securities
{
    public interface ISecuritiesOverviewRepository
    {
        Task<List<string>> GetSecurities();

        Task<bool> UpdateSecuritiesOverview(SecuritiesOverview SecuritiesOverview);

        Task<SecuritiesOverview> GetSecurityOverview(string symbol);

        Task<int> GetSecurityId(string symbol);
        Task<bool> DeleteSecurity(string symbol);
        Task<DateTime?> GetSecurityIpoDate(string symbol);
    }
}
