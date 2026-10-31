using SQLite;
using System.IO;
using System.Threading.Tasks;
using BusinessDashboard.Models;

namespace BusinessDashboard.Data
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "earnings.db");
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<EarningsEntry>().Wait();
        }

        public Task<int> AddEntryAsync(EarningsEntry entry)
        {
            return _database.InsertAsync(entry);
        }

        public Task<List<EarningsEntry>> GetEntriesAsync()
        {
            return _database.Table<EarningsEntry>().ToListAsync();
        }

        public async Task<List<MonthlySummary>> GetMonthlySummariesAsync()
{
    var entries = await _database.Table<EarningsEntry>().ToListAsync();

    var grouped = entries
        .GroupBy(e => new { e.Timestamp.Year, e.Timestamp.Month })
        .Select(g => new MonthlySummary
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            TotalEarnings = g.Sum(e => e.CashierEarnings + e.TipsEarnings),
            TotalExpenses = g.Sum(e => e.DailyExpenses)
        })
        .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month)
        .ToList();

    return grouped;
}

    }

    
}
