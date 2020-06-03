
using System;
using System.Collections.Generic;

namespace TranslatePrograms.Models
{

    public class ParserPascalToCSharp : IParser
    {
        public const int _EOF = 0;
        public const int _ident = 1;
        public const int _number = 2;
        public const int maxT = 45;

        const bool T = true;
        const bool x = false;
        const int minErrDist = 2;

        public ScannerPascalToCSharp scanner;
        public Errors errors { get; set; }

        public Token t;    // last recognized token
        public Token la;   // lookahead token
        int errDist = minErrDist;

        public string ss;
        public string fout { get; set; } //= System.IO.File.CreateText("result.txt");
        int u = 1;
        void W(string x)
        {
            string su = "\n\t\t\t\t\t\t\t\t\t";
            //Console.Write(su.Substring(0,u)+x); 
            fout+=(su.Substring(0, u) + x);
        }
        void Wc(string x) { fout += (x); }
        void W0(string s1, string s2) { fout += (s1 + System.DateTime.Now.ToString() + " " + errors.DLL + " " + errors.version + s2); }

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
        public ParserPascalToCSharp(ScannerPascalToCSharp scanner)
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


        void KP2CS()
        {
            Expect(3);
            Ident();
            //W0("//","");
            W("using System;\nclass " + t.val + "{"); u++;
            Expect(4);
            if (la.kind == 9)
            {
                ImportList();
            }
            while (la.kind == 5)
            {
                Get();
                kg = 0;
                while (la.kind == 1)
                {
                    GlobDecl();
                    Expect(4);
                }
            }
            while (la.kind == 13)
            {
                ProcDecl();
                Expect(4);
            }
            if (la.kind == 6)
            {
                Get();
                W("public static void Main() {"); u++;
                StatSeq();

            }
            u--; W("}");
            Expect(7);
            Ident();
            Expect(8);
            u--; W("}"); 
        }

        void Ident()
        {
            Expect(1);
        }

        void ImportList()
        {
            Expect(9);
            if (la.kind == 1)
            {
                Get();
                Expect(10);
            }
            Expect(1);
            while (la.kind == 11)
            {
                Get();
                if (la.kind == 1)
                {
                    Get();
                    Expect(10);
                }
                Expect(1);
            }
            Expect(4);
        }

        void GlobDecl()
        {

            Ident();
            string s, v; mg[kg++] = t.val;
            while (la.kind == 11)
            {
                Get();
                Ident();
                mg[kg++] = t.val;
            }
            Expect(12);
            Type(out s, out v);
            W("static " + s + " " + mg[k1] + "=" + v);
            for (int i = k1 + 1; i < kg; i++) Wc("," + mg[i] + "=" + v);
            Wc(";"); k1 = kg;

        }

        void ProcDecl()
        {
            Expect(13);
            Ident();
            W("static void " + t.val + "() {"); u++;
            Expect(4);
            k = 0;
            if (la.kind == 5)
            {
                Get();
                while (la.kind == 1)
                {
                    VarDecl();
                    Expect(4);
                }
            }
            var i1 = 0;
            for (int i = 0; i < kg; i++) if (!locv(mg[i]))
                {
                    if (i1 == 0) W("//global " + mg[i]);
                    else Wc("," + mg[i]); i1++;
                }

            Expect(6);

            if (StartOf(1))
            {
                StatSeq();
            }
            Expect(7);
            Ident();
            u--; W("}");
        }

        void StatSeq()
        {
            if (la.kind == 16)
            {
                Comm();
            }
            Stat();
            while (la.kind == 4)
            {
                Get();
                if (la.kind == 16)
                {
                    Comm();
                }
                Stat();
            }
            if (la.kind == 16)
            {
                Comm();
            }
        }

        void Type(out string s, out string v)
        {
            s = ""; v = "";
            if (la.kind == 14)
            {
                Get();
                s = "int"; v = "0";
            }
            else if (la.kind == 15)
            {
                Get();
                s = "bool"; v = "false";
            }
            else SynErr(46);
        }

        void VarDecl()
        {
            Ident();
            string s, v; mv[k++] = t.val;
            while (la.kind == 11)
            {
                Get();
                Ident();
                mv[k++] = t.val;
            }
            Expect(12);
            Type(out s, out v);
            W(s + " " + mv[0]);
            for (int i = 1; i < k; i++) Wc("," + mv[i]);
            Wc(";");

        }

        void Comm()
        {
            Expect(16);
            string s = "/*";
            while (StartOf(2))
            {
                Get();
                s += t.val;
            }
            Expect(17);
            Wc(s8(s) + "*/");
        }

        void Stat()
        {
            string e; bool b = true;
            if (la.kind == 18 || la.kind == 19)
            {
                if (la.kind == 18)
                {
                    Get();
                }
                else
                {
                    Get();
                }
                Expect(20);
                Expr(out e);
                Expect(21);
                W("Console.Write(" + e + ");");
            }
            else if (la.kind == 1)
            {
                Ident();
                W(t.val);
                if (la.kind == 10)
                {
                    Get();
                    Expr(out e);
                    Wc(" = " + e + ";"); b = false;
                }
                if (b) Wc("();");
            }
            else if (la.kind == 22)
            {
                Get();
                Expr(out e);
                W("if (" + e + ") {"); u++;
                Expect(23);
                StatSeq();

                while (la.kind == 24)
                {
                    Get();
                    Expr(out e);
                    u--; W("} else if (" + e + ") {"); u++;
                    Expect(23);
                    StatSeq();
                    u--;
                }
                if (la.kind == 25)
                {
                    Get();
                    W("} else {"); u++;
                    StatSeq();
                }
                Expect(7);
                u--; W("}");
            }
            else if (la.kind == 26)
            {
                Get();
                Expr(out e);
                W("while (" + e + ") {"); u++;
                Expect(27);
                StatSeq();
                Expect(7);
                u--; W("}");
            }
            else SynErr(47);
        }

        void Expr(out string e)
        {
            string e1, e2 = "", op = "";
            SimpleExpr(out e1);
            if (StartOf(3))
            {
                RelOp(out op);
                SimpleExpr(out e2);
            }
            e = e1 + op + e2;
        }

        void SimpleExpr(out string e)
        {
            string e1, e2, op; e = "";
            if (la.kind == 28 || la.kind == 29)
            {
                if (la.kind == 28)
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
            while (la.kind == 28 || la.kind == 29 || la.kind == 39)
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
                e = "==";
            }
            else if (la.kind == 34)
            {
                Get();
                e = "!=";
            }
            else if (StartOf(4))
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
            else SynErr(48);
        }

        void Term(out string e)
        {
            string e1, e2, op;
            Factor(out e1);
            e = e1;
            while (StartOf(5))
            {
                MulOp(out op);
                Factor(out e2);
                e += (op + e2);
            }
        }

        void AddOp(out string e)
        {
            e = "";
            if (la.kind == 28 || la.kind == 29)
            {
                if (la.kind == 28)
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
                e = " || ";
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
            else if (la.kind == 20)
            {
                Get();
                Expr(out e);
                Expect(21);
                e = "(" + e + ")";
            }
            else if (la.kind == 30)
            {
                Get();
                Factor(out e);
                e = "! " + e;
            }
            else if (la.kind == 31)
            {
                Get();
                e = "true";
            }
            else if (la.kind == 32)
            {
                Get();
                e = "false";
            }
            else SynErr(50);
        }

        void MulOp(out string e)
        {
            e = "";
            if (la.kind == 40 || la.kind == 41)
            {
                if (la.kind == 40)
                {
                    Get();
                }
                else
                {
                    Get();
                }
                e = t.val;
            }
            else if (la.kind == 42)
            {
                Get();
                e = "/";
            }
            else if (la.kind == 43)
            {
                Get();
                e = "%";
            }
            else if (la.kind == 44)
            {
                Get();
                e = "&&";
            }
            else SynErr(51);
        }



        public void Parse()
        {
            la = new Token();
            la.val = "";
            Get();
            KP2CS();
            Expect(0);

        }

        static readonly bool[,] set = {
        {T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
        {x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,T, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
        {x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x},
        {x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,x,x}

    };
    } // end Parser




}