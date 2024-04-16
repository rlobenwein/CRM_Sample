using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.SalesModels
{
    public class Pipeline
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Estágio Pipeline")]
        [StringLength(150)]
        public string Stage { get; set; }

    }
}