using Microsoft.AspNetCore.Mvc;
using Recipes.BLL.Services.Interfaces;
using Recipes.Data.DataTransferObjects;

namespace Recipes.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SavedRecipeController : ControllerBase
{
    private readonly ISavedRecipeService _service;

    public SavedRecipeController(ISavedRecipeService service)
    {
        _service = service;
    }
    
    [HttpGet("Get")]
    public async Task<ActionResult<IEnumerable<SavedRecipeDto>>> Get()
    {
        var response = await _service.Get();
        
        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    [HttpPost]
    public async Task<ActionResult> Insert([FromBody] SavedRecipeDto modelDto)
    {
        var response = await _service.Insert(modelDto);
        
        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteById(Guid id)
    {
        var response = await _service.DeleteById(id);
        
        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}