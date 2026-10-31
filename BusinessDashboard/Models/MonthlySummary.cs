public class MonthlySummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public double TotalEarnings { get; set; }
    public double TotalExpenses { get; set; }

    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}
