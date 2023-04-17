/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

/*
This scrip will drop and recreate a test table used to validate all the DAL methods that are supported.

A few helper Commands

drop table [dbo].[testtable]
drop procedure [dbo].[SelectAllData]
truncate table testtable

select * from testtable
exec [dbo].[SelectDataById] '1'
*/

-- create db if it doesnt exist
IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'toolsDb')
CREATE DATABASE [toolsDb]
GO

-- point at correct db
USE [ToolsDB]
GO

-- drop and re-create the table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TestTable]') AND type in (N'U'))
drop table [dbo].[testtable]

CREATE TABLE [dbo].[TestTable](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[bigintTest] [bigint] NOT NULL,
	[bigintTestNull] [bigint] NULL,
	[binaryTest] [binary](1) NOT NULL,
	[binaryTestNull] [binary](1) NULL,
	[bitTest] [bit] NOT NULL,
	[bitTestNull] [bit] NULL,
	[charTest] [char](26) NOT NULL,
	[charTestNull] [char](26) NULL,
	[dateTest] [date] NOT NULL,
	[dateTestNull] [date] NULL,
	[datetimeTest] [datetime] NOT NULL,
	[datetimeTestNull] [datetime] NULL,
	[datetime2Test] [datetime2](7) NOT NULL,
	[datetime2TestNull] [datetime2](7) NULL,
	[datetimeoffsetTest] [datetimeoffset](7) NOT NULL,
	[datetimeoffsetTestNull] [datetimeoffset](7) NULL,
	[decimalTest] [decimal](18, 0) NOT NULL,
	[decimalTestNull] [decimal](18, 0) NULL,
	[floatTest] [float] NOT NULL,
	[floatTestNull] [float] NULL,
	[imageTest] [image] NOT NULL,
	[imageTestNull] [image] NULL,
	[intTest] [int] NOT NULL,
	[intTestNull] [int] NULL,
	--[geographyTest] [geography] NOT NULL,
	--[geographyTestNull] [geography] NULL,
	--[geometryTest] [geometry] NOT NULL,
	---[geometryTestNull] [geometry] NULL,
	--[heiarchyIdTest] [hierarchyid] NOT NULL,
	--[heiarchyIdTestNull] [hierarchyid] NULL,
	[moneyTest] [money] NOT NULL,
	[moneyTestNull] [money] NULL,
	[ncharTest] [nchar](8) NOT NULL,
	[ncharTestNull] [nchar](8) NULL,
	[ntextTest] [ntext] NOT NULL,
	[ntextTestNull] [ntext] NULL,
	[numericTest] [numeric](18, 0) NOT NULL,
	[numericTestNull] [numeric](18, 0) NULL,
	[nvarcharTest] [nvarchar](2) NOT NULL,
	[nvarcharTestNull] [nvarchar](2) NULL,
	[nvarcharMAXTest] [nvarchar](max) NOT NULL,
	[nvarcharMAXTestNull] [nvarchar](max) NULL,
	[realTest] [real] NOT NULL,
	[realTestNull] [real] NULL,
	[smalldatetimeTest] [smalldatetime] NOT NULL,
	[smalldatetimeTestNull] [smalldatetime] NULL,
	[smallintTest] [smallint] NOT NULL,
	[smallintTestNull] [smallint] NULL,
	[smallmoneyTest] [smallmoney] NOT NULL,
	[smallmoneyTestNull] [smallmoney] NULL,
	[sql_variantTest] [sql_variant] NOT NULL,
	[sql_variantTestNull] [sql_variant] NULL,
	[textTest] [text] NOT NULL,
	[textTestNull] [text] NULL,
	[timeTest] [time](7) NOT NULL,
	[timeTestNull] [time](7) NULL,
	[tinyintTest] [tinyint] NOT NULL,
	[tinyintTestNull] [tinyint] NULL,
	--[uniqueidentifierTest] [uniqueidentifier] NOT NULL,
	--[uniqueidentifierTestNull] [uniqueidentifier] NULL,
	[varbinaryTest] [varbinary](1) NOT NULL,
	[varbinaryTestNull] [varbinary](1) NULL,
	[varbinaryMAXTest] [varbinary](max) NOT NULL,
	[varbinaryMAXTestNull] [varbinary](max) NULL,
	[varcharTest] [varchar](26) NOT NULL,
	[varcharTestNull] [varchar](26) NULL,
	[varcharMAXTest] [varchar](max) NOT NULL,
	[varcharMAXTestNull] [varchar](max) NULL,
	[xmlTest] [xml] NOT NULL,
	[xmlTestNull] [xml] NULL,
	[timestampTest] [timestamp] NOT NULL,
 CONSTRAINT [PK_Test] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

--------------------------------------------------------------------------------------------------------------------------------

-- add default values. some values are commented out because we cannot support them in code yet.
ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_bigintTest]  DEFAULT ((9223372036854775807)) FOR [bigintTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_binaryTest]  DEFAULT (0xFF) FOR [binaryTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_bitTest]  DEFAULT ((1)) FOR [bitTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_charTest]  DEFAULT ('abcdefghijklmnopqrstuvwxyz') FOR [charTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_dateTest]  DEFAULT ('9999-12-31Z') FOR [dateTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_datetimeTest]  DEFAULT ('9999-12-31T23:59:59.997') FOR [datetimeTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_datetime2Test]  DEFAULT ('9999-12-31T23:59:59.997') FOR [datetime2Test]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_datetimeoffsetTest]  DEFAULT ('9999-12-31T23:59:59.997Z') FOR [datetimeoffsetTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_decimalTest]  DEFAULT ((1234567890)) FOR [decimalTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_floatTest]  DEFAULT ((1234567890.12345)) FOR [floatTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_imageTest]  DEFAULT (0xFF) FOR [imageTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_intTest]  DEFAULT ((2147483647)) FOR [intTest]
GO

--ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_geographyTest]  DEFAULT ([GEOGRAPHY]::Point((47.6062),(122.3321),(4326))) FOR [geographyTest]
--GO

--ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_geometryTest]  DEFAULT ([geometry]::STPointFromText('POINT (100 100)',(0))) FOR [geometryTest]
--GO

--ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_heiarchyTest]  DEFAULT ('/1/') FOR [heiarchyIdTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_moneyTest]  DEFAULT ((922337203685477.5807)) FOR [moneyTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_ncharTest]  DEFAULT (N'你好') FOR [ncharTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_ntextTest]  DEFAULT (N'你好') FOR [ntextTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_numericTest]  DEFAULT ((1)) FOR [numericTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_nvarcharTest]  DEFAULT (N'你好') FOR [nvarcharTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_nvarcharMAXTest]  DEFAULT (N'你好') FOR [nvarcharMAXTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_realTest]  DEFAULT ((1234567890.123456789)) FOR [realTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_smalldatetimeTest]  DEFAULT (getutcdate()) FOR [smalldatetimeTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_smallintTest]  DEFAULT ((32767)) FOR [smallintTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_smallmoneyTest]  DEFAULT ((214748.3647)) FOR [smallmoneyTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_sql_variantTest]  DEFAULT (0xFF) FOR [sql_variantTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_textTest]  DEFAULT ('The quick brown fox Jumped over the lazy dog') FOR [textTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_timeTest]  DEFAULT (getutcdate()) FOR [timeTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_tinyintTest]  DEFAULT ((255)) FOR [tinyintTest]
GO

--ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_uniqueidentifierTest]  DEFAULT (newid()) FOR [uniqueidentifierTest]
--GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_varbinaryTest]  DEFAULT (0xFF) FOR [varbinaryTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_varbinaryMAXTest]  DEFAULT (0xFF) FOR [varbinaryMAXTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_varcharTest]  DEFAULT ('abcdefghijklmnopqrstuvwxyz') FOR [varcharTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_varcharMAXTest]  DEFAULT ('abcdefghijklmnopqrstuvwxyz') FOR [varcharMAXTest]
GO

ALTER TABLE [dbo].[TestTable] ADD  CONSTRAINT [DF_TestTable_xmlTest]  DEFAULT (0x3C3F786D6C2076657273696F6E3D22312E302220656E636F64696E673D225554462D38223F3E20200D0A3C526F6F743E20200D0A093C50726F647563744465736372697074696F6E2050726F647563744D6F64656C49443D2235223E20200D0A09093C53756D6D6172793E536F6D6520546578743C2F53756D6D6172793E20200D0A093C2F50726F647563744465736372697074696F6E3E20200D0A3C2F526F6F743E2020) FOR [xmlTest]
GO

--------------------------------------------------------------------------------------------------------------------------------

-- put the default test data in the table
truncate table testtable
go

insert [testtable] ([biginttest]) values (9223372036854775807)
insert [testtable] ([biginttest]) values (9223372036854775807)
insert [testtable] ([biginttest]) values (9223372036854775807)

--------------------------------------------------------------------------------------------------------------------------------

-- drop and recreate a SP
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID(N'[dbo].[SelectDataById]'))
DROP PROCEDURE [dbo].[SelectDataById]
GO

/*
	[dbo].[SelectBigint]
	exec [dbo].[SelectDataById] '1'

*/
CREATE PROCEDURE [dbo].[SelectDataById]
(
	@Id INT
)
AS

SELECT		* 
FROM		TestTable
WHERE		Id = @Id
ORDER BY	ID
GO


