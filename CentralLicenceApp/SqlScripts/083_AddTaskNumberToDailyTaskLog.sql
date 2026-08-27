-- ============================================================
-- 083 – Add TaskNumber to DailyTaskLog
-- ============================================================

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('DailyTaskLog') 
      AND name = 'TaskNumber'
)
BEGIN
    ALTER TABLE DailyTaskLog ADD TaskNumber NVARCHAR(50) NULL;
END
GO

UPDATE DailyTaskLog SET TaskNumber = 'T-OLD' + RIGHT('000000' + CAST(Id AS VARCHAR), 6) WHERE TaskNumber IS NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DailyTaskLog_TaskNumber')
BEGIN
    CREATE UNIQUE INDEX IX_DailyTaskLog_TaskNumber ON DailyTaskLog(TaskNumber) WHERE TaskNumber IS NOT NULL;
END
GO
