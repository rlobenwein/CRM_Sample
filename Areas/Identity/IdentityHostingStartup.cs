using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(CRM_Sample.Areas.Identity.IdentityHostingStartup))]
namespace CRM_Sample.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}