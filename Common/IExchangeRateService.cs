using RLBW_ERP.Models.ViewModels;
using System;
using System.Threading.Tasks;

namespace RLBW_ERP.Common
{
    public interface IExchangeRateService
    {
        public Task<ExchangeRateViewModel> GetFullExchangeData(string date,string currency);
    }
}
