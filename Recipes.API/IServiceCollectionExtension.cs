﻿using Recipes.BLL.Interfaces;
using Recipes.BLL.Services;
using Recipes.DAL.Interfaces;
using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.DAL.Repositories;
using Recipes.DAL.Repositories.ModelsRepositories;

namespace Recipes.API
{
    public static class IServiceCollectionExtension
    {
        public static void AddDALRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICookingStepRepository, CookingStepRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<IRecipeIngredientRepository, RecipeIngredientRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IRespondRepository, RespondRepository>();
            services.AddScoped<ISavedRecipeRepository, SavedRecipeRepository>();
            services.AddScoped<IWeightUnitRepository, WeightUnitRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
        public static void AddBLLServices(this IServiceCollection services)
        {
            services.AddScoped<ICookingStepService, CookingStepService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IRecipeIngredientService, RecipeIngredientService>();
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IRespondService, RespondService>();
            services.AddScoped<ISavedRecipeService, SavedRecipeService>();
            services.AddScoped<IWeightUnitService, WeightUnitService>();
        }
    }
}
