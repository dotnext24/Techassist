using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Scheduling.Entities
{
   public sealed record Skill(
    string Category,
    int Level
);

}