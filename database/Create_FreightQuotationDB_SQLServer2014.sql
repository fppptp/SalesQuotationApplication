IF DB_ID('FreightQuotationDB') IS NULL
BEGIN
    CREATE DATABASE FreightQuotationDB;
END
GO

USE FreightQuotationDB;
GO

IF OBJECT_ID('dbo.QuotationItems', 'U') IS NOT NULL
    DROP TABLE dbo.QuotationItems;
GO

IF OBJECT_ID('dbo.QuotationHeaders', 'U') IS NOT NULL
    DROP TABLE dbo.QuotationHeaders;
GO

IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL
    DROP TABLE dbo.Customers;
GO

CREATE TABLE dbo.Customers
(
     CustomerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,CustomerCode NVARCHAR(30) NOT NULL
    ,CustomerNameTH NVARCHAR(200) NULL
    ,CustomerNameEN NVARCHAR(200) NOT NULL
    ,TaxId NVARCHAR(30) NULL
    ,BranchNo NVARCHAR(20) NULL
    ,ContactName NVARCHAR(100) NULL
    ,Email NVARCHAR(200) NULL
    ,Phone NVARCHAR(50) NULL
    ,AddressLine NVARCHAR(300) NULL
    ,IsActive BIT NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT (1)
    ,CreatedAt DATETIME NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT (GETDATE())
);
GO

CREATE TABLE dbo.QuotationHeaders
(
     QuoteId INT IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,QuoteNo NVARCHAR(30) NOT NULL
    ,QuoteDate DATE NOT NULL
    ,ValidUntil DATE NOT NULL
    ,CustomerId INT NOT NULL
    ,Mode NVARCHAR(30) NOT NULL
    ,ServiceType NVARCHAR(50) NOT NULL
    ,Incoterm NVARCHAR(10) NOT NULL
    ,Origin NVARCHAR(200) NOT NULL
    ,Destination NVARCHAR(200) NOT NULL
    ,CarrierOrAgent NVARCHAR(100) NULL
    ,TransitTimeDays INT NULL
    ,Commodity NVARCHAR(200) NULL
    ,PackageCount INT NOT NULL CONSTRAINT DF_QuotationHeaders_PackageCount DEFAULT (0)
    ,GrossWeightKg DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationHeaders_GrossWeightKg DEFAULT (0)
    ,VolumeCbm DECIMAL(18,3) NOT NULL CONSTRAINT DF_QuotationHeaders_VolumeCbm DEFAULT (0)
    ,CurrencyCode NVARCHAR(10) NOT NULL
    ,ExchangeRate DECIMAL(18,6) NOT NULL CONSTRAINT DF_QuotationHeaders_ExchangeRate DEFAULT (1)
    ,Subtotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationHeaders_Subtotal DEFAULT (0)
    ,CostTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationHeaders_CostTotal DEFAULT (0)
    ,ProfitTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationHeaders_ProfitTotal DEFAULT (0)
    ,DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationHeaders_DiscountAmount DEFAULT (0)
    ,IncludeVat BIT NOT NULL CONSTRAINT DF_QuotationHeaders_IncludeVat DEFAULT (1)
    ,VatRate DECIMAL(9,2) NOT NULL CONSTRAINT DF_QuotationHeaders_VatRate DEFAULT (7)
    ,VatAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationHeaders_VatAmount DEFAULT (0)
    ,GrandTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationHeaders_GrandTotal DEFAULT (0)
    ,Status NVARCHAR(20) NOT NULL CONSTRAINT DF_QuotationHeaders_Status DEFAULT ('Draft')
    ,SalesPerson NVARCHAR(100) NULL
    ,Remarks NVARCHAR(MAX) NULL
    ,TermsAndConditions NVARCHAR(MAX) NULL
    ,CreatedAt DATETIME NOT NULL CONSTRAINT DF_QuotationHeaders_CreatedAt DEFAULT (GETDATE())
    ,UpdatedAt DATETIME NOT NULL CONSTRAINT DF_QuotationHeaders_UpdatedAt DEFAULT (GETDATE())
    ,CONSTRAINT FK_QuotationHeaders_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId)
);
GO

CREATE TABLE dbo.QuotationItems
(
     ItemId INT IDENTITY(1,1) NOT NULL PRIMARY KEY
    ,QuoteId INT NOT NULL
    ,SortOrder INT NOT NULL
    ,ChargeCode NVARCHAR(30) NOT NULL
    ,ChargeName NVARCHAR(200) NOT NULL
    ,ChargeBasis NVARCHAR(50) NOT NULL
    ,Quantity DECIMAL(18,4) NOT NULL CONSTRAINT DF_QuotationItems_Quantity DEFAULT (1)
    ,UnitCost DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationItems_UnitCost DEFAULT (0)
    ,UnitPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationItems_UnitPrice DEFAULT (0)
    ,CostAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationItems_CostAmount DEFAULT (0)
    ,SellAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationItems_SellAmount DEFAULT (0)
    ,ProfitAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_QuotationItems_ProfitAmount DEFAULT (0)
    ,CONSTRAINT FK_QuotationItems_QuotationHeaders FOREIGN KEY (QuoteId) REFERENCES dbo.QuotationHeaders(QuoteId) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX UX_Customers_CustomerCode ON dbo.Customers(CustomerCode);
GO

CREATE UNIQUE INDEX UX_QuotationHeaders_QuoteNo ON dbo.QuotationHeaders(QuoteNo);
GO

CREATE INDEX IX_QuotationHeaders_QuoteDate ON dbo.QuotationHeaders(QuoteDate);
GO

CREATE INDEX IX_QuotationHeaders_Status ON dbo.QuotationHeaders(Status);
GO

CREATE INDEX IX_QuotationHeaders_CustomerId ON dbo.QuotationHeaders(CustomerId);
GO

CREATE INDEX IX_QuotationItems_QuoteId_SortOrder ON dbo.QuotationItems(QuoteId, SortOrder);
GO

INSERT INTO dbo.Customers
(
     CustomerCode
    ,CustomerNameTH
    ,CustomerNameEN
    ,TaxId
    ,BranchNo
    ,ContactName
    ,Email
    ,Phone
    ,AddressLine
)
VALUES
 ('CUST-0001', N'บริษัท ไทย อิเล็คทรอนิคส์ จำกัด', 'Thai Electronics Co., Ltd.', '0105559000001', '00000', 'Purchasing Team', 'purchasing@thaielectronics.co.th', '02-111-2222', N'Bangkok, Thailand')
,('CUST-0002', N'บริษัท สยาม ออโต้ พาร์ท จำกัด', 'Siam Auto Parts Co., Ltd.', '0105559000002', '00000', 'Import Team', 'import@siamauto.co.th', '02-333-4444', N'Samut Prakan, Thailand')
,('CUST-0003', N'บริษัท โกลบอล รีเทล จำกัด', 'Global Retail Thailand Co., Ltd.', '0105559000003', '00000', 'Logistics Team', 'logistics@globalretail.co.th', '02-555-6666', N'Pathum Thani, Thailand');
GO

INSERT INTO dbo.QuotationHeaders
(
     QuoteNo
    ,QuoteDate
    ,ValidUntil
    ,CustomerId
    ,Mode
    ,ServiceType
    ,Incoterm
    ,Origin
    ,Destination
    ,CarrierOrAgent
    ,TransitTimeDays
    ,Commodity
    ,PackageCount
    ,GrossWeightKg
    ,VolumeCbm
    ,CurrencyCode
    ,ExchangeRate
    ,Subtotal
    ,CostTotal
    ,ProfitTotal
    ,DiscountAmount
    ,IncludeVat
    ,VatRate
    ,VatAmount
    ,GrandTotal
    ,Status
    ,SalesPerson
    ,Remarks
    ,TermsAndConditions
)
VALUES
 ('QTN202603-0001', '2026-03-10', '2026-03-17', 1, 'Air', 'Door to Door', 'EXW', 'Bangkok, Thailand', 'Singapore', 'SQ Cargo', 2, 'Electronic Parts', 12, 350.00, 2.450, 'THB', 1.000000, 48500.00, 39200.00, 9300.00, 0.00, 1, 7.00, 3395.00, 51895.00, 'Sent', 'Nattapong', N'Includes pickup and export customs clearance', N'Rates are valid based on current tariff and space availability.')
,('QTN202603-0002', '2026-03-11', '2026-03-20', 2, 'Sea', 'Port to Port', 'FOB', 'Laem Chabang, Thailand', 'Ho Chi Minh City, Vietnam', 'Evergreen', 5, 'Automotive Components', 1, 5200.00, 15.800, 'THB', 1.000000, 28500.00, 22400.00, 6100.00, 500.00, 1, 7.00, 1960.00, 29960.00, 'Draft', 'Siriporn', N'20GP estimate only', N'DTHC and destination local charges are excluded unless otherwise stated.');
GO

INSERT INTO dbo.QuotationItems
(
     QuoteId
    ,SortOrder
    ,ChargeCode
    ,ChargeName
    ,ChargeBasis
    ,Quantity
    ,UnitCost
    ,UnitPrice
    ,CostAmount
    ,SellAmount
    ,ProfitAmount
)
VALUES
 (1, 1, 'PICKUP', 'Pickup Trucking', 'Per Shipment', 1.0000, 3500.00, 4500.00, 3500.00, 4500.00, 1000.00)
,(1, 2, 'CUS-EXP', 'Export Customs Clearance', 'Per Shipment', 1.0000, 1200.00, 1800.00, 1200.00, 1800.00, 600.00)
,(1, 3, 'AFRT', 'Air Freight', 'Per Kg', 350.0000, 98.00, 120.00, 34300.00, 42000.00, 7700.00)
,(1, 4, 'DOC', 'Documentation Fee', 'Per Shipment', 1.0000, 200.00, 200.00, 200.00, 200.00, 0.00)
,(2, 1, 'O-FRT', 'Ocean Freight 20GP', 'Per Container', 1.0000, 18000.00, 22500.00, 18000.00, 22500.00, 4500.00)
,(2, 2, 'DOC', 'Documentation Fee', 'Per Shipment', 1.0000, 400.00, 700.00, 400.00, 700.00, 300.00)
,(2, 3, 'THC', 'THC Origin', 'Per Container', 1.0000, 4000.00, 5300.00, 4000.00, 5300.00, 1300.00);
GO
