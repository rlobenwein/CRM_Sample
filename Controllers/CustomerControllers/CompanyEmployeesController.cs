using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CRM_Sample.Data;
using CRM_Sample.Models.CustomerModels;

namespace CRM_Sample.Controllers.CustomerControllers
{
    [Authorize]
    public class CompanyEmployeesController : Controller
    {
        private const string BIND_STRING = "PersonIdId,CompanyId,Position,Department,InitialDate,EndDate,Status,WorkEmail,WorkPhone,CellPhone,Notes";
        private readonly ApplicationDbContext _context;

        public CompanyEmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(int companyId)
        {
            var companyEmployees = _context.CompanyEmployees.Where(c=>c.CompanyId==companyId)
                .Include(c => c.Company)
                .Include(c => c.Person);
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId);
            ViewData["FiendlyName"] = company.FriendlyName;
            ViewData["CompanyId"] = company.Id;

            return View(await companyEmployees.ToListAsync());
        }

        // GET: Employees/Create
        public async Task<IActionResult> Create(int? companyId, int? personId)
        {
            CompanyEmployee companyEmployee = new CompanyEmployee();
            companyEmployee.InitialDate = DateTime.UtcNow;

            if (companyId == null && personId == null)
            {
                return NotFound();
            }
            if (companyId != null)
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Id == companyId);
                companyEmployee.CompanyId = company.Id;
                ViewData["FiendlyName"] = company.FriendlyName;
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "FriendlyName", company.Id);
            }
            else
            {
                ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(c => c.FriendlyName), "Id", "FriendlyName");
            }
            if (personId != null)
            {
                var person = await _context.People
                    .FirstOrDefaultAsync(c => c.Id == personId);
                companyEmployee.PersonId = person.Id;
                ViewData["CompanyId"] = new SelectList(_context.People.OrderBy(p => p.FirstName).ThenBy(p => p.MiddleName).ThenBy(p => p.LastName), "Id", "FullName", person.Id);
            }
            else
            {
                ViewData["CompanyId"] = new SelectList(_context.People.OrderBy(p=>p.FirstName).ThenBy(p=>p.MiddleName).ThenBy(p=>p.LastName), "Id", "FullName");
            }
            return View(companyEmployee);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? companyId,
            [Bind(BIND_STRING)] CompanyEmployee companyEmployee)
        {
            if (companyId == null)
            {
                return NotFound();
            }
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (ModelState.IsValid)
            {
                companyEmployee.CompanyId = company.Id;
                companyEmployee.LastUpdate = DateTime.UtcNow;
                _context.Add(companyEmployee);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Companies", new { id = companyEmployee.CompanyId });
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "FriendlyName", companyEmployee.CompanyId);
            ViewData["PersonId"] = new SelectList(_context.People.OrderBy(p => p.FirstName).ThenBy(p => p.MiddleName).ThenBy(p => p.LastName), "Id", "FullName", companyEmployee.PersonId);
            return View(companyEmployee);
        }

        // GET: Employees/Details/5
        //[HttpGet("{personId}/{Id}", Name = "Details")]
        public async Task<IActionResult> Details(int? personId, int? companyId)
        {
            if (companyId == null || personId == null)
            {
                return NotFound();
            }
            var companyEmployee = await _context.CompanyEmployees
                .Where(p => p.PersonId == personId && p.CompanyId == companyId)
                .Include(c => c.Person)
                .Include(c=>c.Company)
                .FirstOrDefaultAsync();
            if (companyEmployee == null)
            {
                return NotFound();
            }

            return View(companyEmployee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? personId, int? companyId)
        {
            if (companyId == null || personId == null)
            {
                return NotFound();
            }
            var companyEmployee = await _context.CompanyEmployees
                .Where(p => p.PersonId == personId && p.CompanyId == companyId)
                .Include(c => c.Person)
                .FirstOrDefaultAsync();
            if (companyEmployee == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "FriendlyName", companyEmployee.CompanyId);
            ViewData["PersonId"] = new SelectList(_context.People.OrderBy(p => p.FirstName).ThenBy(p => p.MiddleName).ThenBy(p => p.LastName), "Id", "FullName", companyEmployee.PersonId);
            return View(companyEmployee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind(BIND_STRING)] CompanyEmployee companyEmployee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    companyEmployee.LastUpdate = DateTime.Now;
                    _context.Update(companyEmployee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyEmployeeExists(companyEmployee.CompanyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Companies", new { id = companyEmployee.CompanyId });
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "FriendlyName", companyEmployee.CompanyId);
            ViewData["PersonId"] = new SelectList(_context.People.OrderBy(p => p.FirstName).ThenBy(p => p.MiddleName).ThenBy(p => p.LastName), "Id", "FullName", companyEmployee.PersonId);
            return View(companyEmployee);
        }

        // GET: Employees/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyEmployee = await _context.CompanyEmployees
                .Include(c => c.Company)
                .Include(c => c.Person)
                .FirstOrDefaultAsync(m => m.CompanyId == id);
            if (companyEmployee == null)
            {
                return NotFound();
            }

            return View(companyEmployee);
        }

        [Authorize(Roles ="SuperAdmin,Director")]
        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyEmployee = await _context.CompanyEmployees.FindAsync(id);
            _context.CompanyEmployees.Remove(companyEmployee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Companies", new { id = companyEmployee.CompanyId });
        }

        private bool CompanyEmployeeExists(int id)
        {
            return _context.CompanyEmployees.Any(e => e.CompanyId == id);
        }
    }
}
