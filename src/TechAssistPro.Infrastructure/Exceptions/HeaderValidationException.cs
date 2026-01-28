using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Infrastructure.Exceptions
{
    public class HeaderValidationException : Exception
    {
        public HeaderValidationException(string message) : base(message)
        {
        }
    }
}