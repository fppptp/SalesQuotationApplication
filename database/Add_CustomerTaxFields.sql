---- Add Tax ID, Tax Branch, and Address to the Customers master table.
--IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Customers' AND COLUMN_NAME='TaxId')
--    ALTER TABLE dbo.Customers ADD TaxId     NVARCHAR(20)  NULL;

--IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Customers' AND COLUMN_NAME='TaxBranch')
--    ALTER TABLE dbo.Customers ADD TaxBranch NVARCHAR(50)  NULL;

--IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Customers' AND COLUMN_NAME='Address')
--    ALTER TABLE dbo.Customers ADD Address   NVARCHAR(500) NULL;

---- Add snapshot columns to QuotationHeaders so each quotation stores
---- the tax details that were current at the time it was issued.
--IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='QuotationHeaders' AND COLUMN_NAME='CustomerTaxId')
--    ALTER TABLE dbo.QuotationHeaders ADD CustomerTaxId     NVARCHAR(20)  NULL;

--IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='QuotationHeaders' AND COLUMN_NAME='CustomerTaxBranch')
--    ALTER TABLE dbo.QuotationHeaders ADD CustomerTaxBranch NVARCHAR(50)  NULL;

--IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='QuotationHeaders' AND COLUMN_NAME='CustomerAddress')
--    ALTER TABLE dbo.QuotationHeaders ADD CustomerAddress   NVARCHAR(500) NULL;
