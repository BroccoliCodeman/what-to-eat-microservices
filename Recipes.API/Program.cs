using Microsoft.EntityFrameworkCore;
using Recipes.API;
using Recipes.BLL;
using Recipes.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<RecipesContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("RecipesConnectionString"));
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddDALRepositories();
builder.Services.AddBLLServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipes.API v1"));
}

app.UseAuthorization();
app.MapControllers();

app.Run();