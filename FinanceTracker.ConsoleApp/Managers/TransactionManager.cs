using FinanceTracker.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.ConsoleApp.Managers
{
    public class TransactionManager
    {
        private readonly TransactionRepository _transactionRepo;
        private readonly WalletRepository _walletRepo;

        public TransactionManager(TransactionRepository transactionRepo, WalletRepository walletRepo)
        {
            _transactionRepo = transactionRepo;
            _walletRepo = walletRepo;
        }

        /// <summary>
        /// Запуск меню управления транзакциями
        /// </summary>
        public async Task RunAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== УПРАВЛЕНИЕ ТРАНЗАКЦИЯМИ ===");
                Console.WriteLine("=== СЛУЖЕБНАЯ ФУНКЦИЯ ===\n");
                Console.WriteLine("1. Просмотреть все транзакции");
                Console.WriteLine("2. Добавить транзакцию");
                Console.WriteLine("3. Удалить транзакцию");
                Console.WriteLine("0. Назад в главное меню");
                Console.Write("Выберите действие: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ShowTransactionsAsync();
                        break;
                    case "2":
                        await AddTransactionAsync();
                        break;
                    case "3":
                        await DeleteTransactionAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }

                if (choice != "0")
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Показать все транзакции
        /// </summary>
        private async Task ShowTransactionsAsync()
        {
            var transactions = await _transactionRepo.GetAllTransactionsAsync();
            Console.WriteLine("\n=== ТРАНЗАКЦИИ ===");

            if (!transactions.Any())
            {
                Console.WriteLine("Транзакции не найдены");
                return;
            }

            foreach (var transaction in transactions)
            {
                var typeStr = transaction.Type == TransactionType.Income ? "Доход" : "Расход";
                Console.WriteLine($"ID: {transaction.TransactionId}, Кошелек: {transaction.Wallet.Name}, " +
                                $"Сумма: {transaction.Amount}, Тип: {typeStr}, " +
                                $"Дата: {transaction.Date:dd.MM.yyyy}, Описание: {transaction.Description}");
            }
        }

        /// <summary>
        /// Добавить транзакцию через консоль
        /// </summary>
        private async Task AddTransactionAsync()
        {
            var wallets = await _walletRepo.GetAllWalletsAsync();
            if (!wallets.Any())
            {
                Console.WriteLine("Сначала создайте кошелек");
                return;
            }

            Console.WriteLine("Выберите кошелек:");
            foreach (var wallet in wallets)
            {
                Console.WriteLine($"ID: {wallet.Id} - {wallet.Name} ({wallet.Balance} {wallet.Currency})");
            }

            if (!int.TryParse(Console.ReadLine(), out var walletId) ||
                !wallets.Any(w => w.Id == walletId))
            {
                Console.WriteLine("Неверный ID кошелька");
                return;
            }

            Console.Write("Сумма: ");
            if (!decimal.TryParse(Console.ReadLine(), out var amount) || amount <= 0)
            {
                Console.WriteLine("Неверная сумма");
                return;
            }

            Console.Write("Тип (1 - Доход, 2 - Расход): ");
            var typeInput = Console.ReadLine();
            var type = typeInput == "1" ? TransactionType.Income : TransactionType.Expense;

            Console.Write("Описание: ");
            var description = Console.ReadLine();

            Console.Write("Дата (dd.MM.yyyy) или Enter для текущей даты: ");
            var dateInput = Console.ReadLine();
            DateTime? date = null;

            if (!string.IsNullOrEmpty(dateInput))
            {
                if (DateTime.TryParseExact(dateInput, "dd.MM.yyyy", null,
                    System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    date = parsedDate;
                }
                else
                {
                    Console.WriteLine("Неверный формат даты, используется текущая дата");
                }
            }

            await _transactionRepo.CreateTransactionAsync(walletId, amount, type, description, date);
            Console.WriteLine("Транзакция добавлена");
        }

        /// <summary>
        /// Удалить транзакцию
        /// </summary>
        private async Task DeleteTransactionAsync()
        {
            Console.Write("Введите ID транзакции для удаления: ");
            if (int.TryParse(Console.ReadLine(), out var transactionId))
            {
                if (await _transactionRepo.DeleteTransactionAsync(transactionId))
                    Console.WriteLine("Транзакция удалена");
                else
                    Console.WriteLine("Транзакция не найдена");
            }
            else
            {
                Console.WriteLine("Неверный ID");
            }
        }
    }
}
