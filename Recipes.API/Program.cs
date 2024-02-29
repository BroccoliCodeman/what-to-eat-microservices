using Recipes.BLL;
using Recipes.BLL.Interfaces;
using Recipes.BLL.Services;
using Recipes.DAL.Interfaces;
using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.DAL.Repositories;
using Recipes.DAL.Repositories.ModelsRepositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// database connection will be here

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<ICookingStepRepository, CookingStepRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IRecipeIngredientRepository, RecipeIngredientRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IRespondRepository, RespondRepository>();
builder.Services.AddScoped<ISavedRecipeRepository, SavedRecipeRepository>();
builder.Services.AddScoped<IWeightUnitRepository, WeightUnitRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ICookingStepService, CookingStepService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IRecipeIngredientService, RecipeIngredientService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IRespondService, RespondService>();
builder.Services.AddScoped<ISavedRecipeService, SavedRecipeService>();
builder.Services.AddScoped<IWeightUnitService, WeightUnitService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();