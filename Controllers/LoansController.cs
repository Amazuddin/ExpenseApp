using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseApp.Contexts;
using ExpenseApp.Models;

namespace ExpenseApp.Controllers
{
    public class LoansController : Controller
    {
        private readonly ExpenseAppContext _context;

        public LoansController(ExpenseAppContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            var viewModel = new LoanViewModel
            {
                Loans = loans,
                TotalLoanAmount = loans.Sum(l => l.LoanAmount),
                TotalPaidAmount = loans.Sum(l => l.AmountPaid),
                TotalBalanceDue = loans.Sum(l => l.LoanAmount - l.AmountPaid)
            };

            return View(viewModel);
        }

        // GET: Loans/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .FirstOrDefaultAsync(m => m.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            // Get payment history
            var payments = await _context.LoanPayments
                .Where(p => p.LoanId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Create a view model instead of using ViewBag
            var viewModel = new LoanDetailsViewModel
            {
                Loan = loan,
                Payments = payments
            };

            return View(viewModel);
        }

        // GET: Loans/Create
        public IActionResult Create()
        {
            ViewBag.LoanDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        // POST: Loans/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Loan loan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    loan.AmountPaid = 0;
                    _context.Add(loan);
                    await _context.SaveChangesAsync();
                    return Json(new { flag = 'y', msg = "Loan saved successfully" });
                }
                catch (Exception ex)
                {
                    return Json(new { flag = 'n', msg = ex.Message });
                }
            }

            return Json(new { flag = 'n', msg = "Invalid data" });
        }

        // GET: Loans/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            // Pass info to view about whether payments have been made
            ViewBag.HasPayments = loan.AmountPaid > 0;
            ViewBag.LoanDate = loan.LoanDate.ToString("yyyy-MM-dd");

            return View(loan);
        }

        // POST: Loans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,LenderName,LoanAmount,AmountPaid,LoanDate,DueDate,Description")] Loan loan)
        {
            if (id != loan.Id)
            {
                return NotFound();
            }

            // Get the original loan from database to check if payments were made
            var originalLoan = await _context.Loans.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);

            if (originalLoan == null)
            {
                return NotFound();
            }

            // SMART VALIDATION RULE 1: If payments have been made, don't allow changing loan amount
            if (originalLoan.AmountPaid > 0 && originalLoan.LoanAmount != loan.LoanAmount)
            {
                ModelState.AddModelError("LoanAmount",
                    "Cannot change loan amount after payments have been made. ৳" +
                    originalLoan.AmountPaid.ToString("N2") + " has already been paid.");
            }

            // SMART VALIDATION RULE 2: Loan amount cannot be less than amount already paid
            if (loan.LoanAmount < loan.AmountPaid)
            {
                ModelState.AddModelError("LoanAmount",
                    "Loan amount (৳" + loan.LoanAmount.ToString("N2") +
                    ") cannot be less than amount already paid (৳" + loan.AmountPaid.ToString("N2") + ")");
            }

            // SMART VALIDATION RULE 3: Keep the original AmountPaid (prevent manual changes)
            loan.AmountPaid = originalLoan.AmountPaid;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Loan updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanExists(loan.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.HasPayments = originalLoan.AmountPaid > 0;
            ViewBag.LoanDate = loan.LoanDate.ToString("yyyy-MM-dd");
            return View(loan);
        }

        // GET: Loans/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .FirstOrDefaultAsync(m => m.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan != null)
            {
                // Delete all related payments first
                var payments = await _context.LoanPayments.Where(p => p.LoanId == id).ToListAsync();
                _context.LoanPayments.RemoveRange(payments);

                _context.Loans.Remove(loan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/AddPayment/5
        public async Task<IActionResult> AddPayment(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            ViewBag.Loan = loan;
            ViewBag.PaymentDate = DateTime.Now.ToString("yyyy-MM-dd");

            var payment = new LoanPayment { LoanId = id.Value };
            return View(payment);
        }

        // POST: Loans/AddPayment
        [HttpPost]
        public async Task<IActionResult> AddPayment([FromBody] LoanPayment payment)
       {
            ModelState.Remove("Loan");

            if (ModelState.IsValid)
            {
                try
                {
                    // Add payment
                    _context.LoanPayments.Add(payment);

                    // Update loan amount paid
                    var loan = await _context.Loans.FindAsync(payment.LoanId);
                    if (loan != null)
                    {
                        loan.AmountPaid += payment.PaymentAmount;
                        _context.Update(loan);
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { flag = 'y', msg = "Payment added successfully", loanId = payment.LoanId });
                }
                catch (Exception ex)
                {
                    return Json(new { flag = 'n', msg = ex.Message });
                }
            }

            // Return validation errors for debugging
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { flag = 'n', msg = "Invalid payment data: " + string.Join(", ", errors) });
        }

        private bool LoanExists(long id)
        {
            return _context.Loans.Any(e => e.Id == id);
        }
    }
}