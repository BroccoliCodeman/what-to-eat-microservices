using AutoMapper;
using Recipes.BLL.Helpers;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Helpers;
using Recipes.Data.Models;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services;

public class RecipeService : IRecipeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ResponseCreator _responseCreator;

    public RecipeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _responseCreator = new ResponseCreator();
    }
    public async Task<IBaseResponse<PagedList<RecipeDto>>> Get(PaginationParams paginationParams, SearchParams? searchParams)
    {
        try
        {
            var models = await _unitOfWork.RecipeRepository.GetAsync(paginationParams);

            if (models.Count == 0)
                return _responseCreator.CreateBaseNotFound<PagedList<RecipeDto>>("No recipes found.");

            var dtoList = models.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

            if (searchParams != null)
            {
                if (!string.IsNullOrEmpty(searchParams.Title))
                {
                    dtoList = dtoList.Where(dto => dto.Title.Contains(searchParams.Title, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (searchParams.Ingredients != null && searchParams.Ingredients.Any())
                {
                    dtoList = dtoList.Where(dto => searchParams.Ingredients.All(ingredient =>
                        dto.Ingredients.Any(dtoIngredient => dtoIngredient.Name.Contains(ingredient, StringComparison.OrdinalIgnoreCase))
                    )).ToList();
                }
            }

            var pagedList = new PagedList<RecipeDto>(dtoList, models.TotalCount, models.CurrentPage, models.PageSize);

            return _responseCreator.CreateBaseOk(pagedList, pagedList.TotalCount);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<PagedList<RecipeDto>>(e.Message);
        }
    }
    public async Task<IBaseResponse<RecipeDto>> GetById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return _responseCreator.CreateBaseBadRequest<RecipeDto>("Id is empty.");
            
            var recipeDto = _mapper.Map<RecipeDto>(await _unitOfWork.RecipeRepository.GetByIdAsync(id));
            
            if (recipeDto == null)
                return _responseCreator.CreateBaseNotFound<RecipeDto>($"Recipe with id {id} not found.");

            return _responseCreator.CreateBaseOk(recipeDto, 1);
        }
        catch (Exception ex)
        {
            return _responseCreator.CreateBaseServerError<RecipeDto>(ex.Message);
        }
    }
    public async Task<IBaseResponse<string>> Insert(RecipeDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return _responseCreator.CreateBaseBadRequest<string>("Inserted recipe is empty.");
            
            modelDto.Id = Guid.NewGuid();

            await _unitOfWork.RecipeRepository.InsertAsync(_mapper.Map<Recipe>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk($"Recipe added.", 1);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<string>(e.Message);
        }
    }
    
    public async Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredientsAndSteps? recipeRequest)
    {
        try
        {
            if (recipeRequest is null)
                return _responseCreator.CreateBaseBadRequest<string>("Inserted recipe is empty.");
            
            var recipe = _mapper.Map<Recipe>(recipeRequest);

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

            var steps = _mapper.Map<ICollection<CookingStepDtoNoId>, ICollection<CookingStep>>(recipeRequest.Steps);
            
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

            return _responseCreator.CreateBaseOk($"Recipe added.", 1);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<string>(e.Message);
        }
    }
    public async Task<IBaseResponse<string>> DeleteById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return _responseCreator.CreateBaseBadRequest<string>("Id is empty.");
            
            await _unitOfWork.RecipeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk("Recipe deleted.", 1);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<string>(e.Message);
        }
    }
    
    public async Task<IBaseResponse<List<RecipeDto>>> GetByName(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                return _responseCreator.CreateBaseBadRequest<List<RecipeDto>>("Name is empty.");
            
            var models = await _unitOfWork.RecipeRepository.GetAsync();

            if (models.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeDto>>("No recipes found.");

            var dtoList = models.Select(model => _mapper.Map<RecipeDto>(model)).ToList().Where(x => x.Title.Contains(name)).ToList(); ;

            return _responseCreator.CreateBaseOk(dtoList, dtoList.Count);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<List<RecipeDto>>(e.Message);
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
    
    public async Task<IBaseResponse<List<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients)
    {
        try
        {
            var enumerable = ingredients.ToList();
            if (!enumerable.ToList().Any())
                return _responseCreator.CreateBaseBadRequest<List<RecipeDto>>("No ingredients specified.");
            
            var models = await _unitOfWork.RecipeRepository.GetAsync();

            if (models.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeDto>>("No recipes found.");

            // Отримання інгредієнтів з бази даних
            var allIngredients = await _unitOfWork.IngredientRepository.GetAsync();

            // Фільтрація інгредієнтів зі списку ingredients
            var filteredIngredients = allIngredients.Where(ing => enumerable.Contains(ing.Name)).ToList();

            // Отримання рецептів, які містять хоча б один інгредієнт зі списку ingredients, але не містять рецептів без інгредієнтів зі списку
            var matchingRecipes = new List<Recipe>();

            foreach (var model in models)
            {
                if (model.Ingredients.Any(ing => enumerable.Contains(ing.Name)) && !model.Ingredients.All(ing => !enumerable.Contains(ing.Name)))
                {
                    matchingRecipes.Add(model);
                }
            }

            if (matchingRecipes.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeDto>>("No recipes found with any of the specified ingredients.");
            
            // Мапування знайдених рецептів на DTO
            var dtoList = matchingRecipes.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

            return _responseCreator.CreateBaseOk(dtoList, dtoList.Count);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<List<RecipeDto>>(e.Message);
        }
    }
}