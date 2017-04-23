USE [wa001db]
GO

/****** Object: Table [wa001].[Orders] Script Date: 11/21/2015 8:38:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [wa001].[Orders] (
    [Id]      INT             NOT NULL,
    [RoundID] INT             NOT NULL,
    [AssetID] INT             NOT NULL,
    [Price]   DECIMAL (18, 4) NOT NULL,
    [Volume]  DECIMAL (18, 4) NOT NULL
);


