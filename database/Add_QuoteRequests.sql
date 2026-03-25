USE FreightQuotation;
GO

IF OBJECT_ID('dbo.QuoteRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuoteRequests
    (
         RequestId                    INT            IDENTITY(1,1) NOT NULL PRIMARY KEY
        ,RequestNo                    NVARCHAR(30)   NOT NULL
        ,TransportMode                NVARCHAR(20)   NOT NULL
        ,ServiceType                  NVARCHAR(50)   NOT NULL
        ,Origin                       NVARCHAR(200)  NOT NULL
        ,Destination                  NVARCHAR(200)  NOT NULL
        ,Shipper                      NVARCHAR(200)  NOT NULL
        ,Consignee                    NVARCHAR(200)  NOT NULL
        ,CargoReadyDate               DATE           NOT NULL
        ,NumberOfPackages             INT            NOT NULL CONSTRAINT DF_QuoteRequests_NumberOfPackages  DEFAULT (0)
        ,GrossWeightKg                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_QuoteRequests_GrossWeightKg     DEFAULT (0)
        ,VolumeCbm                    DECIMAL(18,3)  NOT NULL CONSTRAINT DF_QuoteRequests_VolumeCbm          DEFAULT (0)
        ,Dimensions                   NVARCHAR(200)  NULL
        ,Commodity                    NVARCHAR(200)  NOT NULL
        ,HsCode                       NVARCHAR(50)   NULL
        ,ContainerOrChargeableWeight  NVARCHAR(100)  NULL
        ,IsDangerousGoods             BIT            NOT NULL CONSTRAINT DF_QuoteRequests_IsDangerousGoods  DEFAULT (0)
        ,IsReefer                     BIT            NOT NULL CONSTRAINT DF_QuoteRequests_IsReefer           DEFAULT (0)
        ,IsOversized                  BIT            NOT NULL CONSTRAINT DF_QuoteRequests_IsOversized        DEFAULT (0)
        ,SpecialRequirementsNote      NVARCHAR(MAX)  NULL
        ,NeedsCustomsClearance        BIT            NOT NULL CONSTRAINT DF_QuoteRequests_NeedsCustoms       DEFAULT (0)
        ,NeedsTrucking                BIT            NOT NULL CONSTRAINT DF_QuoteRequests_NeedsTrucking      DEFAULT (0)
        ,NeedsInsurance               BIT            NOT NULL CONSTRAINT DF_QuoteRequests_NeedsInsurance     DEFAULT (0)
        ,PackingListPath              NVARCHAR(500)  NULL
        ,CommercialInvoicePath        NVARCHAR(500)  NULL
        ,ProductPhotoPath             NVARCHAR(500)  NULL
        ,Remarks                      NVARCHAR(MAX)  NULL
        ,Status                       NVARCHAR(20)   NOT NULL CONSTRAINT DF_QuoteRequests_Status             DEFAULT ('New')
        ,CreatedBy                    NVARCHAR(100)  NULL
        ,CreatedAt                    DATETIME       NOT NULL CONSTRAINT DF_QuoteRequests_CreatedAt          DEFAULT (GETDATE())
    );

    CREATE UNIQUE INDEX UX_QuoteRequests_RequestNo ON dbo.QuoteRequests (RequestNo);
END
GO
