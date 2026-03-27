using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Masters;

namespace SQTWeb.ViewComponents
{
    public class CompanySelectViewComponent : ViewComponent
    {
        private readonly ICompanyService _companyService;

        public CompanySelectViewComponent(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "CompanyCode", string? value = null, string cssClass = "form-select")
        {
            var options = await _companyService.GetOptionsAsync();
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            return View(options);
        }
    }
}
