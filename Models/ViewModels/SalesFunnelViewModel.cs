using System.Collections.Generic;
using static CRM_Sample.Models.SalesModels.Opportunity;

namespace CRM_Sample.Models.ViewModels
{
    public class SalesFunnelViewModel
    {
        public int Id { get; set; }
        public string PipelineName { get; set; }
        public int OpportunitiesCount { get; set; }
        public double OpportunitiesPercentage { get; set; }
        public OpportunityStatus OpportunityStatus { get; set; }
        public ICollection<OpportunityStatus> OpportunityStatusName { get; set; }
    }
}
