using AutoMapper;
using Recipes.BLL.Interfaces;
using Recipes.DAL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Enums;
using Recipes.Data.Interfaces;
using Recipes.Data.Models;
using Recipes.Data.Responses;

namespace Recipes.BLL.Services;

public class RespondService : IRespondService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RespondService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<IBaseResponse<RespondDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<IEnumerable<RespondDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.RespondRepository.GetAsync();

            if (models.Count is 0)
            {
                return BaseResponse<RespondDto>.CreateBaseResponse<IEnumerable<RespondDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<RespondDto>(model)).ToList();

            return BaseResponse<RespondDto>.CreateBaseResponse<IEnumerable<RespondDto>>("Success!", StatusCode.Ok, dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return BaseResponse<RespondDto>.CreateBaseResponse<IEnumerable<RespondDto>>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> Insert(RespondDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return BaseResponse<RespondDto>.CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            
            await _unitOfWork.RespondRepository.InsertAsync(_mapper.Map<Respond>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RespondDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);

        }
        catch (Exception e)
        {
            return BaseResponse<RespondDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> DeleteById(Guid id)
    {
        try
        {
            await _unitOfWork.RespondRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RespondDto>.CreateBaseResponse<string>("Object deleted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RespondDto>.CreateBaseResponse<string>($"{e.Message} or object not found", StatusCode.InternalServerError);
        }
    }
    

}