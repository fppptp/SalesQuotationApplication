using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.UnitOfMeasure;

namespace SQTWeb.ViewComponents
{
    public class UnitOfMeasureSearchLookupViewComponent : ViewComponent
    {
        private readonly IUnitOfMeasureService _service;

        public UnitOfMeasureSearchLookupViewComponent(IUnitOfMeasureService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "UnitOfMeasure", string? value = null, string cssClass = "form-control", string? unitCategoryCode = null, bool required = false)
        {
            var options = await _service.GetOptionsAsync();
            if (!string.IsNullOrWhiteSpace(unitCategoryCode))
            {
                options = options.Where(x =>
                    string.Equals(x.UnitCategoryCode, unitCategoryCode, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            ViewData["Required"] = required;
            return View(options);
        }
    }
}
