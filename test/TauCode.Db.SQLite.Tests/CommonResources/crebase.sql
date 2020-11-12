/*

DROP TABLE [NumericData]

DROP TABLE [DateData]

DROP TABLE [HealthInfo]

DROP TABLE [TaxInfo]

DROP TABLE [WorkInfo]

DROP TABLE [PersonData]

DROP TABLE [Person]

GO

*/

/*** Person ***/
CREATE TABLE [Person](
	[MetaKey] integer NOT NULL,
	[OrdNumber] integer NOT NULL,
	[Id] integer NOT NULL,
	[FirstName] text NOT NULL,
	[LastName] text NOT NULL,
	[Birthday] datetime NOT NULL,
	[Gender] integer NULL,
	[Initials] text NULL,
	CONSTRAINT [PK_person] PRIMARY KEY([Id], [MetaKey], [OrdNumber])
)

/*** PersonData ***/
CREATE TABLE [PersonData](
	[Id] uniqueidentifier NOT NULL,
	[Height] integer NULL,
	[Photo] blob NULL,
	[EnglishDescription] text NOT NULL,
	[UnicodeDescription] text NOT NULL,
	[PersonMetaKey] integer NOT NULL,
	[PersonOrdNumber] integer NOT NULL,
	[PersonId] integer NOT NULL,
	CONSTRAINT [PK_personData] PRIMARY KEY ([Id]),
	CONSTRAINT [FK_personData_Person] FOREIGN KEY([PersonId], [PersonMetaKey], [PersonOrdNumber]) REFERENCES [Person]([Id], [MetaKey], [OrdNumber])
)

/*** WorkInfo ***/
CREATE TABLE [WorkInfo](
	[Id] uniqueidentifier NOT NULL,
	[Position] text NOT NULL,
	[HireDate] datetime NOT NULL,
	[Code] text NULL,
	[PersonMetaKey] integer NOT NULL,
	[DigitalSignature] blob NOT NULL,
	[PersonId] integer NOT NULL,
	[PersonOrdNumber] integer NOT NULL,
	[Hash] uniqueidentifier NOT NULL,
	[Salary] numeric NULL,
	[VaryingSignature] blob NULL,
	CONSTRAINT [PK_workInfo] PRIMARY KEY ([Id]),
	CONSTRAINT [FK_workInfo_Person] FOREIGN KEY([PersonId], [PersonMetaKey], [PersonOrdNumber]) REFERENCES [Person]([Id], [MetaKey], [OrdNumber])
)

/*** WorkInfo - index on [Hash] ***/
CREATE UNIQUE INDEX [UX_workInfo_Hash] ON [WorkInfo]([Hash])

/*** TaxInfo ***/
CREATE TABLE [TaxInfo](
	[Id] uniqueidentifier NOT NULL,
	[PersonId] integer NOT NULL,
	[Tax] numeric NOT NULL,
	[Ratio] real NULL,
	[PersonMetaKey] integer NOT NULL,
	[SmallRatio] real NOT NULL,
	[RecordDate] datetime NULL,
	[CreatedAt] datetime NOT NULL,
	[PersonOrdNumber] integer NOT NULL,
	[DueDate] datetime NULL,
	CONSTRAINT [PK_taxInfo] PRIMARY KEY([Id]),
	CONSTRAINT [FK_taxInfo_Person] FOREIGN KEY([PersonId], [PersonMetaKey], [PersonOrdNumber]) REFERENCES [Person]([Id], [MetaKey], [OrdNumber]))

/*** HealthInfo ***/
CREATE TABLE [HealthInfo](
	[Id] uniqueidentifier NOT NULL,
	[PersonId] integer NOT NULL,
	[Weight] numeric NOT NULL,
	[PersonMetaKey] integer NOT NULL,
	[IQ] numeric NULL,
	[Temper] integer NULL,
	[PersonOrdNumber] integer NOT NULL,
	[MetricB] integer NULL,
	[MetricA] integer NULL,
	CONSTRAINT [PK_healthInfo] PRIMARY KEY ([Id]),
	CONSTRAINT [FK_healthInfo_Person] FOREIGN KEY([PersonId], [PersonMetaKey], [PersonOrdNumber]) REFERENCES [Person]([Id], [MetaKey], [OrdNumber])
)

/*** HealthInfo - index on [MetricA], [MetricB] ***/
CREATE INDEX [IX_healthInfo_metricAmetricB] ON [HealthInfo]([MetricA] ASC, [MetricB] DESC)

/*** NumericData ***/
CREATE TABLE [NumericData](
	[Id] integer PRIMARY KEY AUTOINCREMENT,
	[Int64] integer NULL,
	[NetDouble] real NULL,
	[NumericData] numeric NULL,
	[DecimalData] numeric NULL)

/*** DateData ***/
CREATE TABLE [DateData](
	[Id] uniqueidentifier NOT NULL,
	[Moment] datetime NULL,
	CONSTRAINT [PK_dateData] PRIMARY KEY ([Id])
)
