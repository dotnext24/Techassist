
// SharedKernel/Exceptions/NotFoundException.cs
namespace TechAssistPro.SharedKernel.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
