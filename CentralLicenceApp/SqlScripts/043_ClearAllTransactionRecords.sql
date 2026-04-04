-- ============================================================
-- Script  : 043_ClearAllTransactionRecords.sql
-- Purpose : Consolidated script to clear Quotation, Invoice and
--           Payment transaction data in one atomic transaction.
--
--           Deletion order (leaf → root, respects all FK constraints):
--             1. InvoicePaymentLine   (leaf — cascade from InvoicePayment)
--             2. InvoicePayment       (FK → Invoice, no cascade)
--             3. InvoiceSignatories   (cascade from Invoice)
--             4. InvoiceLine          (cascade from Invoice)
--             5. Invoice
--             6. QuotationSignatories (cascade from Quotation)
--             7. QuotationLine        (cascade from Quotation)
--             8. Quotation
--
--           Side-effects:
--             • ClientPurchasedProduct.InvoiceNo → NULL (text ref, no FK)
--             • All identity seeds reset to 0 (next row gets Id = 1)
--
-- ⚠  THIS IS DESTRUCTIVE — run only on test / staging databases.
-- ============================================================

BEGIN TRANSACTION;

BEGIN TRY

    -- ── 1. Payment Lines ───────────────────────────────────────────
    DELETE FROM dbo.InvoicePaymentLine;
    PRINT CONCAT('InvoicePaymentLine rows deleted   : ', @@ROWCOUNT);

    -- ── 2. Payment Headers ─────────────────────────────────────────
    DELETE FROM dbo.InvoicePayment;
    PRINT CONCAT('InvoicePayment rows deleted       : ', @@ROWCOUNT);

    -- ── 3. Invoice Signatories ─────────────────────────────────────
    DELETE FROM dbo.InvoiceSignatories;
    PRINT CONCAT('InvoiceSignatories rows deleted   : ', @@ROWCOUNT);

    -- ── 4. Invoice Lines ───────────────────────────────────────────
    DELETE FROM dbo.InvoiceLine;
    PRINT CONCAT('InvoiceLine rows deleted          : ', @@ROWCOUNT);

    -- ── 5. Invoices ────────────────────────────────────────────────
    DELETE FROM dbo.Invoice;
    PRINT CONCAT('Invoice rows deleted              : ', @@ROWCOUNT);

    -- ── 5a. Clear denormalised InvoiceNo on ClientPurchasedProduct ─
    UPDATE dbo.ClientPurchasedProduct
    SET    InvoiceNo = NULL
    WHERE  InvoiceNo IS NOT NULL;
    PRINT CONCAT('ClientPurchasedProduct.InvoiceNo cleared : ', @@ROWCOUNT);

    -- ── 6. Quotation Signatories ───────────────────────────────────
    DELETE FROM dbo.QuotationSignatories;
    PRINT CONCAT('QuotationSignatories rows deleted : ', @@ROWCOUNT);

    -- ── 7. Quotation Lines ─────────────────────────────────────────
    DELETE FROM dbo.QuotationLine;
    PRINT CONCAT('QuotationLine rows deleted        : ', @@ROWCOUNT);

    -- ── 8. Quotations ──────────────────────────────────────────────
    DELETE FROM dbo.Quotation;
    PRINT CONCAT('Quotation rows deleted            : ', @@ROWCOUNT);

    -- ── Reset identity seeds ───────────────────────────────────────
    DBCC CHECKIDENT ('dbo.InvoicePaymentLine',   RESEED, 0);
    DBCC CHECKIDENT ('dbo.InvoicePayment',       RESEED, 0);
    DBCC CHECKIDENT ('dbo.InvoiceSignatories',   RESEED, 0);
    DBCC CHECKIDENT ('dbo.InvoiceLine',          RESEED, 0);
    DBCC CHECKIDENT ('dbo.Invoice',              RESEED, 0);
    DBCC CHECKIDENT ('dbo.QuotationSignatories', RESEED, 0);
    DBCC CHECKIDENT ('dbo.QuotationLine',        RESEED, 0);
    DBCC CHECKIDENT ('dbo.Quotation',            RESEED, 0);

    COMMIT TRANSACTION;
    PRINT '✔  043_ClearAllTransactionRecords completed successfully.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '✘  Error — transaction rolled back. No data was changed.';
    PRINT ERROR_MESSAGE();
END CATCH;
