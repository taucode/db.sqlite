/* Table: 'Person' */
CREATE TABLE [Person](
    [Id] integer NOT NULL,
    [Tag] uniqueidentifier NULL,
    [IsChecked] integer NULL,
    [Birthday] datetime NULL,
    [FirstName] text NULL,
    [LastName] text NULL,
    [Initials] text NULL,
    [Gender] integer NULL,
    CONSTRAINT [PK_person] PRIMARY KEY([Id]))

/* Index: 'UX_person_tag' on table 'Person' */
CREATE UNIQUE INDEX [UX_person_tag] ON [Person]([Tag] ASC)

/* Table: 'PersonData' */
CREATE TABLE [PersonData](
    [Id] integer NOT NULL,
    [PersonId] integer NOT NULL,
    [BestAge] integer NULL,
    [Hash] integer NULL,
    [Height] numeric NULL,
    [Weight] numeric NULL,
    [UpdatedAt] datetime NULL,
    [Signature] blob NULL,
    CONSTRAINT [PK_personData] PRIMARY KEY([Id]),
    CONSTRAINT [FK_personData_person] FOREIGN KEY([PersonId]) REFERENCES [Person]([Id]))

/* Table: 'Photo' */
CREATE TABLE [Photo](
    [Id] text NOT NULL,
    [PersonDataId] integer NOT NULL,
    [Content] blob NOT NULL,
    [ContentThumbnail] blob NULL,
    [TakenAt] datetime NULL,
    [ValidUntil] datetime NULL,
    CONSTRAINT [PK_photo] PRIMARY KEY([Id]),
    CONSTRAINT [FK_photo_personData] FOREIGN KEY([PersonDataId]) REFERENCES [PersonData]([Id]))

