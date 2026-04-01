using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services;

namespace SQTWeb.ViewComponents
{
    public class ChargeSearchLookupViewComponent : ViewComponent
    {
        private readonly IChargeService _service;

        public ChargeSearchLookupViewComponent(IChargeService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string name = "ChargeCode",
            string? value = null,
            string cssClass = "form-control",
            string? companyCode = null,
            string department = "IB",
            string quotationType = "FREIGHT",
            bool required = false)
        {
            string? displayText = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                var all = await _service.GetOptionsAsync();
                var match = all.FirstOrDefault(x =>
                    string.Equals(x.ChargeCode, value, StringComparison.OrdinalIgnoreCase));
                displayText = match != null ? $"{match.ChargeCode} - {match.ChargeName}" : value;
            }

            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["DisplayText"] = displayText ?? "";
            ViewData["CssClass"] = cssClass;
            ViewData["CompanyCode"] = companyCode ?? "";
            ViewData["Department"] = department;
            ViewData["QuotationType"] = quotationType;
            ViewData["Required"] = required;
            return View();
        }
    }
}
