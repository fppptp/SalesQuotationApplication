namespace SQTWeb.Navigation;

/// <summary>
/// Each <see cref="AppModule"/> provides its own ribbon definition.
/// This interface decouples module-specific ribbon behaviour from the
/// generic ribbon composition engine.
/// </summary>
public interface IModuleRibbonDefinition
{
    /// <summary>The module this definition serves.</summary>
    AppModule Module { get; }

    /// <summary>
    /// Resolves the URL or command for a given ribbon item key
    /// based on the current page context.
    /// Returns <c>null</c> when the key is not applicable to this module.
    /// </summary>
    string? ResolveAction(RibbonItemKey key, IPageContext context);

    /// <summary>
    /// Returns dropdown child items for a given key (e.g. PrintDropDown, SendDropDown).
    /// Returns empty when the key has no dropdown items.
    /// </summary>
    IReadOnlyList<RibbonDropdownItemModel> GetDropdownItems(RibbonItemKey key, IPageContext context);

    /// <summary>
    /// Returns the breadcrumb trail for the current page context.
    /// </summary>
    IReadOnlyList<BreadcrumbItemModel> GetBreadcrumbs(IPageContext context);

    /// <summary>
    /// Module-level display name used in breadcrumbs and headings.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// URL of the module's list/index page.
    /// </summary>
    string ListUrl { get; }
}
