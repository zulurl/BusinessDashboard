using SQLite;

namespace BusinessDashboard.Models
{
    public class EarningsEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double CashierEarnings { get; set; }
        public double TipsEarnings { get; set; }
        public double DailyExpenses { get; set; }
        public double TotalEarningsSoFar { get; set; }
        public double TotalExpensesSoFar { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
