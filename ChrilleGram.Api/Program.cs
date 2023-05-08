using ChrilleGram.Api.Data;
using ChrilleGram.Api.Interfaces;
using ChrilleGram.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

X509Certificate2 certificate = new X509Certificate2("C:\\Users\\Henri\\source\\repos\\ChrilleGram.UI\\ChrilleGram.Api\\Properties\\certificateGram.pfx", "test123");

builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 7135, listenOptions =>
    {
        listenOptions.UseHttps(certificate);
    });
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<DataInitializer>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IImageService, ImageService>();

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

// Add authentication middleware to the HTTP request pipeline
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

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

// Add authentication and authorization middleware to the HTTP request pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();