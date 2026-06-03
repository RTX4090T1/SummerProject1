using Microsoft.EntityFrameworkCore;
using SummerProject1.Infrastructure;

// This project exists mainly for EF Core design-time tooling.
// Run migrations from repo root, setting startup-project to Api.

Console.WriteLine("SummerProject1.Migrations - design-time only");

var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
         ?? "Server=localhost,1433;Database=SummerProject1;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True";

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(cs)
    .Options;

_ = new AppDbContext(options);
