namespace SQTWeb.Navigation;

/// <summary>
/// Application modules — used for sidebar highlighting, breadcrumbs,
/// and page-level context. Add new entries as modules grow.
/// </summary>
public enum AppModule
{
    None = 0,
    Dashboard,
    Quotation,
    ChargeCode,
    Customer,
    Agent,
    Port,
    Currency,
    Unit,
    Admin
}
