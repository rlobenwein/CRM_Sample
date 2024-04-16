using System;

namespace CRM_Sample.Models.ViewModels
{
    public class OpportunityActionViewModel
    {
        public DateTime Date { get; set; }
        public SalesModels.OpportunityAction.ActionStatus Status { get; set; }
    }
}