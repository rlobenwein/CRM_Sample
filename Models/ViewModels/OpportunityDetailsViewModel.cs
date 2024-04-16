using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.ViewModels
{
    public class OpportunityDetailsViewModel
    {
        public int Id { get; set; }
        [Display(Name="Título")]
        public string Title { get; set; }
        [Display(Name="Data")]
        public string Date { get; set; }
        [Display(Name="Cliente")]
        public string Company { get; set; }
        public int CompanyId { get; set; }
        [Display(Name="Categoria")]
        public string Category { get; set; }
        [Display(Name="Produto")]
        public string Product { get; set; }
        [Display(Name="Gestor")]
        public string Manager { get; set; }
        [Display(Name="Valor")]
        public decimal Value { get; set; }
        public string Pipeline { get; set; }
        [Display(Name="Situação")]
        public string Status { get; set; }
        [Display(Name="Observações")]
        public string Notes { get; set; }
        [Display(Name="Oportunidades relacionadas")]
        public List<OpportunitiesRelatedViewModel> OpportunitiesRelated { get; set; }
        [Display(Name="Oportunidades do Cliente")]
        public List<OpportunitiesRelatedViewModel> CompanyOpportunities { get; set; }
        [Display(Name="Propostas")]
        public List<ProposalViewModel> Proposals{ get; set; }
        [Display(Name="Ações")]
        public List<OpportunityActionsViewModel> Actions { get; set; }
        [Display(Name="Grupo de Oportunidades")]
        public int? OpportunityGroup { get; set; }
    }
}
