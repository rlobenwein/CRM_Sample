using CRM_Sample.Models.CRMModels;
using CRM_Sample.Models.SalesModels;
using System;
using System.Collections.Generic;

namespace CRM_Sample.Models.ViewModels
{
    public class DoneActionsViewModel
    {
        public int ActionId { get; set; }
        public DateTime Date { get; set; }
        public string ActionTypeName { get; set; }
        public ErpUser ErpUser { get; set; }
        public string Company { get; set; }
        public string Person { get; set; }
        public string Description { get; set; }
        public string PipelineStage { get; set; }
        public int OpportunityId { get; set; }
        public string ActionStatus { get; set; }
        public ICollection<OpportunityAction> DoneActions { get; set; }
    }
}
