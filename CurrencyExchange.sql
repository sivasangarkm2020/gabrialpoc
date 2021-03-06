USE [CurrencyExchange]
GO
/****** Object:  Table [dbo].[BestRate]    Script Date: 09-12-2020 20:48:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BestRate](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[Rate] [numeric](25, 15) NOT NULL,
	[DataSourceId] [tinyint] NOT NULL,
	[Date] [date] NOT NULL,
	[CurrencyId] [smallint] NOT NULL,
 CONSTRAINT [PK_BestRate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Currency]    Script Date: 09-12-2020 20:48:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Currency](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Currency] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataSource]    Script Date: 09-12-2020 20:48:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource](
	[Id] [tinyint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Url] [nchar](1000) NOT NULL,
	[IsNew] [bit] NOT NULL,
	[DataElementName] [nchar](20) NOT NULL,
 CONSTRAINT [PK_DataSource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExchangeRate]    Script Date: 09-12-2020 20:48:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExchangeRate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DataSourceId] [tinyint] NOT NULL,
	[CurrencyId] [smallint] NOT NULL,
	[Rate] [numeric](25, 15) NOT NULL,
	[Date] [date] NOT NULL,
 CONSTRAINT [PK_ExchangeRate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DataSource] ADD  CONSTRAINT [DF_DataSource_DataElementName]  DEFAULT ((1)) FOR [DataElementName]
GO
/****** Object:  StoredProcedure [dbo].[GetBestRate]    Script Date: 09-12-2020 20:48:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[GetBestRate]
as
begin
select ds.Name as datasource, c.Code as symbol,br.Rate as rate from BestRate br 
left join currency c on  br.CurrencyId = c.Id
left join datasource ds on br.DataSourceId = ds.Id;
end
GO
INSERT INTO [dbo].[DataSource]
           ([Name]
           ,[Url]
           ,[IsNew]
           ,[DataElementName])
     VALUES
           ('apilayer.net'
           ,'http://www.apilayer.net/api/live?access_key=6653c3ea32932753a5c4a956ddc7de27'
           ,1
           ,'quotes')

INSERT INTO [dbo].[DataSource]
           ([Name]
           ,[Url]
           ,[IsNew]
           ,[DataElementName])
     VALUES
           ('openexchangerates.org'
		   ,'https://openexchangerates.org/api/latest.json?app_id=1de86dfd996b4c9da20c0b3fa6eefaa4&base=USD'
           ,1
           ,'rates')
