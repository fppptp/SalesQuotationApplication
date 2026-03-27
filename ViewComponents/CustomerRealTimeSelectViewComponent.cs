using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSModel.Models;

namespace SQTWeb.ViewComponents
{
    public class CustomerRealTimeSelectViewComponent : ViewComponent
    {
        private readonly IDbContextFactory<LMSContext> _dbFactory;

        public CustomerRealTimeSelectViewComponent(IDbContextFactory<LMSContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string name = "CustomerCode",
            string? value = null,
            string cssClass = "form-control")
        {
            string? displayText = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var match = await db.COM_View_CustomerLists
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CustomerCode == value);
                displayText = match != null ? $"{match.CustomerCode} - {match.CustomerName}" : value;
            }
            ViewData["Name"] = name;
            ViewData["Value"] = value;
            ViewData["DisplayText"] = displayText;
            ViewData["CssClass"] = cssClass;
            return View();
        }
    }
}
