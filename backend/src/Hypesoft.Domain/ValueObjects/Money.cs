namespace Hypesoft.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";
    
    public Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        Amount = amount;
        Currency = currency;
    }
    
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        
        return new Money(a.Amount + b.Amount, a.Currency);
    }
    
    public static Money operator *(Money money, int multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }
    
    public string Format()
    {
        return Currency switch
        {
            "BRL" => $"R$ {Amount:N2}",
            "USD" => $"$ {Amount:N2}",
            "EUR" => $"€ {Amount:N2}",
            _ => $"{Amount:N2} {Currency}"
        };
    }
}