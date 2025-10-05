using FinanceTracker.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Data.Repositories
{
    public class WalletRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// .cstor
        /// </summary>
        /// <param name="context"></param>
        public WalletRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Создать новый кошелек
        /// </summary>
        public async Task<Wallet> CreateWalletAsync(string name, string currency, decimal initialBalance = 0)
        {
            var wallet = new Wallet
            {
                Name = name,
                Currency = currency,
                Balance = initialBalance
            };

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        /// <summary>
        /// Получить все кошельки
        /// </summary>
        public async Task<List<Wallet>> GetAllWalletsAsync()
        {
            return await _context.Wallets.ToListAsync();
        }

        /// <summary>
        /// Заполнить тестовыми кошельками
        /// </summary>
        public async Task SeedTestWalletsAsync()
        {
            if (!await _context.Wallets.AnyAsync())
            {
                var wallets = new[]
                {
                new Wallet { Name = "Основной кошелек", Currency = "RUB", Balance = 10000 },
                new Wallet { Name = "Резервный кошелек", Currency = "USD", Balance = 500 },
                new Wallet { Name = "Инвестиционный", Currency = "EUR", Balance = 2000 }
            };

                await _context.Wallets.AddRangeAsync(wallets);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Тестовые кошельки добавлены");
            }
        }

        /// <summary>
        /// Удалить кошелек по ID
        /// </summary>
        public async Task<bool> DeleteWalletAsync(int walletId)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet != null)
            {
                _context.Wallets.Remove(wallet);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Выводит X самых больших трат 
        /// за указанный месяц 
        /// для каждого кошелька, 
        /// отсортированные по убыванию суммы
        /// </summary>
        /// <param name="monthYear">Указанный месяц</param>
        /// <param name="countPerWallet">количество самых больших трат для каждого кошелька</param>
        /// <exception cref="NotImplementedException"></exception>
        public void ShowTopExpensesPerWallet(DateTime monthYear, int countPerWallet = 3)
        {
            var wallets = _context.Wallets.ToList();
            foreach (var wallet in wallets)
            {
                var topExpenses = _context.Transactions
                    .Where(t => t.WalletId == wallet.Id &&
                                t.Type == TransactionType.Expense &&
                                t.Date.Month == monthYear.Month &&
                                t.Date.Year == monthYear.Year)
                    .OrderByDescending(t => t.Amount)
                    .Take(countPerWallet)
                    .ToList();
                Console.WriteLine($"Кошелек: {wallet.Name} (ID: {wallet.Id})");
                if (topExpenses.Any())
                {
                    foreach (var expense in topExpenses)
                    {
                        Console.WriteLine($"\tID: {expense.TransactionId}, Дата: {expense.Date}, Сумма: {expense.Amount}, Описание: {expense.Description} + {expense.Type}");
                    }
                }
                else
                {
                    Console.WriteLine("\tНет расходов за указанный месяц.");
                }
            }
        }
    }
}
