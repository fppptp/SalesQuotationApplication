-- ============================================================
-- View: COM_View_SalesQuotationInboundReport
-- Database: QTMS
-- Depends on:
--   - COM_View_SalesQuotationInboundReportShipmentDetails
--   - COM_View_SalesQuotationInboundReportCharges
-- SQL Server 2014 compatible
--
-- Main quotation report header with recalculated grand totals.
-- ============================================================
IF OBJECT_ID('[dbo].[COM_View_SalesQuotationInboundReport]', 'V') IS NOT NULL
    DROP VIEW [dbo].[COM_View_SalesQuotationInboundReport];
GO

CREATE VIEW [dbo].[COM_View_SalesQuotationInboundReport]
AS
SELECT
    q.CompanyCode,
    q.QuotationNo,
    q.QuotationDate,
    q.StatusNo,
    q.StatusName,

    -- ── Customer ─────────────────────────────────────────
    q.CustomerCode,
    q.CustomerName,
    q.CustomerContactCode,
    q.CustomerContactname,

    -- ── Currency ─────────────────────────────────────────
    q.CurrencyCode,
    q.ExchangeRate,

    -- ── Port ─────────────────────────────────────────────
    q.PortCode,
    q.PortName,

    -- ── Agent ────────────────────────────────────────────
    q.AgentCode,
    q.AgentName,
    q.AgentAddress1,
    q.AgentAddress2,
    q.AgentLocation,

    -- ── Notes ────────────────────────────────────────────
    q.AdditionalInformation,
    q.ISODocumentVersionNo,

    -- ── Recalculated shipment grand totals ───────────────
    ISNULL(ship.ShipmentLineCount, 0)
                                    AS ShipmentLineCount,
    CAST(ISNULL(ship.CalcGrandTotalGrossWeightKG, 0)
        AS decimal(18,6))           AS CalcGrandTotalGrossWeightKG,
    CAST(ISNULL(ship.CalcGrandTotalVolumeCBM, 0)
        AS decimal(18,6))           AS CalcGrandTotalVolumeCBM,

    -- ── Recalculated charge grand totals ─────────────────
    ISNULL(chg.ChargeLineCount, 0)
                                    AS ChargeLineCount,
    CAST(ISNULL(chg.CalcGrandTotalFreightHeaderCcy, 0)
        AS decimal(18,6))           AS CalcGrandTotalFreightHeaderCcy,

    -- ── Stored totals (for comparison / audit) ───────────
    q.GrandTotalGrossWeightKg       AS StoredGrandTotalGrossWeightKg,
    q.GrandTotalVolumeCBM           AS StoredGrandTotalVolumeCBM,
    q.GrandTotalFrieghtAmountDest   AS StoredGrandTotalFreightAmountDest,
    q.GrandTotalFrieghtAmountTHB    AS StoredGrandTotalFreightAmountTHB,

    -- ── Audit ────────────────────────────────────────────
    q.CreateBy,
    q.CreateDate,
    q.UpdateBy,
    q.UpdateDate,
    q.IsVoid,
    q.VoidBy,
    q.VoidDate,
    q.VoidReason

FROM dbo.COM_Trs_SalesQuotation q

-- ── Aggregate shipment details ───────────────────────────
LEFT JOIN (
    SELECT
        CompanyCode,
        QuotationNo,
        COUNT(*)                        AS ShipmentLineCount,
        SUM(CalcTotalGrossWeightKG)     AS CalcGrandTotalGrossWeightKG,
        SUM(CalcTotalVolumeCBM)         AS CalcGrandTotalVolumeCBM
    FROM dbo.COM_View_SalesQuotationInboundReportShipmentDetails
    GROUP BY CompanyCode, QuotationNo
) ship
    ON  ship.CompanyCode = q.CompanyCode
    AND ship.QuotationNo = q.QuotationNo

-- ── Aggregate charge details ─────────────────────────────
LEFT JOIN (
    SELECT
        CompanyCode,
        QuotationNo,
        COUNT(*)                        AS ChargeLineCount,
        SUM(CalcLineTotalHeaderCcy)     AS CalcGrandTotalFreightHeaderCcy
    FROM dbo.COM_View_SalesQuotationInboundReportCharges
    GROUP BY CompanyCode, QuotationNo
) chg
    ON  chg.CompanyCode = q.CompanyCode
    AND chg.QuotationNo = q.QuotationNo;
GO
