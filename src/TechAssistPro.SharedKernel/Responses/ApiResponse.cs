using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.SharedKernel.Responses
{
    public record ApiResponse<T>(bool Success, T? Data, T? Errors);
}