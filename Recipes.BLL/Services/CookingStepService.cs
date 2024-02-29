using AutoMapper;
using Recipes.BLL.Interfaces;
using Recipes.DAL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Enums;
using Recipes.Data.Interfaces;
using Recipes.Data.Models;
using Recipes.Data.Responses;

namespace Recipes.BLL.Services;

public class CookingStepService : ICookingStepService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CookingStepService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<IBaseResponse<CookingStepDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<IEnumerable<CookingStepDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.CookingStepRepository.GetAsync();

            if (models.Count is 0)
            {
                return CreateBaseResponse<IEnumerable<CookingStepDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<CookingStepDto>(model)).ToList();

            return CreateBaseResponse<IEnumerable<CookingStepDto>>("Success!", StatusCode.Ok, dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return CreateBaseResponse<IEnumerable<CookingStepDto>>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> Insert(CookingStepDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            
            modelDto.Id = Guid.NewGuid();
                
            await _unitOfWork.CookingStepRepository.InsertAsync(_mapper.Map<CookingStep>(modelDto));
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
            await _unitOfWork.CookingStepRepository.DeleteAsync(id);
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