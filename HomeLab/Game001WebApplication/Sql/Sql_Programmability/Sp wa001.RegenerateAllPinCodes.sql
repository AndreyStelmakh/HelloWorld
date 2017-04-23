USE [wa001db]
GO

/****** Object: SqlProcedure [wa001].[RegenerateAllPinCodes] Script Date: 11/21/2015 8:33:17 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE [wa001].[RegenerateAllPinCodes];


GO
CREATE PROCEDURE [wa001].[RegenerateAllPinCodes]
AS

	declare @pinCodeLen int;
	set @pinCodeLen = 4;

begin tran
begin try

	UPDATE [wa001].Owners
	SET Pin = NULL;

	while exists(select 1 from [wa001].Owners where Pin is null)
	begin

		declare @newPin nvarchar(20);
		set @newPin = left(cast(cast(rand()*10000 as int) as nvarchar(max)), @pinCodeLen);

		if len(@newPin) = @pinCodeLen
		begin
			if not exists(select 1 from wa001.Owners where Pin = @newPin)
			begin

				declare @currentOwnerId int;
				select top 1 @currentOwnerId = N.Id from wa001.Owners N where [Pin] is null;

				update [wa001].Owners
				set Pin = @newPin
				where Id = @currentOwnerId;

			end;

		end;

	end;

	commit;

end try
begin catch

	if @@TRANCOUNT > 0 rollback;

end catch;

RETURN 0
