using CRM_Sample.Models.CRMModels;
using CRM_Sample.Models.CustomerModels;
using CRM_Sample.Models.SalesModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM_Sample.Models.ViewModels
{
    public class PlannedActionsViewModel
    {
        public int ActionId { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime Date { get; set; }
        public string ActionTypeName { get; set; }
        public ErpUser ErpUser { get; set; }
        public Company Company { get; set; }
        public Person Person { get; set; }
        public CompanyEmployee CompanyEmployee { get; set; }
        public string Description { get; set; }
        public string PipelineStage { get; set; }
        public Opportunity Opportunity { get; set; }
        public string PlannedStatus { get; set; }
        public ICollection<OpportunityAction> PlannedActions { get; set; }
    }
}
