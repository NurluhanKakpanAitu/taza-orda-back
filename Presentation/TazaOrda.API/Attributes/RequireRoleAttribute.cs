using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;

namespace TazaOrda.API.Attributes;

/// <summary>
/// Атрибут для проверки роли пользователя
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole[] _allowedRoles;

    public RequireRoleAttribute(params UserRole[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.Items["User"] as User;

        if (user == null)
        {
            context.Result = new JsonResult(new { message = "Пользователь не аутентифицирован" })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        if (!_allowedRoles.Contains(user.Role))
        {
            context.Result = new JsonResult(new { message = "Доступ запрещён. Недостаточно прав." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }
    }
}
