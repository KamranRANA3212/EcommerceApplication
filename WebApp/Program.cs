var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Infrastructure DI
builder.Services.AddSingleton<Application.Abstractions.IDbConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("Default") ??
                  "Server=localhost;Database=ProductCrudDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
    return new Infrastructure.Data.SqlServerConnectionFactory(connStr);
});

builder.Services.AddScoped<Infrastructure.Data.DatabaseInitializer>();

// Repositories
builder.Services.AddScoped<Application.Repositories.IProductRepository, Infrastructure.Repositories.ProductRepository>();
builder.Services.AddScoped<Application.Repositories.ICategoryRepository, Infrastructure.Repositories.CategoryRepository>();

// Services
builder.Services.AddScoped<Application.Services.IProductService, Application.Services.ProductService>();
builder.Services.AddScoped<Application.Services.ICategoryService, Application.Services.CategoryService>();

var app = builder.Build();

// Initialize DB
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.DatabaseInitializer>();
    await initializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
