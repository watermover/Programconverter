using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslatePrograms.Models
{
    interface ILang
    {
        IParser Parser{ get; set; }
        IScanner Scanner { get; set; }
    }
    public interface IParser
    {
        Errors errors { get; set; }
        string fout { get; set; }
        void Parse();
    }
    interface IScanner
    {
    }
}
