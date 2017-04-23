USE [wa001db]
GO

/****** Object: SqlProcedure [wa001].[UnlockOrders] Script Date: 11/21/2015 8:33:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE [wa001].[UnlockOrders];


GO
CREATE PROCEDURE [wa001].[UnlockOrders]

AS
	update wa001.ServerState
	set GameState = 'ПриемЗаявок';

	return 0;
