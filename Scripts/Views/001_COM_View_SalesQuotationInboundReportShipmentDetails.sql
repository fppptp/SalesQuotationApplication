-- ============================================================
-- View: COM_View_SalesQuotationInboundReportShipmentDetails
-- Database: QTMS
-- Cross-DB ref: [COMMON].[dbo].[COM_Ms_UnitOfMeasure]
-- SQL Server 2014 compatible
--
-- Recalculates dimensions every time:
--   CBM = Qty × (L×dimFactor) × (W×dimFactor) × (H×dimFactor)
--   KG  = Qty × GrossWeight × (wtFactor / 1000)
-- ============================================================
IF OBJECT_ID('[dbo].[COM_View_SalesQuotationInboundReportShipmentDetails]', 'V') IS NOT NULL
    DROP VIEW [dbo].[COM_View_SalesQuotationInboundReportShipmentDetails];
GO

CREATE VIEW [dbo].[COM_View_SalesQuotationInboundReportShipmentDetails]
AS
SELECT
    sd.CompanyCode,
    sd.QuotationNo,
    sd.No                       AS LineNo,

    -- ── Package ──────────────────────────────────────────
    sd.Quantity,
    sd.PackageUnit,
    pkgU.UnitName               AS PackageUnitName,
    pkgU.Symbol                 AS PackageUnitSymbol,

    -- ── Gross Weight ─────────────────────────────────────
    sd.GrossWeight,
    sd.GrossWeightUnit,
    wtU.UnitName                AS GrossWeightUnitName,
    wtU.Symbol                  AS GrossWeightUnitSymbol,

    -- ── Dimensions ───────────────────────────────────────
    sd.[Length],
    sd.Width,
    sd.Height,
    sd.DimensionUnit,
    dimU.UnitName               AS DimensionUnitName,
    dimU.Symbol                 AS DimensionUnitSymbol,

    -- ── Calculated: TotalGrossWeightKG ───────────────────
    -- Mass base = gram → KG factor = 1000
    -- Formula: Qty × GrossWeight × (FactorTobase / 1000)
    CAST(
        CASE
            WHEN ISNULL(sd.GrossWeightUnit, '') <> ''
                 AND wtU.FactorTobase IS NOT NULL
            THEN ISNULL(sd.Quantity, 0)
                 * ISNULL(sd.GrossWeight, 0)
                 * (wtU.FactorTobase / CAST(1000 AS decimal(32,8)))
            ELSE 0
        END
    AS decimal(18,6))           AS CalcTotalGrossWeightKG,

    -- ── Calculated: TotalVolumeCBM ───────────────────────
    -- Length base = meter → CBM = m³
    -- Formula: Qty × (L×factor) × (W×factor) × (H×factor)
    CAST(
        CASE
            WHEN ISNULL(sd.DimensionUnit, '') <> ''
                 AND dimU.FactorTobase IS NOT NULL
            THEN ISNULL(sd.Quantity, 0)
                 * (ISNULL(sd.[Length], 0) * dimU.FactorTobase)
                 * (ISNULL(sd.Width, 0)    * dimU.FactorTobase)
                 * (ISNULL(sd.Height, 0)   * dimU.FactorTobase)
            ELSE 0
        END
    AS decimal(18,6))           AS CalcTotalVolumeCBM,

    -- ── Stored values (for comparison / audit) ───────────
    sd.TotalGrossWeightKG       AS StoredTotalGrossWeightKG,
    sd.TotalVolumeCBM           AS StoredTotalVolumeCBM

FROM dbo.COM_Trs_SalesQuotationShipmentDetail sd
LEFT JOIN [COMMON].dbo.COM_Ms_UnitOfMeasure pkgU
    ON pkgU.UnitCode = sd.PackageUnit
LEFT JOIN [COMMON].dbo.COM_Ms_UnitOfMeasure wtU
    ON wtU.UnitCode = sd.GrossWeightUnit
LEFT JOIN [COMMON].dbo.COM_Ms_UnitOfMeasure dimU
    ON dimU.UnitCode = sd.DimensionUnit;
GO
