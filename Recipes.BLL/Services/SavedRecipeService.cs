using AutoMapper;
using Recipes.BLL.Interfaces;
using Recipes.DAL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Enums;
using Recipes.Data.Interfaces;
using Recipes.Data.Models;
using Recipes.Data.Responses;

namespace Recipes.BLL.Services;

public class SavedRecipeService : ISavedRecipeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SavedRecipeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<IBaseResponse<SavedRecipeDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<IEnumerable<SavedRecipeDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.SavedRecipeRepository.GetAsync();

            if (models.Count is 0)
            {
                return CreateBaseResponse<IEnumerable<SavedRecipeDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<SavedRecipeDto>(model)).ToList();

            return CreateBaseResponse<IEnumerable<SavedRecipeDto>>("Success!", StatusCode.Ok, dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return CreateBaseResponse<IEnumerable<SavedRecipeDto>>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> Insert(SavedRecipeDto? modelDto)
    {
        try
        {
            if (modelDto is null)
                return CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            
            await _unitOfWork.SavedRecipeRepository.InsertAsync(_mapper.Map<SavedRecipe>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);

        }
        catch (Exception e)
        {
            return CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> DeleteById(Guid id)
    {
        try
        {
            await _unitOfWork.SavedRecipeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return CreateBaseResponse<string>("Object deleted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return CreateBaseResponse<string>($"{e.Message} or object not found", StatusCode.InternalServerError);
        }
    }
    
    private BaseResponse<T> CreateBaseResponse<T>(string description, StatusCode statusCode, T? data = default, int resultsCount = 0)
    {
        return new BaseResponse<T>()
        {
            ResultsCount = resultsCount,
            Data = data!,
            Description = description,
            StatusCode = statusCode
        };
    }
}