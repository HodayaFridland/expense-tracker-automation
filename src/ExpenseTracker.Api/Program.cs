using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=expenses.db"));

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();


app.MapPost("/transactions", async (Transaction transaction, AppDbContext db) =>
{
   var error = ValidateTransaction(transaction);
if (error is not null)
    return Results.BadRequest(error);


    db.Transactions.Add(transaction);
    await db.SaveChangesAsync();
    return Results.Created($"/transactions/{transaction.Id}", transaction);
});

// החזר את כל ה-transactions
app.MapGet("/transactions", async (AppDbContext db) =>
    await db.Transactions.ToListAsync());



app.MapDelete("/transactions/{id}", async (int id, AppDbContext db) =>
{
    var transaction = await db.Transactions.FindAsync(id);
    if (transaction is null)
        return Results.NotFound();

    db.Transactions.Remove(transaction);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapPut("/transactions/{id}", async (int id, Transaction updated, AppDbContext db) =>
{
    var transaction = await db.Transactions.FindAsync(id);
    if (transaction is null)
        return Results.NotFound();

var error = ValidateTransaction(updated);
if (error is not null)
    return Results.BadRequest(error);



    transaction.Amount = updated.Amount;
    transaction.Type = updated.Type;
    transaction.Category = updated.Category;
    transaction.Date = updated.Date;

    await db.SaveChangesAsync();
    return Results.Ok(transaction);
});


app.MapGet("/transactions/summary", async (AppDbContext db) =>
{
    var income = await db.Transactions
        .Where(t => t.Type == "income")
        .SumAsync(t => t.Amount);

    var expenses = await db.Transactions
        .Where(t => t.Type == "expense")
        .SumAsync(t => t.Amount);

    var balance = income - expenses;

    return Results.Ok(new { income, expenses, balance });
});


app.Run();
static string? ValidateTransaction(Transaction t)
{
    if (t.Amount <= 0)
        return "Amount must be greater than 0.";
    if (t.Type != "income" && t.Type != "expense")
        return "Invalid transaction type. Must be either 'income' or 'expense'.";
    if (string.IsNullOrWhiteSpace(t.Category))
        return "Category is required.";
    return null; // null = הכול תקין
}

public partial class Program { }

