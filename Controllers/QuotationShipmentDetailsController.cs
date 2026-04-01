using AppLib;
using AppLib.DocumentControl;
using COMMONModel.Models;
using FMSModel.Models;
using LMSModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QTMSModel.Models;
using SQTWeb.Cache;
using SQTWeb.DTOModels;
using System.Text;
using System.Transactions;
using UtilityLib.Extensions;
using SQTWeb.Extensions;
using SQTWeb.Services.QuotationStatus;

namespace SQTWeb.Controllers;

[Authorize]
public class QuotationShipmentDetailsController : Controller
{
    private readonly QTMSContext qtmsContext;
    private readonly FMSContext fmsContext;
    private readonly COMMONContext commonContext;
    private readonly IQuotationStatusService _statusService;

    private string SeriesNo_TableName => qtmsContext.COM_Trs_SalesQuotations.GetEFCoreTableName();
    private string SeriesNo_ColumnName => qtmsContext.COM_Trs_SalesQuotations.GetEFCoreColumnName(x => x.QuotationNo);
    private string SeriesNo_FormatType => SeriesNo.Format.PrefixY2_M2_No.ToString();

    public QuotationShipmentDetailsController(QTMSContext qtmsContextInput, FMSContext fmsContextInput, COMMONContext commonContextInput, IQuotationStatusService statusService)
    {
        qtmsContext = qtmsContextInput;
        fmsContext = fmsContextInput;
        commonContext = commonContextInput;
        _statusService = statusService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CalculateDetailDimension([FromBody] COM_Trs_SalesQuotationShipmentDetail entity)
    {
        try
        {
            if (entity == null)
            {
                return BadRequest("Entity is required.");
            }
            return Ok(entity.DimensionCal());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}