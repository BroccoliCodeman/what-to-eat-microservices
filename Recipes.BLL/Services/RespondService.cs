using AutoMapper;
using Recipes.BLL.Helpers;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services;

public class RespondService : IRespondService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ResponseCreator _responseCreator;

    public RespondService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _responseCreator = new ResponseCreator();
    }

    public Task<IBaseResponse<RespondDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<List<RespondDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.RespondRepository.GetAsync();

            if (models.Count is 0)
                return _responseCreator.CreateBaseNotFound<List<RespondDto>>("No responds found.");

            var dtoList = models.Select(model => _mapper.Map<RespondDto>(model)).ToList();

            return _responseCreator.CreateBaseOk(dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return _responseCreator.CreateBaseServerError<List<RespondDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<string>> Insert(AddRespondDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return _responseCreator.CreateBaseBadRequest<string>("Respond is empty.");
            
            await _unitOfWork.RespondRepository.InsertAsync(_mapper.Map<Respond>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk($"Respond added.", 1);
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
            
            await _unitOfWork.RespondRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk("Respond deleted.", 1);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<string>(e.Message);
        }
    }
}