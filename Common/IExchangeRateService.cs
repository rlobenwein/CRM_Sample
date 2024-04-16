using CRM_Sample.Models.ViewModels;
using System;
using System.Threading.Tasks;

namespace CRM_Sample.Common
{
    public interface IExchangeRateService
    {
        public Task<ExchangeRateViewModel> GetFullExchangeData(string date,string currency);
    }
}
