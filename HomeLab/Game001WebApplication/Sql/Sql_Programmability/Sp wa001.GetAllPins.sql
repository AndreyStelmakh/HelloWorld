USE [wa001db]
GO

/****** Object: SqlProcedure [wa001].[GetAllPins] Script Date: 11/21/2015 8:35:25 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE [wa001].[GetAllLogins];


GO
CREATE PROCEDURE [wa001].[GetAllLogins]

AS
	SELECT [Login],
		   [Pin]
	FROM   [Owners]
	ORDER BY [Login];

	declare @count int;

	SELECT @count = COUNT(*)
	FROM [Owners];

RETURN @count;
