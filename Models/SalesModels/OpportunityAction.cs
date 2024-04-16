using CRM_Sample.Models.CRMModels;
using CRM_Sample.Models.CustomerModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Sample.Models.SalesModels
{
    public class OpportunityAction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Tipo Ação")]
        public int ActionTypeId { get; set; }

        [ForeignKey("ActionTypeId")]
        public ActionType ActionType { get; set; }

        [Display(Name = "Contato")]
        public int? PersonId { get; set; }

        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Required]
        [Display(Name = "Responsável")]
        public int ErpUserId { get; set; }

        [ForeignKey("ErpUserId")]
        public ErpUser ErpUser { get; set; }

        [Required]
        [StringLength(5000)]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Estágio Pipeline")]
        public int PipelineId { get; set; }

        [ForeignKey("PipelineId")]
        public Pipeline Pipeline { get; set; }

        [Required]
        [Display(Name = "Oportunidade")]
        public int OpportunityId { get; set; }

        [ForeignKey("OpportunityId")]
        public Opportunity Opportunity { get; set; }

        [Required]
        [Display(Name = "Situação")]
        public ActionStatus Status { get; set; }

        public enum ActionStatus
        {
            [Display(Name ="Planejada")]
            Planned,
            [Display(Name ="Executada")]
            Done,
            [Display(Name ="Cancelada")]
            Canceled
        }
        public string GetStatusDisplayName()
        {
            var field = this.Status.GetType().GetField(this.Status.ToString());
            var displayAttribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));

            return displayAttribute != null ? displayAttribute.Name : this.Status.ToString();
        }
    }
}
