using System;
using System.Linq;
using NUnit.Framework;
using TauCode.Extensions;

namespace TauCode.Db.SQLite.Tests
{
    [TestFixture]
    public class SQLiteSerializerComplexTests : TestBase
    {
        private IDbSerializer _dbSerializer;

        [SetUp]
        public void SetUp()
        {
            _dbSerializer = new SQLiteSerializer(this.Connection);
        }

        [Test]
        public void SerializeDbData_ComplexData_ProducesExpectedResult()
        {
            // Arrange
            var insertScript = TestHelper.GetResourceText("ocean.script-data.sql");
            this.Connection.ExecuteCommentedScript(insertScript);

            // Act
            var json = _dbSerializer.SerializeDbData();

            // Assert
            var expectedJson = this.GetType().Assembly.GetResourceText("ocean.data-db.json", true);
            Assert.That(json, Is.EqualTo(expectedJson));
        }

        [Test]
        public void DeserializeDbData_ComplexData_ProducesExpectedResult()
        {
            // Arrange
            var json = this.GetType().Assembly.GetResourceText("ocean.data-db.json", true);

            // Act
            _dbSerializer.DeserializeDbData(json);

            // Assert
            var cruder = this.DbInspector.Factory.CreateCruder(this.Connection, null);
            var users = cruder.GetAllRows("user");
            var userInfos = cruder.GetAllRows("user_info");

            // ak
            var id = new Guid("115777dc-2394-4e14-a587-11afde55588e");
            var user = users.Single(x => x.id == id);
            Assert.That(user.name, Is.EqualTo("ak"));
            Assert.That(user.birthday, Is.EqualTo(new DateTime(1978, 7, 5, 11, 20, 30)));
            Assert.That(user.gender, Is.EqualTo(0));
            CollectionAssert.AreEqual(GetResourceBytes("ak.png"), user.picture);

            var userInfo = userInfos.Single(x => x.user_id == id);
            Assert.That(userInfo.id, Is.EqualTo(new Guid("118833be-1ac7-4161-90d5-11eaa22d1609")));
            Assert.That(userInfo.tax_number, Is.EqualTo("TXNM-111"));
            Assert.That(userInfo.code, Is.EqualTo("COD-Андрей"));
            Assert.That(userInfo.ansi_name, Is.EqualTo("ANSI NAME Andrey"));
            Assert.That(userInfo.ansi_description, Is.EqualTo("ANSI DESCR Andrey Kovalenko"));
            Assert.That(userInfo.unicode_description, Is.EqualTo("UNICODE Андрей Коваленко"));
            Assert.That(userInfo.height, Is.EqualTo(1.79));
            Assert.That(userInfo.weight, Is.EqualTo(68.9));
            Assert.That(userInfo.weight2, Is.EqualTo(92.1d));
            Assert.That(userInfo.salary, Is.EqualTo(6000.3m));
            Assert.That(userInfo.rating_decimal, Is.EqualTo(19.5m));
            Assert.That(userInfo.rating_numeric, Is.EqualTo(7.677165m));
            Assert.That(userInfo.num8, Is.EqualTo(14));
            Assert.That(userInfo.num16, Is.EqualTo(1488));
            Assert.That(userInfo.num32, Is.EqualTo(17401488));
            Assert.That(userInfo.num64, Is.EqualTo(188887401488));

            // deserea
            id = new Guid("223803c1-0d8e-4425-b092-2214b081315d");
            user = users.Single(x => x.id == id);
            Assert.That(user.name, Is.EqualTo("deserea"));
            Assert.That(user.birthday, Is.EqualTo(new DateTime(1989, 1, 1, 7, 33, 44)));
            Assert.That(user.gender, Is.EqualTo(1));
            CollectionAssert.AreEqual(GetResourceBytes("deserea.png"), user.picture);

            userInfo = userInfos.Single(x => x.user_id == id);
            Assert.That(userInfo.id, Is.EqualTo(new Guid("22b0809b-26a0-4962-87b0-2244e5a3265d")));
            Assert.That(userInfo.tax_number, Is.EqualTo("TXNM-222"));
            Assert.That(userInfo.code, Is.EqualTo("COD-Десер"));
            Assert.That(userInfo.ansi_name, Is.EqualTo("ANSI NAME Deserea"));
            Assert.That(userInfo.ansi_description, Is.EqualTo("ANSI DESCR Deserea Goddess"));
            Assert.That(userInfo.unicode_description, Is.EqualTo("UNICODE Десереа Богиня"));
            Assert.That(userInfo.height, Is.EqualTo(1.81));
            Assert.That(userInfo.weight, Is.EqualTo(69.1));
            Assert.That(userInfo.weight2, Is.EqualTo(69.1d));
            Assert.That(userInfo.salary, Is.EqualTo(2000.1m));
            Assert.That(userInfo.rating_decimal, Is.EqualTo(29.5m));
            Assert.That(userInfo.rating_numeric, Is.EqualTo(2.677165m));
            Assert.That(userInfo.num8, Is.EqualTo(24));
            Assert.That(userInfo.num16, Is.EqualTo(2488));
            Assert.That(userInfo.num32, Is.EqualTo(27401488));
            Assert.That(userInfo.num64, Is.EqualTo(288887401488));

            // ira
            id = new Guid("33f48549-075f-4d53-9548-334393d0c534");
            user = users.Single(x => x.id == id);
            Assert.That(user.name, Is.EqualTo("ira"));
            Assert.That(user.birthday, Is.EqualTo(new DateTime(1979, 6, 14, 13, 13, 13)));
            Assert.That(user.gender, Is.EqualTo(1));
            CollectionAssert.AreEqual(GetResourceBytes("ira.png"), user.picture);

            userInfo = userInfos.Single(x => x.user_id == id);
            Assert.That(userInfo.id, Is.EqualTo(new Guid("3367f329-e1cc-4d16-b00d-3347117fa260")));
            Assert.That(userInfo.tax_number, Is.EqualTo("TXNM-333"));
            Assert.That(userInfo.code, Is.EqualTo("COD-Ира"));
            Assert.That(userInfo.ansi_name, Is.EqualTo("ANSI NAME Ira"));
            Assert.That(userInfo.ansi_description, Is.EqualTo("ANSI DESCR Ira Triad"));
            Assert.That(userInfo.unicode_description, Is.EqualTo("UNICODE Ира Триад"));
            Assert.That(userInfo.height, Is.EqualTo(1.68));
            Assert.That(userInfo.weight, Is.EqualTo(59.1));
            Assert.That(userInfo.weight2, Is.EqualTo(59.1d));
            Assert.That(userInfo.salary, Is.EqualTo(3000.3m));
            Assert.That(userInfo.rating_decimal, Is.EqualTo(39.5m));
            Assert.That(userInfo.rating_numeric, Is.EqualTo(3.677165m));
            Assert.That(userInfo.num8, Is.EqualTo(34));
            Assert.That(userInfo.num16, Is.EqualTo(3488));
            Assert.That(userInfo.num32, Is.EqualTo(37401488));
            Assert.That(userInfo.num64, Is.EqualTo(388887401488));

            // marina
            id = new Guid("44725a6c-8a63-40fe-9980-449a6a6be60a");
            user = users.Single(x => x.id == id);
            Assert.That(user.name, Is.EqualTo("marina"));
            Assert.That(user.birthday, Is.EqualTo(new DateTime(1970, 3, 8, 14, 14, 14)));
            Assert.That(user.gender, Is.EqualTo(1));
            CollectionAssert.AreEqual(GetResourceBytes("marina.png"), user.picture);

            userInfo = userInfos.Single(x => x.user_id == id);
            Assert.That(userInfo.id, Is.EqualTo(new Guid("44509dc7-dcf3-4e1f-96cc-44e702a4f3b2")));
            Assert.That(userInfo.tax_number, Is.EqualTo("TXNM-444"));
            Assert.That(userInfo.code, Is.EqualTo("COD-Мари"));
            Assert.That(userInfo.ansi_name, Is.EqualTo("ANSI NAME Marina"));
            Assert.That(userInfo.ansi_description, Is.EqualTo("ANSI DESCR Marina Shukina"));
            Assert.That(userInfo.unicode_description, Is.EqualTo("UNICODE Марина Щукина"));
            Assert.That(userInfo.height, Is.EqualTo(1.58));
            Assert.That(userInfo.weight, Is.EqualTo(57.1));
            Assert.That(userInfo.weight2, Is.EqualTo(57.1d));
            Assert.That(userInfo.salary, Is.EqualTo(4000.3m));
            Assert.That(userInfo.rating_decimal, Is.EqualTo(49.5m));
            Assert.That(userInfo.rating_numeric, Is.EqualTo(4.677165m));
            Assert.That(userInfo.num8, Is.EqualTo(44));
            Assert.That(userInfo.num16, Is.EqualTo(4488));
            Assert.That(userInfo.num32, Is.EqualTo(47401488));
            Assert.That(userInfo.num64, Is.EqualTo(488887401488));

            // olia
            id = new Guid("5525fb3b-7700-459c-aa39-55a78d1ff584");
            user = users.Single(x => x.id == id);
            Assert.That(user.name, Is.EqualTo("olia"));
            Assert.That(user.birthday, Is.EqualTo(new DateTime(1979, 6, 25, 15, 15, 15)));
            Assert.That(user.gender, Is.EqualTo(1));
            CollectionAssert.AreEqual(GetResourceBytes("olia.png"), user.picture);

            userInfo = userInfos.Single(x => x.user_id == id);
            Assert.That(userInfo.id, Is.EqualTo(new Guid("55581fca-2baa-4808-b2f9-55feb45a5a9d")));
            Assert.That(userInfo.tax_number, Is.EqualTo("TXNM-555"));
            Assert.That(userInfo.code, Is.EqualTo("COD-Оля"));
            Assert.That(userInfo.ansi_name, Is.EqualTo("ANSI NAME Olia"));
            Assert.That(userInfo.ansi_description, Is.EqualTo("ANSI DESCR Olia Chernyshova"));
            Assert.That(userInfo.unicode_description, Is.EqualTo("UNICODE Оля Чернышева"));
            Assert.That(userInfo.height, Is.EqualTo(1.69));
            Assert.That(userInfo.weight, Is.EqualTo(53.1));
            Assert.That(userInfo.weight2, Is.EqualTo(53.1d));
            Assert.That(userInfo.salary, Is.EqualTo(5000.3m));
            Assert.That(userInfo.rating_decimal, Is.EqualTo(59.5m));
            Assert.That(userInfo.rating_numeric, Is.EqualTo(5.677165m));
            Assert.That(userInfo.num8, Is.EqualTo(54));
            Assert.That(userInfo.num16, Is.EqualTo(5488));
            Assert.That(userInfo.num32, Is.EqualTo(57401488));
            Assert.That(userInfo.num64, Is.EqualTo(588887401488));
        }

        private byte[] GetResourceBytes(string fileName)
        {
            return this.GetType().Assembly.GetResourceBytes(fileName, true);
        }

        protected override void ExecuteDbCreationScript()
        {
            var script = TestHelper.GetResourceText("ocean.script-create-tables.sql");
            this.Connection.ExecuteCommentedScript(script);
        }
    }
}
