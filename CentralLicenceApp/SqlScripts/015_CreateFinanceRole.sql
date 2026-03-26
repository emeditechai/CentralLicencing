IF NOT EXISTS (SELECT 1 FROM dbo.RoleMaster WHERE RoleName = 'Finance')
BEGIN
    INSERT INTO dbo.RoleMaster (RoleName, Description, IsActive, CreatedAt)
    VALUES ('Finance', 'Expense reimbursement and settlement access', 1, GETDATE());
END