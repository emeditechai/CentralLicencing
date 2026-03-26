-- ============================================================
-- Script: 011_UserMaster_DOB_DOJ.sql
-- Description: Add DateOfBirth and DateOfJoining columns to UserMaster
-- Database: Central_Lic_DB
-- Run once against your database.
-- ============================================================

USE [Central_Lic_DB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'DateOfBirth')
BEGIN
    ALTER TABLE [dbo].[UserMaster] ADD [DateOfBirth] DATE NULL;
    PRINT 'Column DateOfBirth added to UserMaster.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'DateOfJoining')
BEGIN
    ALTER TABLE [dbo].[UserMaster] ADD [DateOfJoining] DATE NULL;
    PRINT 'Column DateOfJoining added to UserMaster.';
END
GO