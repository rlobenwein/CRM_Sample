using System;

namespace CRM_Sample.Models.ViewModels
{
    public class OpportunityActionsViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ActionType { get; set; }
        public string Contact { get; set; }
        public string Manager { get; set; }
        public string Description { get; set; }
        public string Pipeline { get; set; }
        public string Status { get; set; }
    }
}
