using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Results
{
    public record Error(string Code, string Description)
    {
        public static Error None => new Error(string.Empty, string.Empty);
    }
}
