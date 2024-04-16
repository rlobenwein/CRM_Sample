using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Sample.Models.LocationModels
{
    public class City
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Cidade")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "State")]
        public int StateId { get; set; }

        [ForeignKey("StateId")]
        public State State { get; set; }
    }
}
