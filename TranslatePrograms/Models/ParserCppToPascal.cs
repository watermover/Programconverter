using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TranslatePrograms.Models
{


    public class ParserCppToPascal : IParser
    {
        public const int _EOF = 0;
        public const int _ident = 1;
        public const int _number = 2;
        public const int maxT = 44;

        const bool _T = true;
        const bool _x = false;
        const int minErrDist = 2;
        const bool T = true;
        const bool x = false;

        public ScannerCppToPascal scanner;
        public Errors errors { get; set; }

        public Token t;    // last recognized token
        public Token la;   // lookahead token
        int errDist = minErrDist;


        public string ss;
        public string name, name0;
        public string fout { get; set; }
        int u = 2;
        string[] mg = new string[100]; int kg = 0, k1 = 0, k2 = 0;
        string[] mv = new string[100]; int k = 0;
        void Wc(string x) { fout += (x); }
        void W(string x)
        {
            string su = "\r\n\t\t\t\t\t\t\t\t\t";
            //Console.Write(su.Substring(0,u)+x);
            fout += (su.Substring(0, u) + x);
        }
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
        public ParserCppToPascal(ScannerCppToPascal scanner)
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


        void cpp0KP()
        {
            //W0("(*", "*)");
            if (la.kind == 22)
            {
                Comm();
            }
            Expect(3);
            Expect(4);
            Expect(3);
            Expect(5);
            name = t.val; name0 = name;
            string[] mtxt = System.IO.File.ReadAllLines("info.txt");
            string[] m1 = mtxt[0].Split('/');
            string[] m2 = m1[1].Split('.');
            W("MODULE " + m2[0] + ";"); u++;
            W("IMPORT Log:=StdLog;"); W("VAR ");
            while (la.kind == 7)
            {
                GlobDecl();
                Expect(6);
            }
            while (la.kind == 7 || la.kind == 12)
            {
                ProcDecl();
            }
            if (StartOf(1))
            {
                StatSeq();
            }
            u--; W("END " + m2[0] + ".");
        }

        void Comm()
        {
            string s = "(*";
            Expect(22);
            while (StartOf(2))
            {
                Get();
                s += t.val;
            }
            Expect(23);
            Wc(s8(s) + "*)");
        }

        void GlobDecl()
        {

            string s, v;
            Expect(7);
            Type(out s, out v);
            Ident();
            mg[kg++] = t.val; u++;
            if (la.kind == 8)
            {
                Get();
                if (la.kind == 9)
                {
                    Get();
                }
                else if (la.kind == 10)
                {
                    Get();
                }
                else if (la.kind == 2)
                {
                    Get();
                }
                else SynErr(45);
            }
            while (la.kind == 11)
            {
                Get();
                Ident();
                mg[kg++] = t.val;
                if (la.kind == 8)
                {
                    Get();
                    if (la.kind == 9)
                    {
                        Get();
                    }
                    else if (la.kind == 10)
                    {
                        Get();
                    }
                    else if (la.kind == 2)
                    {
                        Get();
                    }
                    else SynErr(46);
                }
            }
            W(mg[k1]);
            for (int i = k1 + 1; i < kg; i++) Wc("," + mg[i]);
            Wc(":"); Wc(s); Wc(";"); k1 = kg; u--;

        }

        void ProcDecl()
        {

            if (la.kind == 7)
            {
                Get();
            }
            Expect(12);
            Ident();
            name = t.val;
            Expect(13);
            Expect(14);
            if (name != "main")
            {
                W("PROCEDURE " + name + ";"); u++; W("VAR "); u++;
            }
            else { u--; W("BEGIN "); u++; }

            if (StartOf(3))
            {
                while (la.kind == 6 || la.kind == 20 || la.kind == 21)
                {
                    VarDecl();
                    Expect(6);
                }
                u--;
            }
            if (name != "main") { u--; W("BEGIN "); }
            u++;
            if (StartOf(1))
            {
                StatSeq();
            }
            u--;
            if (la.kind == 15)
            {
                Get();
                Expect(16);
                Expect(17);
                Expect(6);
                Expect(18);
            }
            Expect(19);
            if (name != "main") { W("END " + name + ";"); }
        }

        void StatSeq()
        {
            while (la.kind == 22)
            {
                Comm();
            }
            Stat();
            while (StartOf(1))
            {
                Wc(";");
                while (la.kind == 22)
                {
                    Comm();
                }
                Stat();
            }
            if (la.kind == 22)
            {
                Comm();
            }
        }

        void Type(out string s, out string v)
        {
            s = ""; v = "";
            if (la.kind == 20)
            {
                Get();
                s = "INTEGER"; v = "0";
            }
            else if (la.kind == 21)
            {
                Get();
                s = "BOOLEAN"; v = "false";
            }
            else SynErr(47);
        }

        void Ident()
        {
            Expect(1);
        }

        void VarDecl()
        {
            string s, v;
            while (la.kind == 20 || la.kind == 21)
            {
                Type(out s, out v);
                Ident();
                mv[k++] = t.val;
                while (la.kind == 11)
                {
                    Get();
                    Ident();
                    mv[k++] = t.val;
                }
                W(mv[k2]);
                for (int i = k2 + 1; i < k; i++) Wc("," + mv[i]);
                Wc(":"); Wc(s + ";"); k2 = k;

            }
        }

        void Stat()
        {
            string e, c1;
            if (la.kind == 24)
            {

                Get();
                Expr(out e);
                Expect(25);
                Expect(6);
                c1 = (e.Substring(0, 1) == "L") ? "Bool" : "Int";
                W("Log." + c1 + "(" + e + ")");
            }
            else if (la.kind == 1)
            {
                Ident();
                W(t.val);
                if (la.kind == 8 || la.kind == 13)
                {
                    if (la.kind == 8)
                    {
                        Get();
                        Expr(out e);
                        Expect(6);
                        Wc(" := " + e);
                    }
                    else
                    {
                        Get();
                        Expect(6);
                    }
                }
            }
            else if (la.kind == 26)
            {
                Get();
                Expect(27);
                Expr(out e);
                Expect(25);
                Expect(14);
                W("IF " + e + " THEN"); u++;
                StatSeq();
                Expect(19);
                if (la.kind == 28)
                {
                    Get();
                    while (la.kind == 26)
                    {
                        Get();
                        Expect(27);
                        Expr(out e);
                        Expect(25);
                        Expect(14);
                        u--; W("ELSIF " + e + " THEN"); u++;
                        StatSeq();
                        Expect(19);
                        Expect(28);
                    }
                    Expect(14);
                    u--; W("ELSE"); u++;
                    StatSeq();
                    Expect(19);
                    u--; W("END");
                }
            }
            else if (la.kind == 29)
            {
                Get();
                Expect(27);
                Expr(out e);
                Expect(25);
                Expect(14);
                W("WHILE " + e + " DO"); u++;
                StatSeq();
                Expect(19);
                u--; W("END");
            }
            else SynErr(48);
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
            if (la.kind == 30 || la.kind == 31)
            {
                if (la.kind == 30)
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
            while (la.kind == 30 || la.kind == 31 || la.kind == 39)
            {
                AddOp(out op);
                Term(out e2);
                e += (op + e2);
            }
        }

        void RelOp(out string e)
        {
            e = "";
            if (la.kind == 33)
            {
                Get();
                e = "=";
            }
            else if (la.kind == 34)
            {
                Get();
                e = "#";
            }
            else if (StartOf(5))
            {
                if (la.kind == 35)
                {
                    Get();
                }
                else if (la.kind == 36)
                {
                    Get();
                }
                else if (la.kind == 37)
                {
                    Get();
                }
                else
                {
                    Get();
                }
                e = t.val;
            }
            else SynErr(49);
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
            if (la.kind == 30 || la.kind == 31)
            {
                if (la.kind == 30)
                {
                    Get();
                }
                else
                {
                    Get();
                }
                e = t.val;
            }
            else if (la.kind == 39)
            {
                Get();
                e = " OR ";
            }
            else SynErr(50);
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
            else if (la.kind == 27)
            {
                Get();
                Expr(out e);
                Expect(25);
                e = "(" + e + ")";
            }
            else if (la.kind == 32)
            {
                Get();
                Factor(out e);
                e = "~ " + e;
            }
            else if (la.kind == 9)
            {
                Get();
                e = "TRUE";
            }
            else if (la.kind == 10)
            {
                Get();
                e = "FALSE";
            }
            else SynErr(51);
        }

        void MulOp(out string e)
        {
            e = "";
            if (la.kind == 40)
            {
                Get();
                e = t.val;
            }
            else if (la.kind == 41)
            {
                Get();
                e = " DIV ";
            }
            else if (la.kind == 42)
            {
                Get();
                e = " MOD ";
            }
            else if (la.kind == 43)
            {
                Get();
                e = " & ";
            }
            else SynErr(52);
        }



        public void Parse()
        {
            la = new Token();
            la.val = "";
            Get();
            cpp0KP();
            Expect(0);

        }

        static readonly bool[,] set = {
        {T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
        {x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
        {x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x},
        {x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,T, T,T,T,x, T,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x}

    };
    } // end Parser
}