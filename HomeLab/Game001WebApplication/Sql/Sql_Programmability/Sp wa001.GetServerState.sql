USE [wa001db]
GO

/****** Object: SqlProcedure [wa001].[GetServerState] Script Date: 11/21/2015 9:42:04 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE [wa001].[GetServerState];


GO
CREATE PROCEDURE wa001.[GetServerState]
AS
	SELECT * FROM wa001.ServerState;

RETURN 0
