namespace SQTWeb.Navigation;

/// <summary>
/// Identifies each ribbon button/item uniquely.
/// Used by <see cref="IModuleRibbonDefinition"/> to map actions per module.
/// </summary>
public enum RibbonItemKey
{
    Add,
    Edit,
    Copy,
    Delete,
    Save,
    Cancel,
    PrintDropDown,
    SendDropDown,
    Breadcrumb,
    DockToggle
}
