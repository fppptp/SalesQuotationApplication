using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services;

namespace SQTWeb.ViewComponents
{
    public class CurrencySearchLookupViewComponent : ViewComponent
    {
        private readonly ICurrencyService _service;

        public CurrencySearchLookupViewComponent(ICurrencyService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "CurrencyCode", string? value = null, string cssClass = "form-control", bool required = false)
        {
            var options = await _service.GetOptionsAsync();
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            ViewData["Required"] = required;
            return View(options);
        }
    }
}
