using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models.FinanceModels;
using System.Xml.Linq;
using Transaction = RLBW_ERP.Models.FinanceModels.Transaction;
using System.Collections.Generic;
using RLBW_ERP.Models.ViewModels;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;

namespace RLBW_ERP.Controllers.FinanceControllers
{
    [Authorize(Roles = "SuperAdmin,Director")]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,Date,IsExpense,IsIncurred,TypeId,Description,Value,Document,PaymentAccountId,Notes,BankTransactionId";

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(
            string isExpenseStr,
            string isIncurredStr,
            int? category,
            int? subcategory,
            int? type,
            int? paymentAccount,
            string dateStart,
            string dateEnd,
            string searchstring,
            string sortOrder,
            string currentFilter,
            int? pageNumber,
            int? pageSize)
        {
            if (searchstring != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchstring = currentFilter;
            }

            var transactions = GetTransactionsList(true, true);

            int count = transactions.Count();
            if (!string.IsNullOrEmpty(searchstring) && searchstring != "undefined")
            {
                transactions = transactions.Where(a => a.Description.Contains(searchstring));
            }
            count = transactions.Count();
            if (category != null && category > 0)
            {
                transactions = transactions.Where(x => x.TransactionType.Subcategory.CategoryId == category);
            }
            count = transactions.Count();
            if (subcategory != null && subcategory > 0)
            {
                transactions = transactions.Where(x => x.TransactionType.SubcategoryId == subcategory);
            }
            count = transactions.Count();
            if (type != null && type > 0)
            {
                transactions = transactions.Where(x => x.TypeId == type);
            }
            count = transactions.Count();
            if (paymentAccount != null && paymentAccount > 0)
            {
                transactions = transactions.Where(x => x.PaymentAccountId == paymentAccount);
            }
            count = transactions.Count();
            if (isExpenseStr != null && isExpenseStr != "undefined")
            {
                bool exp = isExpenseStr == "true";
                transactions = transactions.Where(x => x.IsExpense == exp);
            }
            count = transactions.Count();
            if (isIncurredStr != null && isIncurredStr != "undefined")
            {
                bool icr = isIncurredStr == "true";
                transactions = transactions.Where(x => x.IsIncurred == icr);
            }
            count = transactions.Count();
            if (dateStart != null && dateStart != "undefined")
            {
                transactions = transactions.Where(x => x.Date >= DateTime.Parse(dateStart));
                count = transactions.Count();
            }
            if (dateEnd != null && dateEnd != "undefined")
            {
                transactions = transactions.Where(x => x.Date <= DateTime.Parse(dateEnd));
            }
            count = transactions.Count();
            transactions = ApplySortOrder(transactions, sortOrder);

            pageSize ??= 25;

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            string[] sortableFields = { "IsIncurred", "IsExpense", "TypeId", "PaymentAccountId", "Value", "Date", "Category", "Description", "Subcategory" };
            Dictionary<string, string> viewDataKeys = new()
                {
                    { "searchstring", searchstring },
                    { "pageSize", pageSize.ToString() },
                    { "isExpense", isExpenseStr },
                    { "isIncurred", isIncurredStr },
                    { "dateStart", dateStart },
                    { "dateEnd", dateEnd },
                    { "category", category.ToString() },
                    { "subcategory", subcategory.ToString() },
                    { "type", type.ToString() },
                    { "paymentAccount", paymentAccount.ToString() },
                    { "CurrentPageSize", pageSize.ToString() }
                };
            SetupViewDataForFilters(viewDataKeys);
            SetupViewDataForSorting(sortOrder, sortableFields);
            SetupViewDataForSelectInputs<TransactionCategory>("TransactionCategory");
            SetupViewDataForSelectInputs<TransactionSubcategory>("TransactionSubcategory");
            SetupViewDataForSelectInputs<TransactionType>("TransactionType");
            SetupViewDataForSelectInputs<PaymentAccount>("PaymentAccount");

            return View(await PaginetedList<TransactionsViewModel>.CreateAsync(transactions.AsQueryable().AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> FilteredIndex(
            string isExpenseStr,
            string isIncurredStr,
            int? category,
            int? subcategory,
            int? type,
            int? paymentAccount,
            string dateStart,
            string dateEnd,
            string searchstring,
            string sortOrder,
            string currentFilter,
            int? pageNumber,
            int? pageSize)
        {
            if (searchstring != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchstring = currentFilter;
            }

            var transactions = GetTransactionsList(true, true);

            int count = transactions.Count();
            if (!string.IsNullOrEmpty(searchstring) && searchstring != "undefined")
            {
                transactions = transactions.Where(a => a.Description.Contains(searchstring));
            }
            count = transactions.Count();
            if (category != null && category > 0)
            {
                var categoryName = _context.TransactionCategories.Find(category).Name;
                transactions = transactions.Where(x => x.CategoryName == categoryName);
            }
            count = transactions.Count();
            if (subcategory != null && subcategory > 0)
            {
                var subcategoryName = _context.TransactionSubcategories.Find(subcategory).Name;
                transactions = transactions.Where(x => x.SubcategoryName == subcategoryName);
            }
            count = transactions.Count();
            if (type != null && type > 0)
            {
                var typeName = _context.TransactionTypes.Find(type).Name;
                transactions = transactions.Where(x => x.TypeName == typeName);
            }
            count = transactions.Count();
            if (paymentAccount != null && paymentAccount > 0)
            {
                transactions = transactions.Where(x => x.PaymentAccountId == paymentAccount);
            }
            count = transactions.Count();
            if (isExpenseStr != null && isExpenseStr != "undefined" && isExpenseStr != "0")
            {
                bool isExpense = isExpenseStr == "true";
                transactions = transactions.Where(x => x.IsExpense == isExpense);
            }
            count = transactions.Count();
            if (isIncurredStr != null && isIncurredStr != "undefined" && isIncurredStr != "0")
            {
                bool isIncurred = isIncurredStr == "true";
                transactions = transactions.Where(x => x.IsIncurred == isIncurred);
            }
            count = transactions.Count();
            if (dateStart != null && dateStart != "undefined")
            {
                transactions = transactions.Where(x => x.Date >= DateTime.Parse(dateStart));
                count = transactions.Count();
            }
            if (dateEnd != null && dateEnd != "undefined")
            {
                transactions = transactions.Where(x => x.Date <= DateTime.Parse(dateEnd));
            }
            count = transactions.Count();
            transactions = ApplySortOrder(transactions, sortOrder);

            pageSize ??= 25;

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            string[] sortableFields = { "IsIncurred", "IsExpense", "TypeId", "PaymentAccountId", "Value", "Date", "Category", "Description", "Subcategory" };
            Dictionary<string, string> viewDataKeys = new()
                {
                    { "searchstring", searchstring },
                    { "pageSize", pageSize.ToString() },
                    { "isExpense", isExpenseStr },
                    { "isIncurred", isIncurredStr },
                    { "dateStart", dateStart },
                    { "dateEnd", dateEnd },
                    { "category", category.ToString() },
                    { "subcategory", subcategory.ToString() },
                    { "type", type.ToString() },
                    { "paymentAccount", paymentAccount.ToString() },
                    { "CurrentPageSize", pageSize.ToString() }
                };
            SetupViewDataForFilters(viewDataKeys);
            SetupViewDataForSorting(sortOrder, sortableFields);
            SetupViewDataForSelectInputs<TransactionCategory>("TransactionCategory");
            SetupViewDataForSelectInputs<TransactionSubcategory>("TransactionSubcategory");
            SetupViewDataForSelectInputs<TransactionType>("TransactionType");
            SetupViewDataForSelectInputs<PaymentAccount>("PaymentAccount");


            return PartialView("_IndexPartialView", await PaginetedList<TransactionsViewModel>.CreateAsync(transactions.AsQueryable().AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        private IQueryable<TransactionsViewModel> GetTransactionsList(bool skipPartnerInvestment, bool skipInvestment)
        {
            IQueryable<TransactionsViewModel> transactions = (from t in _context.Transactions
                                                                    .Include(x => x.TransactionType)
                                                                        .ThenInclude(x => x.Subcategory)
                                                                            .ThenInclude(x => x.Category)
                                                                    .Include(x => x.PaymentAccount)
                                                              select new TransactionsViewModel()
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
                                                              }).AsNoTracking();
            if (skipPartnerInvestment)
            {
                transactions = transactions.Where(x =>
                    x.PaymentAccountName != "Rodrigo" &&
                    x.PaymentAccountName != "Alisson" &&
                    x.PaymentAccountName != "Ricardo"
                    );
            }
            if (skipInvestment)
            {
                transactions = transactions.Where(x =>
                    x.TypeName != "Aplicação" &&
                    x.TypeName != "Resgate" &&
                    !x.TypeName.Contains("Transferência")
                    );
            }
            return transactions;
        }
        private void SetupViewDataForFilters(Dictionary<string, string> viewDataKeys)
        {
            foreach (var viewDataKey in viewDataKeys)
            {
                ViewData[viewDataKey.Key] = viewDataKey.Value;
            }
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
        private static IQueryable<TransactionsViewModel> ApplySortOrder(IQueryable<TransactionsViewModel> transactions, string sortOrder)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "Date_desc";
            }

            transactions = sortOrder switch
            {
                "IsIncurred" => transactions.OrderBy(s => s.IsIncurred),
                "IsIncurred_desc" => transactions.OrderByDescending(s => s.IsIncurred),
                "IsExpense" => transactions.OrderBy(s => s.IsExpense),
                "IsExpense_desc" => transactions.OrderByDescending(s => s.IsExpense),
                "TypeId" => transactions.OrderBy(s => s.TypeId),
                "TypeId_desc" => transactions.OrderByDescending(s => s.TypeId),
                "PaymentAccountId" => transactions.OrderBy(s => s.PaymentAccountId),
                "PaymentAccountId_desc" => transactions.OrderByDescending(s => s.PaymentAccountId),
                "Value" => transactions.OrderBy(s => s.Value),
                "Value_desc" => transactions.OrderByDescending(s => s.Value),
                "Date" => transactions.OrderBy(s => s.Date),
                "Date_desc" => transactions.OrderByDescending(s => s.Date),
                "Description" => transactions.OrderBy(s => s.Description),
                "Description_desc" => transactions.OrderByDescending(s => s.Description),
                "Category" => transactions.OrderBy(s => s.TransactionType.Subcategory.Category.Name),
                "Category_desc" => transactions.OrderByDescending(s => s.TransactionType.Subcategory.Category.Name),
                "Subcategory" => transactions.OrderBy(s => s.TransactionType.Subcategory.Name),
                "Subcategory_desc" => transactions.OrderByDescending(s => s.TransactionType.Subcategory.Name),
                _ => transactions.OrderByDescending(s => s.Date),
            };
            return transactions;
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transactions = await _context.Transactions
                .Include(t => t.PaymentAccount)
                .Include(t => t.TransactionType)
                    .ThenInclude(x => x.Subcategory)
                        .ThenInclude(x => x.Category)
                .Include(x => x.Budget)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactions == null)
            {
                return NotFound();
            }

            return View(transactions);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            var categoryList = _context.Set<TransactionCategory>().Where(x => !x.IsExpense).ToList();
            categoryList.Insert(0, new TransactionCategory { Id = 0, Name = "Selecione" });
            ViewData["Id"] = new SelectList(categoryList, "Id", "Name");
            ViewData["PaymentAccountId"] = new SelectList(_context.Set<PaymentAccount>(), "Id", "Name");
            ViewData["SubcategoryId"] = new SelectList(_context.Set<TransactionSubcategory>(), "Id", "Name");
            ViewData["TypeId"] = new SelectList(_context.Set<TransactionType>(), "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] Transaction transaction)
        {
            //transactions.BankTransactionId ??= Guid.NewGuid().ToString();
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "MonthBalances", new { year = transaction.Date.Year, month = transaction.Date.Month });

            }
            var categoryList = _context.Set<TransactionCategory>().Where(x => !x.IsExpense).ToList();
            categoryList.Insert(0, new TransactionCategory { Id = 0, Name = "Selecione" });
            ViewData["Id"] = new SelectList(categoryList, "Id", "Name", transaction.TransactionType.Subcategory.CategoryId);
            ViewData["PaymentAccountId"] = new SelectList(_context.Set<PaymentAccount>(), "Id", "Name", transaction.PaymentAccountId);
            ViewData["SubcategoryId"] = new SelectList(_context.Set<TransactionSubcategory>(), "Id", "Name", transaction.TransactionType.SubcategoryId);
            ViewData["TypeId"] = new SelectList(_context.Set<TransactionType>(), "Id", "Name", transaction.TypeId);

            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(x => x.TransactionType)
                .ThenInclude(x => x.Subcategory)
                .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            var categoryList = _context.Set<TransactionCategory>().Where(x => x.IsExpense == transaction.IsExpense).ToList();
            var subcategoryList = _context.Set<TransactionSubcategory>().Where(x => x.CategoryId == transaction.TransactionType.Subcategory.CategoryId);
            var typeList = _context.Set<TransactionType>().Where(x => x.SubcategoryId == transaction.TransactionType.SubcategoryId);
            ViewData["PaymentAccountId"] = new SelectList(_context.Set<PaymentAccount>(), "Id", "Name", transaction.PaymentAccountId);
            ViewData["Id"] = new SelectList(categoryList, "Id", "Name", transaction.TransactionType.Subcategory.CategoryId);
            ViewData["SubcategoryId"] = new SelectList(subcategoryList, "Id", "Name", transaction.TransactionType.SubcategoryId);
            ViewData["TypeId"] = new SelectList(typeList, "Id", "Name", transaction.TypeId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionsExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Details", "MonthBalances", new { year = transaction.Date.Year, month = transaction.Date.Month });
            }
            ViewData["PaymentAccountId"] = new SelectList(_context.Set<PaymentAccount>(), "Id", "Name", transaction.PaymentAccountId);
            ViewData["TypeId"] = new SelectList(_context.Set<TransactionType>(), "Id", "Name", transaction.TypeId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transactions = await _context.Transactions
                .Include(t => t.PaymentAccount)
                .Include(t => t.TransactionType)
                    .ThenInclude(x => x.Subcategory)
                        .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactions == null)
            {
                return NotFound();
            }

            return View(transactions);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Transactions'  is null.");
            }
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "MonthBalances", new { year = transaction.Date.Year, month = transaction.Date.Month });
        }

        [HttpPost]
        public List<TransactionsViewModel> LoadOfx([FromForm] IFormFile ofxFile)
        {
            if (ofxFile == null || ofxFile.Length == 0)
            {
                return null;
            }

            string tempFilePath = Path.GetTempFileName();

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                ofxFile.CopyTo(stream);
            }

            XElement doc = ImportOfx.ToXElement(tempFilePath);

            var bank = from b in doc.Descendants("BANK") select b.Value;
            var paymentAccount = _context.PaymentAccounts.Where(x => x.Notes.Contains(bank.First())).First();

            var importedData = from c in doc.Descendants("STMTTRN")
                               select new TransactionsViewModel
                               {
                                   Value = decimal.Parse(c.Element("TRNAMT").Value.Replace("-", ""), NumberFormatInfo.InvariantInfo),
                                   Date = DateTime.ParseExact(c.Element("DTPOSTED").Value, "yyyyMMdd", null),
                                   Description = c.Element("MEMO").Value,
                                   IsExpense = decimal.Parse(c.Element("TRNAMT").Value, NumberFormatInfo.InvariantInfo) < 0,
                                   IsIncurred = true,
                                   PaymentAccountId = paymentAccount.Id,
                                   PaymentAccountName = paymentAccount.Name,
                                   BankTransactionId = c.Element("FITID").Value,
                                   StatementLine = c.Element("DTPOSTED").Value + " " + c.Element("MEMO").Value + " " + c.Element("TRNAMT").Value + " " + c.Element("FITID").Value,
                                   Locked = false,
                               };
            var treatedData = new List<TransactionsViewModel>();
            var transactionTypes = _context.TransactionTypes.Include(x => x.Subcategory).ThenInclude(x => x.Category).ToList();
            var dbTransactions = _context.Transactions.Include(x => x.TransactionType).ThenInclude(x => x.Subcategory).ThenInclude(x => x.Category).ToList();
            foreach (var transaction in importedData)
            {
                TransactionsViewModel tempTransaction;
                if (dbTransactions.Any(x => x.BankTransactionId == transaction.BankTransactionId))
                {
                    var dbTrs = dbTransactions.FirstOrDefault(x => x.BankTransactionId == transaction.BankTransactionId);
                    tempTransaction = new()
                    {
                        Id = dbTrs.Id,
                        Locked = true,
                        IsIncurred = true,
                        BankTransactionId = dbTrs.BankTransactionId,
                        Date = dbTrs.Date,
                        Description = dbTrs.Description,
                        Document = dbTrs.Document,
                        IsExpense = dbTrs.IsExpense,
                        Notes = dbTrs.Notes,
                        PaymentAccountId = dbTrs.PaymentAccountId,
                        StatementLine = dbTrs.StatementLine,
                        TypeId = dbTrs.TypeId,
                        TransactionType = dbTrs.TransactionType,
                        Value = dbTrs.Value
                    };
                }
                else
                {
                    tempTransaction = TreatTransaction(transaction, transactionTypes);
                }
                treatedData.Add(tempTransaction);
            }

            System.IO.File.Delete(tempFilePath);
            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();
            ViewData["TypeId"] = new SelectList(_context.Set<TransactionType>(), "Id", "Name");
            ViewData["Id"] = new SelectList(_context.Set<TransactionCategory>(), "Id", "Name");
            ViewData["SubcategoryId"] = new SelectList(_context.Set<TransactionSubcategory>(), "Id", "Name");
            ViewData["PaymentAccountId"] = new SelectList(_context.Set<PaymentAccount>(), "Id", "Name");

            return treatedData;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFile([FromForm] List<Transaction> transactions)
        {
            if (transactions.Count == 0)
            {
                return View(transactions);
            }
            foreach (var t in transactions)
            {
                if (_context.Transactions.Find(t.Id) != null)
                {
                    _context.Entry(t).State = EntityState.Modified;
                }
                else
                {
                    _context.Add(t);
                };
            }
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View(transactions);
            }
        }

        private static TransactionsViewModel TreatTransaction(TransactionsViewModel transaction, IEnumerable<TransactionType> transactionTypes)
        {
            transaction.IsIncurred = true;
            if (transaction.IsExpense) return TreatExpense(transaction, transactionTypes);
            return TreatIncome(transaction, transactionTypes);
        }

        private static TransactionsViewModel TreatIncome(TransactionsViewModel transaction, IEnumerable<TransactionType> transactionTypes)
        {
            if (transaction.Description.Contains("UNIFORJA", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Uniforja - Parcela xx/xx";
                transaction.Document = "NF XXX";
                transaction.Notes = "OC XXX";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "QForm - L. Temp.");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("ACOFORJA", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Açoforja - Parcela xx/xx";
                transaction.Document = "NF XXX";
                transaction.Notes = "OC XXX";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "QForm - L. Perm.");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("ARCO FORJADO", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Onix - Parcela xx/xx";
                transaction.Document = "NF XXX";
                transaction.Notes = "OC XXX";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "QForm - L. Temp.");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("SIDERURGICA SAO JOAQUIM", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "SJ - Parcela xx/xx";
                transaction.Document = "NF XXX";
                transaction.Notes = "OC XXX";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "QForm - L. Perm.");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("VIEMAR INDUSTRIA E", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Viemar - Parcela xx/xx";
                transaction.Document = "NF XXX";
                transaction.Notes = "OC XXX";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "QForm - L. Perm.");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("BB Rende F", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("CDB POS DI LIQ. BANCO INTER", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Resgate da Renda Fixa " + transaction.PaymentAccountName;
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Resgate");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Id == 79);
            transaction.TypeId = transaction.TransactionType.Id;
            return transaction;
        }

        private static TransactionsViewModel TreatExpense(TransactionsViewModel transaction, IEnumerable<TransactionType> transactionTypes)
        {
            if (transaction.PaymentAccountName.Contains("CC", StringComparison.OrdinalIgnoreCase) &&
                (transaction.Description.Contains("Dell") || transaction.Description.Contains("Samsung")))
            {
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Aquisição Equipamentos");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Microsoft*store"))
            {
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Licença Software");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Zoom.us"))
            {
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Infraestrutura de Vendas");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.PaymentAccountName.Contains("CC", StringComparison.OrdinalIgnoreCase) &&
                (!transaction.Description.Contains("Dell") && !transaction.Description.Contains("Samsung")))
            {
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Despesas de Viagem");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("BB Rende F", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("CDB POS DI LIQ. BANCO INTER", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Aplicação na Renda Fixa " + transaction.PaymentAccountName;
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Aplicação");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("CONVENIO - ISS", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("PREF. MUN. SABARA", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "ISS Ref " + transaction.Date.AddMonths(-1).ToString("MMM/yyyy");
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "ISS");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("IRPJ", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "IRPJ Ref " + transaction.Date.AddMonths(-1).ToString("MMM/yyyy");
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "IRPJ");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("CSLL", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "CSLL Ref " + transaction.Date.AddMonths(-1).ToString("MMM/yyyy");
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "CSLL");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("IOF", StringComparison.OrdinalIgnoreCase) && transaction.PaymentAccountName.Contains("CC"))
            {
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "IOF");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("IOF", StringComparison.OrdinalIgnoreCase) && !transaction.PaymentAccountName.Contains("CC"))
            {
                transaction.Description = "IOF Ref ";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "IOF");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("IRRF", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "IRRF Ref ";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "IRRF");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("INSS", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "INSS Ref " + transaction.Date.AddMonths(-1).ToString("MMM/yyyy");
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "INSS");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("TFLF", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "TFLF Ref " + transaction.Date.ToString("yyyy");
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "TFLF");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("PIS", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "PIS Ref " + transaction.Date.AddMonths(-1).ToString("MMM/yyyy");
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "PIS");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Cofins", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "COFINS Ref " + transaction.Date.AddMonths(-1).ToString("MMM/yyyy");
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "COFINS");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Rodrigo Lobenwein Resende", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Rodrigo";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Rodrigo");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Alisson Duarte da Silva", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Alisson";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Alisson");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Ricardo Antonio Micheletti Viana", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Ricardo";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Ricardo");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("João Victor Do Carmo", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "João Victor";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "João Victor");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Fundacao Luiz Englert", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Patrocínio Senafor";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Patrocínio");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("PAGAMENTO FATURA INTER", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Fatura Cartão Inter - Excluir ao lançar todas as entradas";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Despesas de Viagem");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Unimed cop Rodrigo", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("cop. Rodrigo", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Unimed - Rodrigo - Coparticipação";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Unimed - Rodrigo - Coparticipação");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Unimed Mens Rodrigo", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("mens. Rodrigo", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Unimed - Rodrigo - Mensalidade";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Unimed - Rodrigo - Mensalidade");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Unimed cop Ricardo", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("cop. Ricardo", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Unimed - Ricardo - Mensalidade";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Unimed - Ricardo - Coparticipação");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Unimed Mens Ricardo", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("mens. Ricardo", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Unimed - Ricardo - Mensalidade";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Unimed - Ricardo - Mensalidade");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("VIVO MOVEL", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("telefone", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Plano empresarial - Vivo";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Plano Móvel");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("PAGAMENTO DE CONVENIO - Alvar", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Alvará Ref " + transaction.Date.ToString("yyyy") + "-" + transaction.Date.AddYears(1).ToString("yyyy");
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Alvará");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("JEAN WESLEY MORAIS RIBEIRO", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Manutenção PC XXX";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Manutenção" && x.Subcategory.Name == "TI");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Jobly", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Endereço Fiscal - Jobly";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Endereço Fiscal");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Aprenda Eventos Tecnicos", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Patrocínio Congresso Conformação - Grupo Aprenda";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Patrocínio");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("Honorarios Contabeis", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("Honorários Contábeis", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("HonorÃ¡rios ContÃ¡beis", StringComparison.OrdinalIgnoreCase) ||
                transaction.Description.Contains("Roati", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Honorários Contábeis - Roati";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Honorários Contábeis");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("seguro de vida em gr", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Seguro de Vida em Grupo";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Seguro de vida em Grupo");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("MENSALIDADE SEGURO - BBSEGUROS", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Seguro patrimonial (Isenta manutenção da conta)";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Manutenção da Conta");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("C�MBIO", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Câmbio - PR_" + DateTime.UtcNow.ToString("yy");
                transaction.Document = "Inv Micas ";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Royalties");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("TARIFA ENVIO OPE", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "Tarifa Envio OPE - PR_" + DateTime.UtcNow.ToString("yy");
                transaction.Document = "Inv Micas ";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "Outras Tarifas");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            if (transaction.Description.Contains("RETEN��O IMPOSTO DE RENDA", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Description = "IRRF - PR_" + DateTime.UtcNow.ToString("yy");
                transaction.Document = "Inv Micas ";
                transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Name == "IRRF");
                transaction.TypeId = transaction.TransactionType.Id;
                return transaction;
            }
            transaction.TransactionType = transactionTypes.FirstOrDefault(x => x.Id == 66);
            transaction.TypeId = transaction.TransactionType.Id;
            return transaction;
        }



        [HttpPost("UploadFile")]
        public IActionResult UploadFile([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Nenhum arquivo enviado.");

                if (IsFileType(file, ".ofx"))
                {
                    return View(LoadOfx(file));
                }
                else if (IsFileType(file, ".csv"))
                {
                    return View(LoadCSV(file));
                }
                else
                {
                    return BadRequest("Arquivo inválido. Apenas arquivos OFX e CSV são suportados.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }


        private List<TransactionsViewModel> LoadCSV(IFormFile file)
        {
            try
            {
                using var streamReader = new StreamReader(file.OpenReadStream());
                using var parser = new TextFieldParser(streamReader);
                parser.SetDelimiters(",");

                var importedData = new List<TransactionsViewModel>();
                string paymentAccountName = "";
                DateTime date = default;
                decimal totalAmount = 0;
                var cultureInfo = new CultureInfo("pt-BR");
                while (!parser.EndOfData)
                {
                    var line = parser.ReadFields();
                    if (line.Contains("Data")) continue;
                    if (line[0].Contains("Cartão") && line[1].Contains("2506"))
                    {
                        paymentAccountName = "CC Inter";
                        continue;
                    }
                    if (line[0].Contains("Vencimento"))
                    {
                        var temp = line[1].Split("/");
                        var day = int.Parse(temp[0].Replace("\"", ""));
                        var month = int.Parse(temp[1].Replace("\"", ""));
                        var year = DateTime.Now.Year;
                        date = new DateTime(year, month, day);
                        continue;
                    }
                    if (line.Length > 0)
                    {
                        if (line[1].Contains("Pagto Debito")) continue;
                        var valueString = line[4].Replace("R$ ", "", true, cultureInfo);
                        decimal value = valueString == "" ? 0 : decimal.Parse(valueString);
                        if (line[0].Contains("Total")) continue;
                        if (value != 0)
                        {
                            importedData.Add(new TransactionsViewModel()
                            {
                                Date = date,
                                IsExpense = value < 0,
                                Description = line[1] + (line[3].Contains("Parcela") ? " " + line[3] : (value > 0 ? "(Estorno)" : "")),
                                Value = Math.Abs(value),
                                PaymentAccountName = paymentAccountName,
                                Notes = "Data da compra: " + line[0],
                                StatementLine = string.Join(" ", line)
                            });
                        }
                    }
                }
                totalAmount = importedData.Sum(x => x.Value);
                var treatedData = new List<TransactionsViewModel>();
                var transactionTypes = _context.TransactionTypes
                                            .Include(x => x.Subcategory)
                                                .ThenInclude(x => x.Category)
                                            .AsNoTracking().ToList();
                var dbTransactions = _context.Transactions
                                            .Include(x => x.TransactionType)
                                                .ThenInclude(x => x.Subcategory)
                                                    .ThenInclude(x => x.Category)
                                            .AsNoTracking().ToList();

                var paymentAccountsList = _context.PaymentAccounts.ToList();

                foreach (var transaction in importedData)
                {
                    TransactionsViewModel tempTransaction;
                    if (dbTransactions.Any(x => x.BankTransactionId == transaction.BankTransactionId))
                    {
                        var dbTrs = dbTransactions.FirstOrDefault(x => x.BankTransactionId == transaction.BankTransactionId);
                        tempTransaction = new()
                        {
                            Id = dbTrs.Id,
                            Locked = true,
                            IsIncurred = true,
                            BankTransactionId = dbTrs.BankTransactionId,
                            Date = dbTrs.Date,
                            Description = dbTrs.Description,
                            Document = dbTrs.Document,
                            IsExpense = dbTrs.IsExpense,
                            Notes = dbTrs.Notes,
                            PaymentAccountId = dbTrs.PaymentAccountId,
                            StatementLine = dbTrs.StatementLine,
                            TypeId = dbTrs.TypeId,
                            TransactionType = dbTrs.TransactionType,
                            Value = dbTrs.Value
                        };
                    }
                    else
                    {
                        tempTransaction = TreatTransaction(transaction, transactionTypes);
                        tempTransaction.PaymentAccountId = paymentAccountsList.First(x => x.Name == paymentAccountName).Id;
                        tempTransaction.Notes += " Fatura: R$ " + totalAmount;
                        tempTransaction.BankTransactionId = $"Fatura_{paymentAccountName}_({Guid.NewGuid().ToString()[..8]})";
                    }
                    treatedData.Add(tempTransaction);
                }
                GetUrl url = new(HttpContext);
                ViewData["Url"] = url.GetCurrentUrl();
                ViewData["TypeId"] = new SelectList(_context.Set<TransactionType>(), "Id", "Name");
                ViewData["Id"] = new SelectList(_context.Set<TransactionCategory>(), "Id", "Name");
                ViewData["SubcategoryId"] = new SelectList(_context.Set<TransactionSubcategory>(), "Id", "Name");
                ViewData["PaymentAccountId"] = new SelectList(_context.Set<PaymentAccount>(), "Id", "Name");

                return treatedData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static bool IsFileType(IFormFile file, string extension)
        {
            return Path.GetExtension(file.FileName).Equals(extension, StringComparison.OrdinalIgnoreCase);
        }
        private bool TransactionsExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }

        public JsonResult GetSubcategoriesList(int categoryId)
        {
            var subcategoriesList = (from subcategories in _context.TransactionSubcategories
                                     where subcategories.CategoryId == categoryId
                                     orderby subcategories.Name
                                     select subcategories).ToList();

            return Json(new SelectList(subcategoriesList, "Id", "Name"));
        }
        public JsonResult GetCategoriesList(bool isExpense)
        {
            var categoriesList = (from categories in _context.TransactionCategories
                                  where categories.IsExpense == isExpense
                                  orderby categories.Name
                                  select categories).ToList();

            return Json(new SelectList(categoriesList, "Id", "Name"));
        }

        public JsonResult GetTypesList(int subcategoryId)
        {
            var typesList = (from types in _context.TransactionTypes
                             where types.SubcategoryId == subcategoryId
                             orderby types.Name
                             select types).ToList();

            return Json(new SelectList(typesList, "Id", "Name"));
        }
    }
}
