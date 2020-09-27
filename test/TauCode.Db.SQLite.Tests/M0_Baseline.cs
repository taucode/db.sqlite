using FluentMigrator;

namespace TauCode.Db.SQLite.Tests
{
    [Migration(0)]
    public class M0_Baseline : AutoReversingMigration
    {
        public override void Up()
        {
            #region user

            this.Create.Table("user")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_user")
                .WithColumn("username")
                    .AsAnsiString(100)
                    .NotNullable()
                    .Unique("UX_user_username")
                .WithColumn("email")
                    .AsString(100)
                    .NotNullable()
                    .Unique("UX_user_email")
                .WithColumn("password_hash")
                    .AsAnsiString()
                    .NotNullable();

            #endregion

            #region language

            this.Create.Table("language")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_language")
                .WithColumn("code")
                    .AsAnsiString(2)
                    .NotNullable()
                    .Unique("UX_language_code")
                .WithColumn("name")
                    .AsString(100)
                    .NotNullable();

            #endregion

            #region fragment_type

            this.Create.Table("fragment_type")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_fragmentType")
                .WithColumn("code")
                    .AsAnsiString(100)
                    .NotNullable()
                    .Unique("UX_fragmentType_code")
                .WithColumn("name")
                    .AsString(100)
                    .NotNullable()
                .WithColumn("is_default")
                    .AsBoolean()
                    .NotNullable();

            #endregion

            #region fragment_sub_type

            this.Create.Table("fragment_sub_type")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_fragmentSubType")
                .WithColumn("type_id")
                    .AsGuid()
                    .NotNullable()
                    .Indexed("IX_fragmentSubType_fragmentType")
                    .ForeignKey("FK_fragmentSubType_fragmentType", "fragment_type", "id")
                .WithColumn("code")
                    .AsAnsiString(100)
                    .NotNullable()
                .WithColumn("name")
                    .AsString(100)
                    .NotNullable()
                .WithColumn("is_default")
                    .AsBoolean()
                    .NotNullable();

            #endregion

            #region tag

            this.Create.Table("tag")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_tag")
                .WithColumn("code")
                    .AsAnsiString(100)
                    .NotNullable()
                    .Unique("UX_tag_code")
                .WithColumn("name")
                    .AsString(100)
                    .NotNullable();

            #endregion

            #region note

            this.Create.Table("note")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_note")
                .WithColumn("code")
                    .AsAnsiString(100)
                    .NotNullable()
                    .Unique("UX_note_code")
                .WithColumn("created_at")
                    .AsDateTime()
                    .NotNullable()
                    .Unique("UX_note_createdAt");

            #endregion

            #region note_tag

            this.Create.Table("note_tag")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_noteTag")
                .WithColumn("note_id")
                    .AsGuid()
                    .Nullable() // actually, never 'null' in committed state, but 'Nullable' is needed as work-around for NHibernate
                    .Indexed("IX_noteTag_note")
                    .ForeignKey("FK_noteTag_note", "note", "id")
                .WithColumn("tag_id")
                    .AsGuid()
                    .NotNullable()
                    .Indexed("IX_noteTag_tag")
                    .ForeignKey("FK_noteTag_tag", "tag", "id")
                .WithColumn("order")
                    .AsInt32()
                    .NotNullable();

            #endregion

            #region note_translation

            this.Create.Table("note_translation")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_noteTranslation")
                .WithColumn("note_id")
                    .AsGuid()
                    .NotNullable()
                    .Indexed("IX_noteTranslation_note")
                    .ForeignKey("FK_noteTranslation_note", "note", "id")
                .WithColumn("language_id")
                    .AsGuid()
                    .NotNullable()
                    .Indexed("IX_noteTranslation_language")
                    .ForeignKey("FK_noteTranslation_language", "language", "id")
                .WithColumn("is_default")
                    .AsBoolean()
                    .NotNullable()
                .WithColumn("is_published")
                    .AsBoolean()
                    .NotNullable()
                .WithColumn("title")
                    .AsString(1000)
                    .NotNullable()
                .WithColumn("last_updated_on")
                    .AsDateTime()
                    .NotNullable();

            #endregion

            #region fragment

            this.Create.Table("fragment")
                .WithColumn("id")
                    .AsGuid()
                    .NotNullable()
                    .PrimaryKey("PK_fragment")
                .WithColumn("note_translation_id")
                    .AsGuid()
                    .NotNullable()
                    .ForeignKey("FK_fragment_noteTranslation", "note_translation", "id")
                .WithColumn("sub_type_id")
                    .AsGuid()
                    .NotNullable()
                    .Indexed("IX_fragment_fragmentSubType")
                    .ForeignKey("FK_fragment_fragmentSubType", "fragment_sub_type", "id")
                .WithColumn("code")
                    .AsAnsiString(100)
                    .Nullable()
                .WithColumn("order")
                    .AsInt32()
                    .NotNullable()
                .WithColumn("content")
                    .AsString(int.MaxValue)
                    .NotNullable();

            #endregion

            #region foo

            this.Create.Table("foo")
                .WithColumn("my_string")
                    .AsString()
                .WithColumn("my_ansi_string")
                    .AsAnsiString()
                .WithColumn("my_binary")
                    .AsBinary()
                .WithColumn("my_bool")
                    .AsBoolean()
                .WithColumn("my_byte")
                    .AsByte()
                .WithColumn("my_currency")
                    .AsCurrency()
                .WithColumn("my_datetime")
                    .AsDateTime()
                .WithColumn("my_decimal")
                    .AsDecimal()
                .WithColumn("my_double")
                    .AsDouble()
                .WithColumn("my_fixed_string")
                    .AsFixedLengthString(100)
                .WithColumn("my_fixed_ansi_string")
                    .AsFixedLengthAnsiString(100)
                .WithColumn("my_float")
                    .AsFloat()
                .WithColumn("my_int16")
                    .AsInt16()
                .WithColumn("my_int32")
                    .AsInt32()
                .WithColumn("my_int64")
                    .AsInt64();

            #endregion

            #region hoo

            this.Create.Table("hoo")
                .WithColumn("id")
                    .AsInt32()
                    .NotNullable()
                    .PrimaryKey()
                .WithColumn("name")
                    .AsString()
                    .Nullable()
                .WithColumn("enum_int32")
                    .AsInt32()
                    .Nullable()
                .WithColumn("enum_string")
                    .AsString()
                    .Nullable();

            #endregion
        }
    }
}
