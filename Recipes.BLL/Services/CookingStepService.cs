using AutoMapper;
using Recipes.BLL.Helpers;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services;

public class CookingStepService : ICookingStepService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ResponseCreator _responseCreator;

    public CookingStepService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _responseCreator = new ResponseCreator();
    }

    public Task<IBaseResponse<CookingStepDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<List<CookingStepDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.CookingStepRepository.GetAsync();

            if (models.Count is 0)
                return _responseCreator.CreateBaseNotFound<List<CookingStepDto>>("No cooking steps found.");

            var dtoList = models.Select(model => _mapper.Map<CookingStepDto>(model)).ToList();

            return _responseCreator.CreateBaseOk(dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return _responseCreator.CreateBaseServerError<List<CookingStepDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<string>> Insert(CookingStepDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return _responseCreator.CreateBaseBadRequest<string>("Inserted cooking step is empty.");
            
            modelDto.Id = Guid.NewGuid();
                
            await _unitOfWork.CookingStepRepository.InsertAsync(_mapper.Map<CookingStep>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk($"Cooking step added.", 1);

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
            
            await _unitOfWork.CookingStepRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk("Cooking step deleted.", 1);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<string>(e.Message);
        }
    }
}