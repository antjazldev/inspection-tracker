using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionTracker.Application.Dtos
{
    public record RegisterDto(string Email, string DisplayName, string Password);
}
