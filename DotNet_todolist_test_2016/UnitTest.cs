using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.ViewManagement;
using DotNet_todolist_2016;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace DotNet_todolist_test_2016
{
    [TestClass]
    public class UnitTest1
    {
        public async void asyncTest()
        {
            Debug.WriteLine("start test");
            StorageFile filed;

            try
            {
                filed = await ApplicationData.Current.LocalFolder.GetFileAsync("db.sqlite");
            }
            catch (FileNotFoundException e)
            {
                filed = null;
            }

            if (filed != null)
            {
                await filed.DeleteAsync();
                Debug.WriteLine("file deleted");
            }

            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            var Conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
            Conn.CreateTable<ToDo>();

            Debug.WriteLine("table created");

            var now = DateTimeOffset.Now;
            Conn.Insert(new ToDo()
            {
                Title = "todo test",
                DueDate = now,
                Description = "todo description test",
                Done = false
            });

            Debug.WriteLine("todo created");

            ToDo test = Conn.Query<ToDo>("SELECT * FROM ToDo WHERE Id =" + 1).FirstOrDefault();
            Assert.IsNotNull(test);
            Assert.AreEqual("todo test", test.Title);
            Assert.AreEqual("todo description test", test.Description);
            Assert.AreEqual(now, test.DueDate);
            Assert.IsFalse(test.Done);

            Debug.WriteLine("todo checked");

            now = DateTimeOffset.Now;
            test.Description = "new description";
            test.Title = "new title";
            test.DueDate = now;
            test.Done = !test.Done;
            Conn.RunInTransaction(() =>
            {
                Conn.Update(test);
            });

            Debug.WriteLine("todo modified");

            test = Conn.Query<ToDo>("SELECT * FROM ToDo WHERE Id =" + 1).FirstOrDefault();
            Assert.IsNotNull(test);
            Assert.AreEqual("new title", test.Title);
            Assert.AreEqual("new description", test.Description);
            Assert.AreEqual(now, test.DueDate);
            Assert.IsTrue(test.Done);

            Debug.WriteLine("todo rechecked");

            Conn.Query<ToDo>("DELETE FROM ToDo WHERE Id =" + 1);

            Debug.WriteLine("todo erased");

            var query = Conn.Table<ToDo>();
            Assert.AreEqual(0, query.Count());

            Debug.WriteLine("end test");
        }

        [TestMethod]
        public void TestMethod1()
        {
            Debug.WriteLine("start");
            asyncTest();
            Debug.WriteLine("end");
        }
    }
}
