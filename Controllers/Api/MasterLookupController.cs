using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSModel.Models;
using SQTWeb.Services.Masters;

namespace SQTWeb.Controllers.Api;

[Authorize]
[Route("api/masters")]
public class MasterLookupController : Controller
{
    private readonly IAgentService _agentService;
    private readonly IPortService _portService;
    private readonly IDbContextFactory<LMSContext> _lmsDbFactory;

    public MasterLookupController(
        IAgentService agentService,
        IPortService portService,
        IDbContextFactory<LMSContext> lmsDbFactory)
    {
        _agentService = agentService;
        _portService = portService;
        _lmsDbFactory = lmsDbFactory;
    }

    [HttpGet("customers")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string? q, [FromQuery] string? companyCode, [FromQuery] int take = 20)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
            return Json(Array.Empty<object>());

        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(Array.Empty<object>());

        await using var db = await _lmsDbFactory.CreateDbContextAsync();
        var kw = "%" + q.Trim() + "%";

        var results = await db.COM_View_CustomerLists
            .AsNoTracking()
            .Where(x => x.CompanyCode == companyCode.Trim())
            .Where(x => x.StopUse != true)
            .Where(x =>
                EF.Functions.Like(x.CustomerCode ?? "", kw) ||
                EF.Functions.Like(x.CustomerName ?? "", kw) ||
                EF.Functions.Like(x.IDNo ?? "", kw))
            .OrderBy(x => x.CustomerCode)
            .Take(take)
            .ToListAsync();

        return Json(results);
    }

    [HttpGet("agents")]
    public async Task<IActionResult> SearchAgents([FromQuery] string? q, [FromQuery] int take = 20)
    {
        var all = await _agentService.GetOptionsAsync();

        IEnumerable<COM_View_Agent> filtered = string.IsNullOrWhiteSpace(q)
            ? all
            : all.Where(x =>
                (x.custcode ?? "").Contains(q.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (x.custname ?? "").Contains(q.Trim(), StringComparison.OrdinalIgnoreCase));

        return Json(filtered.Take(take));
    }

    [HttpGet("ports")]
    public async Task<IActionResult> SearchPorts([FromQuery] string? q, [FromQuery] int take = 20)
    {
        var all = await _portService.GetOptionsAsync();

        IEnumerable<COM_View_Port> filtered = string.IsNullOrWhiteSpace(q)
            ? all
            : all.Where(x =>
                (x.Code ?? "").Contains(q.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (x.Name ?? "").Contains(q.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (x.CountryName ?? "").Contains(q.Trim(), StringComparison.OrdinalIgnoreCase));

        return Json(filtered.Take(take));
    }
}
