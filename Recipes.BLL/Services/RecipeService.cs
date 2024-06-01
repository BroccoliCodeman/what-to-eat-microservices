using AutoMapper;
using Recipes.BLL.Helpers;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Helpers;
using Recipes.Data.Models;
using Recipes.Data.Responses;
using Recipes.Data.Responses.Enums;
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

    public async Task<IBaseResponse<string>> SaveRecipe(Guid UserId, Guid RecipeId)
    {
        try
        {
            await _unitOfWork.RecipeRepository.SaveRecipe(UserId, RecipeId);

 

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Recipe saved!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> RemoveRecipeFromSaved(Guid UserId, Guid RecipeId)
    {
        try
        {
            await _unitOfWork.RecipeRepository.RemoveRecipeFromSaved(UserId, RecipeId);



            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Recipe Removed From Saved!", StatusCode.Ok, resultsCount: 1);
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
         
            foreach (var newIngredient in newIngredients)
            {
                newIngredient.Id = Guid.NewGuid();
                var unit = _unitOfWork.WeightUnitRepository.GetAsync().Result.FirstOrDefault(p => p.Type == newIngredient.WeightUnit.Type);

                if (unit == null)
                {
               
                    await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(newIngredient.WeightUnit));
                }
                else
                    newIngredient.WeightUnit = unit;
              await  _unitOfWork.IngredientRepository.InsertAsync(newIngredient);
            }
            foreach (var existingIngredient in existingIngredients)
            {
                var recipesIngredient = recipe.Ingredients.FirstOrDefault(p =>
                    p.Name == existingIngredient.Name &&
                    p.WeightUnit.Type == existingIngredient.WeightUnit.Type &&
                    p.Quantity == existingIngredient.Quantity);

                if (recipesIngredient != null)
                {

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

}