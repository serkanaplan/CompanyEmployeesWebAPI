namespace Entities.Responses;

public sealed class ApiOkResponse<TResult>(TResult result) : ApiBaseResponse(true) 
{
    public TResult Result { get; set; } = result;
}
