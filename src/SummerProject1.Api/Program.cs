using Microsoft.EntityFrameworkCore;
using SummerProject1.Infrastructure;
using SummerProject1.Infrastructure.Background;
using SummerProject1.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db
var cs = builder.Configuration.GetConnectionString("Default")
         ?? builder.Configuration["ConnectionStrings__Default"];

if (string.IsNullOrWhiteSpace(cs))
{
    throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Default'");
}

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(cs));

// Background queue
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

// Execution service (creates its own scope per job)
builder.Services.AddSingleton<ICheckExecutionService, CheckExecutionService>();

// Providers registry
builder.Services.AddSingleton<ISourceRegistry, SourceRegistry>();
builder.Services.AddScoped<IDummySourceRunner, DummySourceRunner>();

// Stripe config object
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
