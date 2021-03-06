﻿using Database.Model;
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
            csvString.AppendLine("ID da Leitura, Data da Leitura");
            foreach (Registre registre in registres)
            {
                csvString.AppendLine(registre.Id+","+registre.RegistreDate.ToString("dd/MM/yyyy HH:mm"));
            }
            return csvString.ToString();
        }

    }
}
