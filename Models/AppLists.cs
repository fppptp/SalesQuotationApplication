namespace SQTWeb.Models;

public static class AppLists
{
    public static readonly string[] Modes =
    [
        "Sea",
        "Air",
        "Truck",
        "Rail",
        "Multimodal"
    ];

    public static readonly string[] ServiceTypes =
    [
        "Door to Door",
        "Door to Port",
        "Port to Door",
        "Port to Port"
    ];

    public static readonly string[] Incoterms =
    [
        "EXW",
        "FCA",
        "FOB",
        "CFR",
        "CIF",
        "CPT",
        "CIP",
        "DAP",
        "DDP"
    ];

    public static readonly string[] Statuses =
    [
        "Draft",
        "Sent",
        "Approved",
        "Rejected",
        "Expired",
        "Booked"
    ];

    public static readonly string[] Currencies =
    [
        "THB",
        "USD",
        "EUR",
        "CNY",
        "JPY"
    ];

    public static readonly string[] ChargeBases =
    [
        "Per Shipment",
        "Per Container",
        "Per Kg",
        "Per CBM",
        "Per Document",
        "Lump Sum"
    ];

    public static readonly string[] ChargeCategories =
    [
        "Origin",
        "Freight",
        "Destination",
        "Customs",
        "Trucking",
        "Documentation"
    ];

    public static readonly string[] SurchargeCodes =
    [
        "BAF", "CAF", "PSS", "THC", "D/O", "AMS", "ISPS",
        "GRI", "EBS", "LSS", "PCS", "CIC", "WRS", "ERS"
    ];

    public static readonly string[] MarginTypes =
    [
        "Percent",
        "Fixed Amount"
    ];

    public static readonly string[] ChargeDimensions =
    [
        "Weight",
        "Volume",
        "Chargeable Weight",
        "Container",
        "Shipment",
        "Document",
        "Amount",
        "Day",
        "Piece"
    ];
}
