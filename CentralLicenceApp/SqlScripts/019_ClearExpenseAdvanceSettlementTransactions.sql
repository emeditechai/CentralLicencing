SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @DeletedAttachments INT = 0;
    DECLARE @DeletedHistory INT = 0;
    DECLARE @DeletedLines INT = 0;
    DECLARE @DeletedRequests INT = 0;

    IF OBJECT_ID('dbo.ExpenseRequest', 'U') IS NULL
    BEGIN
        PRINT 'ExpenseRequest table does not exist. No transaction data was cleared.';
        COMMIT TRANSACTION;
        RETURN;
    END

    -- Clears only expense workflow transactions: expense lines, advance booking lines,
    -- approval trail, reimbursement progress, and settlement data stored under ExpenseRequest.
    IF OBJECT_ID('dbo.ExpenseRequestLineAttachment', 'U') IS NOT NULL
    BEGIN
        DELETE attachment
        FROM dbo.ExpenseRequestLineAttachment AS attachment
        INNER JOIN dbo.ExpenseRequestLine AS line ON line.Id = attachment.RequestLineId
        INNER JOIN dbo.ExpenseRequest AS requestHeader ON requestHeader.Id = line.RequestId;

        SET @DeletedAttachments = @@ROWCOUNT;
        DBCC CHECKIDENT ('dbo.ExpenseRequestLineAttachment', RESEED, 0) WITH NO_INFOMSGS;
    END

    IF OBJECT_ID('dbo.ExpenseRequestApprovalHistory', 'U') IS NOT NULL
    BEGIN
        DELETE history
        FROM dbo.ExpenseRequestApprovalHistory AS history
        INNER JOIN dbo.ExpenseRequest AS requestHeader ON requestHeader.Id = history.RequestId;

        SET @DeletedHistory = @@ROWCOUNT;
        DBCC CHECKIDENT ('dbo.ExpenseRequestApprovalHistory', RESEED, 0) WITH NO_INFOMSGS;
    END

    IF OBJECT_ID('dbo.ExpenseRequestLine', 'U') IS NOT NULL
    BEGIN
        DELETE line
        FROM dbo.ExpenseRequestLine AS line
        INNER JOIN dbo.ExpenseRequest AS requestHeader ON requestHeader.Id = line.RequestId;

        SET @DeletedLines = @@ROWCOUNT;
        DBCC CHECKIDENT ('dbo.ExpenseRequestLine', RESEED, 0) WITH NO_INFOMSGS;
    END

    DELETE FROM dbo.ExpenseRequest;
    SET @DeletedRequests = @@ROWCOUNT;
    DBCC CHECKIDENT ('dbo.ExpenseRequest', RESEED, 0) WITH NO_INFOMSGS;

    COMMIT TRANSACTION;

    SELECT
        @DeletedAttachments AS DeletedExpenseRequestLineAttachments,
        @DeletedHistory AS DeletedExpenseRequestApprovalHistory,
        @DeletedLines AS DeletedExpenseRequestLines,
        @DeletedRequests AS DeletedExpenseRequests;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;