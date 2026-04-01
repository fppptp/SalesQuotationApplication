namespace SQTWeb.Navigation;

/// <summary>
/// Composes a fully render-ready <see cref="RibbonMenuModel"/> for the current page.
/// This is the single entry point — no other code should build ribbon state.
/// </summary>
public interface IRibbonMenuService
{
    /// <summary>
    /// Builds the complete ribbon menu model based on the current page context.
    /// The returned model is fully composed and ready to render.
    /// </summary>
    RibbonMenuModel LoadRibbonMenu(IPageContext context);
}
