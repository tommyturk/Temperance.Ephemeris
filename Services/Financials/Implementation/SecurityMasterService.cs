using Temperance.Ephemeris.Repositories.Financials.Interfaces;
using Temperance.Ephemeris.Services.Financials.Interfaces;

namespace Temperance.Ephemeris.Services.Financials.Implementation
{
    public class SecurityMasterService : ISecurityMasterService
    {
        private readonly ISecurityMasterRepsotory _securityMasterRepository;

        public SecurityMasterService(ISecurityMasterRepsotory securityMasterRepository)
        {
            _securityMasterRepository = securityMasterRepository;
        }

        public async Task<DateTime?> GetSecurityIpoDate(string symbol)
        {
            return await _securityMasterRepository.GetSecurityIpoDate(symbol);
        }
    }
}
