using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.SalesModels
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Categoria")]
        public string CategoryName { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
