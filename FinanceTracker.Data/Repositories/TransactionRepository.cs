using FinanceTracker.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Data.Repositories
{
    public class TransactionRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// .cstor
        /// </summary>
        /// <param name="context"></param>
        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Создать транзакцию
        /// </summary>
        public async Task<Transactions> CreateTransactionAsync(int walletId, decimal amount,
            TransactionType type, string description, DateTime? date = null)
        {
            var transaction = new Transactions
            {
                WalletId = walletId,
                Amount = amount,
                Type = type,
                Description = description,
                Date = date ?? DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Обновляем баланс кошелька
            await UpdateWalletBalance(walletId);

            return transaction;
        }

        /// <summary>
        /// Обновить баланс кошелька
        /// </summary>
        private async Task UpdateWalletBalance(int walletId)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet != null)
            {
                var balance = await _context.Transactions
                    .Where(t => t.WalletId == walletId)
                    .SumAsync(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);

                wallet.Balance = balance;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Получить все транзакции
        /// </summary>
        public async Task<List<Transactions>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Wallet)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Удалить транзакцию по ID
        /// </summary>
        public async Task<bool> DeleteTransactionAsync(int transactionId)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction != null)
            {
                var walletId = transaction.WalletId;
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                // Обновляем баланс кошелька
                await UpdateWalletBalance(walletId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Заполнить тестовыми транзакциями
        /// </summary>
        public async Task SeedTestTransactionsAsync()
        {
            if (!await _context.Transactions.AnyAsync())
            {
                var wallets = await _context.Wallets.ToListAsync();
                if (!wallets.Any()) return;

                var random = new Random();
                var transactions = new List<Transactions>();
                var descriptions = new[]
                {
                "Продукты", "Кафе", "Транспорт", "Одежда", "Развлечения",
                "Зарплата", "Инвестиции", "Подарок", "Коммунальные услуги", "Интернет"
            };

                for (int i = 0; i < 20; i++)
                {
                    var wallet = wallets[random.Next(wallets.Count)];
                    var isIncome = random.Next(0, 3) == 0; // 33% доходов, 67% расходов
                    var amount = isIncome ? random.Next(1000, 10000) : random.Next(100, 2000);
                    var description = descriptions[random.Next(descriptions.Length)];

                    transactions.Add(new Transactions
                    {
                        WalletId = wallet.Id,
                        Amount = amount,
                        Type = isIncome ? TransactionType.Income : TransactionType.Expense,
                        Description = description,
                        Date = DateTime.Now.AddDays(-random.Next(0, 30))
                    });
                }

                await _context.Transactions.AddRangeAsync(transactions);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Тестовые транзакции добавлены");
            }
        }

        /// <summary>
        /// Передает данные о всех транзакциях за указанный месяц,
        /// сгруппированый по типу (Income/Expense), 
        /// отсортированых по общей сумме (по убыванию), 
        /// и по дате (от самых старых к самым новым)
        /// </summary>
        /// <param name="monthYear"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ShowForMonth(DateTime monthYear)
        {
            var transactions = _context.Transactions
                .Where(t => t.Date.Month == monthYear.Month && t.Date.Year == monthYear.Year)
                .AsEnumerable() // to use C# grouping and sorting
                .GroupBy(t => t.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Transactions = g.OrderBy(t => t.Date).ToList()
                })
                .OrderByDescending(g => g.TotalAmount)
                .ToList();

            foreach (var group in transactions)
            {
                Console.WriteLine($"Тип: {group.Type}, Общая сумма: {group.TotalAmount}");
                foreach (var transaction in group.Transactions)
                {
                    Console.WriteLine($"\tID: {transaction.TransactionId}, Дата: {transaction.Date}, Сумма: {transaction.Amount}, Описание: {transaction.Description}");
                }
            }
        }

    }
}
