using System;

namespace CRM_Sample.Models.ViewModels
{
    public class DelayActionViewModel
    {
        public int ActionId { get; set; }
        public int Days { get; set; }
        public DateTime NewDate { get; set; }
        public DateTime OldDate { get; set; }

    }
}
