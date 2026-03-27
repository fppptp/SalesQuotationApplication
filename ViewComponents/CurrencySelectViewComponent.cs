using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Masters;

namespace SQTWeb.ViewComponents
{
    public class CurrencySelectViewComponent : ViewComponent
    {
        private readonly ICurrencyService _service;

        public CurrencySelectViewComponent(ICurrencyService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "CurrencyCode", string? value = null, string cssClass = "form-select")
        {
            var options = await _service.GetOptionsAsync();
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            return View(options);
        }
    }
}
