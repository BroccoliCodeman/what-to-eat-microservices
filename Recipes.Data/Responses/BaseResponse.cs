using Recipes.Data.Enums;
using Recipes.Data.Interfaces;

namespace Recipes.Data.Responses;

public class BaseResponse<T> : IBaseResponse<T>
{
    /// <summary>
    /// Errors, warnings, success describing property
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Status of process, can be: OK, NotFound, InternalServerError
    /// </summary>
    public StatusCode StatusCode { get; set; }

    /// <summary>
    /// Returns count of objects from get query
    /// </summary>
    public int ResultsCount { get; set; }

    /// <summary>
    /// Received data from DAL
    /// </summary>
    public T Data { get; set; } 

    public static BaseResponse<T> CreateBaseResponse<T>(string description, StatusCode statusCode, T? data = default, int resultsCount = 0)
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