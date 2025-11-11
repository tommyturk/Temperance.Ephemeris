namespace Temperance.Ephemeris.Repositories.BackFill.Interfaces
{
    public interface ISecurityQueueRepository
    {
        Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate);
    }
}
