using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.LocationModels
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "País")]
        public string Name { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Abreviação (ISO3)")]
        public string Iso3 { get; set; }

        [StringLength(4)]
        [Display(Name = "Código Telefônico")]
        public string PhoneCode { get; set; }

        public ICollection<State> States { get; set; }
    }
}
