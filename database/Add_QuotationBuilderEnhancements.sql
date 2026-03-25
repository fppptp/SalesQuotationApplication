USE FreightQuotation;
GO

-- Drop the existing single-column unique index so the same QuoteNo can exist across revisions
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_QuotationHeaders_QuoteNo' AND object_id = OBJECT_ID('dbo.QuotationHeaders'))
    DROP INDEX UX_QuotationHeaders_QuoteNo ON dbo.QuotationHeaders;
GO

-- Contact person (copied from customer default but overridable per quote)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationHeaders') AND name = 'ContactPerson')
    ALTER TABLE dbo.QuotationHeaders ADD ContactPerson NVARCHAR(100) NULL;
GO

-- Free time at origin/destination port in days
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationHeaders') AND name = 'FreeTimeDays')
    ALTER TABLE dbo.QuotationHeaders ADD FreeTimeDays INT NULL;
GO

-- Exclusions / scope-out text
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationHeaders') AND name = 'Exclusions')
    ALTER TABLE dbo.QuotationHeaders ADD Exclusions NVARCHAR(MAX) NULL;
GO

-- Revision number; starts at 1 for every original quotation
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationHeaders') AND name = 'RevisionNo')
    ALTER TABLE dbo.QuotationHeaders ADD RevisionNo INT NOT NULL
        CONSTRAINT DF_QuotationHeaders_RevisionNo DEFAULT (1);
GO

-- Link back to the quotation that was revised (NULL for originals)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationHeaders') AND name = 'ParentQuoteId')
    ALTER TABLE dbo.QuotationHeaders ADD ParentQuoteId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_QuotationHeaders_Parent')
    ALTER TABLE dbo.QuotationHeaders
        ADD CONSTRAINT FK_QuotationHeaders_Parent
            FOREIGN KEY (ParentQuoteId) REFERENCES dbo.QuotationHeaders (QuoteId);
GO

-- Replace single-column unique index with a composite one that allows multiple revisions per QuoteNo
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_QuotationHeaders_QuoteNo_RevisionNo' AND object_id = OBJECT_ID('dbo.QuotationHeaders'))
    CREATE UNIQUE INDEX UX_QuotationHeaders_QuoteNo_RevisionNo
        ON dbo.QuotationHeaders (QuoteNo, RevisionNo);
GO

-- Charge category groups lines for readability (Origin / Freight / Destination / etc.)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationItems') AND name = 'ChargeCategory')
    ALTER TABLE dbo.QuotationItems ADD ChargeCategory NVARCHAR(50) NOT NULL
        CONSTRAINT DF_QuotationItems_ChargeCategory DEFAULT ('Freight');
GO
