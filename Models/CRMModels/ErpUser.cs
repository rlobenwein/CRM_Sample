using Microsoft.EntityFrameworkCore;
using CRM_Sample.Models.SalesModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.CRMModels
{
    public class ErpUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Responsável")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Usuário Ativo")]
        public bool Active { get; set; } = true;

        public List<OpportunityAction> OpportunityActions { get; set; }
    }
}
