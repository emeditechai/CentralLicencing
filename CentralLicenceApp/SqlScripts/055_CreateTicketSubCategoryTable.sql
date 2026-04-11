-- ============================================
-- Ticket Sub Category + HelpDeskTicket.SubCategoryId
-- ============================================

-- 1. Ticket Sub Category Master
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='TicketSubCategoryMaster')
BEGIN
    CREATE TABLE [dbo].[TicketSubCategoryMaster] (
        [Id]              INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CategoryId]      INT            NOT NULL,
        [SubCategoryName] NVARCHAR(100)  NOT NULL,
        [Description]     NVARCHAR(300)  NULL,
        [IsActive]        BIT            NOT NULL DEFAULT 1,
        [CreatedAt]       DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TicketSubCategory_Category FOREIGN KEY (CategoryId) REFERENCES TicketCategoryMaster(Id)
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_TicketSubCategoryMaster_CategoryId'
      AND object_id = OBJECT_ID('TicketSubCategoryMaster'))
BEGIN
    CREATE INDEX IX_TicketSubCategoryMaster_CategoryId
        ON dbo.TicketSubCategoryMaster(CategoryId);
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_TicketSubCategoryMaster_Name_Category'
      AND object_id = OBJECT_ID('TicketSubCategoryMaster'))
BEGIN
    CREATE UNIQUE INDEX UX_TicketSubCategoryMaster_Name_Category
        ON dbo.TicketSubCategoryMaster(CategoryId, SubCategoryName);
END

-- 2. Add SubCategoryId column to HelpDeskTicket (nullable FK)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('HelpDeskTicket') AND name = 'SubCategoryId')
BEGIN
    ALTER TABLE [dbo].[HelpDeskTicket]
        ADD [SubCategoryId] INT NULL;

    ALTER TABLE [dbo].[HelpDeskTicket]
        ADD CONSTRAINT FK_HelpDeskTicket_SubCategory
            FOREIGN KEY (SubCategoryId) REFERENCES TicketSubCategoryMaster(Id);
END
