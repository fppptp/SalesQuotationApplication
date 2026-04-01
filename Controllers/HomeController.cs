using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QTMSModel.Models;
using Microsoft.EntityFrameworkCore;

namespace SQTWeb.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly QTMSContext _context;

    public HomeController(QTMSContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _context.COM_View_SalesQuotationDashboards
            .AsNoTracking()
            .OrderByDescending(x => x.QuotationDate)
            .ThenByDescending(x => x.QuotationNo)
            .ToListAsync();

        return View(model);
    }

    public IActionResult Error()
    {
        return View();
    }
}
