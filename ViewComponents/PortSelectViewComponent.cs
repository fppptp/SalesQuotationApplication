using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSModel.Models;

namespace SQTWeb.ViewComponents
{
    public class PortSelectViewComponent : ViewComponent
    {
        private readonly IDbContextFactory<LMSContext> _dbFactory;

        public PortSelectViewComponent(IDbContextFactory<LMSContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name = "PortCode", string? value = null, string cssClass = "form-control")
        {
            string? displayText = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var match = await db.COM_View_Ports
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Code == value);
                displayText = match != null ? $"{match.Code} - {match.Name}" : value;
            }
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["DisplayText"] = value;
            ViewData["CssClass"] = cssClass;
            return View();
        }
    }
}
