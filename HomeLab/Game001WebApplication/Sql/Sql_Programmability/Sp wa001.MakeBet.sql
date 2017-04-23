USE [wa001db]
GO

/****** Object: SqlProcedure [wa001].[MakeBet] Script Date: 11/21/2015 8:34:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE [wa001].[MakeBet];


GO
CREATE PROCEDURE [wa001].[MakeBet]
	@login nvarchar(50),
	@pin nvarchar(20),
	@xml xml
AS
	--declare @xml xml;
	--set @xml = '<order>
	--<object id="1" volume="12.12" price="14.12" />
	--<object id="2" volume="12.12" price="14.12" />
	--</order>';

	declare @t_orders table(objectID int, volume decimal(18,4), price decimal(18,4));

	INSERT INTO @t_orders
	SELECT
		[order].value('@id[1]', 'int'),
		[order].value('@volume[1]', 'decimal(18,4)'),
		[order].value('@price[1]', 'decimal(18,4)')

	FROM @xml.nodes('order/object') T([order]);

	declare @ownerID int;

	select @ownerID = ID from wa001.Owners where [Login] = @login and Pin = @pin;

	if @ownerID is null
	begin
		raiserror('Не удалось определить пользователя по указанным логину и пин-коду.', 16, 1);

		return;

	end;


	if exists(	select 1
				from @t_orders R
				left join wa001.Assets S on R.ObjectID = S.ID
				where S.OwnerID is null or S.OwnerID <> @ownerID
			 )
	begin
		raiserror('Один из объектов не найден или игроку не принадлежит', 16, 1);

		return;

	end;

	return 0;
