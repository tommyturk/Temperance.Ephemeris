using Temperance.Ephemeris.Repositories.Financials.Interfaces;
using Temperance.Ephemeris.Services.Financials.Interfaces;

namespace Temperance.Ephemeris.Services.Financials.Implementation
{
    public class SecurityOverviewService : ISecurityOverviewService
    {
        private readonly ISecurityOverviewRepository _securityOverviewRepository;

        public SecurityOverviewService(ISecurityOverviewRepository securityOverviewRepository)
        {
            _securityOverviewRepository = securityOverviewRepository;
        }

        public async Task<int> GetSecurityId(string symbol)
        {
            return await _securityOverviewRepository.GetSecurityId(symbol);
        }

    }
}
