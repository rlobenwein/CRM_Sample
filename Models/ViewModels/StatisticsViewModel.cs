using Microsoft.AspNetCore.Identity;
using CRM_Sample.Models.CRMModels;
using CRM_Sample.Models.SalesModels;
using System;
using System.Collections.Generic;

namespace CRM_Sample.Models.ViewModels
{
    public class StatisticsViewModel
    {
        public int CompaniesAll { get; set; }
        public int CompaniesActive { get; set; }
        public int CompaniesInactive { get; set; }
        public int PeopleAll { get; set; }
        public int PeopleActive { get; set; }
        public int PeopleInactive { get; set; }
        public int OpportinitiesAll { get; set; }
        public int OpportunitiesSold { get; set; }
        public int OpportunitiesCanceled { get; set; }
        public int OpportunitiesFreezed { get; set; }
        public int OpportunitiesOpened { get; set; }
        public int OpportunitiesLost { get; set; }
        public int ActionsAll { get; set; }
        public int ActionsDone { get; set; }
        public int ActionsPlanned { get; set; }
        public int ActionsDelayed { get; set; }
        public int ActionsUpToDate { get; set; }
        public int ActionsThisWeek { get; set; }
        public int ActionsToday { get; set; }
        public decimal DollarValue { get; set; }
        public decimal EuroValue { get; set; }
        public string Bulletin { get; set; }
        public DateTime ExchangeDate { get; set; }
        public List<ErpUserViewModel> ErpUsers { get; set; }
        public ICollection<OpportunityAction> OpportunityActions { get; set; }

    }
}
