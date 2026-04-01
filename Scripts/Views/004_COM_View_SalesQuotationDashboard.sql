-- ============================================================
-- View: COM_View_SalesQuotationDashboard
-- Database: QTMS
-- Depends on:
--   - COM_View_SalesQuotationInboundReportShipmentDetails
--   - COM_View_SalesQuotationInboundReportCharges
-- SQL Server 2014 compatible
--
-- Dashboard summary: one row per quotation with recalculated
-- shipment totals, charge totals, and margin.
--
-- Matches C# model: COM_View_SalesQuotationDashboard
-- ============================================================
IF OBJECT_ID('[dbo].[COM_View_SalesQuotationDashboard]', 'V') IS NOT NULL
    DROP VIEW [dbo].[COM_View_SalesQuotationDashboard];
GO

CREATE VIEW [dbo].[COM_View_SalesQuotationDashboard]
AS
SELECT
    q.CompanyCode,
    q.QuotationNo,
    q.QuotationDate,
    q.StatusNo,
    q.StatusName,

    q.CustomerCode,
    q.CustomerName,
    q.CurrencyCode,
    q.ExchangeRate,

    -- ── Stored header totals ─────────────────────────────
    q.GrandTotalGrossWeightKg,
    q.GrandTotalVolumeCBM,
    q.GrandTotalFrieghtAmountTHB,

    -- ── Recalculated shipment aggregates ─────────────────
    --    TotalQuantity  = sum of all shipment line quantities
    --    TotalShipmentCBM    = sum of recalculated CBM
    --    TotalShipmentWeight = sum of recalculated Weight KG
    CAST(ISNULL(ship.TotalQuantity, 0)       AS decimal(38,6)) AS TotalQuantity,
    CAST(ISNULL(ship.TotalShipmentCBM, 0)    AS decimal(38,6)) AS TotalShipmentCBM,
    CAST(ISNULL(ship.TotalShipmentWeight, 0) AS decimal(38,6)) AS TotalShipmentWeight,

    -- ── Recalculated charge aggregates ───────────────────
    --    TotalCharge = sum of (EffectiveUnitPrice + Margin) × ExRate
    --                  per charge line (in header currency)
    --    TotalMargin = sum of MarginAmount × ExRate
    --                  across all charge lines
    CAST(ISNULL(chg.TotalCharge, 0) AS decimal(38,6)) AS TotalCharge,
    CAST(ISNULL(chg.TotalMargin, 0) AS decimal(38,6)) AS TotalMargin,

    -- ── Line counts ──────────────────────────────────────
    ship.ShipmentLines,
    chg.ChargeLines

FROM dbo.COM_Trs_SalesQuotation q

-- ── Shipment aggregates ──────────────────────────────────
LEFT JOIN (
    SELECT
        CompanyCode,
        QuotationNo,
        CAST(COUNT(*) AS int)          AS ShipmentLines,
        SUM(ISNULL(Quantity, 0))       AS TotalQuantity,
        SUM(CalcTotalVolumeCBM)        AS TotalShipmentCBM,
        SUM(CalcTotalGrossWeightKG)    AS TotalShipmentWeight
    FROM dbo.COM_View_SalesQuotationInboundReportShipmentDetails
    GROUP BY CompanyCode, QuotationNo
) ship
    ON  ship.CompanyCode = q.CompanyCode
    AND ship.QuotationNo = q.QuotationNo

-- ── Charge aggregates ────────────────────────────────────
LEFT JOIN (
    SELECT
        CompanyCode,
        QuotationNo,
        CAST(COUNT(*) AS int)          AS ChargeLines,
        -- TotalCharge: full line total in header currency
        SUM(CalcLineTotalHeaderCcy)     AS TotalCharge,
        -- TotalMargin: margin portion only, converted to header currency
        SUM(
            ISNULL(MarginAmount, 0)
            * CASE WHEN ISNULL(ExchangeRateToHeader, 0) = 0
                   THEN 1
                   ELSE ExchangeRateToHeader
              END
        )                               AS TotalMargin
    FROM dbo.COM_View_SalesQuotationInboundReportCharges
    GROUP BY CompanyCode, QuotationNo
) chg
    ON  chg.CompanyCode = q.CompanyCode
    AND chg.QuotationNo = q.QuotationNo

WHERE ISNULL(q.IsVoid, 0) = 0;
GO
