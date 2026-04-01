using Microsoft.AspNetCore.Routing;

namespace SQTWeb.Navigation;

/// <summary>
/// Scoped service that derives the current <see cref="AppModule"/> and
/// <see cref="FormMode"/> from the route data of the current HTTP request.
/// <para>
/// Registered as <c>Scoped</c> in DI — one instance per request.
/// No static/global state.
/// </para>
/// </summary>
public sealed class PageContext : IPageContext
{
    public AppModule Module { get; }
    public FormMode FormMode { get; }
    public string? EntityId { get; }
    public bool IsList { get; }
    public bool IsForm { get; }

    public PageContext(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var routeData = httpContext.GetRouteData();
        var controller = routeData?.Values["controller"]?.ToString() ?? string.Empty;
        var action = routeData?.Values["action"]?.ToString() ?? string.Empty;
        var id = routeData?.Values["id"]?.ToString();

        Module = ResolveModule(controller);
        FormMode = ResolveFormMode(action);
        EntityId = id;
        IsList = action.Contains("List", StringComparison.OrdinalIgnoreCase)
              || action.Equals("Index", StringComparison.OrdinalIgnoreCase);
        IsForm = FormMode is FormMode.Add or FormMode.Edit or FormMode.View;
    }

    /// <summary>
    /// Maps controller name → <see cref="AppModule"/>.
    /// </summary>
    private static AppModule ResolveModule(string controller) =>
        controller.ToUpperInvariant() switch
        {
            "HOME"                   => AppModule.Dashboard,
            "QUOTATIONS"             => AppModule.Quotation,
            "QUOTATIONSHIPMENTDETAILS" => AppModule.Quotation,
            "CHARGECODES"            => AppModule.ChargeCode,
            "CUSTOMERS"              => AppModule.Customer,
            "AGENTS"                 => AppModule.Agent,
            "PORTS"                  => AppModule.Port,
            "UNITS"                  => AppModule.Unit,
            "ADMIN"                  => AppModule.Admin,
            _                        => AppModule.None
        };

    /// <summary>
    /// Maps action name → <see cref="FormMode"/>.
    /// Convention: Create* → Add, Edit* → Edit, *Details → View.
    /// </summary>
    private static FormMode ResolveFormMode(string action)
    {
        if (action.StartsWith("Create", StringComparison.OrdinalIgnoreCase))
            return FormMode.Add;

        if (action.StartsWith("Edit", StringComparison.OrdinalIgnoreCase))
            return FormMode.Edit;

        if (action.EndsWith("Details", StringComparison.OrdinalIgnoreCase)
            || action.StartsWith("View", StringComparison.OrdinalIgnoreCase))
            return FormMode.View;

        return FormMode.View;
    }
}
