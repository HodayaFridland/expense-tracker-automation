using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;




public class TransactionsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

 public TransactionsApiTests(CustomWebApplicationFactory factory)
{
    _client = factory.CreateClient();

    // איפוס: מחיקת כל הרשומות לפני כל בדיקה → כל בדיקה מתחילה נקייה
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Transactions.RemoveRange(db.Transactions);
    db.SaveChanges();
}


    [Fact]
    public async Task GetTransactions_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/transactions");
        response.EnsureSuccessStatusCode();
    }


    [Fact]
public async Task PostTransaction_WithValidData_ReturnsCreated()
{
    // Arrange
    var newTransaction = new
    {
        amount = 50,
        type = "expense",
        category = "food",
        date = "2026-10-07"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/transactions", newTransaction);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var created = await response.Content.ReadFromJsonAsync<Transaction>();
    Assert.NotNull(created);
    Assert.True(created!.Id > 0);
    Assert.Equal(50, created.Amount);
    Assert.Equal("expense", created.Type);
}
[Fact]
public async Task PostTransaction_WithNegativeAmount_ReturnsBadRequest()
{
    // Arrange
    var newTransaction = new
    {
        amount = -50,
        type = "expense",
        category = "food",
        date = "2026-10-07"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/transactions", newTransaction);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

}
[Fact]
public async Task GetSummary_WithIncomeAndExpense_ReturnsCorrectBalance()
{
    // Arrange — יוצרים מצב ידוע: הכנסה 100, הוצאה 30
    await _client.PostAsJsonAsync("/transactions",
        new { amount = 100, type = "income", category = "salary", date = "2026-10-08" });
    await _client.PostAsJsonAsync("/transactions",
        new { amount = 30, type = "expense", category = "food", date = "2026-10-08" });

    // Act
    var response = await _client.GetAsync("/transactions/summary");

    // Assert
    response.EnsureSuccessStatusCode();
    var summary = await response.Content.ReadFromJsonAsync<Summary>();
    Assert.NotNull(summary);
    Assert.Equal(100, summary!.Income);
    Assert.Equal(30, summary.Expenses);
    Assert.Equal(70, summary.Balance);   // ⬅️ ה-assertion על הערך המחושב
}

[Fact]
public async Task GetSummary_WithNoTransactions_ReturnsZeroBalance()
{
    // Act
    var response = await _client.GetAsync("/transactions/summary");

    // Assert
    response.EnsureSuccessStatusCode();
    var summary = await response.Content.ReadFromJsonAsync<Summary>();
    Assert.NotNull(summary);
    Assert.Equal(0, summary!.Income);
    Assert.Equal(0, summary.Expenses);
    Assert.Equal(0, summary.Balance);   
}

}
public record Summary(decimal Income, decimal Expenses, decimal Balance);
