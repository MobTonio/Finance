using Finance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Repositories
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
