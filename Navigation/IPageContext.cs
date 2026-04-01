namespace SQTWeb.Navigation;

/// <summary>
/// Per-request UI context derived from the current route.
/// Injected as <c>Scoped</c> — one instance per HTTP request.
/// </summary>
public interface IPageContext
{
    /// <summary>Current application module (which menu/page the user is on).</summary>
    AppModule Module { get; }

    /// <summary>Form mode for data-entry pages (View, Add, Edit, etc.).</summary>
    FormMode FormMode { get; }

    /// <summary>The entity identifier when editing/viewing a specific record.</summary>
    string? EntityId { get; }

    /// <summary>Whether the current page is a list/index page.</summary>
    bool IsList { get; }

    /// <summary>Whether the current page is a form (create/edit/view).</summary>
    bool IsForm { get; }
}
