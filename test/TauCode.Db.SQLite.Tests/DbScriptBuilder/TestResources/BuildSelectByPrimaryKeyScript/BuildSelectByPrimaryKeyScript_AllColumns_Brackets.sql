SELECT
    [Id],
    [Height],
    [Photo],
    [EnglishDescription],
    [UnicodeDescription],
    [PersonMetaKey],
    [PersonOrdNumber],
    [PersonId]
FROM
    [PersonData]
WHERE
    [Id] = @p_id