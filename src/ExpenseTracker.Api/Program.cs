using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=expenses.db"));

var app = builder.Build();


app.MapPost("/transactions", async (Transaction transaction, AppDbContext db) =>
{
    if (transaction.Amount <= 0)
    return Results.BadRequest("Amount must be greater than 0.");

if  (transaction.Type != "income" && transaction.Type != "expense")
    return Results.BadRequest("Invalid transaction type. Must be either 'income' or 'expense'.");

if (string.IsNullOrWhiteSpace(transaction.Category))
    return Results.BadRequest("Category is required.");

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

 if (updated.Amount <= 0)
    return Results.BadRequest("Amount must be greater than 0.");

if  (updated.Type != "income" && updated.Type != "expense")
    return Results.BadRequest("Invalid transaction type. Must be either 'income' or 'expense'.");

if (string.IsNullOrWhiteSpace(updated.Category))
    return Results.BadRequest("Category is required.");


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
