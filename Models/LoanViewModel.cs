namespace ExpenseApp.Models
{
    public class LoanViewModel
    {
        public List<Loan> Loans { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalBalanceDue { get; set; }
    }
}
