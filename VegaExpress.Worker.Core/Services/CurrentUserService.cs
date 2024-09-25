using Microsoft.AspNetCore.Http;
using System.Security.Claims;

using VegaExpress.Worker.Core.Services.Contracts;

namespace VegaExpress.Worker.Core.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            UserId = httpContextAccessor.HttpContext?.User.FindFirstValue("uid")!;
            UserName = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Email = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)!;
            Claims = httpContextAccessor.HttpContext?.User?.Claims.AsEnumerable().Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToList()!;
        }

        public string UserId { get; }
        public string UserName { get; }
        public string Email { get; }
        public List<KeyValuePair<string, string>> Claims { get; set; }

    }
}
