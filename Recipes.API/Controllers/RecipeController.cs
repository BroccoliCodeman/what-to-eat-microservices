using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Recipes.BLL.Services.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Helpers;

namespace Recipes.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService _service;

    public RecipeController(IRecipeService service)
    {
        _service = service;
    }

    [HttpPost("Get")]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> Get([FromQuery] PaginationParams? paginationParams = null,
                                                                [FromBody] SearchParams? searchParams = null,
                                                                [FromQuery] int sortType = 0)
    {
        var response = await _service.Get(paginationParams!, searchParams!, sortType);

        object metadata = new object();

        if (response.Data != null)
        {
            metadata = new
            {
                response.Data.TotalCount,
                response.Data.PageSize,
                response.Data.CurrentPage,
                response.Data.TotalPages,
                response.Data.HasNext,
                response.Data.HasPrevious
            };
        }


        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpGet("GetMostPopularRecipesTitles")]
    public async Task<ActionResult<IEnumerable<RecipeIntroDto>>> GetMostPopularRecipesTitles()
    {
        var response = await _service.GetMostPopularRecipesTitles();

        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpGet("GetByTitle")]
    public async Task<ActionResult<IEnumerable<RecipeIntroDto>>> GetByTitle([FromQuery] string title)
    {
        var response = await _service.GetByTitle(title);

        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpGet("GetRandom")]
    public async Task<ActionResult<RecipeDto>> GetRandom()
    {
        var response = await _service.GetRandom();

        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecipeDto>> GetByid(Guid id)
    {
        var response = await _service.GetById(id);

        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    [HttpGet("GetRecipesByUserId/{UserId}")]
    public async Task<ActionResult<IEnumerable<RecipeIntroDto>>> GetByUserIdAsync(Guid UserId)
        {

        var response = await _service.GetByUserId(UserId);

        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpPost("SaveRecipe")]
    public async Task<ActionResult> SaveRecipeAsync([FromQuery]Guid UserId, [FromQuery]Guid RecipeId)
    {
        var response = await _service.SaveRecipe(UserId,RecipeId);

        return response.StatusCode switch
        {
            Data.Responses.Enums.StatusCode.Ok => Ok(response),
            Data.Responses.Enums.StatusCode.NotFound => NotFound(response),
            Data.Responses.Enums.StatusCode.BadRequest => BadRequest(response),
            Data.Responses.Enums.StatusCode.InternalServerError => StatusCode(500, response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpPost("RemoveRecipeFromSaved")]
    public async Task<ActionResult> RemoveRecipeFromSavedAsync([FromQuery] Guid UserId, [FromQuery] Guid RecipeId)
    {
        var response = await _service.RemoveRecipeFromSaved(UserId, RecipeId);

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
    public async Task<ActionResult> Insert([FromBody] RecipeDto modelDto)
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
    
    [HttpPost("PostWithIngredientsAndCookingSteps")]
    public async Task<ActionResult> InsertWithIngredientsAndCookingSteps([FromBody] RecipeDtoWithIngredientsAndSteps modelDto)
    {
        var response = await _service.InsertWithIngredients(modelDto);
        
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