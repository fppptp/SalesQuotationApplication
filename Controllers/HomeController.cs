using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQTWeb.Models.QTMS;
using SQTWeb.Models;
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
        //var model = new DashboardViewModel();

        //var q = _context.Trs_SalesQuotations.AsQueryable();

        //model.TotalQuotes = await q.CountAsync();
        //model.DraftQuotes = await q.CountAsync(x => x.StatusName == "Draft");
        //model.SentQuotes = await q.CountAsync(x => x.StatusName == "Sent");
        //model.ApprovedOrBookedQuotes = await q.CountAsync(x => x.StatusName == "Approved" || x.StatusName == "Booked");

        //var now = DateTime.Now;
        //model.CurrentMonthSales = await q.Where(x => x.QuotationDate.HasValue && x.QuotationDate.Value.Year == now.Year && x.QuotationDate.Value.Month == now.Month)
        //    .SumAsync(x => x.GrandTotalFrieghtAmountTHB ?? 0);
        //// Profit not available in QTMS model; set 0
        //model.CurrentMonthProfit = 0;

        //var latest = await q.OrderByDescending(x => x.QuotationDate).ThenByDescending(x => x.QuotationNo)
        //    .Take(8)
        //    .Select(x => new QuoteListItemViewModel
        //    {
        //        QuoteId = x.QuotationNo,
        //        QuoteNo = x.QuotationNo,
        //        QuoteDate = x.QuotationDate ?? DateTime.Today,
        //        CustomerName = x.CustomerName,
        //        Origin = "",
        //        Destination = x.PortName,
        //        Status = x.StatusName,
        //        CurrencyCode = x.CurrencyCode,
        //        GrandTotal = x.GrandTotalFrieghtAmountTHB ?? 0,
        //        ProfitTotal = 0,
        //        RevisionNo = 1
        //    })
        //    .ToListAsync();

        //model.LatestQuotations = latest;

        //return View(model);
        return View(null);
    }

    public IActionResult Error()
    {
        return View();
    }
}
