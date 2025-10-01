using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Finance.Contexts;

namespace Finance.Services
{
    public class WalletService
    {
        private readonly AppDbContext _context;

        public WalletService(AppDbContext context)
        {
            _context = context;
        }

        // Добавить транзакцию с проверкой баланса
        public async Task<bool> AddTransactionAsync(int walletId, Transactions transaction)
        {
            var wallet = await _context.Wallets.Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.Id == walletId);
            if (wallet == null) return false;

            if (transaction.Type == TransactionType.Expense && transaction.Amount > wallet.CurrentBalance)
                return false; // Недостаточно средств

            transaction.WalletId = walletId;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        // Месячный доход/расход
        public async Task<(decimal income, decimal expense)> GetMonthlyStatsAsync(int walletId, int year, int month)
        {
            var transactions = await _context.Transactions
                .Where(t => t.WalletId == walletId && t.Date.Year == year && t.Date.Month == month)
                .ToListAsync();
            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            return (income, expense);
        }
    }
}
