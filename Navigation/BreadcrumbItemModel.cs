namespace SQTWeb.Navigation;

/// <summary>
/// A single breadcrumb segment.
/// </summary>
public sealed class BreadcrumbItemModel
{
    /// <summary>Display label.</summary>
    public required string Label { get; init; }

    /// <summary>Navigation URL (null for the current/active segment).</summary>
    public string? Url { get; init; }

    /// <summary>Whether this is the active (last) breadcrumb.</summary>
    public bool IsActive { get; init; }
}
