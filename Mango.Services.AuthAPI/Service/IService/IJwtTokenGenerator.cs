using Mango.Services.AuthAPI.Models;
using System.Collections.Generic;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken( ApplicationUser applicationUser, IEnumerable<string> roles);
    }
}
