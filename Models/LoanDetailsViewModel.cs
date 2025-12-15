namespace ExpenseApp.Models
{
    public class LoanDetailsViewModel
    {
        public Loan Loan { get; set; }
        public List<LoanPayment> Payments { get; set; }
    }
}
