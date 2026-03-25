---- Allow quotations to exist without a linked Customer master record.

---- 1. Make CustomerId nullable (the FK still applies for non-NULL values).
--IF EXISTS (
--    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
--    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'QuotationHeaders'
--      AND COLUMN_NAME = 'CustomerId' AND IS_NULLABLE = 'NO'
--)
--    ALTER TABLE dbo.QuotationHeaders ALTER COLUMN CustomerId INT NULL;

---- 2. Store customer name and code directly on the quotation.
--IF NOT EXISTS (
--    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
--    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'QuotationHeaders' AND COLUMN_NAME = 'CustomerName'
--)
--    ALTER TABLE dbo.QuotationHeaders ADD CustomerName NVARCHAR(400) NULL;

--IF NOT EXISTS (
--    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
--    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'QuotationHeaders' AND COLUMN_NAME = 'CustomerCode'
--)
--    ALTER TABLE dbo.QuotationHeaders ADD CustomerCode NVARCHAR(20) NULL;
