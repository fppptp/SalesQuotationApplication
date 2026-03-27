using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSModel.Models;

namespace SQTWeb.ViewComponents
{
    public class AgentSelectViewComponent : ViewComponent
    {
        private readonly IDbContextFactory<LMSContext> _dbFactory;

        public AgentSelectViewComponent(IDbContextFactory<LMSContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "AgentCode", string? value = null, string cssClass = "form-control")
        {
            string? displayText = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var match = await db.COM_View_Agents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.custcode == value);
                displayText = match != null ? $"{match.custcode} - {match.custname}" : value;
            }
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["DisplayText"] = value;
            ViewData["CssClass"] = cssClass;
            return View();
        }
    }
}
