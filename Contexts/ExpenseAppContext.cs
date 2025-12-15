using ExpenseApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApp.Contexts
{
    public class ExpenseAppContext : DbContext
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
