USE [wa001db]
GO

/****** Object: Table [wa001].[Owners] Script Date: 11/21/2015 8:38:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [wa001].[Owners] (
    [Id]    INT           IDENTITY (1, 1) NOT NULL,
    [Login] NVARCHAR (50) NOT NULL,
    [Pin]   NVARCHAR (20) NULL
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Owners_Login_Unique]
    ON [wa001].[Owners]([Login] ASC);


GO
ALTER TABLE [wa001].[Owners]
    ADD CONSTRAINT [PK_Owners] PRIMARY KEY CLUSTERED ([Id] ASC);


