USE [wa001db]
GO

/****** Object: Table [wa001].[Assets] Script Date: 11/21/2015 8:37:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [wa001].[Assets] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (200)  NULL,
    [Type]         NVARCHAR (10)   NULL,
    [SomeConstant] DECIMAL (18, 4) NULL,
    [OwnerID]      INT             NULL
);


