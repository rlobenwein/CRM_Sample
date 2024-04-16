using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.ViewModels
{
    public class OpportunitiesRelatedViewModel
    {
        public int Id { get; set; }
        [Display(Name ="Título")]
        public string Title { get; set; }
        [Display(Name ="Categoria")]
        public string Category { get; set; }
        [Display(Name ="Produto")]
        public string Product { get; set; }

    }
}