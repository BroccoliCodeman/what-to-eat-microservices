using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Recipes.API;
using Recipes.BLL;
using Recipes.BLL.Configurations;
using Recipes.BLL.Helpers;
using Recipes.BLL.Interfaces;
using Recipes.BLL.Services;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL;
using Recipes.DAL.Seeding;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// context configuration and database connection
builder.Services.AddDbContext<RecipesContext>(options => options.UseSqlite(
    builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Recipes.DAL")))
    .AddIdentity<User, UserRole>(config =>
    {
        config.Password.RequireNonAlphanumeric = false;
        config.Password.RequireDigit = true;
        config.Password.RequiredLength = 8;
        config.Password.RequireLowercase = true;
        config.Password.RequireUppercase = true;
    }).AddEntityFrameworkStores<RecipesContext>().AddDefaultTokenProviders();

builder.Services.AddTransient<EmailSenderConfiguration>();
builder.Services.AddTransient<GoogleClientConfiguration>();
builder.Services.AddTransient<IEmailSender,EmailSender>();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Authorization
builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("OnlyAdmin", policyBuilder => policyBuilder.RequireClaim("UserRole", "Administrator"));
    option.AddPolicy("OnlyUser", policyBuilder => policyBuilder.RequireClaim("UserRole", "User"));
});

// AutoMapper 
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddDALRepositories();
builder.Services.AddBLLServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "What to Eat API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(opt =>
{
    opt.AddPolicy(name: "GoogleCors", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithExposedHeaders("X-Pagination");
    });
});

var app = builder.Build();
//seeding
using (var scope = app.Services.CreateScope())
{
    await RolesUsersSeeding.SeedRolesAsync(scope.ServiceProvider);

    await RolesUsersSeeding.SeedUsersAsync(scope.ServiceProvider);

    var dbcontext= scope.ServiceProvider.GetRequiredService<RecipesContext>();
    var recipeService=scope.ServiceProvider.GetRequiredService<IRecipeService>();
    if (dbcontext.Recipes.Count() == 0)
    {
        string json = File.ReadAllText(@"../recipes/dishes.txt");
        var Recipes = JsonConvert.DeserializeObject<List<RecipeDtoWithIngredientsAndSteps>>(json);

        for (int i = 0; i < Recipes.Count(); i++)
        {
            Recipes[i].Photo = "https://www.cookwithcampbells.ca/wp-content/uploads/sites/24/2016/05/SimmeredChickenDinner.jpg";
            recipeService.InsertWithIngredients(Recipes[i]);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipes.API v1"));

}

app.UseRouting();
app.UseCors("GoogleCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();