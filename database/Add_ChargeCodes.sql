USE FreightQuotation;
GO

IF OBJECT_ID('dbo.ChargeCodeRates', 'U') IS NOT NULL DROP TABLE dbo.ChargeCodeRates;
IF OBJECT_ID('dbo.ChargeCodes',     'U') IS NOT NULL DROP TABLE dbo.ChargeCodes;
GO

CREATE TABLE dbo.ChargeCodes
(
     ChargeCodeId     INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,Code             NVARCHAR(30)   NOT NULL
    ,Name             NVARCHAR(200)  NOT NULL
    ,Category         NVARCHAR(50)   NOT NULL CONSTRAINT DF_ChargeCodes_Category   DEFAULT ('Freight')
    ,DefaultBasis     NVARCHAR(50)   NOT NULL CONSTRAINT DF_ChargeCodes_Basis      DEFAULT ('Lump Sum')
    ,TransportMode    NVARCHAR(20)   NULL
    ,Description      NVARCHAR(300)  NULL
    ,DefaultBuyRate   DECIMAL(18,4)  NOT NULL CONSTRAINT DF_ChargeCodes_BuyRate    DEFAULT (0)
    ,DefaultSellRate  DECIMAL(18,4)  NOT NULL CONSTRAINT DF_ChargeCodes_SellRate   DEFAULT (0)
    ,MinCharge        DECIMAL(18,2)  NULL
    ,MaxCharge        DECIMAL(18,2)  NULL
    ,IsActive         BIT            NOT NULL CONSTRAINT DF_ChargeCodes_IsActive   DEFAULT (1)
    ,SortOrder        INT            NOT NULL CONSTRAINT DF_ChargeCodes_Sort       DEFAULT (1)
    ,CreatedAt        DATETIME       NOT NULL CONSTRAINT DF_ChargeCodes_CreatedAt  DEFAULT (GETDATE())
    ,UpdatedAt        DATETIME       NOT NULL CONSTRAINT DF_ChargeCodes_UpdatedAt  DEFAULT (GETDATE())
);
GO

CREATE UNIQUE INDEX UX_ChargeCodes_Code ON dbo.ChargeCodes (Code);
GO

-- Per-dimension rate tiers: each row defines a bracketed rate for one dimension type
-- e.g. Weight 0–100 kg → BuyRate 0.50/kg, MinCharge 50
CREATE TABLE dbo.ChargeCodeRates
(
     RateId          INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,ChargeCodeId    INT            NOT NULL
    ,DimensionType   NVARCHAR(50)   NOT NULL   -- Weight | Volume | Chargeable Weight | Container | Shipment | Amount | Document | Day | Piece
    ,FromValue       DECIMAL(18,4)  NULL        -- lower bound (inclusive); NULL = 0
    ,ToValue         DECIMAL(18,4)  NULL        -- upper bound (exclusive); NULL = unlimited
    ,BuyRate         DECIMAL(18,4)  NOT NULL CONSTRAINT DF_CCRates_BuyRate   DEFAULT (0)
    ,SellRate        DECIMAL(18,4)  NOT NULL CONSTRAINT DF_CCRates_SellRate  DEFAULT (0)
    ,MinCharge       DECIMAL(18,2)  NULL        -- floor for this tier
    ,MaxCharge       DECIMAL(18,2)  NULL        -- ceiling for this tier
    ,SortOrder       INT            NOT NULL CONSTRAINT DF_CCRates_Sort      DEFAULT (1)
    ,CONSTRAINT FK_ChargeCodeRates_ChargeCodes
        FOREIGN KEY (ChargeCodeId) REFERENCES dbo.ChargeCodes (ChargeCodeId) ON DELETE CASCADE
);
GO

-- Standard freight charge codes seed
INSERT INTO dbo.ChargeCodes (Code, Name, Category, DefaultBasis, TransportMode, SortOrder)
VALUES
 ('OFR',  'Ocean Freight',                    'Freight',       'Per Container', 'Sea',  1)
,('FCR',  'FCL Ocean Freight',                'Freight',       'Per Container', 'Sea',  2)
,('LCL',  'LCL Freight',                      'Freight',       'Per CBM',       'Sea',  3)
,('AFR',  'Air Freight',                      'Freight',       'Per Kg',        'Air',  4)
,('THC',  'Terminal Handling Charge',         'Origin',        'Per Container', NULL,   10)
,('DTHC', 'Destination Terminal Handling',    'Destination',   'Per Container', NULL,   11)
,('BAF',  'Bunker Adjustment Factor',         'Freight',       'Per Container', 'Sea',  20)
,('CAF',  'Currency Adjustment Factor',       'Freight',       'Per Container', 'Sea',  21)
,('PSS',  'Peak Season Surcharge',            'Freight',       'Per Container', 'Sea',  22)
,('LSS',  'Low Sulphur Surcharge',            'Freight',       'Per Container', 'Sea',  23)
,('GRI',  'General Rate Increase',            'Freight',       'Per Container', 'Sea',  24)
,('EBS',  'Emergency Bunker Surcharge',       'Freight',       'Per Container', 'Sea',  25)
,('ISPS', 'ISPS Security Charge',             'Origin',        'Per Container', 'Sea',  30)
,('AMS',  'Automated Manifest System',        'Documentation', 'Per Shipment',  'Sea',  40)
,('BL',   'Bill of Lading Fee',               'Documentation', 'Per Shipment',  NULL,   41)
,('DO',   'Delivery Order Fee',               'Destination',   'Per Shipment',  NULL,   42)
,('DCRT', 'Document Courier',                'Documentation', 'Per Shipment',  NULL,   43)
,('CUST', 'Customs Clearance',               'Customs',       'Per Shipment',  NULL,   50)
,('TRCK', 'Trucking',                         'Trucking',      'Per Shipment',  NULL,   60)
,('INS',  'Insurance',                        'Freight',       'Per Shipment',  NULL,   70)
,('CFS',  'CFS / Consolidation Charge',      'Origin',        'Per Shipment',  NULL,   80)
,('STRG', 'Storage',                          'Destination',   'Per Container', NULL,   90)
,('DEMM', 'Demurrage',                        'Destination',   'Per Container', NULL,   91)
,('DETM', 'Detention',                        'Destination',   'Per Container', NULL,   92)
,('FSC',  'Fuel Surcharge',                   'Freight',       'Per Kg',        'Air',  100)
,('FHGE', 'Fuel Handling & Ground Equipment', 'Freight',       'Per Shipment',  'Air',  101)
;
GO
