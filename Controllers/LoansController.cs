using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ExpenseApp.Contexts;
using ExpenseApp.Models;

namespace ExpenseApp.Controllers
{
    public class LoansController : Controller
    {
        private readonly ExpenseAppContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LoansController(ExpenseAppContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Create([FromForm] Loan loan, IFormFile? document)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    loan.AmountPaid = 0;

                    // Handle document upload
                    if (document != null && document.Length > 0)
                    {
                        var uploadResult = await SaveDocument(document);
                        if (uploadResult.Success)
                        {
                            loan.DocumentPath = uploadResult.FilePath;
                        }
                        else
                        {
                            return Json(new { flag = 'n', msg = uploadResult.ErrorMessage });
                        }
                    }

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
        public async Task<IActionResult> Edit(long id, [Bind("Id,LenderName,LoanAmount,AmountPaid,LoanDate,DueDate,Description,DocumentPath")] Loan loan, IFormFile? document, bool removeDocument = false)
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
                    // Handle document removal
                    if (removeDocument && !string.IsNullOrEmpty(originalLoan.DocumentPath))
                    {
                        DeleteDocument(originalLoan.DocumentPath);
                        loan.DocumentPath = null;
                    }
                    // Handle new document upload
                    else if (document != null && document.Length > 0)
                    {
                        // Delete old document if exists
                        if (!string.IsNullOrEmpty(originalLoan.DocumentPath))
                        {
                            DeleteDocument(originalLoan.DocumentPath);
                        }

                        var uploadResult = await SaveDocument(document);
                        if (uploadResult.Success)
                        {
                            loan.DocumentPath = uploadResult.FilePath;
                        }
                        else
                        {
                            ModelState.AddModelError("", uploadResult.ErrorMessage);
                            ViewBag.HasPayments = originalLoan.AmountPaid > 0;
                            ViewBag.LoanDate = loan.LoanDate.ToString("yyyy-MM-dd");
                            return View(loan);
                        }
                    }
                    else
                    {
                        // Keep existing document
                        loan.DocumentPath = originalLoan.DocumentPath;
                    }

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
                // Delete document if exists
                if (!string.IsNullOrEmpty(loan.DocumentPath))
                {
                    DeleteDocument(loan.DocumentPath);
                }

                // Delete all related payments first
                var payments = await _context.LoanPayments.Where(p => p.LoanId == id).ToListAsync();
                _context.LoanPayments.RemoveRange(payments);

                _context.Loans.Remove(loan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/DownloadDocument/5
        public async Task<IActionResult> DownloadDocument(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null || string.IsNullOrEmpty(loan.DocumentPath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, loan.DocumentPath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Get file name from path
            var fileName = Path.GetFileName(filePath);

            // Determine content type from extension
            var extension = Path.GetExtension(filePath).ToLower();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, contentType, fileName);
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


        // Helper: Save document (1MB limit)
        private async Task<(bool Success, string FilePath, string ErrorMessage)> SaveDocument(IFormFile file)
        {
            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return (false, null, "Only PDF, JPG, PNG, DOC, and DOCX files are allowed.");
                }

                // Validate file size: Max 1MB
                if (file.Length > 1 * 1024 * 1024)
                {
                    return (false, null, "File size must be less than 1MB.");
                }

                // Create uploads directory
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "loan-documents");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Return relative path
                var relativePath = $"/uploads/loan-documents/{uniqueFileName}";
                return (true, relativePath, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error uploading file: {ex.Message}");
            }
        }

        // Helper: Delete document
        private void DeleteDocument(string documentPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(documentPath))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, documentPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            catch
            {
                // Silently fail - don't block main operation
            }
        }

        private bool LoanExists(long id)
        {
            return _context.Loans.Any(e => e.Id == id);
        }
    }
}