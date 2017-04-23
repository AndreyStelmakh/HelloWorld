USE [wa001db]
GO

/****** Object: SqlProcedure [wa001].[LockOrders] Script Date: 11/21/2015 8:34:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE [wa001].[LockOrders];


GO
CREATE PROCEDURE [wa001].[LockOrders]

AS
	update wa001.ServerState
	set GameState = 'ПриемЗаявокЗавершен';

	return 0;
