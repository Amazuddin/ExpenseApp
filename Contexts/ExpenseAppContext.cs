using ExpenseApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ExpenseApp.Contexts
{
    public class ExpenseAppContext : IdentityDbContext<IdentityUser>
    {
        public ExpenseAppContext(DbContextOptions<ExpenseAppContext> options)
            : base(options)
        {
        }

        public DbSet<ExpenseInfo> ExpenseInfos { get; set; }
        public DbSet<ExpenseHead> ExpenseHeads { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanPayment> LoanPayments { get; set; }
    }
}