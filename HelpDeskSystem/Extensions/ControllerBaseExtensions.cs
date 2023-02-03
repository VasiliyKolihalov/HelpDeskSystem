using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Extensions;

public static class ControllerBaseExtensions
{
    public static Guid GetUserId(this ControllerBase @this)
    {
        Claim userIdClaim = @this.User.Claims.Single(_ => _.Type == ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim.Value);
    }
    
}