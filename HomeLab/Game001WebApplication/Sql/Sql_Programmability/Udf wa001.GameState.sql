USE [wa001db]
GO

/****** Object: Scalar Function [wa001].[GameState] Script Date: 11/21/2015 8:31:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP FUNCTION [wa001].[GameState];


GO
CREATE FUNCTION [wa001].[GameState]
( )
RETURNS nvarchar(50)
AS
BEGIN

	declare @result nvarchar(50);

	select @result =  GameState from wa001.ServerState;

	return @result;

END
