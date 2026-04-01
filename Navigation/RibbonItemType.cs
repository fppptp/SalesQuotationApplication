namespace SQTWeb.Navigation;

/// <summary>
/// Ribbon item rendering type — determines how the item appears in the UI.
/// </summary>
public enum RibbonItemType
{
    /// <summary>Standard button that navigates or submits.</summary>
    Button,

    /// <summary>Dropdown button with child items.</summary>
    Dropdown,

    /// <summary>A form-submit button (e.g., Save uses form post).</summary>
    Submit
}
