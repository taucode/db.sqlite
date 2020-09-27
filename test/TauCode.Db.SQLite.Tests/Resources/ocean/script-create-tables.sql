/* create table: user */
CREATE TABLE [user](
	[id] UNIQUEIDENTIFIER NOT NULL,
	[name] TEXT NOT NULL,
	[birthday] DATETIME NOT NULL,
	[gender] INTEGER NOT NULL,
	[picture] BLOB NOT NULL,
	CONSTRAINT [PK_user] PRIMARY KEY([id]))


/* create table: user_info */
CREATE TABLE [user_info](
	[id] UNIQUEIDENTIFIER NOT NULL,
	[user_id] UNIQUEIDENTIFIER NOT NULL,
	[tax_number] TEXT NOT NULL,
	[code] TEXT NOT NULL,
	[ansi_name] TEXT NULL,
	[ansi_description] TEXT NOT NULL,
	[unicode_description] TEXT NOT NULL,
	[height] REAL NOT NULL,
	[weight] REAL NOT NULL,
	[weight2] REAL NOT NULL,
	[salary] NUMERIC NOT NULL,
	[rating_decimal] NUMERIC NOT NULL,
	[rating_numeric] NUMERIC NOT NULL,
	[num8] INTEGER NOT NULL,
	[num16] INTEGER NOT NULL,
	[num32] INTEGER NOT NULL,
	[num64] INTEGER NOT NULL,

	CONSTRAINT [PK_userInfo] PRIMARY KEY([id] DESC),
	CONSTRAINT [FK_userInfo_user] FOREIGN KEY([user_id]) REFERENCES [user]([id]))

/* create index: UX_userInfo_taxNumber */
CREATE UNIQUE INDEX [UX_userInfo_taxNumber] ON [user_info]([tax_number])