using System.ComponentModel.DataAnnotations;
using static CRM_Sample.Common.Currency;

namespace CRM_Sample.Models.SalesModels
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Produto")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Display(Name = "Moeda")]
        public Currencies? Currency { get; set; } = Currencies.BRL;
    }
}
