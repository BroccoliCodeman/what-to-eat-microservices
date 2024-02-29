namespace Recipes.Data.Interfaces;

public interface IBaseResponse<T>
{
    T Data { get; set; }
}