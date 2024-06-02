using AutoMapper;
using Recipes.BLL.Helpers;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services;

public class IngredientService : IIngredientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ResponseCreator _responseCreator;

    public IngredientService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _responseCreator = new ResponseCreator();
    }

    public Task<IBaseResponse<IngredientDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<List<IngredientDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.IngredientRepository.GetAsync();

            if (models.Count is 0)
                return _responseCreator.CreateBaseNotFound<List<IngredientDto>>("No ingredients found.");

            var dtoList = models.Select(model => _mapper.Map<IngredientDto>(model)).ToList();

            return _responseCreator.CreateBaseOk(dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return _responseCreator.CreateBaseServerError<List<IngredientDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<List<IngredientIntroDto>>> GetByName(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                return _responseCreator.CreateBaseBadRequest<List<IngredientIntroDto>>("Name can't be empty.");
            
            var ingredients = await _unitOfWork.IngredientRepository.GetByName(name);
            
            if (ingredients.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<IngredientIntroDto>>("No ingredients found.");

            var ingredientIntroDtos = ingredients.Select(recipe => _mapper.Map<IngredientIntroDto>(recipe)).ToList();
            
            return _responseCreator.CreateBaseOk(ingredientIntroDtos, ingredientIntroDtos.Count);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<List<IngredientIntroDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<string>> Insert(IngredientDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return _responseCreator.CreateBaseBadRequest<string>("Inserted ingredient is empty.");
            
            var ing = _mapper.Map<Ingredient>(modelDto);
            ing.Id = Guid.NewGuid();
            var value = _unitOfWork.WeightUnitRepository.GetAsync().Result.Where(p=>p.Type == modelDto.WeightUnit.Type);

            if (value.Count()!=0)
                await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(modelDto.WeightUnit));

            await _unitOfWork.IngredientRepository.InsertAsync(ing);
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk($"Ingredient added.", 1);
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
            
            await _unitOfWork.IngredientRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk("Ingredient deleted.", 1);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<string>(e.Message);
        }
    }
}