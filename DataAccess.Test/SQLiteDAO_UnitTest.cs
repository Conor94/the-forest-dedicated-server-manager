using DataAccess.Test.Models;
using NUnit.Framework;
using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace DataAccess.Test
{
    [TestFixture]
    public class SQLiteDAO_UnitTest
    {
        private const string DB_NAME = "test";
        private const string TBL_NAME = "test_tbl";
        private const string TBL_COLUMNS_WITH_TYPES = "Name TEXT, Age INTEGER, Email TEXT";
        private const string TBL_COLUMNS = "Name, Age, Email";

        private static Person testPerson;

        private static SQLiteDAO DAO;

        [OneTimeSetUp]
        public static void SetUp()
        {
            DAO = new SQLiteDAO($"Data Source={DB_NAME}.db");

            DAO.CreateDatabase(DB_NAME);

            // Create a table for testing and insert a record into it
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={DB_NAME}.db"))
            {
                using (SQLiteCommand command = new SQLiteCommand(conn))
                {
                    conn.Open();

                    command.CommandText = $"CREATE TABLE {TBL_NAME} ({TBL_COLUMNS_WITH_TYPES})";
                    command.ExecuteNonQuery();

                    command.CommandText = $"INSERT INTO {TBL_NAME} ({TBL_COLUMNS}) VALUES ('Jim', 43, 'jim1970s@hotmail.com')";
                    command.ExecuteNonQuery();
                }
            }

            // Create a model for testing
            testPerson = new Person()
            {
                Name = "Jim",
                Age = 43,
                Email = "jim1970s@hotmail.com"
            };
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            File.Delete($"{DB_NAME}.db");
        }

        [Test]
        public void ShouldCreateDatabase()
        {
            Assert.IsTrue(File.Exists($"{DB_NAME}.db"));
        }

        [Test]
        public void DatabaseShouldExist()
        {
            Assert.IsTrue(DAO.DatabaseExists($"{DB_NAME}"));
        }

        [Test]
        public void ShouldCreateTable()
        {
            DAO.CreateTable("tmp_test_tbl", TBL_COLUMNS_WITH_TYPES);

            // Check if the table exists
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={DB_NAME}.db"))
            {
                using (SQLiteCommand command = new SQLiteCommand(conn))
                {
                    command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='tmp_test_tbl';";

                    conn.Open();

                    Assert.IsNotNull(command.ExecuteScalar());
                }
            }
        }

        [Test]
        public void TableShouldExist()
        {
            Assert.IsTrue(DAO.TableExists(TBL_NAME));
        }

        [Test]
        public void TableShouldNotExist()
        {
            Assert.IsFalse(DAO.TableExists("should_not_exist_tbl"));
        }

        [Test]
        public void ShouldInsertSingle()
        {
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@Name", "Mike"),
                new SQLiteParameter("@Age", 13),
                new SQLiteParameter("@Email", "mike@gmail.com")
            };

            int count = DAO.InsertSingle(TBL_NAME, parameters);

            Assert.AreEqual(count, 1);
        }

        [Test]
        public void ShouldInsertMultipleInLessThan100Milliseconds()
        {
            SQLiteParameter[][] parameters = new SQLiteParameter[10000][];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = new SQLiteParameter[3];

                parameters[i][0] = new SQLiteParameter("@Name", "Mike");
                parameters[i][1] = new SQLiteParameter("@Age", 13);
                parameters[i][2] = new SQLiteParameter("@Email", "mike13@gmail.com");
            }

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            int count = DAO.InsertMultiple(TBL_NAME, parameters);
            stopwatch.Stop();

            Assert.AreEqual(count, 10000);
            Assert.LessOrEqual(stopwatch.ElapsedMilliseconds, 100, "Failed to insert 10000 records in less than 100 milliseconds");
        }

        [Test]
        public void ShouldRead()
        {
            Person person = DAO.Read(TBL_NAME, TBL_COLUMNS, (dataReader) =>
            {
                Person p = new Person();

                while (dataReader.Read())
                {
                    p.Name = (string)dataReader["Name"];
                    p.Age = Convert.ToInt32(dataReader["Age"]);
                    p.Email = (string)dataReader["Email"];
                }

                return p;
            }, "Name = 'Jim'");

            Assert.AreEqual(testPerson.Name, person.Name);
            Assert.AreEqual(testPerson.Age, person.Age);
            Assert.AreEqual(testPerson.Email, person.Email);
        }

        [Test]
        public void ShouldUpdateSingle()
        {
            // Create a record
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={DB_NAME}.db"))
            {
                using (SQLiteCommand command = new SQLiteCommand(conn))
                {
                    conn.Open();

                    command.CommandText = $"INSERT INTO {TBL_NAME} ({TBL_COLUMNS}) VALUES ('Billy', 17, 'billyhargrove@gmail.com')";
                    command.ExecuteNonQuery();
                }
            }

            // Update the record that was created
            SQLiteParameter[] parameters = new SQLiteParameter[3]
            {
                new SQLiteParameter("@Name", "Lucas"),
                new SQLiteParameter("@Age", 13),
                new SQLiteParameter("@Email", "lucas13@gmail.com")
            };

            int count = DAO.UpdateSingle(TBL_NAME, parameters, "Email = 'billyhargrove@gmail.com'");

            Assert.AreEqual(count, 1);
        }

        [Test]
        public void ShouldDelete()
        {
            // Create a record
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={DB_NAME}.db"))
            {
                using (SQLiteCommand command = new SQLiteCommand(conn))
                {
                    conn.Open();

                    command.CommandText = $"INSERT INTO {TBL_NAME} ({TBL_COLUMNS}) VALUES ('Billy', 17, 'billyhargrove@gmail.com')";
                    command.ExecuteNonQuery();
                }
            }

            // Delete the record that was created
            int count = DAO.Delete(TBL_NAME, "Email = 'billyhargrove@gmail.com'");

            Assert.AreEqual(count, 1);
        }
    }
}
