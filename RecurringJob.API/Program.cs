//using Microsoft.EntityFrameworkCore;
//using RecurringJob.API.Databases;
//using RecurringJob.API.IServices;
//using RecurringJob.API.Services;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddScoped<ITimeTriggerService, TimeTriggerService>();
//builder.Services.AddScoped<IMethodExecutor, MethodExecutor>();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using Microsoft.EntityFrameworkCore;
using RecurringJob.API.Databases;
using RecurringJob.API.IServices;
using RecurringJob.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// or use InMemory for testing: options.UseInMemoryDatabase("TriggerDb"));

// Our services
builder.Services.AddScoped<ITimeTriggerService, TimeTriggerService>();
builder.Services.AddScoped<IMethodExecutor, MethodExecutor>();
builder.Services.AddHostedService<TriggerBackgroundService>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Create database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.Run();



// https://claude.ai/share/4db44b97-10e3-4d20-8830-a574fe5b6e69