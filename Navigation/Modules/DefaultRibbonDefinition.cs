namespace SQTWeb.Navigation.Modules;

/// <summary>
/// Default/fallback ribbon definition for modules that have not
/// yet provided a custom implementation.
/// </summary>
public sealed class DefaultRibbonDefinition : IModuleRibbonDefinition
{
    private readonly AppModule _module;

    public DefaultRibbonDefinition(AppModule module)
    {
        _module = module;
    }

    public AppModule Module => _module;
    public string DisplayName => _module.ToString();
    public string ListUrl => "/";

    public string? ResolveAction(RibbonItemKey key, IPageContext context) =>
        key switch
        {
            RibbonItemKey.Cancel => "/",
            _ => null
        };

    public IReadOnlyList<RibbonDropdownItemModel> GetDropdownItems(RibbonItemKey key, IPageContext context) => [];

    public IReadOnlyList<BreadcrumbItemModel> GetBreadcrumbs(IPageContext context) =>
    [
        new() { Label = "Home", Url = "/" },
        new() { Label = DisplayName, IsActive = true }
    ];
}
