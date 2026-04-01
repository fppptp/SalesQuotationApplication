using AppLib.Service.Unit;
using QTMSModel.Models;
using UtilityLib.Extensions;

namespace SQTWeb.Extensions;

public static class ShipmentDetailExtensions
{
    public static COM_Trs_SalesQuotationShipmentDetail DimensionCal(this COM_Trs_SalesQuotationShipmentDetail entity)
    {
        if (entity == null) return null;
        entity.TotalVolumeCBM = 0;
        entity.TotalGrossWeightKG = 0;

        // Unit
        if (entity.PackageUnit.IsNullOrNothing()) { entity.PackageUnit = ""; }
        if (entity.DimensionUnit.IsNullOrNothing()) { entity.DimensionUnit = ""; }
        if (entity.GrossWeightUnit.IsNullOrNothing()) { entity.GrossWeightUnit = ""; }

        // Value
        entity.GrossWeight ??= 0;
        entity.Length ??= 0;
        entity.Width ??= 0;
        entity.Height ??= 0;

        // Gross Weight
        if (entity.GrossWeightUnit != "")
        {
            entity.TotalGrossWeightKG = UnitConvertMass.Convert(entity.GrossWeight, entity.GrossWeightUnit, "KG") * entity.Quantity;
        }

        // Volume
        if (entity.DimensionUnit != "")
        {
            var length = UnitConvertLength.Convert(entity.Length, entity.DimensionUnit, "m");
            var width = UnitConvertLength.Convert(entity.Width, entity.DimensionUnit, "m");
            var height = UnitConvertLength.Convert(entity.Height, entity.DimensionUnit, "m");
            if (length.HasValue && width.HasValue && height.HasValue)
            {
                entity.TotalVolumeCBM = length.Value * width.Value * height.Value * entity.Quantity;
            }
        }

        return entity;
    }
}
