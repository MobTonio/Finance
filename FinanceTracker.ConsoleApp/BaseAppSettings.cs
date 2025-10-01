using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace FinanceTracker.ConsoleApp
{
    /// <summary>
    /// ������� ����� ��� ������ � ����������� ����������.
    /// </summary>
    internal abstract class BaseAppSettings<T> where T : BaseAppSettings<T>, new()
    {
        private static T _instance;
        public static T Instance => _instance ?? throw new InvalidOperationException("AppSettings not initialized. Call Initialize first.");

        public string ConnectionString { get; private set; }
        public string NormalizedConnectionString { get; private set; }

        /// <summary>
        /// ������������� �������� ����������.
        /// </summary>
        public static void Initialize(AppSettingsOptions options)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

            _instance = new T
            {
                ConnectionString = connectionString,
                NormalizedConnectionString = NormalizeConnectionString(connectionString)
            };

            // ����� �������� ��������� options ��� �������������
        }

        /// <summary>
        /// ������ ������������ ������ ����������� (����� ���������� ��� ���� ����������)
        /// </summary>
        private static string NormalizeConnectionString(string connectionString)
        {
            // ����� ����� ����������� ������ ������������, ���� ���������
            return connectionString?.Trim() ?? string.Empty;
        }
    }

    /// <summary>
    /// ����� ��� ������������� AppSettings
    /// </summary>
    internal class AppSettingsOptions
    {
        public string[] ProgrammeArguments { get; set; }
        public bool ExcludeMigration { get; set; }
        public bool UseEfCoreProvider { get; set; }
    }
}