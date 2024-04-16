using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Sample.Models.LocationModels
{
    public class State
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Estado")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name="Sigla")]
        public string Acronym { get; set; }

        [Required]
        [Display(Name = "País")]
        public int CountryId { get; set; }

        [ForeignKey("CountryId")]
        public Country Country { get; set; }

        public ICollection<City> Cities { get; set; }
    }
}
