using Database.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database
{
    public class DataBaseAccess
    {


        public DataBaseAccess()
        {
            using (var db = new ModelContext())
            {
                db.Database.Migrate();
            }
        }

        public void AddRegister(Registre registre)
        {
            using (var db = new ModelContext())
            {
                db.Registres.Add(registre);
                db.SaveChanges();
            }
        }

        public async Task<List<Registre>> GetRegistersAsync()
        {
            using (var db = new ModelContext())
            {
                return  await db.Registres.ToListAsync();
            }
        }

        public void RemoveRegisters(List<Registre> registres)
        {
            using (var db = new ModelContext())
            {
                db.Registres.RemoveRange(registres);
                db.SaveChanges();
            }
        }

    }
}
