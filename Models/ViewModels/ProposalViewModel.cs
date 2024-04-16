using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.ViewModels
{
    public class ProposalViewModel
    {
        public string Id { get; set; }
        [Display(Name = "Data")]
        public string Date { get; set; }
        [Display(Name = "Preço")]
        public string Price { get; set; }
        [Display(Name = "Situação")]
        public string Status { get; set; }
        [Display(Name = "Moeda")]
        public string Currency{ get; set; }
    }
}