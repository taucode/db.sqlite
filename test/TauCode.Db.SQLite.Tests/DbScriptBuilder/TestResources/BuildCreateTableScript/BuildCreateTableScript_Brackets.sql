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