USE [wa001db]
GO

/****** Object: Table [wa001].[Results] Script Date: 11/21/2015 8:38:54 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [wa001].[Results] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [RoundID]   INT             NOT NULL,
    [AssetID]   INT             NOT NULL,
    [RsvVolume] NUMERIC (18, 4) NOT NULL,
    [RsvPrice]  NUMERIC (18, 4) NOT NULL,
    [BrVolume]  NUMERIC (18, 4) NOT NULL,
    [BrPrice]   NUMERIC (18, 4) NOT NULL
);


