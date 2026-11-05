-- ============================================================
-- 085 – Link DailyTaskLog to TicketPriorityMaster (PriorityId)
-- Existing tasks become Medium. Idempotent.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DailyTaskLog') AND name = 'PriorityId')
BEGIN
    ALTER TABLE DailyTaskLog ADD PriorityId INT NULL
        CONSTRAINT FK_DailyTaskLog_Priority FOREIGN KEY REFERENCES TicketPriorityMaster(Id);
END
GO

UPDATE DailyTaskLog
SET PriorityId = (SELECT TOP 1 Id FROM TicketPriorityMaster WHERE PriorityName = N'Medium')
WHERE PriorityId IS NULL;
GO
