USE [wa001db]
GO

/****** Object: SqlProcedure [wa001].[PublishResults] Script Date: 11/21/2015 8:33:54 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE [wa001].[PublishResults];


GO

CREATE PROCEDURE [wa001].[PublishResults]
	@Xml xml
AS

	--if wa001.GameState() = 'ПриемЗаявокЗавершен'
	--begin
	--	raiserror('Перед публикацией требуется перевести игру в состояние "ПриемЗаявокЗавершен", вызвав про.', 16, 1);
	--
	--	return;
	--
	--end;

	set @Xml = '<?xml version="1.0" encoding="utf-8"?>
<results>
   <object br_price="97.5404" br_volume="157.6131" object_id="1" rsv_price="814.7237" rsv_volume="141.8863"/>
   <object br_price="278.4982" br_volume="970.5928" object_id="2" rsv_price="905.7919" rsv_volume="421.7613"/>
   <object br_price="546.8815" br_volume="957.1669" object_id="3" rsv_price="126.9868" rsv_volume="915.7355"/>
   <object br_price="957.5068" br_volume="485.3756" object_id="4" rsv_price="913.3759" rsv_volume="792.2073"/>
   <object br_price="964.8885" br_volume="800.2805" object_id="5" rsv_price="632.3592" rsv_volume="959.4924"/>
</results>';

	declare @currentRoundNumber int;
	set @currentRoundNumber = wa001.CurrentRoundNumber();

	delete from wa001.Results where RoundID = @currentRoundNumber;

	INSERT INTO wa001.Results (RoundID, AssetID, RsvVolume, RsvPrice, BrVolume, BrPrice)
	SELECT	@currentRoundNumber,
			T.x.value('@object_id', 'int'),
			T.x.value('@rsv_volume', 'decimal(18,4)'),
			T.x.value('@rsv_price', 'decimal(18,4)'),
			T.x.value('@br_volume', 'decimal(18,4)'),
			T.x.value('@br_price', 'decimal(18,4)')

	FROM @Xml.nodes('results/object') T(x);


	RETURN 0
