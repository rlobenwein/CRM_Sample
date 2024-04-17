using CRM_Sample.Data;
using CRM_Sample.Models.SalesModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM_Sample.Controllers.CustomerControllers
{
    public class PipelinesController : Controller
    {
        private const string BIND_STRING = "Id,Stage";
        private readonly ApplicationDbContext _context;
        public PipelinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pipelines
        public async Task<IActionResult> Index()
        {
            return View(await _context.Pipelines.ToListAsync());
        }

        // GET: Pipelines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pipeline = await _context.Pipelines
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pipeline == null)
            {
                return NotFound();
            }

            return View(pipeline);
        }

        // GET: Pipelines/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pipelines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] Pipeline pipeline)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pipeline);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pipeline);
        }

        // GET: Pipelines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pipeline = await _context.Pipelines.FindAsync(id);
            if (pipeline == null)
            {
                return NotFound();
            }
            return View(pipeline);
        }

        // POST: Pipelines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] Pipeline pipeline)
        {
            if (id != pipeline.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pipeline);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PipelineExists(pipeline.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pipeline);
        }

        // GET: Pipelines/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Pipeline = await _context.Pipelines
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Pipeline == null)
            {
                return NotFound();
            }

            return View(Pipeline);
        }

        // POST: Pipelines/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pipeline = await _context.Pipelines.FindAsync(id);
            _context.Pipelines.Remove(pipeline);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PipelineExists(int id)
        {
            return _context.Pipelines.Any(e => e.Id == id);
        }
    }
}
