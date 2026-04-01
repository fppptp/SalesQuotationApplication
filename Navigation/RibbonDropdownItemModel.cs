namespace SQTWeb.Navigation;

/// <summary>
/// A single item inside a dropdown ribbon button.
/// </summary>
public sealed class RibbonDropdownItemModel
{
    /// <summary>Display label.</summary>
    public required string Label { get; init; }

    /// <summary>Navigation URL (null if the item triggers a JS command).</summary>
    public string? Url { get; init; }

    /// <summary>Optional JavaScript command instead of navigation.</summary>
    public string? Command { get; init; }

    /// <summary>Bootstrap icon class (e.g. "bi bi-printer").</summary>
    public string? Icon { get; init; }

    public bool IsEnabled { get; init; } = true;
}
