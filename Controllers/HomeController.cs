using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models;
using RLBW_ERP.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static RLBW_ERP.Models.SalesModels.Opportunity;
using static RLBW_ERP.Models.SalesModels.OpportunityAction;


namespace RLBW_ERP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _rateService;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY = "statistics";

        public HomeController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
            _rateService = new ExchangeRateService(cache);
        }

        public async Task<IActionResult> Index()
        {
            StatisticsViewModel cachedData = (StatisticsViewModel)_cache.Get(key: CACHE_KEY);
            if (cachedData != null)
            {
                return View(cachedData);
            }
            var statistics = await _cache.GetOrCreateAsync(CACHE_KEY, async (entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return await LoadStatisticsFromDatabase();
            });

            return View(statistics);
        }
        private async Task<StatisticsViewModel> LoadStatisticsFromDatabase()
        {
            var statistics = new StatisticsViewModel();

            statistics.OpportinitiesAll = await _context.Opportunities.CountAsync();
            statistics.OpportunitiesOpened = await _context.Opportunities.CountAsync(o => o.Status == OpportunityStatus.Opened);
            statistics.OpportunitiesSold = await _context.Opportunities.CountAsync(o => o.Status == OpportunityStatus.Sold);
            statistics.OpportunitiesCanceled = await _context.Opportunities.CountAsync(o => o.Status == OpportunityStatus.Canceled);
            statistics.OpportunitiesFreezed = await _context.Opportunities.CountAsync(o => o.Status == OpportunityStatus.Closed);
            statistics.OpportunitiesLost = await _context.Opportunities.CountAsync(o => o.Status == OpportunityStatus.Lost);

            statistics.ActionsAll = await _context.OpportunityActions.CountAsync();
            statistics.ActionsDone = await _context.OpportunityActions.CountAsync(a => a.Status == ActionStatus.Done);
            statistics.ActionsPlanned = await _context.OpportunityActions.CountAsync(a => a.Status == ActionStatus.Planned);
            statistics.ActionsDelayed = await _context.OpportunityActions.CountAsync(a => a.Status == ActionStatus.Planned && a.Date.Date < DateTime.Today);
            statistics.ActionsUpToDate = await _context.OpportunityActions.CountAsync(a => a.Status == ActionStatus.Planned && a.Date.Date > DateTime.Today);
            statistics.ActionsToday = await _context.OpportunityActions.CountAsync(a => a.Status == ActionStatus.Planned && a.Date.Date == DateTime.Today);

            statistics.ErpUsers = (from u in _context.ErpUsers
                                   .Where(u => u.Active)
                                   select new ErpUserViewModel
                                   {
                                       ErpUserName = u.Name,
                                       OpportunityActions = u.OpportunityActions
                                           .Where(action => action.Status == ActionStatus.Done)
                                           .Select(action => new OpportunityActionViewModel
                                           {
                                               Date = action.Date,
                                               Status = action.Status
                                           }).ToList()
                                   }).ToList();
            SaveDataToCache(statistics);
            return statistics;
        }

        private void SaveDataToCache(StatisticsViewModel statistics)
        {
            _cache.Set(CACHE_KEY, statistics, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<ActionResult> GetExchangeRateValues(string strDate)
        {
            strDate = strDate?.Replace("\"", "");

            if (!DateTime.TryParse(strDate, out DateTime date))
            {
                date = DateTime.Today;
            }

            var statistics = new StatisticsViewModel();

            var i = 0;
            int maxAttempts = 10;
            int attemptCount = 0;
            do
            {
                var usdRate = (await _rateService.GetFullExchangeData(date.AddDays(i).ToString("MM-dd-yyyy"), "USD"));
                var eurRate = (await _rateService.GetFullExchangeData(date.AddDays(i).ToString("MM-dd-yyyy"), "EUR"));
                statistics.DollarValue = usdRate.ExchangeSale;
                statistics.EuroValue = eurRate.ExchangeSale;
                statistics.Bulletin = usdRate.Bulletin;
                statistics.ExchangeDate = Convert.ToDateTime(usdRate.ExchangeDateTime);
                i--;
                attemptCount++;
            } while ((statistics.DollarValue == 0 || statistics.EuroValue == 0) && attemptCount < maxAttempts);

            return Json(new
            {
                DollarValue = statistics.DollarValue,
                EuroValue = statistics.EuroValue,
                Bulletin = statistics.Bulletin,
                ExchangeDate = statistics.ExchangeDate
            });
        }

    }
}
