using CRM_Sample.Models.CRMModels;
using CRM_Sample.Models.CustomerModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Sample.Models.SalesModels
{
    public class Opportunity
    {
        [Key]
        [Display(Name = "Número")]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data")]
        public DateTime Date { get; set; }


        [Required]
        [Display(Name = "Cliente")]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [Display(Name = "Produto")]
        [DisplayFormat(NullDisplayText ="Não informado")]
        public int? ProductId { get; set; } = null;

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [NotMapped]
        [Display(Name="Categoria")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Valor")]
        public decimal Value { get; set; } = 0;

        [Required]
        [Display(Name = "Gestor")]
        public int ErpUserId { get; set; }

        [ForeignKey("ErpUserId")]
        public ErpUser Manager { get; set; }

        [Display(Name = "Estágio Pipeline")]
        public int? PipelineId { get; set; } = null;

        [ForeignKey("PipelineId")]
        public Pipeline Pipeline { get; set; }

        [Display(Name = "Observações")]
        [StringLength(1000)]
        public string Notes { get; set; }

        public int? OpportunityGroup { get; set; } = null;

        [NotMapped]
        public List<Opportunity> OpportunitiesRelated { get; set; }
        
        [NotMapped]
        public List<Opportunity> CompanyOpportunities { get; set; }

        [NotMapped]
        public bool Checked { get; set; }

        public OpportunityStatus Status { get; set; }

        [Display(Name ="Título")]
        [StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Ações")]
        public ICollection<OpportunityAction> OpportunityActions { get; set; }

        [Display(Name = "Propostas")]
        public ICollection<Proposal> Proposals { get; set; }

        public enum OpportunityStatus
        {
            [Display(Name = "Aberta")]
            Opened,
            [Display(Name = "Vendida")]
            Sold,
            [Display(Name = "Perdida")]
            Lost,
            [Display(Name = "Cancelada")]
            Canceled,
            [Display(Name = "Fechada (apenas op. internas)")]
            Closed,
            [Display(Name = "Declinada")]
            Declined,
        }
        public string GetStatusDisplayName()
        {
            var field = Status.GetType().GetField(Status.ToString());
            var displayAttribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));

            return displayAttribute != null ? displayAttribute.Name : Status.ToString();
        }
    }
}
