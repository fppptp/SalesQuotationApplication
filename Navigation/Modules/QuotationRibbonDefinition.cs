namespace SQTWeb.Navigation.Modules;

/// <summary>
/// Ribbon definition for the Quotation module.
/// Provides action mapping, dropdown items, and breadcrumbs specific to quotations.
/// </summary>
public sealed class QuotationRibbonDefinition : IModuleRibbonDefinition
{
    public AppModule Module => AppModule.Quotation;
    public string DisplayName => "Quotations";
    public string ListUrl => "/quotations/quotationslist";

    public string? ResolveAction(RibbonItemKey key, IPageContext context)
    {
        return key switch
        {
            RibbonItemKey.Add    => "/quotations/createquotation",
            RibbonItemKey.Edit   => context.EntityId is not null ? $"/quotations/editquotation/{context.EntityId}" : null,
            RibbonItemKey.Copy   => context.EntityId is not null ? $"/quotations/copyquotation/{context.EntityId}" : null,
            RibbonItemKey.Cancel => ListUrl,
            RibbonItemKey.Delete => null,  // handled via JS command
            RibbonItemKey.Save   => null,  // handled via form submit
            _ => null
        };
    }

    public IReadOnlyList<RibbonDropdownItemModel> GetDropdownItems(RibbonItemKey key, IPageContext context)
    {
        if (key == RibbonItemKey.PrintDropDown)
        {
            return
            [
                new() { Label = "Print Quotation", Icon = "bi bi-file-earmark-text", Url = context.EntityId is not null ? $"/quotations/printquotation/{context.EntityId}" : null },
                new() { Label = "Print Invoice",   Icon = "bi bi-receipt",            Url = context.EntityId is not null ? $"/quotations/printinvoice/{context.EntityId}" : null },
                new() { Label = "Print Packing List", Icon = "bi bi-box-seam",        Url = context.EntityId is not null ? $"/quotations/printpackinglist/{context.EntityId}" : null }
            ];
        }

        if (key == RibbonItemKey.SendDropDown)
        {
            return
            [
                new() { Label = "Send Email",  Icon = "bi bi-envelope",       Command = "ribbonSendEmail()" },
                new() { Label = "Send EDI",    Icon = "bi bi-arrow-left-right", Command = "ribbonSendEdi()" },
                new() { Label = "Export PDF",  Icon = "bi bi-filetype-pdf",   Url = context.EntityId is not null ? $"/quotations/exportpdf/{context.EntityId}" : null }
            ];
        }

        return [];
    }

    public IReadOnlyList<BreadcrumbItemModel> GetBreadcrumbs(IPageContext context)
    {
        var crumbs = new List<BreadcrumbItemModel>
        {
            new() { Label = "Home", Url = "/" },
            new() { Label = DisplayName, Url = ListUrl }
        };

        var pageName = context.FormMode switch
        {
            FormMode.Add  => "Create Quotation",
            FormMode.Edit => $"Edit Quotation",
            FormMode.View => $"View Quotation",
            FormMode.Copy => $"Copy Quotation",
            FormMode.Void => $"Void Quotation",
            _ => context.IsList ? "Quotations List" : "Quotation"
        };

        crumbs.Add(new BreadcrumbItemModel { Label = pageName, IsActive = true });

        return crumbs;
    }
}
