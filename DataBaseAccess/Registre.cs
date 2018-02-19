using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAccess
{
    public class Registre
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime RegistreDate { get; set; }

    }
}
