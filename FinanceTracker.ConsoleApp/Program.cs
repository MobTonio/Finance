using FinanceTracker.ConsoleApp;
using FinanceTracker.ConsoleApp.Managers;
using FinanceTracker.Data.Contexts;
using FinanceTracker.Data.Repositories;
using FinanceTracker.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

internal class Program
{
    private static readonly Action<DbContextOptionsBuilder> Builder = optionsBuilder =>
    {
        var connectionString = AppSettings.Instance.ConnectionString;

        // Определяем тип базы данных по строке подключения
        if (IsPostgreSQLConnectionString(connectionString))
        {
            optionsBuilder.UseNpgsql(connectionString);
            Console.WriteLine("Using PostgreSQL provider");
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString);
            Console.WriteLine("Using SQL Server provider");
        }
    };

    private static bool IsPostgreSQLConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return false;

        // Проверяем признаки PostgreSQL строки подключения
        var lowerConnection = connectionString.ToLowerInvariant();
        return lowerConnection.Contains("host=") ||
               lowerConnection.Contains("server=localhost") ||
               lowerConnection.Contains("user id=") ||
               lowerConnection.Contains("username=") ||
               lowerConnection.Contains("port=") &&
               !lowerConnection.Contains("initial catalog") &&
               !lowerConnection.Contains("integrated security");
    }


    private static readonly string[] DateTimeFormats = { "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy" };
    private const string MessageInvalidFormatDate = "Invalid date format specified for \"{0}\" = \"{1}\", expected format \"{2}\"";

    static void Main(string[] args)
    {
        // Принудительно устанавливаем кодировку UTF-8
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        AppSettings.Initialize(new AppSettingsOptions
        {
            ProgrammeArguments = args,
            ExcludeMigration = true,
            UseEfCoreProvider = false
        });

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(Builder);
        services
            .AddScoped<WalletRepository>()
            .AddScoped<TransactionRepository>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        using (var context = serviceProvider.GetRequiredService<AppDbContext>())
        {
            context.Database.EnsureCreated();

            // Заполняем тестовыми данными
            SeedTestData(serviceProvider).Wait();

            // Запускаем главное меню финансов
            RunFinance(serviceProvider);
        }


        Console.WriteLine("Done. Press any key to exit...");
        Console.ReadLine();
    }

    /// <summary>
    /// Заполнение тестовыми данными
    /// </summary>
    static async Task SeedTestData(IServiceProvider serviceProvider)
    {
        var walletRepo = serviceProvider.GetRequiredService<WalletRepository>();
        var transactionRepo = serviceProvider.GetRequiredService<TransactionRepository>();

        Console.WriteLine("Заполнить базу тестовыми данными? (y/n)");
        var response = Console.ReadLine();

        if (response?.ToLower() == "y")
        {
            await walletRepo.SeedTestWalletsAsync();
            await transactionRepo.SeedTestTransactionsAsync();
            Console.WriteLine("База заполнена тестовыми данными");
        }
    }


    private static void RunFinance(IServiceProvider serviceProvider)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== СИСТЕМА МЕНЕДЖМЕНТА ФИНАНСОВ ===");
            Console.WriteLine("=== ОСНОВНЫЕ ДЕЙСТВИЯ ===");
            Console.WriteLine("1. Анализ транзакций по месяцам");
            Console.WriteLine("2. Топ расходов по кошелькам");
            Console.WriteLine("");
            Console.WriteLine("=== СЛУЖЕБНЫЕ ФУНКЦИИ ===");
            Console.WriteLine("3. Управление кошельками");
            Console.WriteLine("4. Управление транзакциями");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите действие: ");

            var chosenAction = Console.ReadLine();

            switch (chosenAction)
            {
                case "1":
                    ActionOne(serviceProvider);
                    break;
                case "2":
                    ActionTwo(serviceProvider);
                    break;
                case "3":
                    RunWalletManager(serviceProvider).Wait();
                    break;
                case "4":
                    RunTransactionManager(serviceProvider).Wait();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Некорректный ввод");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    

    /// <summary>
    /// Для указанного месяца сгруппировать все транзакции по типу (Income/Expense), отсортировать группы по общей сумме (по убыванию), в каждой группе отсортировать транзакции по дате (от самых старых к самым новым)
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void ActionOne(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Введите месяц и год в формате MM.yyyy (например, 03.2023):");
        var input = Console.ReadLine();
        if (!DateTime.TryParseExact(input, "MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthYear))
        {
            Console.WriteLine(string.Format(MessageInvalidFormatDate, "MM.yyyy", input, "MM.yyyy"));
            return;
        }
        var month = monthYear.Month;
        var year = monthYear.Year;
        var transactions = serviceProvider.GetRequiredService<TransactionRepository>();

        transactions.ShowForMonth(monthYear);
        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    /// <summary>
    ///  3 самые большие траты за указанный месяц для каждого кошелька, отсортированные по убыванию суммы
    /// </summary>
    /// <param name="serviceProvider"></param>
    private static void ActionTwo(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Введите месяц и год в формате MM.yyyy (например, 03.2023):");
        var input = Console.ReadLine();
        if (!DateTime.TryParseExact(input, "MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthYear))
        {
            Console.WriteLine(string.Format(MessageInvalidFormatDate, "MM.yyyy", input, "MM.yyyy"));
            return;
        }
        var month = monthYear.Month;
        var year = monthYear.Year;
        var wallets = serviceProvider.GetRequiredService<WalletRepository>();
        int countPerWallet = 3;

        wallets.ShowTopExpensesPerWallet(monthYear, countPerWallet);
        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }


    /// <summary>
    /// Запуск менеджера кошельков
    /// </summary>
    private static async Task RunWalletManager(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var walletRepo = scope.ServiceProvider.GetRequiredService<WalletRepository>();
        var walletManager = new WalletManager(walletRepo);
        await walletManager.RunAsync();
    }

    /// <summary>
    /// Запуск менеджера транзакций
    /// </summary>
    private static async Task RunTransactionManager(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var transactionRepo = scope.ServiceProvider.GetRequiredService<TransactionRepository>();
        var walletRepo = scope.ServiceProvider.GetRequiredService<WalletRepository>();
        var transactionManager = new TransactionManager(transactionRepo, walletRepo);
        await transactionManager.RunAsync();
    }
}