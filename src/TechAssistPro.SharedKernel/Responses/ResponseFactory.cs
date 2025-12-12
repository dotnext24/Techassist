namespace TechAssistPro.SharedKernel.Responses;

public class ResponseFactory : IResponseFactory
{
    public ApiResponse<T> Success<T>(T data)
        => new ApiResponse<T>(true, data, default!);

    public ApiResponse<string> Success(string message)
        => new ApiResponse<string>(true, message, null);

    public ApiResponse<T> Error<T>(T errors)
        => new ApiResponse<T>(false, default!, errors);
}
