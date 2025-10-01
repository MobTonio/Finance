using Finance.Contexts;
using Finance.Repositories;
using Finance.Services;
using FinanceTracker.ConsoleApp;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

internal class Program
{

    private static readonly Action<DbContextOptionsBuilder> Builder = optionsBuilder =>
    {
        var connectionString = AppSettings.Instance.ConnectionString;
        optionsBuilder.UseSqlServer(connectionString);
    };
    private static readonly string[] DateTimeFormats = { "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy" };
    private const string MessageInvalidFormatDate = "Invalid date format specified for \"{0}\" = \"{1}\", expected format \"{2}\"";

    static void Main(string[] args)
    {
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


        // --- Создание базы и заполнение тестовыми данными ---
        using (var context = serviceProvider.GetRequiredService<AppDbContext>())
        {
            context.Database.EnsureCreated();
            context.Database.Migrate();

            RunFinance(serviceProvider);
        }
        // -----------------------------------------------------


        Console.WriteLine("Done. Press any key to exit...");
        Console.ReadLine();
    }

    private static void SeedTestData(AppDbContext context)
    {
        var wallets = new[]
        {
            new Wallet { Name = "Основной кошелек", Currency = "RUB", Balance = 1000 },
            new Wallet { Name = "Запасной кошелек", Currency = "RUB", Balance = 500 },
            new Wallet { Name = "Долларовый кошелек", Currency = "USD", Balance = 100 },
            new Wallet { Name = "Евро кошелек", Currency = "EUR", Balance = 200 }
        };

        context.Wallets.AddRange(wallets);
        context.SaveChanges();

        var random = new Random();
        var transactions = new[]
        {
            // Кошелек 1 (Основной) - 7 транзакций
            new Transactions { WalletId = 1, Date = DateTime.Now.AddDays(-30), Amount = 50000, Type = TransactionType.Income, Description = "Зарплата" },
            new Transactions { WalletId = 1, Date = DateTime.Now.AddDays(-25), Amount = 15000, Type = TransactionType.Expense, Description = "Аренда квартиры" },
            new Transactions { WalletId = 1, Date = DateTime.Now.AddDays(-20), Amount = 5000, Type = TransactionType.Expense, Description = "Коммунальные услуги" },
            new Transactions { WalletId = 1, Date = DateTime.Now.AddDays(-15), Amount = 3000, Type = TransactionType.Expense, Description = "Продукты" },
            new Transactions { WalletId = 1, Date = DateTime.Now.AddDays(-10), Amount = 2000, Type = TransactionType.Expense, Description = "Одежда" },
            new Transactions { WalletId = 1, Date = DateTime.Now.AddDays(-5), Amount = 8000, Type = TransactionType.Income, Description = "Фриланс" },
            new Transactions { WalletId = 1, Date = DateTime.Now.AddDays(-2), Amount = 1500, Type = TransactionType.Expense, Description = "Ресторан" },

            // Кошелек 2 (Запасной) - 6 транзакций
            new Transactions { WalletId = 2, Date = DateTime.Now.AddDays(-28), Amount = 10000, Type = TransactionType.Income, Description = "Подарок" },
            new Transactions { WalletId = 2, Date = DateTime.Now.AddDays(-22), Amount = 3000, Type = TransactionType.Expense, Description = "Техника" },
            new Transactions { WalletId = 2, Date = DateTime.Now.AddDays(-18), Amount = 2000, Type = TransactionType.Expense, Description = "Книги" },
            new Transactions { WalletId = 2, Date = DateTime.Now.AddDays(-12), Amount = 5000, Type = TransactionType.Income, Description = "Возврат долга" },
            new Transactions { WalletId = 2, Date = DateTime.Now.AddDays(-8), Amount = 1000, Type = TransactionType.Expense, Description = "Такси" },
            new Transactions { WalletId = 2, Date = DateTime.Now.AddDays(-3), Amount = 1500, Type = TransactionType.Expense, Description = "Кино" },

            // Кошелек 3 (Долларовый) - 6 транзакций
            new Transactions { WalletId = 3, Date = DateTime.Now.AddDays(-27), Amount = 500, Type = TransactionType.Income, Description = "Перевод" },
            new Transactions { WalletId = 3, Date = DateTime.Now.AddDays(-21), Amount = 100, Type = TransactionType.Expense, Description = "Amazon" },
            new Transactions { WalletId = 3, Date = DateTime.Now.AddDays(-17), Amount = 50, Type = TransactionType.Expense, Description = "Netflix" },
            new Transactions { WalletId = 3, Date = DateTime.Now.AddDays(-14), Amount = 200, Type = TransactionType.Income, Description = "Фриланс $" },
            new Transactions { WalletId = 3, Date = DateTime.Now.AddDays(-9), Amount = 75, Type = TransactionType.Expense, Description = "Spotify" },
            new Transactions { WalletId = 3, Date = DateTime.Now.AddDays(-4), Amount = 150, Type = TransactionType.Expense, Description = "Udemy" },

            // Кошелек 4 (Евро) - 6 транзакций
            new Transactions { WalletId = 4, Date = DateTime.Now.AddDays(-26), Amount = 300, Type = TransactionType.Income, Description = "Перевод EUR" },
            new Transactions { WalletId = 4, Date = DateTime.Now.AddDays(-19), Amount = 80, Type = TransactionType.Expense, Description = "AliExpress" },
            new Transactions { WalletId = 4, Date = DateTime.Now.AddDays(-16), Amount = 40, Type = TransactionType.Expense, Description = "Steam" },
            new Transactions { WalletId = 4, Date = DateTime.Now.AddDays(-13), Amount = 120, Type = TransactionType.Income, Description = "Продажа" },
            new Transactions { WalletId = 4, Date = DateTime.Now.AddDays(-7), Amount = 60, Type = TransactionType.Expense, Description = "PlayStation" },
            new Transactions { WalletId = 4, Date = DateTime.Now.AddDays(-1), Amount = 90, Type = TransactionType.Expense, Description = "Booking" }
        };

        context.Transactions.AddRange(transactions);
        context.SaveChanges();
    }


    private static void RunFinance(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Добро пожаловать в Систему Менеджмента Финансов");

        Console.WriteLine("Пожалуйста выберете действие:\n"
            + "1. Для указанного месяца сгруппировать все транзакции по типу (Income/Expense), отсортировать группы по общей сумме (по убыванию), в каждой группе отсортировать транзакции по дате (от самых старых к самым новым)\n"
            + "2. 3 самые большие траты за указанный месяц для каждого кошелька, отсортированные по убыванию суммы");

        var chosenAction = Console.ReadLine();

        if (chosenAction != "1" && chosenAction != "2")
        {
            Console.WriteLine("Некорректный ввод");
            return;
        }

        switch (chosenAction)
        {
            case "1":
                ActionOne(serviceProvider);
                break;
            case "2":
                ActionTwo(serviceProvider);
                break;
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
    }
}