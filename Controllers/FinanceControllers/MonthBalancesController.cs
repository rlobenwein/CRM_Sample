using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RLBW_ERP.Data;
using RLBW_ERP.Models.FinanceModels;
using RLBW_ERP.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RLBW_ERP.Controllers.FinanceControllers
{
    [Authorize(Roles = "SuperAdmin,Director")]
    public class MonthBalancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonthBalancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GET: MonthBalances
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .Include(x => x.PaymentAccount)
                .Include(x => x.TransactionType)
                .ToListAsync();

            transactions = transactions
                .Where(x => x.PaymentAccount.Name != "Alisson" &&
                            x.PaymentAccount.Name != "Rodrigo" &&
                            x.PaymentAccount.Name != "Ricardo" &&
                            x.TransactionType.Name != "Aplicação" &&
                            x.TransactionType.Name != "Resgate" &&
                            !x.TransactionType.Name.Contains("Transferência"))
                .ToList();

            var groupedTransactions = transactions
                                        .GroupBy(t => new { t.Date.Year, t.Date.Month })
                                        .OrderBy(g => g.Key.Year)
                                        .ThenBy(g => g.Key.Month);

            var balances = new List<MonthBalanceIndexViewModel>();

            foreach (var group in groupedTransactions)
            {
                var viewModel = new MonthBalanceIndexViewModel
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    TotalIncomeIncurred = group.Where(t => t.IsIncurred && !t.IsExpense).Sum(t => t.Value),
                    TotalExpenseIncurred = group.Where(t => t.IsIncurred && t.IsExpense).Sum(t => t.Value),
                    PreviousBalanceIncurred = balances.Any() ? balances.Last().AccumulatedBalanceIncurred : 0,
                    TotalIncomeForecast = group.Where(t => !t.IsIncurred && !t.IsExpense).Sum(t => t.Value),
                    TotalExpenseForecast = group.Where(t => !t.IsIncurred && t.IsExpense).Sum(t => t.Value),
                };

                balances.Add(viewModel);
            }
            balances = balances.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList();
            return View(balances);
        }
        public async Task<IActionResult> FilteredIndex(bool? isIncurred)
        {
            var transactions = await _context.Transactions
                .Include(x => x.PaymentAccount)
                .Include(x => x.TransactionType)
                .ToListAsync();

            transactions = transactions
                .Where(x => x.PaymentAccount.Name != "Alisson" &&
                            x.PaymentAccount.Name != "Rodrigo" &&
                            x.PaymentAccount.Name != "Ricardo" &&
                            x.TransactionType.Name != "Aplicação" &&
                            x.TransactionType.Name != "Resgate" &&
                            !x.TransactionType.Name.Contains("Transferência"))
                .ToList();
            if (isIncurred.HasValue)
            {
                transactions = transactions.Where(x => x.IsIncurred == isIncurred).ToList();
            }
            var groupedTransactions = transactions
                                        .GroupBy(t => new { t.Date.Year, t.Date.Month })
                                        .OrderBy(g => g.Key.Year)
                                        .ThenBy(g => g.Key.Month);

            var balances = new List<MonthBalanceIndexViewModel>();

            foreach (var group in groupedTransactions)
            {
                var viewModel = new MonthBalanceIndexViewModel
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    TotalIncomeIncurred = group.Where(t => t.IsIncurred && !t.IsExpense).Sum(t => t.Value),
                    TotalExpenseIncurred = group.Where(t => t.IsIncurred && t.IsExpense).Sum(t => t.Value),
                    PreviousBalanceIncurred = balances.Any() ? balances.Last().AccumulatedBalanceIncurred : 0,
                    TotalIncomeForecast = group.Where(t => !t.IsIncurred && !t.IsExpense).Sum(t => t.Value),
                    TotalExpenseForecast = group.Where(t => !t.IsIncurred && t.IsExpense).Sum(t => t.Value),
                };

                balances.Add(viewModel);
            }
            balances = balances.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList();
            return PartialView("_BalancesTablePartial", balances);
        }

        [HttpPost]
        public List<object> GetIndexChartInfo(int? qty)
        {
            const int BALANCES_NUMBER = 10;
            qty ??= BALANCES_NUMBER;
            List<object> data = new();

            var transactions = _context.Transactions
                .Include(x => x.PaymentAccount)
                .Include(x => x.TransactionType)
                .ToList();

            transactions = transactions
                .Where(x => x.PaymentAccount.Name != "Alisson" &&
                            x.PaymentAccount.Name != "Rodrigo" &&
                            x.PaymentAccount.Name != "Ricardo" &&
                            x.TransactionType.Name != "Aplicação" &&
                            x.TransactionType.Name != "Resgate" &&
                            !x.TransactionType.Name.Contains("Transferência"))
                .ToList();

            var groupedTransactions = transactions
                                        .GroupBy(t => new { t.Date.Year, t.Date.Month })
                                        .OrderBy(g => g.Key.Year)
                                        .ThenBy(g => g.Key.Month);


            var balances = new List<MonthBalanceIndexViewModel>();
            foreach (var group in groupedTransactions)
            {
                var viewModel = new MonthBalanceIndexViewModel
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    TotalIncomeIncurred = group.Where(t => t.IsIncurred && !t.IsExpense).Sum(t => t.Value),
                    TotalExpenseIncurred = group.Where(t => t.IsIncurred && t.IsExpense).Sum(t => t.Value),
                    PreviousBalanceIncurred = balances.Any() ? balances.Last().AccumulatedBalanceIncurred : 0,
                    TotalIncomeForecast = group.Where(t => !t.IsIncurred && !t.IsExpense).Sum(t => t.Value),
                    TotalExpenseForecast = group.Where(t => !t.IsIncurred && t.IsExpense).Sum(t => t.Value),
                };
                balances.Add(viewModel);
            }

            List<int> years = balances.Select(x => x.Year).ToList();
            List<int> months = balances.Select(x => x.Month).ToList();
            List<string> labels = new();

            foreach (var item in balances.TakeLast((int)qty))
            {
                labels.Add(string.Concat(item.Year.ToString(), '/', item.Month.ToString()));
            }

            data.Add(labels);
            List<decimal> income = balances.Select(x => x.TotalIncomeIncurred).ToList();
            data.Add(income);
            List<decimal> expenses = balances.Select(x => x.TotalExpenseIncurred).ToList();
            data.Add(expenses);
            List<decimal> result = balances.Select(x => x.ResultIncurred).ToList();
            data.Add(result);
            List<decimal> accumulatedBalance = balances.Select(x => x.AccumulatedBalanceIncurred).ToList();
            data.Add(accumulatedBalance);

            return data;
        }

        //GET: MonthBalances/Details?year=2023&month=09
        public async Task<IActionResult> Details(
            int? year,
            int? month,
            string isExpenseStr,
            string isIncurredStr,
            int? category,
            int? subcategory,
            int? type,
            int? paymentAccount,
            string dateStart,
            string dateEnd,
            string searchString,
            string sortOrder,
            string currentFilter
            )
        {
            if (searchString == "undefined") searchString = null;
            if (currentFilter == "undefined") currentFilter = null;
            if (sortOrder == "undefined") sortOrder = null;
            if (searchString != null) currentFilter = searchString;

            year ??= DateTime.UtcNow.Year;
            month ??= DateTime.UtcNow.Month;

            var balance = new MonthBalanceDetailsViewModel
            {
                Transactions = await GetTransactionsAsync((int)year, (int)month, true, false),
                Accounts = _context.PaymentAccounts.ToList(),
                Year = (int)year,
                Month = (int)month,
            };
            var date = new DateTime(balance.Year, balance.Month, 1);
            var allTransactions = _context.Transactions.Include(x => x.TransactionType).ToList();
            balance.PreviousBalance = balance.CalcPreviousBalance(allTransactions);
            foreach (var a in balance.Accounts)
            {
                a.Transactions = _context.Transactions.Where(x => x.PaymentAccountId == a.Id).ToList();
            }

            if (!string.IsNullOrEmpty(searchString) && searchString != "undefined")
            {
                balance.Transactions = FilterTransactions(balance.Transactions, a => a.Description.Contains(searchString));
            }

            if (category != null && category > 0)
            {
                var categoryName = _context.TransactionCategories.Find(category).Name;
                balance.Transactions = balance.Transactions.Where(x => x.CategoryName == categoryName).ToList();
            }

            if (subcategory != null && subcategory > 0)
            {
                var subcategoryName = _context.TransactionSubcategories.Find(subcategory).Name;
                balance.Transactions = balance.Transactions.Where(x => x.SubcategoryName == subcategoryName).ToList();
            }

            if (type != null && type > 0)
            {
                var typeName = _context.TransactionTypes.Find(type).Name;
                balance.Transactions = balance.Transactions.Where(x => x.TypeName == typeName).ToList();
            }

            if (paymentAccount != null && paymentAccount > 0)
            {
                balance.Transactions = balance.Transactions.Where(x => x.PaymentAccountId == paymentAccount).ToList();
            }

            if (isIncurredStr != null && isIncurredStr != "undefined" && isIncurredStr != "0")
            {
                bool isIncurred = isIncurredStr == "true";
                balance.Transactions = balance.Transactions.Where(x => x.IsIncurred == isIncurred).ToList();
            }

            if (isExpenseStr != null && isExpenseStr != "undefined" && isExpenseStr != "0")
            {
                bool isExpense = isExpenseStr == "true";
                balance.Transactions = balance.Transactions.Where(x => x.IsExpense == isExpense).ToList();
            }

            if (dateStart != null && dateStart != "undefined")
            {
                balance.Transactions = balance.Transactions.Where(x => x.Date >= DateTime.Parse(dateStart)).ToList();
            }

            if (dateEnd != null && dateEnd != "undefined")
            {
                balance.Transactions = balance.Transactions.Where(x => x.Date <= DateTime.Parse(dateEnd)).ToList();
            }

            balance.Transactions = ApplySortOrder(balance.Transactions, sortOrder);

            balance.TransactionsQuantity = balance.Transactions.Count;
            var lastTransaction = _context.Transactions.OrderBy(x => x.Date).LastOrDefault().Date;
            var lastIncurredTransaction = _context.Transactions.Where(x => x.IsIncurred).OrderBy(x => x.Date).LastOrDefault().Date;
            var lastIncurredMonth = new DateTime(lastIncurredTransaction.Year, lastIncurredTransaction.Month, DateTime.DaysInMonth(lastIncurredTransaction.Year, lastIncurredTransaction.Month)).Date;

            ViewData["LastYear"] = lastTransaction.Year;
            ViewData["LastMonth"] = lastTransaction.Month;
            ViewData["CurrentFilter"] = currentFilter;
            ViewData["Accounts"] = _context.PaymentAccounts.ToList();
            ViewData["LastIncurredMonth"] = lastIncurredMonth;

            string[] sortableFields = { "IsIncurred", "IsExpense", "TypeName", "PaymentAccountId", "Value", "Date", "CategoryName", "Description", "SubcategoryName" };
            SetupViewDataForSorting(sortOrder, sortableFields);
            SetupViewDataForSelectInputs<TransactionCategory>("Categories");
            SetupViewDataForSelectInputs<TransactionSubcategory>("Subcategories");
            SetupViewDataForSelectInputs<TransactionType>("Types");
            SetupViewDataForSelectInputs<PaymentAccount>("PaymentAccounts");

            return View(balance);
        }

        public async Task<IActionResult> FilteredDetails(
            int? year,
            int? month,
            string isExpenseStr,
            string isIncurredStr,
            int? category,
            int? subcategory,
            int? type,
            int? paymentAccount,
            string dateStart,
            string dateEnd,
            string searchString,
            string sortOrder,
            string currentFilter
            )
        {
            if (searchString == "undefined") searchString = null;
            if (currentFilter == "undefined") currentFilter = null;
            if (sortOrder == "undefined") sortOrder = null;
            if (searchString != null) currentFilter = searchString;

            var transactions = await GetTransactionsAsync((int)year, (int)month, false, false);

            if (!string.IsNullOrEmpty(searchString) && searchString != "undefined")
            {
                transactions = transactions.Where(a => a.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (category != null && category > 0)
            {
                var categoryName = _context.TransactionCategories.Find(category).Name;
                transactions = transactions.Where(x => x.CategoryName == categoryName).ToList();
            }

            if (subcategory != null && subcategory > 0)
            {
                var subcategoryName = _context.TransactionSubcategories.Find(subcategory).Name;
                transactions = transactions.Where(x => x.SubcategoryName == subcategoryName).ToList();
            }

            if (type != null && type > 0)
            {
                var typeName = _context.TransactionTypes.Find(type).Name;
                transactions = transactions.Where(x => x.TypeName == typeName).ToList();
            }

            if (paymentAccount != null && paymentAccount > 0)
            {
                transactions = transactions.Where(x => x.PaymentAccountId == paymentAccount).ToList();
            }

            if (isIncurredStr != null && isIncurredStr != "undefined" && isIncurredStr != "0")
            {
                bool isIncurred = isIncurredStr == "true";
                transactions = transactions.Where(x => x.IsIncurred == isIncurred).ToList();
            }

            if (isExpenseStr != null && isExpenseStr != "undefined" && isExpenseStr != "0")
            {
                bool isExpense = isExpenseStr == "true";
                transactions = transactions.Where(x => x.IsExpense == isExpense).ToList();
            }

            if (dateStart != null && dateStart != "undefined")
            {
                transactions = transactions.Where(x => x.Date >= DateTime.Parse(dateStart)).ToList();
            }

            if (dateEnd != null && dateEnd != "undefined")
            {
                transactions = transactions.Where(x => x.Date <= DateTime.Parse(dateEnd)).ToList();
            }

            transactions = ApplySortOrder(transactions, sortOrder);

            var lastTransaction = _context.Transactions.OrderBy(x => x.Date).LastOrDefault().Date;
            var lastIncurredTransaction = _context.Transactions.Where(x => x.IsIncurred).OrderBy(x => x.Date).LastOrDefault().Date;
            var lastIncurredMonth = new DateTime(lastIncurredTransaction.Year, lastIncurredTransaction.Month, DateTime.DaysInMonth(lastIncurredTransaction.Year, lastIncurredTransaction.Month)).Date;

            ViewData["LastYear"] = lastTransaction.Year;
            ViewData["LastMonth"] = lastTransaction.Month;
            ViewData["CurrentFilter"] = currentFilter;
            ViewData["Accounts"] = _context.PaymentAccounts.ToList();
            ViewData["LastIncurredMonth"] = lastIncurredMonth;

            string[] sortableFields = { "IsIncurred", "IsExpense", "TypeName", "PaymentAccountId", "Value", "Date", "CategoryName", "Description", "SubcategoryName" };
            SetupViewDataForSorting(sortOrder, sortableFields);
            SetupViewDataForSelectInputs<TransactionCategory>("Categories");
            SetupViewDataForSelectInputs<TransactionSubcategory>("Subcategories");
            SetupViewDataForSelectInputs<TransactionType>("Types");
            SetupViewDataForSelectInputs<PaymentAccount>("PaymentAccounts");

            if (!transactions.Any())
            {
                ViewData["Year"] = year;
                ViewData["Month"] = month;
                return PartialView("_EmptyPartialView", transactions);
            }

            return PartialView("_MonthBalancePartialView", transactions);
        }

        private void SetupViewDataForSelectInputs<T>(string viewDataKey) where T : class
        {
            var dbSet = _context.Set<T>();
            ViewData[viewDataKey] = new SelectList(dbSet, "Id", "Name");
        }

        private void SetupViewDataForSorting(string sortOrder, string[] sortableFields)
        {
            foreach (var field in sortableFields)
            {
                ViewData[$"{field}SortParm"] = sortOrder == field ? $"{field}_desc" : field;
            }
        }

        private static List<MonthBalanceTransactionsViewModel> ApplySortOrder(List<MonthBalanceTransactionsViewModel> transactions, string sortOrder)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "Date_desc";
            }

            string[] parts = sortOrder.Split('_');
            string propertyName = parts[0];
            bool isDescending = parts.Length > 1 && parts[1] == "desc";

            var property = typeof(MonthBalanceTransactionsViewModel).GetProperty(propertyName);
            if (property != null)
            {
                if (isDescending)
                {
                    transactions = transactions.OrderByDescending(t => property.GetValue(t, null)).ToList();
                }
                else
                {
                    transactions = transactions.OrderBy(t => property.GetValue(t, null)).ToList();
                }
            }
            else
            {
                transactions = transactions.OrderByDescending(t => t.Date).ToList();
            }

            return transactions;
        }

        private static List<MonthBalanceTransactionsViewModel> FilterTransactions(List<MonthBalanceTransactionsViewModel> transactions, Func<MonthBalanceTransactionsViewModel, bool> predicate)
        {
            if (predicate != null)
            {
                return transactions.Where(predicate).ToList();
            }
            return transactions;
        }


        [HttpPost]
        public List<object> GetDetailsChartInfo(int year, int month)
        {
            List<object> data = new();

            var balance = new MonthBalanceDetailsViewModel
            {
                Year = year,
                Month = month,
                Transactions = GetTransactions((int)year, (int)month, false, false)
            };
            decimal income = balance.TotalIncomeIncurred;
            data.Add(income);
            decimal expenses = balance.TotalExpenseIncurred;
            data.Add(expenses);
            decimal result = balance.ResultIncurred;
            data.Add(result);
            decimal previousBalance = balance.PreviousBalance;
            data.Add(previousBalance);
            decimal accumulatedBalance = balance.AccumulatedBalanceIncurred;
            data.Add(accumulatedBalance);
            data.Add(balance.TotalIncomeForecast);
            data.Add(balance.TotalExpenseForecast);
            data.Add(balance.ResultForecast);
            data.Add(balance.AccumulatedBalanceForecast);

            return data;
        }

        private async Task<List<MonthBalanceTransactionsViewModel>> GetTransactionsAsync(int year, int month, bool skipPartnerInvestment, bool skipInvestment)
        {
            List<MonthBalanceTransactionsViewModel> transactions = await (from t in _context.Transactions
                                                                    .Where(x => x.Date.Month == month && x.Date.Year == year)
                                                                    .Include(x => x.TransactionType)
                                                                        .ThenInclude(x => x.Subcategory)
                                                                            .ThenInclude(x => x.Category)
                                                                    .Include(x => x.PaymentAccount)
                                                                          select new MonthBalanceTransactionsViewModel()
                                                                          {
                                                                              Id = t.Id,
                                                                              Date = t.Date,
                                                                              IsExpense = t.IsExpense,
                                                                              TypeId = t.TypeId,
                                                                              TypeName = t.TransactionType.Name,
                                                                              CategoryName = t.TransactionType.Subcategory.Category.Name,
                                                                              SubcategoryName = t.TransactionType.Subcategory.Name,
                                                                              Description = t.Description,
                                                                              PaymentAccountName = t.PaymentAccount.Name,
                                                                              PaymentAccountId = t.PaymentAccountId,
                                                                              IsIncurred = t.IsIncurred,
                                                                              Value = t.Value
                                                                          })
                                                                    .AsNoTracking()
                                                                    .ToListAsync();
            bool onlyMainAccounts = true;
            if (onlyMainAccounts)
            {
                transactions = transactions.Where(x =>
                    x.PaymentAccountName != "BB Rende Fácil"
                    ).ToList();
            }

            if (skipPartnerInvestment)
            {
                transactions = transactions.Where(x =>
                    x.PaymentAccountName != "Rodrigo" &&
                    x.PaymentAccountName != "Alisson" &&
                    x.PaymentAccountName != "Ricardo"
                    ).ToList();
            }
            if (skipInvestment)
            {
                transactions = transactions.Where(x =>
                    x.TypeName != "Aplicação" &&
                    x.TypeName != "Resgate" &&
                    !x.TypeName.Contains("Transferência")
                    ).ToList();
            }
            return transactions;
        }
        private List<MonthBalanceTransactionsViewModel> GetTransactions(int year, int month, bool skipPartnerInvestment, bool skipInvestment)
        {
            List<MonthBalanceTransactionsViewModel> transactions = (from t in _context.Transactions
                                                                    .Where(x => x.Date.Month == month && x.Date.Year == year)
                                                                    .Include(x => x.TransactionType)
                                                                        .ThenInclude(x => x.Subcategory)
                                                                            .ThenInclude(x => x.Category)
                                                                    .Include(x => x.PaymentAccount)
                                                                    select new MonthBalanceTransactionsViewModel()
                                                                    {
                                                                        Id = t.Id,
                                                                        Date = t.Date,
                                                                        IsExpense = t.IsExpense,
                                                                        TypeId = t.TypeId,
                                                                        TypeName = t.TransactionType.Name,
                                                                        CategoryName = t.TransactionType.Subcategory.Category.Name,
                                                                        SubcategoryName = t.TransactionType.Subcategory.Name,
                                                                        Description = t.Description,
                                                                        PaymentAccountName = t.PaymentAccount.Name,
                                                                        PaymentAccountId = t.PaymentAccountId,
                                                                        IsIncurred = t.IsIncurred,
                                                                        Value = t.Value
                                                                    })
                                                                    .AsNoTracking()
                                                                    .ToList();
            if (skipPartnerInvestment)
            {
                transactions = transactions.Where(x =>
                    x.PaymentAccountName != "Rodrigo" &&
                    x.PaymentAccountName != "Alisson" &&
                    x.PaymentAccountName != "Ricardo"
                    ).ToList();
            }
            if (skipInvestment)
            {
                transactions = transactions.Where(x =>
                    x.TypeName != "Aplicação" &&
                    x.TypeName != "Resgate" &&
                    !x.TypeName.Contains("Transferência")
                    ).ToList();
            }
            return transactions;
        }

        public async Task<IActionResult> ExportToExcel(int year, int month)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var workbook = package.Workbook;
            var worksheet = workbook.Worksheets.Add("Diário");
            //var transactions = await GetTransactions(year, month);
            string fileName = $"{year}.{month:00}_diario.xlsx";

            var transactions = GetTransactionsToExport(year, month, true);

            int row = 1;
            foreach (var item in transactions)
            {
                int col = 1;
                worksheet.Cells[row, col++].Value = item.Date;
                worksheet.Cells[row, col++].Value = item.Nature;
                worksheet.Cells[row, col++].Value = item.Category;
                worksheet.Cells[row, col++].Value = item.Description;
                worksheet.Cells[row, col++].Value = item.Value;
                worksheet.Cells[row, col++].Value = item.Doc;
                worksheet.Cells[row, col++].Value = item.Account;
                worksheet.Cells[row, col++].Value = item.Notes;
                worksheet.Cells[row, col++].Value = "Exportado";
                row++;
            }
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName);

            await Response.Body.WriteAsync(package.GetAsByteArray());

            return new EmptyResult();
        }
        /// <summary>
        /// Get all transactions given an year and a month.
        /// </summary>
        /// <param name="year">The year that will filter transactions dates</param>
        /// <param name="month"></param>
        /// <param name="onlyIncurred"></param>
        /// <returns>A list of transactions</returns>
        private List<TransactionToExport> GetTransactionsToExport(int year, int month, bool onlyIncurred)
        {
            List<Transaction> transactions = (from t in _context.Transactions
                                                        .Where(x => x.Date.Month == month && x.Date.Year == year)
                                                        .Include(x => x.TransactionType)
                                                            .ThenInclude(x => x.Subcategory)
                                                                .ThenInclude(x => x.Category)
                                                        .Include(x => x.PaymentAccount)
                                              select t)
                                                        .AsNoTracking()
                                                        .ToList();

            if (onlyIncurred) transactions = transactions.Where(x => x.IsIncurred).ToList();

            var transactionList = new List<TransactionToExport>();
            foreach (var item in transactions)
            {
                var t = new TransactionToExport()
                {
                    Date = item.Date,
                    Nature = item.IsExpense ? "Despesa" : "Receita",
                    Value = item.Value,
                    Doc = item.Document,
                    Account = item.PaymentAccount.Name,
                };
                if (item.TransactionType.Name == "Rodrigo")
                {
                    t.Category = "Retiradas";
                    t.Description = "Rodrigo";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Ricardo")
                {
                    t.Category = "Retiradas";
                    t.Description = "Ricardo";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Alisson")
                {
                    t.Category = "Retiradas";
                    t.Description = "Alisson";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "João Victor")
                {
                    t.Category = "RH";
                    t.Description = "João Victor";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Seguro" || item.TransactionType.Name == "Manutenção da Conta")
                {
                    t.Category = "Tarifas";
                    t.Description = $"{item.Description} ({item.Notes})";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Despesas de Viagem")
                {
                    t.Category = "Vendas";
                    t.Description = $"{item.Description} ({item.Notes})";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Subcontratações")
                {
                    t.Category = "Projetos";
                    t.Description = $"{item.Description} ({item.Notes})";
                    transactionList.Add(t);
                    continue;
                }
                if ((item.TransactionType.Name == "Resgate" || item.TransactionType.Name == "Aplicação" || item.TransactionType.Name == "Rendimento") && item.PaymentAccount.Name == "BB")
                {
                    t.Category = item.TransactionType.Name;
                    t.Description = "BB Rende Fácil -";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "IRRF" && item.PaymentAccount.Name == "BB Rende Fácil")
                {
                    t.Category = "Impostos Fixos";
                    t.Description = $"{item.TransactionType.Name} - {item.Description}";
                    t.Account = "BB";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Rendimento" && item.PaymentAccount.Name == "BB Rende Fácil")
                {
                    t.Category = item.TransactionType.Name;
                    t.Description = "BB Rende Fácil -";
                    t.Account = "BB";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Rendimento" && item.PaymentAccount.Name == "Inter CDB")
                {
                    t.Category = item.TransactionType.Name;
                    t.Description = "CDB POS DI LIQ. BANCO INTER SA";
                    t.Account = "Inter";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Resgate" && item.PaymentAccount.Name == "Inter")
                {
                    t.Category = item.TransactionType.Name;
                    t.Description = "RESGATE - CDB POS DI LIQ. BANCO INTER SA";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "Aplicação" && item.PaymentAccount.Name == "Inter")
                {
                    t.Category = item.TransactionType.Name;
                    t.Description = "APLICAÇÃO - CDB POS DI LIQ. BANCO INTER SA";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("Transferência entre contas") && item.PaymentAccount.Name != "BB Rende Fácil" && item.PaymentAccount.Name != "Inter CDB")
                {
                    t.Category = "Movimentação";
                    t.Description = item.Description;
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("Transferência entre contas") && (item.PaymentAccount.Name == "BB Rende Fácil" || item.PaymentAccount.Name == "Inter CDB"))
                {
                    continue;
                }
                if (item.TransactionType.Name.Contains("Honorários Contábeis"))
                {
                    t.Category = "Administrativo";
                    t.Description = item.TransactionType.Name;
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("Unimed"))
                {
                    t.Category = "Retiradas";
                    t.Description = item.TransactionType.Name;
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("Patrocínio"))
                {
                    t.Category = "Marketing";
                    t.Description = "Patrocínio " + item.Description;
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("Plano Móvel"))
                {
                    t.Category = "Telecom";
                    t.Description = "Plano empresarial - Vivo";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("Seguro de vida em Grupo"))
                {
                    t.Category = "RH";
                    t.Description = "Seguro de vida em Grupo";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("Parcela BDMG"))
                {
                    t.Category = "Empréstimo";
                    t.Description = "Empréstimo BDMG " + item.Description;
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("Endereço Fiscal"))
                {
                    t.Category = "Administrativo";
                    t.Description = "Jobly";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("INSS"))
                {
                    t.Category = "Impostos Fixos";
                    t.Description = "INSS";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "ISS")
                {
                    t.Category = "Impostos Venda";
                    t.Description = "ISS";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "IRPJ")
                {
                    t.Category = "Impostos Venda";
                    t.Description = "IRPJ";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "CSLL")
                {
                    t.Category = "Impostos Venda";
                    t.Description = "CSLL";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "PIS")
                {
                    t.Category = "Impostos Venda";
                    t.Description = "Pis";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name == "COFINS")
                {
                    t.Category = "Impostos Venda";
                    t.Description = "COFINS";
                    transactionList.Add(t);
                    continue;
                }
                if (item.TransactionType.Name.Contains("QForm") || item.TransactionType.Name.Contains("Serviço"))
                {
                    t.Category = item.TransactionType.Name;
                    t.Description = $"{item.Description} - {item.Notes} - {item.Document}";
                    transactionList.Add(t);
                    continue;
                }
                if (item.PaymentAccount.Name == "Inter CDB" || item.PaymentAccount.Name == "BB Rende Fácil")
                {
                    continue;
                }
                t.Category = item.TransactionType.Name;
                t.Description = item.Description;
                t.Notes = item.Notes;
                transactionList.Add(t);
            }
            return transactionList;
        }
    }

    internal class TransactionToExport
    {
        public DateTime Date { get; set; }
        public string Nature { get; set; }
        public decimal Value { get; set; }
        public string Doc { get; set; }
        public string Account { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Notes { get; internal set; }
    }
}
