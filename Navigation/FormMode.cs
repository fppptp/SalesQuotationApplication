namespace SQTWeb.Navigation;

/// <summary>
/// Form mode for data-entry pages.
/// This is a UI/presentation concern — lives in the web project, not in domain libraries.
/// </summary>
public enum FormMode
{
    Add,
    Edit,
    View,
    Copy,
    Void
}
