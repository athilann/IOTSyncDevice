using Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSync
{
    public class CreateCSVFile
    {

        public static string CreateFromRegistres(List<Registre> registres)
        {
            StringBuilder csvString = new StringBuilder();
            //csvString.AppendLine("\"ID da Leitura\", \"Data da Leitura\", \"Descricao\"");
            foreach (Registre registre in registres)
            {
                csvString.AppendLine(registre.Id+","+registre.RegistreDate.ToString("dd/MM/yyyy HH:mm:ss")+", \"Pressao considerada 0.5 BAR - Volume 100ml\"");
            }
            return csvString.ToString();
        }

    }
}
