using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.SharedKernel.Responses
{
    public interface IResponseFactory
    {
        ApiResponse<T> Success<T>(T data);
        ApiResponse<string> Success(string message);
        ApiResponse<T> Error<T>(T? errors);
    }
}