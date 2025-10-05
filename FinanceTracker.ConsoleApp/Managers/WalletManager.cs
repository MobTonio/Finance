using FinanceTracker.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.ConsoleApp.Managers
{
    public class WalletManager
    {
        private readonly WalletRepository _walletRepo;

        public WalletManager(WalletRepository walletRepo)
        {
            _walletRepo = walletRepo;
        }

        /// <summary>
        /// Запуск меню управления кошельками
        /// </summary>
        public async Task RunAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== УПРАВЛЕНИЕ КОШЕЛЬКАМИ ===");
                Console.WriteLine("=== СЛУЖЕБНАЯ ФУНКЦИЯ ===\n");
                Console.WriteLine("1. Просмотреть все кошельки");
                Console.WriteLine("2. Добавить кошелек");
                Console.WriteLine("3. Удалить кошелек");
                Console.WriteLine("4. Статистика по кошелькам");
                Console.WriteLine("0. Назад в главное меню");
                Console.Write("Выберите действие: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ShowWalletsAsync();
                        break;
                    case "2":
                        await AddWalletAsync();
                        break;
                    case "3":
                        await DeleteWalletAsync();
                        break;
                    case "4":
                        await ShowWalletsStatisticsAsync();
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
        /// Показать все кошельки
        /// </summary>
        private async Task ShowWalletsAsync()
        {
            var wallets = await _walletRepo.GetAllWalletsAsync();
            Console.WriteLine("\n=== КОШЕЛЬКИ ===");

            if (!wallets.Any())
            {
                Console.WriteLine("Кошельки не найдены");
                return;
            }

            foreach (var wallet in wallets)
            {
                Console.WriteLine($"ID: {wallet.Id}, Название: {wallet.Name}, " +
                                $"Баланс: {wallet.Balance} {wallet.Currency}");
            }
        }

        /// <summary>
        /// Добавить кошелек через консоль
        /// </summary>
        private async Task AddWalletAsync()
        {
            Console.Write("Название кошелька: ");
            var name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым");
                return;
            }

            Console.Write("Валюта (RUB/USD/EUR): ");
            var currency = Console.ReadLine();

            Console.Write("Начальный баланс: ");
            if (decimal.TryParse(Console.ReadLine(), out var balance))
            {
                await _walletRepo.CreateWalletAsync(name, currency, balance);
                Console.WriteLine("Кошелек добавлен");
            }
            else
            {
                Console.WriteLine("Неверная сумма");
            }
        }

        /// <summary>
        /// Удалить кошелек
        /// </summary>
        private async Task DeleteWalletAsync()
        {
            Console.Write("Введите ID кошелька для удаления: ");
            if (int.TryParse(Console.ReadLine(), out var walletId))
            {
                if (await _walletRepo.DeleteWalletAsync(walletId))
                    Console.WriteLine("Кошелек удален");
                else
                    Console.WriteLine("Кошелек не найден");
            }
            else
            {
                Console.WriteLine("Неверный ID");
            }
        }

        /// <summary>
        /// Статистика по кошелькам
        /// </summary>
        private async Task ShowWalletsStatisticsAsync()
        {
            var wallets = await _walletRepo.GetAllWalletsAsync();
            Console.WriteLine("\n=== СТАТИСТИКА ПО КОШЕЛЬКАМ ===");

            if (!wallets.Any())
            {
                Console.WriteLine("Кошельки не найдены");
                return;
            }

            var totalBalance = wallets.Sum(w => w.Balance);
            Console.WriteLine($"Общий баланс: {totalBalance}");

            foreach (var wallet in wallets)
            {
                var percentage = totalBalance > 0 ? (wallet.Balance / totalBalance * 100) : 0;
                Console.WriteLine($"\n{wallet.Name}: {wallet.Balance} {wallet.Currency} ({percentage:F1}%)");
            }
        }
    }
}
