-- ============================================================
-- View: COM_View_SalesQuotationInboundReportCharges
-- Database: QTMS
-- Depends on: COM_View_SalesQuotationInboundReportShipmentDetails
-- Cross-DB ref: [COMMON].[dbo].[COM_Ms_UnitOfMeasure]
-- SQL Server 2014 compatible
--
-- Recalculates charge amounts every time:
--   1. Resolves tier pricing using shipment grand totals
--      - ChargeUnit IN ('KG','KGS') → match on GrandTotalGrossWeightKG
--      - Otherwise                  → match on GrandTotalVolumeCBM
--   2. EffectiveUnitPrice = matched tier price OR stored UnitPrice
--   3. LineTotalChargeCcy   = EffectiveUnitPrice + MarginAmount
--   4. LineTotalHeaderCcy   = LineTotalChargeCcy × ExchangeRateToHeader
-- ============================================================
IF OBJECT_ID('[dbo].[COM_View_SalesQuotationInboundReportCharges]', 'V') IS NOT NULL
    DROP VIEW [dbo].[COM_View_SalesQuotationInboundReportCharges];
GO

CREATE VIEW [dbo].[COM_View_SalesQuotationInboundReportCharges]
AS
WITH ShipmentTotals AS (
    SELECT
        CompanyCode,
        QuotationNo,
        SUM(CalcTotalGrossWeightKG)  AS GrandTotalGrossWeightKG,
        SUM(CalcTotalVolumeCBM)      AS GrandTotalVolumeCBM
    FROM dbo.COM_View_SalesQuotationInboundReportShipmentDetails
    GROUP BY CompanyCode, QuotationNo
)
SELECT
    c.CompanyCode,
    c.QuotationNo,
    c.No                        AS LineNo,

    -- ── Charge info ──────────────────────────────────────
    c.ChargeCode,
    c.ChargeName,
    c.ChargeUnit,
    chargeU.UnitName            AS ChargeUnitName,
    chargeU.Symbol              AS ChargeUnitSymbol,

    -- ── Currency ─────────────────────────────────────────
    c.CurrencyCode,
    CASE WHEN ISNULL(c.ExchangeRateToHeader, 0) = 0
         THEN CAST(1 AS decimal(18,6))
         ELSE c.ExchangeRateToHeader
    END                         AS ExchangeRateToHeader,
    c.MarginAmount,

    -- ── Tier resolution context ──────────────────────────
    ISNULL(st.GrandTotalGrossWeightKG, 0)  AS TierLookupWeightKG,
    ISNULL(st.GrandTotalVolumeCBM, 0)      AS TierLookupVolumeCBM,
    CASE
        WHEN UPPER(LTRIM(RTRIM(ISNULL(c.ChargeUnit, '')))) IN ('KG', 'KGS')
        THEN ISNULL(st.GrandTotalGrossWeightKG, 0)
        ELSE ISNULL(st.GrandTotalVolumeCBM, 0)
    END                         AS TierLookupValue,

    -- ── Tier match result ────────────────────────────────
    matchedTier.FromValue       AS MatchedTierFrom,
    matchedTier.ToValue         AS MatchedTierTo,
    matchedTier.UnitPrice       AS MatchedTierUnitPrice,

    -- ── Effective UnitPrice (tier wins if matched) ───────
    ISNULL(matchedTier.UnitPrice, c.UnitPrice)
                                AS EffectiveUnitPrice,
    c.UnitPrice                 AS StoredUnitPrice,

    -- ── Calculated line totals ───────────────────────────
    -- LineTotalChargeCcy = EffectiveUnitPrice + MarginAmount
    CAST(
        ISNULL(ISNULL(matchedTier.UnitPrice, c.UnitPrice), 0)
        + ISNULL(c.MarginAmount, 0)
    AS decimal(18,6))           AS CalcLineTotalChargeCcy,

    -- LineTotalHeaderCcy = LineTotalChargeCcy × ExRate
    CAST(
        ( ISNULL(ISNULL(matchedTier.UnitPrice, c.UnitPrice), 0)
          + ISNULL(c.MarginAmount, 0) )
        * CASE WHEN ISNULL(c.ExchangeRateToHeader, 0) = 0
               THEN 1
               ELSE c.ExchangeRateToHeader
          END
    AS decimal(18,6))           AS CalcLineTotalHeaderCcy

FROM dbo.COM_Trs_SalesQuotationFreightCharge c

LEFT JOIN ShipmentTotals st
    ON  st.CompanyCode = c.CompanyCode
    AND st.QuotationNo = c.QuotationNo

LEFT JOIN [COMMON].dbo.COM_Ms_UnitOfMeasure chargeU
    ON chargeU.UnitCode = c.ChargeUnit

-- ── Tier resolution: find best matching tier ─────────────
-- Matches C# logic: FreightChargeTierResolver.ResolveMatchedTier
OUTER APPLY (
    SELECT TOP 1
        t.UnitPrice,
        t.FromValue,
        t.ToValue
    FROM dbo.COM_Trs_SalesQuotationFreightChargePriceTier t
    WHERE t.CompanyCode = c.CompanyCode
      AND t.QuotationNo = c.QuotationNo
      AND t.ChargeNo    = c.No
      AND ISNULL(t.FromValue, 0) <=
          CASE
              WHEN UPPER(LTRIM(RTRIM(ISNULL(c.ChargeUnit, '')))) IN ('KG', 'KGS')
              THEN ISNULL(st.GrandTotalGrossWeightKG, 0)
              ELSE ISNULL(st.GrandTotalVolumeCBM, 0)
          END
      AND CASE
              WHEN UPPER(LTRIM(RTRIM(ISNULL(c.ChargeUnit, '')))) IN ('KG', 'KGS')
              THEN ISNULL(st.GrandTotalGrossWeightKG, 0)
              ELSE ISNULL(st.GrandTotalVolumeCBM, 0)
          END <= ISNULL(t.ToValue, CAST(999999999999 AS decimal(18,6)))
    ORDER BY t.FromValue DESC
) matchedTier;
GO
