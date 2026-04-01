namespace SQTWeb.Navigation;

/// <summary>
/// Fully-composed, render-ready ribbon menu model.
/// <para>
/// This is the single source of truth for the entire ribbon bar.
/// The view must only read from this model — no logic in Razor.
/// </para>
/// </summary>
public sealed class RibbonMenuModel
{
    /// <summary>Left-aligned action buttons (Add, Edit, Save, dropdowns, etc.).</summary>
    public IReadOnlyList<RibbonItemModel> LeftItems { get; init; } = [];

    /// <summary>Breadcrumb trail for the right side.</summary>
    public IReadOnlyList<BreadcrumbItemModel> Breadcrumbs { get; init; } = [];

    /// <summary>Whether the sidebar/dock toggle button is visible.</summary>
    public bool ShowDockToggle { get; init; } = true;

    /// <summary>Label for the dock toggle button.</summary>
    public string DockToggleLabel { get; init; } = "Toggle Panel";

    /// <summary>Icon for the dock toggle button.</summary>
    public string DockToggleIcon { get; init; } = "bi bi-layout-sidebar-reverse";
}
