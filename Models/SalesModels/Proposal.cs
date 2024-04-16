using System.ComponentModel.DataAnnotations;
using System;
using static CRM_Sample.Common.Currency;
using System.Collections;
using System.Collections.Generic;

namespace CRM_Sample.Models.SalesModels
{
    public class Proposal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Oportunidade")]
        public int OpportunityId { get; set; }

        [Display(Name = "Proposta Revisada")]
        public int? Revision { get; set; }

        [Required]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Validade (Dias)")]
        public int ExpirationTime { get; set; }

        [Required]
        [Display(Name = "Moeda")]
        public Currencies Currency { get; set; } = Currencies.BRL;

        [Required]
        [Display(Name = "Câmbio")]
        public decimal ExchangeRate { get; set; } = 1;

        [Required]
        [Display(Name = "Preço")]
        public decimal BasePrice { get; set; } = 0;

        [Required]
        [Display(Name = "Desconto")]
        public decimal Discount { get; set; } = 0;

        private decimal price;

        [Required]
        [Display(Name = "Preço Final")]
        public decimal Price { get => price; set => price = BasePrice * (1 - Discount); }
        private decimal priceBrl;

        [Required]
        [Display(Name = "Preço Final BRL")]
        public decimal PriceBrl { get => priceBrl; set => priceBrl = Price * ExchangeRate; }

        [StringLength(2000)]
        [Display(Name = "Notas")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Required]
        [Display(Name = "Pz Entrega (Dias corridos)")]
        public int DelireryTime { get; set; }


        [Display(Name = "Preço Manual")]
        public bool ManualPrice { get; set; }

        public ProposalStatus Status { get; set; }

        public Opportunity Opportunity { get; set; }

        public ICollection<ProposalProduct> Products { get; set; }

        public enum ProposalStatus
        {
            [Display(Name = "Aberta")]
            Opened,
            [Display(Name = "Vendida")]
            Sold,
            [Display(Name = "Perdida")]
            Lost,
            [Display(Name = "Cancelada")]
            Canceled,
            [Display(Name = "Revisada")]
            Revised,
            [Display(Name = "Declinada")]
            Declined,
            [Display(Name = "Antiga")]
            Old,
        }
        public string GetStatusDisplayName()
        {
            var field = this.Status.GetType().GetField(this.Status.ToString());
            var displayAttribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));

            return displayAttribute != null ? displayAttribute.Name : this.Status.ToString();
        }
    }
}
