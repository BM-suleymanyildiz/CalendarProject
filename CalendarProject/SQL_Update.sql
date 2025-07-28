-- StartDate ve EndDate alanlarını nullable yap
ALTER TABLE [dbo].[Events] ALTER COLUMN [StartDate] DATETIME NULL;
ALTER TABLE [dbo].[Events] ALTER COLUMN [EndDate] DATETIME NULL; 