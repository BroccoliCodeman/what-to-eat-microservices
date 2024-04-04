using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Recipes.BLL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models.HelpersModel;
using System.Text.Json.Nodes;
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
    
    [HttpGet("Get")]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> Get([FromQuery] CookingTimeModel? cookingTimeModel = null)
    {
        var recipes = await _service.Get(cookingTimeModel);
        return Ok(recipes);
    }
    
    [HttpPost]
    public async Task<ActionResult> Insert([FromBody] RecipeDto modelDto)
    {
        return Ok(await _service.Insert(modelDto));
    }
    [HttpPost("PostWithIgredients")]
    public async Task<ActionResult> InsertWithIngredients([FromBody] RecipeDtoWithIngredients modelDto)
    {
        return Ok(await _service.InsertWithIngredients(modelDto));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteById(Guid id)
    {

        return Ok(await _service.DeleteById(id));

    }


    [HttpGet("GetByName")]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetByName( string name)
    {
        return Ok(await _service.GetByName(name));
    }


    [HttpPost("GetByIngredients")]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetByIngredients( RecipeByIngredientsRequest request)
    {
        return Ok(await _service.GetByIngredients(request.Ingredients));
    }
}