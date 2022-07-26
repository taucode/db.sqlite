SELECT
    [Height],
    [EnglishDescription],
    [UnicodeDescription],
    [PersonMetaKey],
    [PersonOrdNumber],
    [PersonId]
FROM
    [PersonData]
WHERE
    [Id] = @p_id