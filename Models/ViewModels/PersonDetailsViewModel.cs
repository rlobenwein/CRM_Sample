using CRM_Sample.Models.CustomerModels;
using CRM_Sample.Models.SalesModels;
using System.Collections.Generic;

namespace CRM_Sample.Models.ViewModels
{
    public class PersonDetailsViewModel
    {
        public Person Person { get; set; }
        public Company Company { get; set; }
    }
}
