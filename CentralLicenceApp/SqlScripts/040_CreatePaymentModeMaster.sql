IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PaymentMode')
BEGIN
    CREATE TABLE dbo.PaymentMode (
        Id        INT           IDENTITY(1,1) NOT NULL,
        Name      NVARCHAR(100) NOT NULL,
        IsActive  BIT           NOT NULL DEFAULT(1),
        SortOrder INT           NOT NULL DEFAULT(0),
        CONSTRAINT PK_PaymentMode PRIMARY KEY (Id)
    );

    -- Seed default payment modes
    INSERT INTO dbo.PaymentMode (Name, IsActive, SortOrder) VALUES
        ('Cash',   1, 1),
        ('Cheque', 1, 2),
        ('NEFT',   1, 3),
        ('RTGS',   1, 4),
        ('UPI',    1, 5),
        ('IMPS',   1, 6);
END
