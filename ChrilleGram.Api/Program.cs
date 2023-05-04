using ChrilleGram.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<DataInitializer>();

builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<Context>()
    .AddSignInManager<SignInManager<IdentityUser>>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultSignInScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.Name = "ChrilleGram.Cookie";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
})
.AddIdentityCookies();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetService<DataInitializer>().SeedData();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
