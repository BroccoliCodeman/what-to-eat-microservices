using AutoMapper;
using Recipes.BLL.Helpers;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services;

public class SavedRecipeService : ISavedRecipeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ResponseCreator _responseCreator;

    public SavedRecipeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _responseCreator = new ResponseCreator();
    }

    public Task<IBaseResponse<SavedRecipeDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<List<SavedRecipeDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.SavedRecipeRepository.GetAsync();

            if (models.Count is 0)
                return _responseCreator.CreateBaseNotFound<List<SavedRecipeDto>>("No saved recipes found.");

            var dtoList = models.Select(model => _mapper.Map<SavedRecipeDto>(model)).ToList();

            return _responseCreator.CreateBaseOk(dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return _responseCreator.CreateBaseServerError<List<SavedRecipeDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<string>> Insert(SavedRecipeDto? modelDto)
    {
        try
        {
            if (modelDto is null)
                return _responseCreator.CreateBaseBadRequest<string>("Inserted saved recipes is empty.");
            
            await _unitOfWork.SavedRecipeRepository.InsertAsync(_mapper.Map<SavedRecipe>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk($"Saved recipes added.", 1);
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
            
            await _unitOfWork.SavedRecipeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk("Saved recipe deleted.", 1);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<string>(e.Message);
        }
    }
}