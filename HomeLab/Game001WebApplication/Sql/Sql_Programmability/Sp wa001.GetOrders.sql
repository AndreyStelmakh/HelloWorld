USE [wa001db]
GO

/****** Object: SqlProcedure [wa001].[GetOrders] Script Date: 11/21/2015 8:35:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE [wa001].[GetOrders];


GO
CREATE PROCEDURE [wa001].[GetOrders]
	@RoundID int = NULL OUTPUT
AS

	if @RoundID is null
	begin
		SELECT @RoundID = MAX(RoundID)
		FROM [wa001].Orders;

	end;

	SELECT R.AssetID, A.SomeConstant, A.[Type] as AssetType,  Price as OrderPrice, Volume as OrderVolume
	FROM [wa001].Orders R
	LEFT JOIN [wa001].Assets A ON R.AssetID = A.Id
	WHERE RoundID = @RoundID;


	RETURN 0
