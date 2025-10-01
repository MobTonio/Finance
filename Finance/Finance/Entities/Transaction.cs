using System;
using System.Collections.Generic;

/// <summary>
/// Транзакция
/// </summary>
public class Transaction
{
    /// <summary>
    /// Id транзакции
    /// </summary>
    public int TransactionId { get; set; }

    /// <summary>
    /// Id кошелька
    /// </summary>
    public int WalletId { get; set; }

    /// <summary>
    /// Дата
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Сумма
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Тип
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    public string Description { get; set; }

    public Wallet Wallet { get; set; }
}

/// <summary>
/// Тип транзакции
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Доход
    /// </summary>
    Income,
    /// <summary>
    /// Расход
    /// </summary>
    Expense
}