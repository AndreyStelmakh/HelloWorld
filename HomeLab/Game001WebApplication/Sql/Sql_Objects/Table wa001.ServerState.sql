USE [wa001db]
GO

/****** Object: Table [wa001].[ServerState] Script Date: 11/21/2015 8:39:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [wa001].[ServerState] (
    [ID]                 INT           NOT NULL,
    [CurrentRoundNumber] INT           NOT NULL,
    [GameState]          NVARCHAR (50) NOT NULL
);


