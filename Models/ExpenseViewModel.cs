namespace ExpenseApp.Models
{
    public class ExpenseViewModel
    {
        public IEnumerable<ExpenseInfo> ExpenseInfo { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
