using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TranslatePrograms.Models
{
    public class CodeConverter
    {

        public static string ConvertCode(string text,string lang1,string lang2)
        {
            IParser pars1 = ParserManager.CreateParser(text, lang1, lang2);
            pars1.Parse();
            var errs = pars1.errors;
            return pars1.fout;
        }
    }
}