namespace Temperance.Ephemeris.Repositories.Financials.Interfaces
{
    public interface ISecurityOverviewRepository
    {
        Task<int> GetSecurityId(string symbol);
    }
}
