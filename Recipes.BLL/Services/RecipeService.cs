using AutoMapper;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Recipes.BLL.Interfaces;
using Recipes.DAL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Enums;
using Recipes.Data.Interfaces;
using Recipes.Data.Models;
using Recipes.Data.Responses;

namespace Recipes.BLL.Services;

public class RecipeService : IRecipeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RecipeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IBaseResponse<RecipeDto>> GetById(Guid id)
    {
        try
        {
            var recipe = await _unitOfWork.RecipeRepository.GetByIdAsync(id);
            if (recipe == null)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<RecipeDto>($"The recipe with id {id} wasn't found", StatusCode.NotFound);
            }

            return BaseResponse<RecipeDto>.CreateBaseResponse<RecipeDto>("Sucess!", StatusCode.Ok, _mapper.Map<RecipeDto>(recipe));
        }
        catch (Exception ex)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<RecipeDto>(ex.Message, StatusCode.InternalServerError);

        }
    }

    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.RecipeRepository.GetAsync();

            if (models.Count is 0)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, dtoList.Count);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> Insert(RecipeDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            
            modelDto.Id = Guid.NewGuid();

            await _unitOfWork.RecipeRepository.InsertAsync(_mapper.Map<Recipe>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);

        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }


    public async Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredientsAndSteps? recipereqest)
    {
        try
        {
            if (recipereqest is null)
                return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object can't be empty...", StatusCode.BadRequest);


            var recipe = _mapper.Map<Recipe>(recipereqest);

            recipe.Id = Guid.NewGuid();

            // Отримати всі інгредієнти з бази даних
            var Ingredients = await _unitOfWork.IngredientRepository.GetAsync();

            // Отримати список інгредієнтів з моделі DTO, які відсутні в базі даних
            var newIngredients = recipe.Ingredients.Where(dtoIng => !Ingredients.AsReadOnly().Any(dbIng => dbIng.Name == dtoIng.Name && dbIng.WeightUnit.Type == dtoIng.WeightUnit.Type && dbIng.Quantity == dtoIng.Quantity));

            var existingIngredients = Ingredients.Where(dbIng =>
                recipe.Ingredients.Any(dtoIng =>
                dbIng.Name == dtoIng.Name &&
                dbIng.WeightUnit.Type == dtoIng.WeightUnit.Type &&
                dbIng.Quantity == dtoIng.Quantity));

            var steps = _mapper.Map<ICollection<CookingStepDtoNoId>, ICollection<CookingStep>>(recipereqest.Steps);

            
            foreach(var step in steps)
            {

                step.Id = Guid.NewGuid();

                step.RecipeId = recipe.Id;
                await _unitOfWork.CookingStepRepository.InsertAsync(step);


            }





            // Додати нові інгредієнти в базу даних
            foreach (var newIngredient in newIngredients)
            {
                newIngredient.Id = Guid.NewGuid();
                //перевіряємо чи є одиниці виміру які зазаначені в запиті є в базі
                var unit = _unitOfWork.WeightUnitRepository.GetAsync().Result.FirstOrDefault(p => p.Type == newIngredient.WeightUnit.Type);

                if (unit == null)
                {
               
                    await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(newIngredient.WeightUnit));
                    //await _unitOfWork.SaveChangesAsync();
                }
                else
                    newIngredient.WeightUnit = unit;
              await  _unitOfWork.IngredientRepository.InsertAsync(newIngredient);
            }

            // Перезаписати Id інгредієнтів, які вже існують в базі даних
      
            foreach (var existingIngredient in existingIngredients)
            {
                var recipesIngredient = recipe.Ingredients.FirstOrDefault(p =>
                    p.Name == existingIngredient.Name &&
                    p.WeightUnit.Type == existingIngredient.WeightUnit.Type &&
                    p.Quantity == existingIngredient.Quantity);

                if (recipesIngredient != null)
                {
                    // Оновлюємо властивості об'єкта, який вже відстежується контекстом
                  

                     var unit = _unitOfWork.WeightUnitRepository.GetAsync().Result.FirstOrDefault(p => p.Type == recipesIngredient.WeightUnit.Type);
                    
                    if (unit == null)
                    {
                        await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(recipesIngredient.WeightUnit));
                    }
                    else
                    {
                        recipesIngredient.WeightUnit = unit;
                    }

                    
                    recipe.Ingredients.Remove(recipesIngredient);
                    recipe.Ingredients.Add(existingIngredient);
                }
            }


            // Вставити рецепт в базу даних
            await _unitOfWork.RecipeRepository.InsertAsync(recipe);
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError, recipereqest.ToString());
        }
    }
    public async Task<IBaseResponse<string>> DeleteById(Guid id)
    {
        try
        {
            await _unitOfWork.RecipeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object deleted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>($"{e.Message} or object not found", StatusCode.InternalServerError);
        }
    }



    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByName(string name)
    {
        try
        {
            var models = await _unitOfWork.RecipeRepository.GetAsync();

            if (models.Count == 0)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<RecipeDto>(model)).ToList().Where(x => x.Title.Contains(name)); ;

            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, dtoList.ToList().Count);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
        }
    }


    /*    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients)
        {
            try
            {
                var models = await _unitOfWork.RecipeRepository.GetAsync();

                if (models.Count == 0)
                {
                    return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
                }

                // Отримання інгредієнтів з бази даних
                var allIngredients = await _unitOfWork.IngredientRepository.GetAsync();

                // Фільтрація інгредієнтів зі списку ingredients
                var filteredIngredients = allIngredients.Where(ing => ingredients.Contains(ing.Name)).ToList();

                // Отримання рецептів, які містять хоча б один інгредієнт зі списку ingredients
                var matchingRecipes = new List<Recipe>();

                foreach (var model in models)
                {
                    foreach (var ingredient in model.Ingredients)
                    {
                        if (filteredIngredients.Any(ing => ing.Name == ingredient.Name))
                        {
                            matchingRecipes.Add(model);
                            break;
                        }
                    }
                }

                if (matchingRecipes.Count == 0)
                {
                    return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("No recipes found with any of the specified ingredients", StatusCode.NotFound);
                }

                // Мапування знайдених рецептів на DTO
                var dtoList = matchingRecipes.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, matchingRecipes.Count);
            }
            catch (Exception e)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
            }
        }
    */

    /*    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients)
        {
            try
            {
                var models = await _unitOfWork.RecipeRepository.GetAsync();

                if (models.Count == 0)
                {
                    return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
                }

                // Отримання інгредієнтів з бази даних
                var allIngredients = await _unitOfWork.IngredientRepository.GetAsync();

                // Фільтрація інгредієнтів зі списку ingredients
                var filteredIngredients = allIngredients.Where(ing => ingredients.Contains(ing.Name)).ToList();

                // Отримання рецептів, які містять усі інгредієнти зі списку ingredients
                var matchingRecipes = new List<Recipe>();

                foreach (var model in models)
                {
                    bool hasAllIngredients = true;
                    foreach (var ingredientName in ingredients)
                    {
                        if (!model.Ingredients.Any(ing => ing.Name == ingredientName))
                        {
                            hasAllIngredients = false;
                            break;
                        }
                    }
                    if (hasAllIngredients)
                    {
                        matchingRecipes.Add(model);
                    }
                }

                if (matchingRecipes.Count == 0)
                {
                    return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("No recipes found with all of the specified ingredients", StatusCode.NotFound);
                }

                // Мапування знайдених рецептів на DTO
                var dtoList = matchingRecipes.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, matchingRecipes.Count);
            }
            catch (Exception e)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
            }
        }*/
    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients)
    {
        try
        {
            var models = await _unitOfWork.RecipeRepository.GetAsync();

            if (models.Count == 0)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
            }

            // Отримання інгредієнтів з бази даних
            var allIngredients = await _unitOfWork.IngredientRepository.GetAsync();

            // Фільтрація інгредієнтів зі списку ingredients
            var filteredIngredients = allIngredients.Where(ing => ingredients.Contains(ing.Name)).ToList();

            // Отримання рецептів, які містять хоча б один інгредієнт зі списку ingredients, але не містять рецептів без інгредієнтів зі списку
            var matchingRecipes = new List<Recipe>();

            foreach (var model in models)
            {
                if (model.Ingredients.Any(ing => ingredients.Contains(ing.Name)) && !model.Ingredients.All(ing => !ingredients.Contains(ing.Name)))
                {
                    matchingRecipes.Add(model);
                }
            }

            if (matchingRecipes.Count == 0)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("No recipes found with any of the specified ingredients", StatusCode.NotFound);
            }

            // Мапування знайдених рецептів на DTO
            var dtoList = matchingRecipes.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, matchingRecipes.Count);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
        }
    }


}