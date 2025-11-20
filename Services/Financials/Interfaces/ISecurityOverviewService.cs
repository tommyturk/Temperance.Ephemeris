namespace Temperance.Ephemeris.Services.Financials.Interfaces
{
    public interface ISecurityOverviewService
    {
        Task<int> GetSecurityId(string symbol);
    }
}
