using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Agents;

namespace SQTWeb.ViewComponents
{
    public class AgentSearchLookupViewComponent : ViewComponent
    {
        private readonly IAgentService _service;

        public AgentSearchLookupViewComponent(IAgentService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "AgentCode", string? value = null, string cssClass = "form-control")
        {
            var options = await _service.GetOptionsAsync();
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["CssClass"] = cssClass;
            return View(options);
        }
    }
}
