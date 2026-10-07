public class Transaction
{
public int Id { get; set; }

public decimal Amount { get; set; }
public string Type { get; set; }=string.Empty;
public string Category { get; set; }=string.Empty;
public DateTime Date { get; set; }
}