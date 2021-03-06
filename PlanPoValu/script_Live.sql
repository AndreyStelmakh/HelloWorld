USE [test_Live]
GO
/****** Object:  Table [dbo].[Generations_Content]    Script Date: 09/21/2011 14:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Generations_Content](
	[x] [int] NOT NULL,
	[y] [int] NOT NULL,
	[generation] [int] NOT NULL,
 CONSTRAINT [IX_Generations_Content] UNIQUE NONCLUSTERED 
(
	[generation] ASC,
	[x] ASC,
	[y] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmptyMatrix_WholeField]    Script Date: 09/21/2011 14:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmptyMatrix_WholeField](
	[x] [int] NOT NULL,
	[y] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmptyMatrix_Neighbors]    Script Date: 09/21/2011 14:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmptyMatrix_Neighbors](
	[x] [int] NOT NULL,
	[y] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[udp_InitializeMatrix]    Script Date: 09/21/2011 14:19:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[udp_InitializeMatrix]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @t table (n int);

	INSERT INTO @t (n) values (0);
	insert into @t (n) values (1);
	insert into @t (n) values (2);
	insert into @t (n) values (3);
	insert into @t (n) values (4);
	insert into @t (n) values (5);
	insert into @t (n) values (6);
	insert into @t (n) values (7);
	insert into @t (n) values (8);
	insert into @t (n) values (9);

	DELETE FROM
		dbo.EmptyMatrix_WholeField;

	INSERT INTO
		dbo.EmptyMatrix_WholeField
		( x, y)
	SELECT
		T1.n
		,T2.n
	FROM
		@t T1
	CROSS JOIN
		@t T2;


	DELETE FROM
		@t;

	INSERT INTO @t (n) values (-1);
	INSERT INTO @t (n) values (0);
	INSERT INTO @t (n) values (1);

	DELETE FROM
		dbo.EmptyMatrix_Neighbors;

	INSERT INTO
		dbo.EmptyMatrix_Neighbors
		( x, y)
	SELECT
		T1.n
		,T2.n
	FROM
		@t T1
	CROSS JOIN
		@t T2
	WHERE
		NOT (T1.n = 0 AND T2.n = 0);	-- центральная ячейка в матрицу не входит


END
GO
/****** Object:  UserDefinedFunction [dbo].[udf_IsCellExistsAt]    Script Date: 09/21/2011 14:19:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[udf_IsCellExistsAt]
(
	 @x int
	,@y int
	,@generation int
)
RETURNS int
AS
BEGIN

	if  exists
		(
			SELECT
				*
			FROM
				dbo.Generations_Content
			WHERE
				x = @x
				AND y = @y
				AND generation = @generation
		)
	begin
		return 1;
		
	end;

	return 0;		

END
GO
/****** Object:  UserDefinedFunction [dbo].[udf_CountCellNeighbors]    Script Date: 09/21/2011 14:19:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	Возвращает количество соседей указанной ячейки
-- =============================================
CREATE FUNCTION [dbo].[udf_CountCellNeighbors]
(
	-- Add the parameters for the function here
	 @x int
	,@y int
	,@generation int
)
RETURNS int
AS
BEGIN

	declare
		@neighbor_counter int;

	SELECT
		@neighbor_counter = COUNT(*)
	FROM
		dbo.EmptyMatrix_Neighbors M
	INNER JOIN
	(
		SELECT
			 x
			,y
		FROM
			dbo.Generations_Content
		WHERE
			generation = @generation
	) G
	ON
		M.x + @x = G.x
		AND M.y + @y = G.y
;

	RETURN @neighbor_counter;

END
GO
/****** Object:  StoredProcedure [dbo].[udp_ProduceGeneration]    Script Date: 09/21/2011 14:19:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[udp_ProduceGeneration]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare
		@last_generation int;

	SELECT
		@last_generation = MAX(generation)
	FROM
		dbo.Generations_Content;


	-- state: 0 пусто, 1 клетка, 2 умирает, 3 рождается
	declare
		@result table(x int, y int, state int);

	INSERT INTO
		@result
	-- вычисляю что должно произойти с последним поколением
	SELECT
		 x
		,y
		,CASE
			WHEN bCellPresent = 0 AND iNeighborCount = 3 THEN 3
			WHEN bCellPresent = 1 AND (iNeighborCount < 2 OR iNeighborCount > 3) THEN 2
			ELSE bCellPresent
		 END
	FROM
	(
		-- накладываю на пустое "игровое" поле статистику по соседям и состояниям клеток в указанном поколении
		SELECT
			 M.x
			,M.y
			,dbo.udf_CountCellNeighbors(M.x, M.y, @last_generation) as iNeighborCount
			,dbo.udf_IsCellExistsAt(M.x, M.y, @last_generation)			as bCellPresent
		FROM
			dbo.EmptyMatrix_WholeField M
		LEFT JOIN
			dbo.Generations_Content G
		ON
			M.x = G.x AND M.y = G.y AND G.generation = @last_generation
	) T;

	-- сохраняю результат в базу (новорожденные и умершие превращаются в живые и пустые, соответственно)
	INSERT INTO
		dbo.Generations_Content
		(
			 x
			,y
			,generation
		)
	SELECT
		 x
		,y
		,@last_generation + 1
	FROM
		@result	
	WHERE
		state = 1
		OR state = 3;
		
	-- возвращаю (на клиент) полные данные (с новорожденными и умершими)
	SELECT
		*
	FROM
		@result;

END
GO
