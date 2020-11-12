CREATE TABLE [SuperTable](
	[Id] integer NOT NULL PRIMARY KEY,
	[TheGuid] uniqueidentifier NULL,
	[TheBigInt] integer NULL,
	[TheDecimal] numeric NULL,
	[TheReal] real NULL,
	[TheDateTime] datetime NULL,
	[TheTime] time NULL,
	[TheText] text NULL,
	[TheBlob] blob NULL)