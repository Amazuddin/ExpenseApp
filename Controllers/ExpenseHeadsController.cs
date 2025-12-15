using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseApp.Contexts;
using ExpenseApp.Models;

namespace ExpenseApp.Controllers
{
    public class ExpenseHeadsController : Controller
    {
        private readonly ExpenseAppContext _context;

        public ExpenseHeadsController(ExpenseAppContext context)
        {
            _context = context;
        }

        // GET: ExpenseHeads
        public async Task<IActionResult> Index()
        {
              return _context.ExpenseHeads != null ? 
                          View(await _context.ExpenseHeads.ToListAsync()) :
                          Problem("Entity set 'ExpenseAppContext.ExpenseHeads'  is null.");
        }

        // GET: ExpenseHeads/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ExpenseHeads == null)
            {
                return NotFound();
            }

            var expenseHead = await _context.ExpenseHeads
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expenseHead == null)
            {
                return NotFound();
            }

            return View(expenseHead);
        }

        // GET: ExpenseHeads/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ExpenseHeads/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ExpenseHead expenseHead)
        {
            if (ModelState.IsValid)
            {
                _context.Add(expenseHead);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expenseHead);
        }

        // GET: ExpenseHeads/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ExpenseHeads == null)
            {
                return NotFound();
            }

            var expenseHead = await _context.ExpenseHeads.FindAsync(id);
            if (expenseHead == null)
            {
                return NotFound();
            }
            return View(expenseHead);
        }

        // POST: ExpenseHeads/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] ExpenseHead expenseHead)
        {
            if (id != expenseHead.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expenseHead);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseHeadExists(expenseHead.Id))
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
            return View(expenseHead);
        }

        // GET: ExpenseHeads/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ExpenseHeads == null)
            {
                return NotFound();
            }

            var expenseHead = await _context.ExpenseHeads
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expenseHead == null)
            {
                return NotFound();
            }

            return View(expenseHead);
        }

        // POST: ExpenseHeads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ExpenseHeads == null)
            {
                return Problem("Entity set 'ExpenseAppContext.ExpenseHeads'  is null.");
            }
            var expenseHead = await _context.ExpenseHeads.FindAsync(id);
            if (expenseHead != null)
            {
                _context.ExpenseHeads.Remove(expenseHead);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseHeadExists(int id)
        {
          return (_context.ExpenseHeads?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
