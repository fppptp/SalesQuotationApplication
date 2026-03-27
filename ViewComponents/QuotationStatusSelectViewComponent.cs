using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Masters;

namespace SQTWeb.ViewComponents
{
    public class QuotationStatusSelectViewComponent : ViewComponent
    {
        private readonly IQuotationStatusService _service;

        public QuotationStatusSelectViewComponent(IQuotationStatusService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string name = "StatusName",
            string? value = null,
            string cssClass = "form-select",
            string[]? filter = null,
            bool disabled = false)
        {
            var options = await _service.GetOptionsAsync();

            if (filter is { Length: > 0 })
            {
                var allowed = new HashSet<string>(filter, StringComparer.OrdinalIgnoreCase);
                options = options.Where(o => allowed.Contains(o.StatusDescription ?? "")).ToList();
            }

            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            ViewData["Disabled"] = disabled;
            return View(options);
        }
    }
}
