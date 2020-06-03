using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TranslatePrograms.Models
{

    public class ParserCSharpToPascal : IParser
    {
        public const int _EOF = 0;
        public const int _ident = 1;
        public const int _number = 2;
        public const int maxT = 41;

        const bool _T = true;
        const bool _x = false;
        const int minErrDist = 2;
        const bool T = true;
        const bool x = false;

        public string ss;
        public string fout { get; set; } //= System.IO.File.CreateText("result.txt");

        public ScannerCSharpToPascal scanner;
        public Errors errors { get; set; }

        public Token t;    // last recognized token
        public Token la;   // lookahead token
        int errDist = minErrDist;

        void W(string x)
        {
            string su = "\n\t\t\t\t\t\t\t\t\t";
            //Console.Write(su.Substring(0,u)+x); 
            fout += (su.Substring(0, u) + x);
        }
        void Wc(string x) { fout += (x); }

        public string name, name0;
        //public System.IO.StreamWriter fout = System.IO.File.CreateText("result.txt");
        int u = 2;
        string[] mg = new string[100]; int kg = 0, k1 = 0;
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

        public ParserCSharpToPascal(ScannerCSharpToPascal scanner)
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


        void CS0KP()
        {
            //W0("(*", "*)");
            while (la.kind == 19)
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
            Expect(19);
            while (StartOf(1))
            {
                Get();
                s += t.val;
            }
            Expect(20);
            Wc(s8(s) + "*)");
        }

        void Ident()
        {
            Expect(1);
        }

        void GlobDecl()
        {

            string s, v;
            Expect(9);
            Type(out s, out v);
            Ident();
            mg[kg++] = t.val; u++;
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
                else SynErr(42);
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
                    else SynErr(43);
                }
            }
            W(mg[k1]);
            for (int i = k1 + 1; i < kg; i++) Wc("," + mg[i]);
            Wc(":"); Wc(s); Wc(";"); k1 = kg; u--;

        }

        void ProcDecl()
        {

            Expect(14);
            Expect(9);
            Expect(15);
            Ident();
            name = t.val;
            Expect(16);
            Expect(7);
            if (name != "Main")
            {
                W("PROCEDURE " + name + ";"); u++; W("VAR "); u++; k = 0;
            }
            else { u--; W("BEGIN "); u++; }

            if (StartOf(2))
            {
                while (la.kind == 5 || la.kind == 17 || la.kind == 18)
                {
                    VarDecl();
                    Expect(5);
                }
                u--;
            }
            if (name != "Main") { u--; W("BEGIN "); }
            u++;
            if (StartOf(3))
            {
                StatSeq();
            }
            u--;
            Expect(8);
            if (name != "Main") { W("END " + name + ";"); }
            else W("END " + name0 + ".");

        }

        void Type(out string s, out string v)
        {
            s = ""; v = "";
            if (la.kind == 17)
            {
                Get();
                s = "INTEGER"; v = "0";
            }
            else if (la.kind == 18)
            {
                Get();
                s = "BOOLEAN"; v = "false";
            }
            else SynErr(44);
        }

        void VarDecl()
        {
            string s, v;
            while (la.kind == 17 || la.kind == 18)
            {
                Type(out s, out v);
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
                    else SynErr(45);
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
                        else SynErr(46);
                    }
                }
                W(mv[0]);
                for (int i = 1; i < k; i++) Wc("," + mv[i]);
                Wc(":" + s + ";");

            }
        }

        void StatSeq()
        {
            while (la.kind == 19)
            {
                Comm();
            }
            Stat();
            while (StartOf(3))
            {
                Wc(";");
                while (la.kind == 19)
                {
                    Comm();
                }
                Stat();
            }
            if (la.kind == 19)
            {
                Comm();
            }
        }

        void Stat()
        {
            string e, c1;
            if (la.kind == 21)
            {

                Get();
                Expect(22);
                Expr(out e);
                Expect(23);
                Expect(5);
                c1 = (e.Substring(0, 1) == "L") ? "Bool" : "Int";
                W("Log." + c1 + "(" + e + ")");
            }
            else if (la.kind == 1)
            {
                Ident();
                W(t.val);
                if (la.kind == 10 || la.kind == 16)
                {
                    if (la.kind == 10)
                    {
                        Get();
                        Expr(out e);
                        Expect(5);
                        Wc(" := " + e);
                    }
                    else
                    {
                        Get();
                        Expect(5);
                    }
                }
            }
            else if (la.kind == 24)
            {
                Get();
                Expect(22);
                Expr(out e);
                Expect(23);
                Expect(7);
                W("IF " + e + " THEN"); u++;
                StatSeq();
                Expect(8);
                if (la.kind == 25)
                {
                    Get();
                    while (la.kind == 24)
                    {
                        Get();
                        Expect(22);
                        Expr(out e);
                        Expect(23);
                        Expect(7);
                        u--; W("ELSIF " + e + " THEN"); u++;
                        StatSeq();
                        Expect(8);
                        Expect(25);
                    }
                    Expect(7);
                    u--; W("ELSE"); u++;
                    StatSeq();
                    Expect(8);
                    u--; W("END");
                }
            }
            else if (la.kind == 26)
            {
                Get();
                Expect(22);
                Expr(out e);
                Expect(23);
                Expect(7);
                W("WHILE " + e + " DO"); u++;
                StatSeq();
                Expect(8);
                u--; W("END");
            }
            else SynErr(47);
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
            if (la.kind == 27 || la.kind == 28)
            {
                if (la.kind == 27)
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
            while (la.kind == 27 || la.kind == 28 || la.kind == 36)
            {
                AddOp(out op);
                Term(out e2);
                e += (op + e2);
            }
        }

        void RelOp(out string e)
        {
            e = "";
            if (la.kind == 30)
            {
                Get();
                e = "=";
            }
            else if (la.kind == 31)
            {
                Get();
                e = "#";
            }
            else if (StartOf(5))
            {
                if (la.kind == 32)
                {
                    Get();
                }
                else if (la.kind == 33)
                {
                    Get();
                }
                else if (la.kind == 34)
                {
                    Get();
                }
                else
                {
                    Get();
                }
                e = t.val;
            }
            else SynErr(48);
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
            if (la.kind == 27 || la.kind == 28)
            {
                if (la.kind == 27)
                {
                    Get();
                }
                else
                {
                    Get();
                }
                e = t.val;
            }
            else if (la.kind == 36)
            {
                Get();
                e = " OR ";
            }
            else SynErr(49);
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
            else if (la.kind == 22)
            {
                Get();
                Expr(out e);
                Expect(23);
                e = "(" + e + ")";
            }
            else if (la.kind == 29)
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
            else SynErr(50);
        }

        void MulOp(out string e)
        {
            e = "";
            if (la.kind == 37)
            {
                Get();
                e = t.val;
            }
            else if (la.kind == 38)
            {
                Get();
                e = " DIV ";
            }
            else if (la.kind == 39)
            {
                Get();
                e = " MOD ";
            }
            else if (la.kind == 40)
            {
                Get();
                e = " & ";
            }
            else SynErr(51);
        }



        public void Parse()
        {
            la = new Token();
            la.val = "";
            Get();
            CS0KP();
            Expect(0);

        }

        static readonly bool[,] set = {
        {T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
        {x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x},
        {x,T,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,T,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
        {x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x,x, x,x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x}

    };
    } // end Parser



}