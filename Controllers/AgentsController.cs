using LMSModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQTWeb.Services.Masters;

namespace SQTWeb.Controllers;

[Authorize]
public class AgentsController : Controller
{
    private readonly IAgentService _agentService;
    private readonly IDbContextFactory<LMSContext> _dbFactory;

    public AgentsController(IAgentService agentService, IDbContextFactory<LMSContext> dbFactory)
    {
        _agentService = agentService;
        _dbFactory = dbFactory;
    }

    public async Task<IActionResult> AgentsList(string? keyword)
    {
        ViewBag.Keyword = keyword;

        var all = await _agentService.GetOptionsAsync();

        var items = string.IsNullOrWhiteSpace(keyword)
            ? all
            : all.Where(x =>
                (x.custcode ?? "").Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (x.custname ?? "").Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase))
              .ToList();

        return View(items);
    }

    // TODO: Agents come from a database view (Keyless). Full CRUD requires a base table.
    //       Uncomment and adapt when the writable entity/table is available.

    //[HttpGet]
    //public IActionResult CreateAgent() => View(new COM_View_Agent());

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> CreateAgent(COM_View_Agent model)
    //{
    //    if (!ModelState.IsValid) return View(model);
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // db.Agents.Add(model);
    //    // await db.SaveChangesAsync();
    //    TempData["Success"] = "Agent created.";
    //    return RedirectToAction(nameof(AgentsList));
    //}

    //[HttpGet]
    //public async Task<IActionResult> EditAgent(string id)
    //{
    //    if (string.IsNullOrWhiteSpace(id)) return NotFound();
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // var entity = await db.Agents.FindAsync(id);
    //    // if (entity is null) return NotFound();
    //    // return View(entity);
    //    return NotFound();
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> EditAgent(COM_View_Agent model)
    //{
    //    if (!ModelState.IsValid) return View(model);
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // db.Agents.Update(model);
    //    // await db.SaveChangesAsync();
    //    TempData["Success"] = "Agent updated.";
    //    return RedirectToAction(nameof(AgentsList));
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> DeleteAgent(string id)
    //{
    //    if (string.IsNullOrWhiteSpace(id)) return NotFound();
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // var entity = await db.Agents.FindAsync(id);
    //    // if (entity is not null) { db.Agents.Remove(entity); await db.SaveChangesAsync(); }
    //    TempData["Success"] = "Agent deleted.";
    //    return RedirectToAction(nameof(AgentsList));
    //}
}
