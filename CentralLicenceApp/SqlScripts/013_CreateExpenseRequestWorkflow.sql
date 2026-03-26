IF OBJECT_ID('dbo.ExpenseRequest', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExpenseRequest
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RequestNumber NVARCHAR(30) NOT NULL,
        EmployeeId INT NOT NULL,
        ApproverId INT NULL,
        PurposeOfTravel NVARCHAR(200) NOT NULL,
        EmployeeRemarks NVARCHAR(500) NULL,
        Status NVARCHAR(30) NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_ExpenseRequest_TotalAmount DEFAULT(0),
        ItemCount INT NOT NULL CONSTRAINT DF_ExpenseRequest_ItemCount DEFAULT(0),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_ExpenseRequest_CreatedAt DEFAULT(GETDATE()),
        SubmittedAt DATETIME NULL,
        ApprovedAt DATETIME NULL,
        RejectedAt DATETIME NULL,
        ApprovalRemarks NVARCHAR(500) NULL,
        ApprovedById INT NULL,
        CONSTRAINT FK_ExpenseRequest_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.UserMaster(Id),
        CONSTRAINT FK_ExpenseRequest_Approver FOREIGN KEY (ApproverId) REFERENCES dbo.UserMaster(Id),
        CONSTRAINT FK_ExpenseRequest_ApprovedBy FOREIGN KEY (ApprovedById) REFERENCES dbo.UserMaster(Id)
    );

    CREATE UNIQUE INDEX UX_ExpenseRequest_RequestNumber ON dbo.ExpenseRequest(RequestNumber);
    CREATE INDEX IX_ExpenseRequest_Employee_Status ON dbo.ExpenseRequest(EmployeeId, Status, CreatedAt DESC);
    CREATE INDEX IX_ExpenseRequest_Approver_Status ON dbo.ExpenseRequest(ApproverId, Status, SubmittedAt DESC);
END

IF OBJECT_ID('dbo.ExpenseRequestLine', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExpenseRequestLine
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RequestId INT NOT NULL,
        ItemType NVARCHAR(30) NOT NULL,
        ExpenseCategoryId INT NULL,
        Title NVARCHAR(150) NOT NULL,
        ProjectOrCostCenter NVARCHAR(120) NULL,
        ExpenseDate DATE NOT NULL,
        CurrencyCode NVARCHAR(10) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        PayableAmountInr DECIMAL(18,2) NULL,
        AccommodationCountry NVARCHAR(100) NULL,
        AccommodationCity NVARCHAR(100) NULL,
        CheckInDate DATE NULL,
        CheckOutDate DATE NULL,
        ReceiptPath NVARCHAR(300) NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_ExpenseRequestLine_CreatedAt DEFAULT(GETDATE()),
        CONSTRAINT FK_ExpenseRequestLine_Request FOREIGN KEY (RequestId) REFERENCES dbo.ExpenseRequest(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ExpenseRequestLine_Category FOREIGN KEY (ExpenseCategoryId) REFERENCES dbo.ExpenseCategoryMaster(Id)
    );

    CREATE INDEX IX_ExpenseRequestLine_RequestId ON dbo.ExpenseRequestLine(RequestId, ExpenseDate DESC);
END

IF OBJECT_ID('dbo.ExpenseRequestApprovalHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExpenseRequestApprovalHistory
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RequestId INT NOT NULL,
        ActionTaken NVARCHAR(50) NOT NULL,
        ActionByUserId INT NULL,
        Remarks NVARCHAR(500) NULL,
        ActionAt DATETIME NOT NULL CONSTRAINT DF_ExpenseRequestApprovalHistory_ActionAt DEFAULT(GETDATE()),
        CONSTRAINT FK_ExpenseRequestApprovalHistory_Request FOREIGN KEY (RequestId) REFERENCES dbo.ExpenseRequest(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ExpenseRequestApprovalHistory_User FOREIGN KEY (ActionByUserId) REFERENCES dbo.UserMaster(Id)
    );
END