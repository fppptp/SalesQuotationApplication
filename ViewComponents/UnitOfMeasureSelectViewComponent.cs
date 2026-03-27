using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Masters;

namespace SQTWeb.ViewComponents
{
    public class UnitOfMeasureSelectViewComponent : ViewComponent
    {
        private readonly IUnitOfMeasureService _service;

        public UnitOfMeasureSelectViewComponent(IUnitOfMeasureService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "UnitOfMeasure", string? value = null, string cssClass = "form-select")
        {
            var options = await _service.GetOptionsAsync();
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            return View(options);
        }
    }
}
