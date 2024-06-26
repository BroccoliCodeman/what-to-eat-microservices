﻿using Recipes.BLL.Services;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.DAL.Repositories;
using Recipes.DAL.Repositories.Interfaces;

namespace Recipes.API
{
    public static class IServiceCollectionExtension
    {
        public static void AddDALRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICookingStepRepository, CookingStepRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IRespondRepository, RespondRepository>();
            services.AddScoped<IWeightUnitRepository, WeightUnitRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
        public static void AddBLLServices(this IServiceCollection services)
        {
            services.AddScoped<ICookingStepService, CookingStepService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IRespondService, RespondService>();
            services.AddScoped<IWeightUnitService, WeightUnitService>();
            services.AddScoped<ITokenService, TokenService>();
        }
    }
}
