-- Add Email and Telephone to QuotationHeaders (contact person fields).
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'QuotationHeaders' AND COLUMN_NAME = 'ContactEmail'
)
    ALTER TABLE dbo.QuotationHeaders ADD ContactEmail NVARCHAR(255) NULL;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'QuotationHeaders' AND COLUMN_NAME = 'ContactPhone'
)
    ALTER TABLE dbo.QuotationHeaders ADD ContactPhone NVARCHAR(50) NULL;
