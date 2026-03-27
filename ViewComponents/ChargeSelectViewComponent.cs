using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Masters;

namespace SQTWeb.ViewComponents
{
    public class ChargeSelectViewComponent : ViewComponent
    {
        private readonly IChargeService _service;

        public ChargeSelectViewComponent(IChargeService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "ChargeCode", string? value = null, string cssClass = "form-select")
        {
            var options = await _service.GetOptionsAsync();
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            return View(options);
        }
    }
}
