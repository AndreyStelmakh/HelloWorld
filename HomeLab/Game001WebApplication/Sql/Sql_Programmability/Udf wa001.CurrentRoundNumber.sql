USE [wa001db]
GO

/****** Object: Scalar Function [wa001].[CurrentRoundNumber] Script Date: 11/21/2015 8:31:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP FUNCTION [wa001].[CurrentRoundNumber];


GO


CREATE FUNCTION [wa001].[CurrentRoundNumber]
( )
RETURNS int
AS
BEGIN

	declare @result int;

	select @result = CurrentRoundNumber from wa001.ServerState;

	return @result;

END
