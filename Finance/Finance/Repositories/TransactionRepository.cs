using Finance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Repositories
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
