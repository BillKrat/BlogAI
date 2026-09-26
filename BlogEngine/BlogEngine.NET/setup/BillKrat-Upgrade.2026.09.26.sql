-- BlogAI fork schema additions that Setup.sql (stock BlogEngine 3.3.6) does not contain.
-- Idempotent: safe to run on a new database after Setup.sql, and on the live database.
-- DbMembershipProvider selects be_Users.Comment (see DbMembershipProvider.MAIN_QUERY).
IF COL_LENGTH('dbo.be_Users', 'Comment') IS NULL
    ALTER TABLE [dbo].[be_Users] ADD [Comment] [nvarchar](max) NULL;
GO
