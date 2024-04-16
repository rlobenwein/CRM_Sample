using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.ViewModels
{
    public class ExchangeRateViewModel
    {
        [Display(Name ="Compra")]
        [JsonProperty("cotacaoCompra")]
        public decimal ExchangePurchase { get; set; }

        [Display(Name ="Venda")]
        [JsonProperty("cotacaoVenda")]
        public decimal ExchangeSale { get; set; }

        [Display(Name ="Paridade Compra")]
        [JsonProperty("paridadeCompra")]
        public decimal ParityPurchase { get; set; }

        [Display(Name = "Paridade Venda")]
        [JsonProperty("paridadeVenda")]
        public decimal ParitySale { get; set; }

        [Display(Name ="Data")]
        [JsonProperty("dataHoraCotacao")]
        public string ExchangeDateTime { get; set; }


        [Display(Name ="Boletim")]
        [JsonProperty("tipoBoletim")]
        public string Bulletin { get; set; }

    }
    public class ExchangeRateViewModelRoot
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("value")]
        public List<ExchangeRateViewModel> Value { get; set; }
    }
}
