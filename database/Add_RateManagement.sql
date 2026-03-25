USE FreightQuotation;
GO

-- Rate Cards: one card per carrier / route / validity window
IF OBJECT_ID('dbo.RateLines',    'U') IS NOT NULL DROP TABLE dbo.RateLines;
IF OBJECT_ID('dbo.RateCards',    'U') IS NOT NULL DROP TABLE dbo.RateCards;
IF OBJECT_ID('dbo.RateSheetImports', 'U') IS NOT NULL DROP TABLE dbo.RateSheetImports;
IF OBJECT_ID('dbo.Surcharges',   'U') IS NOT NULL DROP TABLE dbo.Surcharges;
IF OBJECT_ID('dbo.RateTemplateLines', 'U') IS NOT NULL DROP TABLE dbo.RateTemplateLines;
IF OBJECT_ID('dbo.RateTemplates','U') IS NOT NULL DROP TABLE dbo.RateTemplates;
IF OBJECT_ID('dbo.MarginRules',  'U') IS NOT NULL DROP TABLE dbo.MarginRules;
GO

CREATE TABLE dbo.RateCards
(
     RateCardId          INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,RateCardCode        NVARCHAR(30)   NOT NULL
    ,RateCardName        NVARCHAR(150)  NOT NULL
    ,TransportMode       NVARCHAR(20)   NOT NULL
    ,CarrierOrAgent      NVARCHAR(100)  NULL
    ,OriginCountry       NVARCHAR(100)  NULL
    ,OriginPort          NVARCHAR(100)  NULL
    ,DestinationCountry  NVARCHAR(100)  NULL
    ,DestinationPort     NVARCHAR(100)  NULL
    ,CurrencyCode        NVARCHAR(10)   NOT NULL CONSTRAINT DF_RateCards_CurrencyCode  DEFAULT ('USD')
    ,EffectiveDate       DATE           NOT NULL
    ,ExpiryDate          DATE           NOT NULL
    ,Notes               NVARCHAR(MAX)  NULL
    ,IsActive            BIT            NOT NULL CONSTRAINT DF_RateCards_IsActive      DEFAULT (1)
    ,CreatedBy           NVARCHAR(100)  NULL
    ,CreatedAt           DATETIME       NOT NULL CONSTRAINT DF_RateCards_CreatedAt     DEFAULT (GETDATE())
    ,UpdatedAt           DATETIME       NOT NULL CONSTRAINT DF_RateCards_UpdatedAt     DEFAULT (GETDATE())
);
GO

CREATE UNIQUE INDEX UX_RateCards_RateCardCode ON dbo.RateCards (RateCardCode);
GO

CREATE TABLE dbo.RateLines
(
     RateLineId      INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,RateCardId      INT            NOT NULL
    ,SortOrder       INT            NOT NULL CONSTRAINT DF_RateLines_SortOrder  DEFAULT (1)
    ,ChargeCategory  NVARCHAR(50)   NOT NULL CONSTRAINT DF_RateLines_Category   DEFAULT ('Freight')
    ,ChargeCode      NVARCHAR(30)   NOT NULL
    ,ChargeName      NVARCHAR(200)  NOT NULL
    ,RouteDetail     NVARCHAR(200)  NULL
    ,ChargeBasis     NVARCHAR(50)   NOT NULL CONSTRAINT DF_RateLines_Basis      DEFAULT ('Per Container')
    ,BuyRate         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_RateLines_BuyRate    DEFAULT (0)
    ,SellRate        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_RateLines_SellRate   DEFAULT (0)
    ,MinCharge       DECIMAL(18,2)  NULL
    ,Notes           NVARCHAR(200)  NULL
    ,CONSTRAINT FK_RateLines_RateCards FOREIGN KEY (RateCardId) REFERENCES dbo.RateCards (RateCardId) ON DELETE CASCADE
);
GO

-- Standalone surcharge reference table
CREATE TABLE dbo.Surcharges
(
     SurchargeId     INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,SurchargeCode   NVARCHAR(20)   NOT NULL
    ,SurchargeName   NVARCHAR(100)  NOT NULL
    ,TransportMode   NVARCHAR(20)   NOT NULL
    ,CarrierOrAgent  NVARCHAR(100)  NULL
    ,POL             NVARCHAR(100)  NULL
    ,POD             NVARCHAR(100)  NULL
    ,CurrencyCode    NVARCHAR(10)   NOT NULL CONSTRAINT DF_Surcharges_Currency  DEFAULT ('USD')
    ,ChargeBasis     NVARCHAR(50)   NOT NULL CONSTRAINT DF_Surcharges_Basis     DEFAULT ('Per Container')
    ,BuyRate         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Surcharges_BuyRate   DEFAULT (0)
    ,SellRate        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Surcharges_SellRate  DEFAULT (0)
    ,EffectiveDate   DATE           NOT NULL
    ,ExpiryDate      DATE           NOT NULL
    ,IsActive        BIT            NOT NULL CONSTRAINT DF_Surcharges_IsActive  DEFAULT (1)
    ,Notes           NVARCHAR(200)  NULL
    ,CreatedAt       DATETIME       NOT NULL CONSTRAINT DF_Surcharges_CreatedAt DEFAULT (GETDATE())
    ,UpdatedAt       DATETIME       NOT NULL CONSTRAINT DF_Surcharges_UpdatedAt DEFAULT (GETDATE())
);
GO

-- Automatic margin / markup rules applied when pricing from buy rates
CREATE TABLE dbo.MarginRules
(
     MarginRuleId    INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,RuleName        NVARCHAR(100)  NOT NULL
    ,TransportMode   NVARCHAR(20)   NULL   -- NULL = all modes
    ,ChargeCategory  NVARCHAR(50)   NULL   -- NULL = all categories
    ,CustomerGroup   NVARCHAR(100)  NULL   -- NULL = all customers
    ,MarginType      NVARCHAR(20)   NOT NULL CONSTRAINT DF_MarginRules_Type     DEFAULT ('Percent')
    ,MarginValue     DECIMAL(18,4)  NOT NULL CONSTRAINT DF_MarginRules_Value    DEFAULT (0)
    ,Priority        INT            NOT NULL CONSTRAINT DF_MarginRules_Priority DEFAULT (1)
    ,IsActive        BIT            NOT NULL CONSTRAINT DF_MarginRules_IsActive DEFAULT (1)
    ,Notes           NVARCHAR(200)  NULL
    ,CreatedAt       DATETIME       NOT NULL CONSTRAINT DF_MarginRules_CreatedAt DEFAULT (GETDATE())
);
GO

-- Pre-defined charge line templates per mode / customer group
CREATE TABLE dbo.RateTemplates
(
     RateTemplateId  INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,TemplateName    NVARCHAR(100)  NOT NULL
    ,TransportMode   NVARCHAR(20)   NOT NULL
    ,CustomerGroup   NVARCHAR(100)  NULL
    ,Description     NVARCHAR(300)  NULL
    ,IsActive        BIT            NOT NULL CONSTRAINT DF_RateTemplates_IsActive DEFAULT (1)
    ,CreatedAt       DATETIME       NOT NULL CONSTRAINT DF_RateTemplates_CreatedAt DEFAULT (GETDATE())
);
GO

CREATE TABLE dbo.RateTemplateLines
(
     TemplateLineId   INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,RateTemplateId   INT            NOT NULL
    ,SortOrder        INT            NOT NULL CONSTRAINT DF_RateTplLines_SortOrder DEFAULT (1)
    ,ChargeCategory   NVARCHAR(50)   NOT NULL CONSTRAINT DF_RateTplLines_Category  DEFAULT ('Freight')
    ,ChargeCode       NVARCHAR(30)   NOT NULL
    ,ChargeName       NVARCHAR(200)  NOT NULL
    ,ChargeBasis      NVARCHAR(50)   NOT NULL CONSTRAINT DF_RateTplLines_Basis     DEFAULT ('Lump Sum')
    ,DefaultBuyRate   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_RateTplLines_BuyRate   DEFAULT (0)
    ,DefaultSellRate  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_RateTplLines_SellRate  DEFAULT (0)
    ,CONSTRAINT FK_RateTemplateLines_Templates FOREIGN KEY (RateTemplateId) REFERENCES dbo.RateTemplates (RateTemplateId) ON DELETE CASCADE
);
GO

-- Track uploaded rate sheet files for audit and future parsing
CREATE TABLE dbo.RateSheetImports
(
     ImportId        INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,FileName        NVARCHAR(500)  NOT NULL
    ,FilePath        NVARCHAR(500)  NOT NULL
    ,TransportMode   NVARCHAR(20)   NOT NULL
    ,ImportedBy      NVARCHAR(100)  NULL
    ,ImportedAt      DATETIME       NOT NULL CONSTRAINT DF_RateSheetImports_At DEFAULT (GETDATE())
    ,Status          NVARCHAR(20)   NOT NULL CONSTRAINT DF_RateSheetImports_Status DEFAULT ('Uploaded')
    ,Notes           NVARCHAR(MAX)  NULL
);
GO
