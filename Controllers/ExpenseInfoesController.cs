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
    public class ExpenseInfoesController : Controller
    {
        private readonly ExpenseAppContext _context;

        public ExpenseInfoesController(ExpenseAppContext context)
        {
            _context = context;
        }

        // GET: ExpenseInfoes
        public async Task<IActionResult> Index()
        {
            
            //Month 1st Date
            DateTime firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            string formattedDate = firstDayOfMonth.ToString("yyyy-MM-dd");
            ViewBag.StartDate = formattedDate;

            //Month Last Date
            DateTime today = DateTime.Today;
            DateTime lastDayOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            ViewBag.EndDate = lastDayOfMonth.ToString("yyyy-MM-dd"); 

            ViewBag.ExpenseHeads = await _context.ExpenseHeads.ToListAsync();
            ViewData["ExpenseHeadId"] = 0;

            var dailyExpenseContext = await _context.ExpenseInfos
                .Include(e => e.ExpenseHead).OrderBy(i => i.ExpenseDate).ToListAsync();

            var totalAmount = dailyExpenseContext.Sum(i => i.ExpenseAmount);
            var viewModel = new ExpenseViewModel { ExpenseInfo = dailyExpenseContext, TotalAmount = totalAmount };


            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SearchExpenseInput input, int? ExpenseHeadId)
        {
            //input.ExpenseDate = input.ExpenseDate.ToLocalTime().Date;
            ViewBag.StartDate = input.StartDate.Date.ToString("yyyy-MM-dd");
            ViewBag.EndDate = input.EndDate.Date.ToString("yyyy-MM-dd");
            ViewBag.ExpenseHeads = await _context.ExpenseHeads.ToListAsync();
            ViewData["ExpenseHeadId"] = ExpenseHeadId.ToString();


            var result = (ExpenseHeadId == 0)
                ? await _context.ExpenseInfos.Include(i => i.ExpenseHead)
                    .Where(d => d.ExpenseDate >= input.StartDate && d.ExpenseDate <= input.EndDate)
                    .ToListAsync()
                : await _context.ExpenseInfos.Include(i => i.ExpenseHead)
                    .Where(d => d.ExpenseDate >= input.StartDate && d.ExpenseDate <= input.EndDate &&
                                d.ExpenseHeadId == ExpenseHeadId)
                    .ToListAsync();

            var totalAmount = result.Sum(i => i.ExpenseAmount);
            var viewModel = new ExpenseViewModel { ExpenseInfo = result, TotalAmount = totalAmount };

            return View(viewModel);
        }


        // GET: ExpenseInfoes/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.ExpenseInfos == null)
            {
                return NotFound();
            }

            var expenseInfo = await _context.ExpenseInfos
                .Include(e => e.ExpenseHead)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expenseInfo == null)
            {
                return NotFound();
            }

            return View(expenseInfo);
        }

        // GET: ExpenseInfoes/Create
        public IActionResult Create()
        {
            ViewData["ExpenseHeadId"] = new SelectList(_context.ExpenseHeads, "Id", "Name");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<ExpenseInfo> expenseInfos)
        {
           
                try
                {
                //_context.Add(expenseInfo);
                _context.AddRange(expenseInfos);
                await _context.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    return Json(new { flag = 'n', msg = exception.Message.ToString() });
                }
                        
            return Json(new { flag = 'y', msg = "Data Saved Successfully" });
        }

        // GET: ExpenseInfoes/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.ExpenseInfos == null)
            {
                return NotFound();
            }

            var expenseInfo = await _context.ExpenseInfos.FindAsync(id);
            if (expenseInfo == null)
            {
                return NotFound();
            }
            ViewData["ExpenseHeadId"] = new SelectList(_context.ExpenseHeads, "Id", "Name", expenseInfo.ExpenseHeadId);
            return View(expenseInfo);
        }

        // POST: ExpenseInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,ExpenseHeadId,ExpenseDate,Description,ExpenseAmount")] ExpenseInfo expenseInfo)
        {
            if (id != expenseInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expenseInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseInfoExists(expenseInfo.Id))
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
            ViewData["ExpenseHeadId"] = new SelectList(_context.ExpenseHeads, "Id", "Name", expenseInfo.ExpenseHeadId);
            return View(expenseInfo);
        }

        // GET: ExpenseInfoes/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.ExpenseInfos == null)
            {
                return NotFound();
            }

            var expenseInfo = await _context.ExpenseInfos
                .Include(e => e.ExpenseHead)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expenseInfo == null)
            {
                return NotFound();
            }

            return View(expenseInfo);
        }

        // POST: ExpenseInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.ExpenseInfos == null)
            {
                return Problem("Entity set 'ExpenseAppContext.ExpenseInfos'  is null.");
            }
            var expenseInfo = await _context.ExpenseInfos.FindAsync(id);
            if (expenseInfo != null)
            {
                _context.ExpenseInfos.Remove(expenseInfo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseInfoExists(long id)
        {
          return (_context.ExpenseInfos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
