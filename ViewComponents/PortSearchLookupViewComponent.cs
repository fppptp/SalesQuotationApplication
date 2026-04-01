using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Ports;

namespace SQTWeb.ViewComponents
{
    public class PortSearchLookupViewComponent : ViewComponent
    {
        private readonly IPortService _service;

        public PortSearchLookupViewComponent(IPortService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "PortCode", string? value = null, string cssClass = "form-control")
        {
            var options = await _service.GetOptionsAsync();
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            return View(options);
        }
    }
}
