using System;
using System.Collections.Generic;

/// <summary>
/// Кошелек
/// </summary>
public class Wallet
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Валюта
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Начальный баланс
    /// </summary>
    public decimal Balance { get; set; }


    public virtual ICollection<Transactions> Transactions { get; set; }

    /// <summary>
    /// Текущий баланс
    /// </summary>
    public decimal CurrentBalance
    {
        get
        {
            var income = Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expense = Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            return Balance + income - expense;
        }
    }
}