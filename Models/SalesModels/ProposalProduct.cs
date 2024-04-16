using CRM_Sample.Models.SalesModels;
using static CRM_Sample.Data.Constants.Permissions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static CRM_Sample.Common.Currency;
using System.ComponentModel.DataAnnotations.Schema;
using CRM_Sample.Common;

namespace CRM_Sample.Models.SalesModels
{
    public class ProposalProduct
    {
        private decimal price;

        [Key]
        public int Id { get; set; }

        [Required]
        public int ProposalId { get; set; }

        [Required]
        [Display(Name = "Categoria")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Produto")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Preço")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:N2}")]
        public decimal BasePrice { get; set; } = 0;

        [Required]
        [Display(Name = "Desconto")]
        [DisplayFormat(DataFormatString = "{0:##.##}", ApplyFormatInEditMode = true)]
        public decimal Discount { get; set; } = 0;

        [Required]
        [Display(Name = "Preço Final")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:N2}")]
        public decimal Price { get => price; set => price = BasePrice * (1 - Discount); }

        [Display(Name = "Determinar Preço Manualmente")]
        public bool ManualPrice { get; set; }

        [Display(Name="Proposta")]
        public Proposal Proposal { get; set; }

        [Display(Name = "Categoria")]
        public Category Category { get; set; }

        [Display(Name = "Produto")]
        public Product Product { get; set; }
    }
}
