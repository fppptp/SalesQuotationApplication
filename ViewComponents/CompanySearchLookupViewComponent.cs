using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services;

namespace SQTWeb.ViewComponents
{
    public class CompanySearchLookupViewComponent : ViewComponent
    {
        private readonly ICompanyService _service;

        public CompanySearchLookupViewComponent(ICompanyService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "CompanyCode", string? value = null, string cssClass = "form-control", string[]? filter = null)
        {
            var options = await _service.GetOptionsAsync();

            if (filter is { Length: > 0 })
            {
                var allowed = new HashSet<string>(filter, StringComparer.OrdinalIgnoreCase);
                options = options.Where(o => allowed.Contains(o.CompanyCode ?? "")).ToList();
            }

            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            return View(options);
        }
    }
}
