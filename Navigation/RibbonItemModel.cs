namespace SQTWeb.Navigation;

/// <summary>
/// Represents a single ribbon button or dropdown.
/// Fully composed — the view only reads properties, never decides logic.
/// </summary>
public sealed class RibbonItemModel
{
    /// <summary>Unique key identifying this ribbon item.</summary>
    public required RibbonItemKey Key { get; init; }

    /// <summary>Display label shown on the button.</summary>
    public required string Label { get; init; }

    /// <summary>Rendering type: Button, Dropdown, or Submit.</summary>
    public RibbonItemType ItemType { get; init; } = RibbonItemType.Button;

    /// <summary>Bootstrap icon class (e.g. "bi bi-plus-lg").</summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>CSS class for the button (e.g. "btn-primary", "btn-danger").</summary>
    public string CssClass { get; init; } = "btn-outline-secondary";

    /// <summary>Navigation URL for Button type items.</summary>
    public string? Url { get; init; }

    /// <summary>JavaScript command for client-side actions (e.g. form submit, confirm delete).</summary>
    public string? Command { get; init; }

    /// <summary>Whether this item should be rendered at all.</summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>Whether the button is clickable.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Dropdown child items (only for <see cref="RibbonItemType.Dropdown"/>).</summary>
    public IReadOnlyList<RibbonDropdownItemModel> DropdownItems { get; init; } = [];

    /// <summary>Sort order within the section (lower = first).</summary>
    public int Order { get; init; }
}
