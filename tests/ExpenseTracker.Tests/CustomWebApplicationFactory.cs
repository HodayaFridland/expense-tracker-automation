using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. מצא והסר את רישום ה-DbContext של האפליקציה (זה שמצביע ל-expenses.db)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // 2. רשום מחדש — הפעם מול DB בדיקות נפרד
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=test.db"));

            // 3. ודא שה-DB של הבדיקות קיים (יוצר אותו מה-model)
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
