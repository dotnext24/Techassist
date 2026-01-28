using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Infrastructure.SchemaRegistry
{
    public class SchemaValidationException : Exception
    {
        public SchemaValidationException(string message) : base(message)
        {
        }
    }
}