using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TranslatePrograms.Models
{
    public class Errors
    {
        public int count = 0;                                    // number of errors detected
                                                                 //public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
        public List<string> errorList = new List<string>();
        public string DLL;      // name Convertor` 
        public string version;  // date Convertor
        public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

        public virtual void SynErr(int line, int col, int n)
        {
            string s;
            switch (n)
            {
                case 0: s = "EOF expected"; break;
                case 1: s = "ident expected"; break;
                case 2: s = "number expected"; break;
                case 3: s = "\"MODULE\" expected"; break;
                case 4: s = "\";\" expected"; break;
                case 5: s = "\"VAR\" expected"; break;
                case 6: s = "\"BEGIN\" expected"; break;
                case 7: s = "\"END\" expected"; break;
                case 8: s = "\".\" expected"; break;
                case 9: s = "\"IMPORT\" expected"; break;
                case 10: s = "\":=\" expected"; break;
                case 11: s = "\",\" expected"; break;
                case 12: s = "\":\" expected"; break;
                case 13: s = "\"PROCEDURE\" expected"; break;
                case 14: s = "\"INTEGER\" expected"; break;
                case 15: s = "\"BOOLEAN\" expected"; break;
                case 16: s = "\"(*\" expected"; break;
                case 17: s = "\"*)\" expected"; break;
                case 18: s = "\"Log.Int\" expected"; break;
                case 19: s = "\"Log.Bool\" expected"; break;
                case 20: s = "\"(\" expected"; break;
                case 21: s = "\")\" expected"; break;
                case 22: s = "\"IF\" expected"; break;
                case 23: s = "\"THEN\" expected"; break;
                case 24: s = "\"ELSIF\" expected"; break;
                case 25: s = "\"ELSE\" expected"; break;
                case 26: s = "\"WHILE\" expected"; break;
                case 27: s = "\"DO\" expected"; break;
                case 28: s = "\"+\" expected"; break;
                case 29: s = "\"-\" expected"; break;
                case 30: s = "\"~\" expected"; break;
                case 31: s = "\"TRUE\" expected"; break;
                case 32: s = "\"FALSE\" expected"; break;
                case 33: s = "\"=\" expected"; break;
                case 34: s = "\"#\" expected"; break;
                case 35: s = "\"<\" expected"; break;
                case 36: s = "\"<=\" expected"; break;
                case 37: s = "\">\" expected"; break;
                case 38: s = "\">=\" expected"; break;
                case 39: s = "\"OR\" expected"; break;
                case 40: s = "\"*\" expected"; break;
                case 41: s = "\"/\" expected"; break;
                case 42: s = "\"DIV\" expected"; break;
                case 43: s = "\"MOD\" expected"; break;
                case 44: s = "\"&\" expected"; break;
                case 45: s = "??? expected"; break;
                case 46: s = "invalid Type"; break;
                case 47: s = "invalid Stat"; break;
                case 48: s = "invalid RelOp"; break;
                case 49: s = "invalid AddOp"; break;
                case 50: s = "invalid Factor"; break;
                case 51: s = "invalid MulOp"; break;

                default: s = "error " + n; break;
            }
            errorList.Add(errMsgFormat + " " + line.ToString() + " " + col.ToString() + " " + s);
            count++;
        }

        public virtual void SemErr(int line, int col, string s)
        {
            errorList.Add(errMsgFormat + " " + line.ToString() + " " + col.ToString() + " " + s);
            count++;
        }

        public virtual void SemErr(string s)
        {
            errorList.Add(s);
            count++;
        }

        public virtual void Warning(int line, int col, string s)
        {
            errorList.Add(errMsgFormat + " " + line.ToString() + " " + col.ToString() + " " + s);
        }

        public virtual void Warning(string s)
        {
            errorList.Add(s);
        }
    } // Errors


    public class FatalError : Exception
    {
        public FatalError(string m) : base(m) { }
    }
}