using CRM_Sample.Models.SalesModels;
using System.Collections.Generic;

namespace CRM_Sample.Models.ViewModels
{
    public class ErpUserViewModel
    {
        public int Id { get; set; }
        public string ErpUserName { get; set; }
        //public bool IsActive { get; set; }
        public List<OpportunityActionViewModel> OpportunityActions { get; set; }
    }
}
