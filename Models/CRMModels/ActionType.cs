using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CRM_Sample.Models.CRMModels
{
    public class ActionType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tipo da Ação")]
        [StringLength(100)]
        public string Name { get; set; }
    }
}