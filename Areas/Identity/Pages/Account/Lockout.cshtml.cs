using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM_Sample.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "SuperAdmin")]
    public class LockoutModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}
