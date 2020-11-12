INSERT INTO [Person](
    [MetaKey],
    [OrdNumber],
    [Id],
    [FirstName],
    [LastName],
    [Birthday],
    [Gender],
    [Initials])
VALUES(
    @p_metaKey,
    @p_ordNumber,
    @p_id,
    @p_firstName,
    @p_lastName,
    @p_birthday,
    @p_gender,
    @p_initials)