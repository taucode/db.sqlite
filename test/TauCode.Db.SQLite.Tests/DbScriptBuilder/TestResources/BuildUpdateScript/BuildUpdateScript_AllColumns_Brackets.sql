UPDATE [PersonData]
SET
    [Height] = @p_height,
    [Photo] = @p_photo,
    [EnglishDescription] = @p_englishDescription,
    [UnicodeDescription] = @p_unicodeDescription,
    [PersonMetaKey] = @p_personMetaKey,
    [PersonOrdNumber] = @p_personOrdNumber,
    [PersonId] = @p_personId
WHERE
    [Id] = @p_id