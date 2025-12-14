using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Infrastructure.SchemaRegistry
{
    public class SchemaValidationException : Exception
    {
        public Dictionary<string, string[]> Errors { get; }

        public SchemaValidationException(Dictionary<string, string[]> errors)
            : base("Schema validation failed")
        {
            Errors = errors;
        }
    }
}