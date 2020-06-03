using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TranslatePrograms.Models
{
    public class ParserPythonToPascal:IParser
    {
        public const int _EOF = 0;
        public const int _ident = 1;
        public const int _number = 2;
        public const int maxT = 45;

        const bool _T = true;
        const bool _x = false;
        const int minErrDist = 2;
        const bool T = true;
        const bool x = false;

        public ScannerPythonToPascal scanner;
        public Errors errors { get; set; }

        public Token t;    // last recognized token
        public Token la;   // lookahead token
        int errDist = minErrDist;

        public string ss;
        public string name, name0;
        public string fout { get; set; }
        //public System.IO.StreamWriter fout = System.IO.File.CreateText("result.txt");
        int u = 2;
        void W(string x)
        {
            string su = "\r\n\t\t\t\t\t\t\t\t\t";
            //Console.Write(su.Substring(0,u)+x); 
            fout+=(su.Substring(0, u) + x);
        }
        void Wc(string x)
        {
            //Console.Write(x); 
            fout+=(x);
        }
        //void W0(string s1,string s2){fout.Write(s1 + System.DateTime.Now.ToString()+" "+errors.DLL+" "+errors.version+s2); }
        void W0(string s1, string s2) { fout+=(s1 + System.DateTime.Now.ToString() + s2); }

        string[] mg = new string[100]; int kg = 0, k1 = 0, k2 = 0;
        string[] mv = new string[100]; int k = 0;
        bool locv(string v)
        {
            bool b = false; // local variables?
            for (int i = 0; i < k; i++)
                if (v == mv[i]) { b = true; break; }
            return b;
        }
        /*--------------------------------------------------------------------------*/

        string s8(string s)
        {
            string ss = ""; // 
            int k;
            for (int i = 0; i < 64; i++)
            {
                k = 4 * 256 + 16 + i; ss += ((char)k).ToString();
            }
            char[] mc = s.ToCharArray();
            string sR = ""; int stat = 0;
            foreach (char c in mc)
            { //n++;
                int kod = Convert.ToInt32(c);
                string hex = String.Format("{0:X}", kod); // 90..
                if (hex == "D0" || hex == "D1")
                {
                    stat = (hex == "D0") ? 1 : 2;
                }
                else
                {
                    if (stat > 0)
                    {
                        k = (stat == 1) ? kod - 16 * 9 : 48 + kod - 16 * 8;
                        sR += ss.Substring(k, 1);
                        stat = 0;
                    }
                    else sR += c;
                }
            }
            return sR;
        }
        public ParserPythonToPascal(ScannerPythonToPascal scanner)
        {
            this.scanner = scanner;
            errors = new Errors();
        }

        void SynErr(int n)
        {
            if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
            errDist = 0;
        }

        public void SemErr(string msg)
        {
            if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
            errDist = 0;
        }

        void Get()
        {
            for (; ; )
            {
                t = la;
                la = scanner.Scan();
                if (la.kind <= maxT) { ++errDist; break; }

                la = t;
            }
        }

        void Expect(int n)
        {
            if (la.kind == n) Get(); else { SynErr(n); }
        }

        bool StartOf(int s)
        {
            return set[s, la.kind];
        }

        void ExpectWeak(int n, int follow)
        {
            if (la.kind == n) Get();
            else
            {
                SynErr(n);
                while (!StartOf(follow)) Get();
            }
        }


        bool WeakSeparator(int n, int syFol, int repFol)
        {
            int kind = la.kind;
            if (kind == n) { Get(); return true; }
            else if (StartOf(repFol)) { return false; }
            else
            {
                SynErr(n);
                while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind]))
                {
                    Get();
                    kind = la.kind;
                }
                return StartOf(syFol);
            }
        }


        void py2KP()
        {
            W0("(*", "*)");
            while (la.kind == 24)
            {
                Comm();
            }
            Expect(3);
            Expect(4);
            Expect(5);
            Expect(6);
            Ident();
            name = t.val; name0 = name;
            W("MODULE " + t.val + ";"); u++;
            W("IMPORT Log:=StdLog;");
            Expect(7);
            kg = 0; W("VAR ");
            while (la.kind == 9)
            {
                GlobDecl();
                Expect(5);
            }
            while (la.kind == 14)
            {
                ProcDecl();
            }
            Expect(8);
        }

        void Comm()
        {
            string s = "(*";
            Expect(24);
            while (StartOf(1))
            {
                Get();
                s += t.val;
            }
            Expect(24);
            Wc(s8(s) + "*)");
        }

        void Ident()
        {
            Expect(1);
        }

        void GlobDecl()
        {

            string s = "";
            Expect(9);
            Ident();
            mg[kg++] = t.val; u++;
            s = (t.val.Substring(0, 1) == "L") ? "BOOLEAN" : "INTEGER";

            if (la.kind == 10)
            {
                Get();
                if (la.kind == 11)
                {
                    Get();
                }
                else if (la.kind == 12)
                {
                    Get();
                }
                else if (la.kind == 2)
                {
                    Get();
                }
                else SynErr(46);
            }
            while (la.kind == 13)
            {
                Get();
                Ident();
                mg[kg++] = t.val;
                if (la.kind == 10)
                {
                    Get();
                    if (la.kind == 11)
                    {
                        Get();
                    }
                    else if (la.kind == 12)
                    {
                        Get();
                    }
                    else if (la.kind == 2)
                    {
                        Get();
                    }
                    else SynErr(47);
                }
            }
            W(mg[k1]);
            for (int i = k1 + 1; i < kg; i++) Wc("," + mg[i]);
            Wc(":"); Wc(s); Wc(";"); k1 = kg; u--;

        }

        void ProcDecl()
        {

            Expect(14);
            Ident();
            name = t.val;
            Expect(15);
            Expect(16);
            Expect(7);
            if (name != "main")
            {
                W("PROCEDURE " + name + ";"); u++; W("VAR "); u++;
            }
            else { u--; W("BEGIN "); u++; }

            if (StartOf(2))
            {
                while (la.kind == 5 || la.kind == 9)
                {
                    VarDecl();
                    Expect(5);
                }
                u--;
            }
            if (name != "main") { u--; W("BEGIN "); }
            u++;
            if (StartOf(3))
            {
                StatSeq();
            }
            u--;
            if (la.kind == 17)
            {
                Get();
                Expect(18);
                Expect(10);
                Expect(19);
                Expect(20);
                Expect(21);
                Expect(22);
                Expect(10);
                Expect(23);
            }
            Expect(8);
            if (name != "main") { W("END " + name + ";"); }
            else W("END " + name0 + ".");

        }

        void VarDecl()
        {
            string s = "";
            while (la.kind == 9)
            {
                Get();
                Ident();
                mv[k++] = t.val;
                s = (t.val.Substring(0, 1) == "L") ? "BOOLEAN" : "INTEGER";

                if (la.kind == 10)
                {
                    Get();
                    if (la.kind == 11)
                    {
                        Get();
                    }
                    else if (la.kind == 12)
                    {
                        Get();
                    }
                    else if (la.kind == 2)
                    {
                        Get();
                    }
                    else SynErr(48);
                }
                while (la.kind == 13)
                {
                    Get();
                    Ident();
                    mv[k++] = t.val;
                    if (la.kind == 10)
                    {
                        Get();
                        if (la.kind == 11)
                        {
                            Get();
                        }
                        else if (la.kind == 12)
                        {
                            Get();
                        }
                        else if (la.kind == 2)
                        {
                            Get();
                        }
                        else SynErr(49);
                    }
                }
                W(mv[k2]);
                for (int i = k2 + 1; i < k; i++) Wc("," + mv[i]);
                Wc(":"); Wc(s + ";"); k2 = k;

            }
        }

        void StatSeq()
        {
            while (la.kind == 24)
            {
                Comm();
            }
            Stat();
            while (StartOf(3))
            {
                Wc(";");
                while (la.kind == 24)
                {
                    Comm();
                }
                Stat();
            }
            if (la.kind == 24)
            {
                Comm();
            }
        }

        void Stat()
        {
            string e, c1;
            if (la.kind == 14)
            {

                ProcDecl();
            }
            else if (la.kind == 25)
            {
                Get();
                Expect(26);
                Expr(out e);
                Expect(27);
                if (la.kind == 5)
                {
                    Get();
                }
                c1 = (e.Substring(0, 1) == "L") ? "Bool" : "Int";
                W("Log." + c1 + "(" + e + ")");
            }
            else if (la.kind == 1)
            {
                Ident();
                W(t.val);
                if (la.kind == 10 || la.kind == 15)
                {
                    if (la.kind == 10)
                    {
                        Get();
                        Expr(out e);
                        if (la.kind == 5)
                        {
                            Get();
                        }
                        Wc(" := " + e);
                    }
                    else
                    {
                        Get();
                        if (la.kind == 5)
                        {
                            Get();
                        }
                    }
                }
            }
            else if (la.kind == 28)
            {
                Get();
                Expect(26);
                Expr(out e);
                Expect(27);
                Expect(7);
                W("IF " + e + " THEN"); u++;
                StatSeq();
                Expect(8);
                if (la.kind == 29)
                {
                    Get();
                    while (la.kind == 28)
                    {
                        Get();
                        Expect(26);
                        Expr(out e);
                        Expect(27);
                        Expect(7);
                        u--; W("ELSIF " + e + " THEN"); u++;
                        StatSeq();
                        Expect(8);
                        Expect(29);
                    }
                    Expect(7);
                    u--; W("ELSE"); u++;
                    StatSeq();
                    Expect(8);
                    u--; W("END");
                }
            }
            else if (la.kind == 30)
            {
                Get();
                Expect(26);
                Expr(out e);
                Expect(27);
                Expect(7);
                W("WHILE " + e + " DO"); u++;
                StatSeq();
                Expect(8);
                u--; W("END");
            }
            else SynErr(50);
        }

        void Expr(out string e)
        {
            string e1, e2 = "", op = "";
            SimpleExpr(out e1);
            if (StartOf(4))
            {
                RelOp(out op);
                SimpleExpr(out e2);
            }
            e = e1 + op + e2;
        }

        void SimpleExpr(out string e)
        {
            string e1, e2, op; e = "";
            if (la.kind == 31 || la.kind == 32)
            {
                if (la.kind == 31)
                {
                    Get();
                }
                else
                {
                    Get();
                    e = t.val;
                }
            }
            Term(out e1);
            e += e1;
            while (la.kind == 31 || la.kind == 32 || la.kind == 40)
            {
                AddOp(out op);
                Term(out e2);
                e += (op + e2);
            }
        }

        void RelOp(out string e)
        {
            e = "";
            if (la.kind == 34)
            {
                Get();
                e = "=";
            }
            else if (la.kind == 35)
            {
                Get();
                e = "#";
            }
            else if (StartOf(5))
            {
                if (la.kind == 36)
                {
                    Get();
                }
                else if (la.kind == 37)
                {
                    Get();
                }
                else if (la.kind == 38)
                {
                    Get();
                }
                else
                {
                    Get();
                }
                e = t.val;
            }
            else SynErr(51);
        }

        void Term(out string e)
        {
            string e1, e2, op;
            Factor(out e1);
            e = e1;
            while (StartOf(6))
            {
                MulOp(out op);
                Factor(out e2);
                e += (op + e2);
            }
        }

        void AddOp(out string e)
        {
            e = "";
            if (la.kind == 31 || la.kind == 32)
            {
                if (la.kind == 31)
                {
                    Get();
                }
                else
                {
                    Get();
                }
                e = t.val;
            }
            else if (la.kind == 40)
            {
                Get();
                e = " OR ";
            }
            else SynErr(52);
        }

        void Factor(out string e)
        {
            e = "";
            if (la.kind == 1 || la.kind == 2)
            {
                if (la.kind == 1)
                {
                    Ident();
                }
                else
                {
                    Get();
                }
                e = t.val;
            }
            else if (la.kind == 26)
            {
                Get();
                Expr(out e);
                Expect(27);
                e = "(" + e + ")";
            }
            else if (la.kind == 33)
            {
                Get();
                Factor(out e);
                e = "~ " + e;
            }
            else if (la.kind == 11)
            {
                Get();
                e = "TRUE";
            }
            else if (la.kind == 12)
            {
                Get();
                e = "FALSE";
            }
            else SynErr(53);
        }

        void MulOp(out string e)
        {
            e = "";
            if (la.kind == 41)
            {
                Get();
                e = t.val;
            }
            else if (la.kind == 42)
            {
                Get();
                e = " DIV ";
            }
            else if (la.kind == 43)
            {
                Get();
                e = " MOD ";
            }
            else if (la.kind == 44)
            {
                Get();
                e = " & ";
            }
            else SynErr(54);
        }



        public void Parse()
        {
            la = new Token();
            la.val = "";
            Get();
            py2KP();
            Expect(0);

        }

        static readonly bool[,] set = {
        {T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
        {x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x},
        {x,T,x,x, x,T,x,x, T,T,x,x, x,x,T,x, x,T,x,x, x,x,x,x, T,T,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
        {x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, T,T,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x,x, x,x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x}

    };
    } // end Parser

}