# OGC Environment Data Retrieval API 

This is an implementation of the OGC Environmental Data Retrieval API developed by Cefas as part of our existing Master Data Register software.

This repository is the subset of code required to implement the OGC functionality into our systems. This is an extract of a much larger project, the Cefas Master Data Register.

The MDR catalogues all of our data holdings internally and also makes some of our data available to the public via our data portal https://data.cefas.co.uk.  

The public facing API of the data portal, which contains the OGC functionality is available on https://data-api.cefas.co.uk/

The repo contains only the OGC endpoints of that API. Running the project in this repo will result in the main API definition page being empty.

The repo is fully functional and will build and run against a suitable database. It has been tested against the main MDR database, but should work on a standalone database as long as the tables are defined.

## Database support

The MDR is designed to run on Azure cloud hosting using an Azure SQL Database as its main data repository. Data can be stored in the main database, or other databases as long as connections are provided. It supports Microsoft SQL Server and Azure SQL. 

The codebase also contains references to PostGIS database support. However, this feature was never fully implemented or tested. Postgres SQL differs from the Microsoft implementation in how it handles field names and case sensitivity. Therefore, this code may not work with Postgres or any other SQL variants.

Connection strings are stored in an Azure Key Vault.  When running in Visual Studio a secrets file can be used instead. The structure of the secrets is:

```
{
  "AzureAd": {
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    "Scope": ""
  },
  "Seq": {
    "ApiKey": "",
    "ServerUrl": ""
  },
  "ConnectionStrings": {
 
    "MDRDB": "Server=XXXXX,1433;Initial Catalog=XXXXX;Persist Security Info=False;User ID=XXXXX;Password=XXX;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "BlobStorage": "DefaultEndpointsProtocol=https;AccountName=XXXX;AccountKey=XXXX;EndpointSuffix=core.windows.net",    
  },
  "AppOptions": {
    "AdminGroup": "Admin Group",
    "AdminRole": "Admin",
    "ExportHashSalt": "",
    "SMTPServer": "localhost",
    "ExternalSchema": "DataHub",
    "UiUri": "http://localhost:4200"
  }
}
```

### Table structure

The MDR database is structured around data holdings with individual recordsets containing data. The OGC works directly with the recordsets. Each recordset consists of a definition in the Recordsets.Recordsets table with a list of fields in the Recordsets.Fields table. These point to a table in a database (defined in Recordsets.Locations containing the data including the geometry.

The following tables are required to run this project. Note: The following SQL script has not been tested and the order of the table creation may be incorrect. Only tables relevant to this repo have been included, this is not the full database definition for the MDR.

```
CREATE TABLE [MDR].[Keywords](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[Vocabulary] [nvarchar](250) NULL,
	[DisplayName] [nvarchar](max) NULL,
	[VocabularyCode] [nvarchar](max) NULL,
	[ExportName] [nvarchar](max) NULL,
	[ExportCode] [nvarchar](max) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Order] [int] NOT NULL,
 CONSTRAINT [PK_Keywords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [MDR].[Keywords] ADD  CONSTRAINT [DF_Keywords_Order]  DEFAULT ((1)) FOR [Order]
GO

ALTER TABLE [MDR].[Keywords] CHECK CONSTRAINT [FK_Keyword_ToVocabulary]
GO

CREATE TABLE [Ogc].[Locations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NULL,
	[Geometry] [geometry] NULL,
 CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [Recordsets].[Fields](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecordsetId] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Type] [nvarchar](50) NOT NULL,
	[ColumnName] [nvarchar](max) NULL,
	[Sequence] [int] NULL,
	[Units] [nvarchar](max) NULL,
	[Vocabulary] [nvarchar](50) NULL,
	[MinValue] [decimal](28, 14) NULL,
	[MaxValue] [decimal](28, 14) NULL,
	[Pattern] [nvarchar](max) NULL,
	[Locked] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Recordsets].[Fields] ADD  CONSTRAINT [DF_Fields_Locked]  DEFAULT ((0)) FOR [Locked]
GO

CREATE TABLE [Recordsets].[Licences](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Href] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Recordsets.Licences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [Recordsets].[Locations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[AcceptsTables] [bit] NOT NULL,
	[AcceptsBlobs] [bit] NOT NULL,
	[IsExternal] [bit] NOT NULL,
	[IsThisDatabase] [bit] NOT NULL,
	[Details] [nvarchar](max) NOT NULL,
	[ReadOnly] [bit] NOT NULL,
	[TableNamePrefix] [nvarchar](max) NOT NULL,
	[Schema] [nvarchar](max) NOT NULL,
	[Georeferencable] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Recordsets].[Locations] ADD  DEFAULT ((0)) FOR [IsThisDatabase]
GO

ALTER TABLE [Recordsets].[Locations] ADD  DEFAULT ((0)) FOR [Georeferencable]
GO

CREATE TABLE [Recordsets].[Recordsets](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Location] [int] NOT NULL,
	[HoldingId] [int] NOT NULL,
	[Tabular] [bit] NOT NULL,
	[SourceRecordset] [int] NULL,
	[SourceVersion] [int] NULL,
	[CreationStep] [nvarchar](max) NULL,
	[KeyField] [nvarchar](max) NULL,
	[QuoteCharacter] [nvarchar](1) NULL,
	[NullString] [nvarchar](max) NULL,
	[External] [bit] NOT NULL,
	[SRID] [int] NULL,
	[Versioned] [bit] NOT NULL,
	[TableName] [nvarchar](128) NULL,
	[Priority] [int] NOT NULL,
	[Licence] [int] NOT NULL,
	[Mode] [int] NOT NULL,
	[Draft] [bit] NOT NULL,
	[Hidden] [bit] NOT NULL,
	[DeletedVersion] [int] NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[PublishToOgcEdr] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Recordsets].[Recordsets] ADD  CONSTRAINT [DF_Recordsets_Priority]  DEFAULT ((0)) FOR [Priority]
GO

ALTER TABLE [Recordsets].[Recordsets] ADD  CONSTRAINT [DF_Recordsets_Licence]  DEFAULT ((1)) FOR [Licence]
GO

ALTER TABLE [Recordsets].[Recordsets] ADD  CONSTRAINT [DF_Recordsets_Mode]  DEFAULT ((0)) FOR [Mode]
GO

ALTER TABLE [Recordsets].[Recordsets] ADD  CONSTRAINT [DF_Recordsets_Draft]  DEFAULT ((0)) FOR [Draft]
GO

ALTER TABLE [Recordsets].[Recordsets] ADD  CONSTRAINT [DF_Recordsets_Hidden]  DEFAULT ((0)) FOR [Hidden]
GO

ALTER TABLE [Recordsets].[Recordsets] ADD  CONSTRAINT [DF_Recordsets_PublishToOgcEdr]  DEFAULT ((0)) FOR [PublishToOgcEdr]
GO

ALTER TABLE [Recordsets].[Recordsets]  WITH NOCHECK ADD  CONSTRAINT [FK_Recordsets_ToLocations] FOREIGN KEY([Location])
REFERENCES [Recordsets].[Locations] ([Id])
GO

ALTER TABLE [Recordsets].[Recordsets] CHECK CONSTRAINT [FK_Recordsets_ToLocations]
GO

CREATE TABLE [Recordsets].[RecordsetFilters](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecordsetId] [int] NOT NULL,
	[FieldId] [int] NOT NULL,
	[FinalFieldId] [int] NULL,
	[FilterType] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Recordsets].[RecordsetFilters]  WITH NOCHECK ADD  CONSTRAINT [FK_RecordsetFilterFieldId_ToField] FOREIGN KEY([FieldId])
REFERENCES [Recordsets].[Fields] ([Id])
GO

ALTER TABLE [Recordsets].[RecordsetFilters] CHECK CONSTRAINT [FK_RecordsetFilterFieldId_ToField]
GO

ALTER TABLE [Recordsets].[RecordsetFilters]  WITH NOCHECK ADD  CONSTRAINT [FK_RecordsetFilterRecordset_ToRecordset] FOREIGN KEY([RecordsetId])
REFERENCES [Recordsets].[Recordsets] ([Id])
GO

ALTER TABLE [Recordsets].[RecordsetFilters] CHECK CONSTRAINT [FK_RecordsetFilterRecordset_ToRecordset]
GO

CREATE TABLE [Recordsets].[Versions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecordsetId] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[IsReady] [bit] NOT NULL,
	[RecordsAdded] [int] NOT NULL,
	[RecordsRemoved] [int] NOT NULL,
	[ActiveRecords] [int] NOT NULL,
	[TotalRecords] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Recordsets].[Versions]  WITH NOCHECK ADD  CONSTRAINT [FK_Versions_ToRecordset] FOREIGN KEY([RecordsetId])
REFERENCES [Recordsets].[Recordsets] ([Id])
GO

ALTER TABLE [Recordsets].[Versions] CHECK CONSTRAINT [FK_Versions_ToRecordset]
GO

CREATE TABLE [Service].[ActionLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Method] [nvarchar](10) NOT NULL,
	[Url] [nvarchar](max) NOT NULL,
	[IpAddress] [nvarchar](100) NOT NULL,
	[Date] [datetime] NOT NULL,
	[StatusCode] [int] NOT NULL,
	[Internal] [bit] NOT NULL,
	[ObjectId] [int] NULL,
	[ObjectType] [nvarchar](50) NULL,
	[ObjectVerb] [nvarchar](50) NULL,
	[ResponseTime] [float] NULL,
	[AdditionalParam] [nvarchar](max) NULL,
	[UserAgent] [nvarchar](max) NULL,
	[Origin] [nvarchar](max) NULL,
	[Referer] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [Recordsets].[FilterTypes](
	[ShortName] [nvarchar](50) NOT NULL,
	[LongName] [nvarchar](max) NULL,
	[Operands] [int] NULL,
	[OperandType] [nvarchar](50) NULL,
	[SQL] [nvarchar](max) NULL,
	[CQL] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ShortName] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE VIEW [Shared].[FilterList]
WITH SCHEMABINDING
	AS 
	SELECT ff.FieldType, ft.LongName, ft.ShortName, ft.Operands, ft.OperandType 
	from Recordsets.FieldFilterTypes as ff inner join Recordsets.FilterTypes as ft on ff.FilterType = ft.ShortName
GO
```

## Licences

This software is released under the Open Government Licence version 3. 
[Open Government Licence (nationalarchives.gov.uk)](https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/)

Portions of this code are licenced under the Apache License, Version 2.0.
http://www.apache.org/licenses/LICENSE-2.0

This software depends on various libraries that are under open source licences. Please refer to the licences of the individual libraries available via Nuget.


