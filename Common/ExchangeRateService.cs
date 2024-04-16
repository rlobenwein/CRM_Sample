using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using CRM_Sample.Models.ViewModels;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;

namespace CRM_Sample.Common
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IMemoryCache _cache;

        public ExchangeRateService(IMemoryCache cache)
        {
            _cache = cache;
        }
        public async Task<ExchangeRateViewModel> GetFullExchangeData(string date, string currency)
        {
            string exchangeDate = "'" + date + "'";
            string fullUrl = $"https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoMoedaDia(moeda=@moeda,dataCotacao=@dataCotacao)?@moeda='{currency}'&@dataCotacao={exchangeDate}&$top=100&$format=json&$select=cotacaoCompra,cotacaoVenda,dataHoraCotacao,tipoBoletim";
            var cacheKey = $"ExchangeRate_{date}_{currency}";


            if (_cache?.TryGetValue(cacheKey, out ExchangeRateViewModel cachedRate) ?? false)
            {
                return cachedRate;
            }

            var model = new ExchangeRateViewModel();

            try
            {
                using HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ExchangeRateViewModelRoot>(content);

                    if (!(result == null || result.Value.Count == 0))

                    {
                        var returnLast = result.Value.Last();
                        model.ExchangeDateTime = returnLast.ExchangeDateTime;
                        model.ExchangePurchase = returnLast.ExchangePurchase;
                        model.ExchangeSale = returnLast.ExchangeSale;
                        model.Bulletin = returnLast.Bulletin;
                        model.ParityPurchase = returnLast.ParityPurchase;
                        model.ParitySale = returnLast.ParitySale;

                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(60));

                        _cache.Set(cacheKey, model, cacheOptions);
                        return model;
                    }
                }
                else
                {
                    HandleError(model, currency);
                }
            }
            catch (HttpRequestException)
            {
                HandleError(model, currency);
            }

            return model;
        }
        private static ExchangeRateViewModel HandleError(ExchangeRateViewModel model, string currency)
        {
            if (currency == "BRL")
            {
                model.ExchangeSale = 1;
                model.ExchangePurchase = 1;
                model.ExchangeDateTime = null;
                model.Bulletin = "Não encontrado";
                return model;
            }
            model.ExchangeSale = 0;
            model.ExchangePurchase = 0;
            model.ExchangeDateTime = null;
            model.Bulletin = "Não encontrado";

            return model;
        }
    }
}
