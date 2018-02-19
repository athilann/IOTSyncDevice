using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace DBAccess
{
    public class DataBaseAccess
    {
        private string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");

        public DataBaseAccess()
        {
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path))
            {
                var hasCreated = conn.CreateTable<Registre>();
                var query = conn.Table<Registre>();
            }
        }

        public void AddRegister(Registre registre)
        {
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path))
            {
                conn.Insert(new Registre { RegistreDate = registre.RegistreDate });
            }
        }

        public  List<Registre> GetRegisters()
        {
            List<Registre> registres = new List<Registre>();
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path))
            {
                var query = conn.Table<Registre>();
                registres = query.ToList();
            }
            return registres;
        }
    }

    public class Registre
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime RegistreDate { get; set; }

    }
}
