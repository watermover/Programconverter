using System;
using System.IO;
using System.Collections;

namespace TranslatePrograms.Models
{

    public class ScannerPascalToCpp:IScanner
    {
        const char EOL = '\n';
        const int eofSym = 0; /* pdt */
        const int maxT = 45;
        const int noSym = 45;


        public Buffer buffer; // scanner buffer

        Token t;          // current token
        int ch;           // current input character
        int pos;          // byte position of current character
        int charPos;      // position by unicode characters starting with 0
        int col;          // column number of current character
        int line;         // line number of current character
        int oldEols;      // EOLs that appeared in a comment;
        static readonly Hashtable start; // maps first token character to start state

        Token tokens;     // list of tokens already peeked (first token is a dummy)
        Token pt;         // current peek token

        char[] tval = new char[128]; // text of current token
        int tlen;         // length of current token

        static ScannerPascalToCpp()
        {
            start = new Hashtable(128);
            for (int i = 65; i <= 75; ++i) start[i] = 1;
            for (int i = 77; i <= 90; ++i) start[i] = 1;
            for (int i = 97; i <= 122; ++i) start[i] = 1;
            for (int i = 48; i <= 57; ++i) start[i] = 2;
            start[59] = 3;
            start[46] = 4;
            start[58] = 26;
            start[44] = 6;
            start[40] = 27;
            start[42] = 28;
            start[76] = 29;
            start[41] = 16;
            start[43] = 17;
            start[45] = 18;
            start[126] = 19;
            start[61] = 20;
            start[35] = 21;
            start[60] = 30;
            start[62] = 31;
            start[47] = 24;
            start[38] = 25;
            start[Buffer.EOF] = -1;

        }

        public ScannerPascalToCpp(string fileName)
        {
            try
            {
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                buffer = new Buffer(stream, false);
                Init();
            }
            catch (IOException)
            {
                throw new FatalError("Cannot open file " + fileName);
            }
        }

        public ScannerPascalToCpp(Stream s)
        {
            buffer = new Buffer(s, true);
            Init();
        }

        void Init()
        {
            pos = -1; line = 1; col = 0; charPos = -1;
            oldEols = 0;
            NextCh();
            if (ch == 0xEF)
            { // check optional byte order mark for UTF-8
                NextCh(); int ch1 = ch;
                NextCh(); int ch2 = ch;
                if (ch1 != 0xBB || ch2 != 0xBF)
                {
                    throw new FatalError(String.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
                }
                buffer = new UTF8Buffer(buffer); col = 0; charPos = -1;
                NextCh();
            }
            pt = tokens = new Token();  // first token is a dummy
        }

        void NextCh()
        {
            if (oldEols > 0) { ch = EOL; oldEols--; }
            else
            {
                pos = buffer.Pos;
                // buffer reads unicode chars, if UTF8 has been detected
                ch = buffer.Read(); col++; charPos++;
                // replace isolated '\r' by '\n' in order to make
                // eol handling uniform across Windows, Unix and Mac
                if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
                if (ch == EOL) { line++; col = 0; }
            }

        }

        void AddCh()
        {
            if (tlen >= tval.Length)
            {
                char[] newBuf = new char[2 * tval.Length];
                Array.Copy(tval, 0, newBuf, 0, tval.Length);
                tval = newBuf;
            }
            if (ch != Buffer.EOF)
            {
                tval[tlen++] = (char)ch;
                NextCh();
            }
        }




        void CheckLiteral()
        {
            switch (t.val)
            {
                case "MODULE": t.kind = 3; break;
                case "VAR": t.kind = 5; break;
                case "BEGIN": t.kind = 6; break;
                case "END": t.kind = 7; break;
                case "IMPORT": t.kind = 9; break;
                case "PROCEDURE": t.kind = 13; break;
                case "INTEGER": t.kind = 14; break;
                case "BOOLEAN": t.kind = 15; break;
                case "IF": t.kind = 18; break;
                case "THEN": t.kind = 19; break;
                case "ELSIF": t.kind = 20; break;
                case "ELSE": t.kind = 21; break;
                case "WHILE": t.kind = 22; break;
                case "DO": t.kind = 23; break;
                case "TRUE": t.kind = 31; break;
                case "FALSE": t.kind = 32; break;
                case "OR": t.kind = 39; break;
                case "DIV": t.kind = 42; break;
                case "MOD": t.kind = 43; break;
                default: break;
            }
        }

        Token NextToken()
        {
            while (ch == ' ' ||
                ch >= 9 && ch <= 10 || ch == 13
            ) NextCh();

            int recKind = noSym;
            int recEnd = pos;
            t = new Token();
            t.pos = pos; t.col = col; t.line = line; t.charPos = charPos;
            int state;
            if (start.ContainsKey(ch)) { state = (int)start[ch]; }
            else { state = 0; }
            tlen = 0; AddCh();

            switch (state)
            {
                case -1: { t.kind = eofSym; break; } // NextCh already done
                case 0:
                    {
                        if (recKind != noSym)
                        {
                            tlen = recEnd - t.pos;
                            SetScannerBehindT();
                        }
                        t.kind = recKind; break;
                    } // NextCh already done
                case 1:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); goto case 1; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 2:
                    recEnd = pos; recKind = 2;
                    if (ch >= '0' && ch <= '9') { AddCh(); goto case 2; }
                    else { t.kind = 2; break; }
                case 3:
                    { t.kind = 4; break; }
                case 4:
                    { t.kind = 8; break; }
                case 5:
                    { t.kind = 10; break; }
                case 6:
                    { t.kind = 11; break; }
                case 7:
                    { t.kind = 16; break; }
                case 8:
                    { t.kind = 17; break; }
                case 9:
                    if (ch == 'n') { AddCh(); goto case 10; }
                    else { goto case 0; }
                case 10:
                    if (ch == 't') { AddCh(); goto case 11; }
                    else { goto case 0; }
                case 11:
                    { t.kind = 24; break; }
                case 12:
                    if (ch == 'o') { AddCh(); goto case 13; }
                    else { goto case 0; }
                case 13:
                    if (ch == 'o') { AddCh(); goto case 14; }
                    else { goto case 0; }
                case 14:
                    if (ch == 'l') { AddCh(); goto case 15; }
                    else { goto case 0; }
                case 15:
                    { t.kind = 25; break; }
                case 16:
                    { t.kind = 27; break; }
                case 17:
                    { t.kind = 28; break; }
                case 18:
                    { t.kind = 29; break; }
                case 19:
                    { t.kind = 30; break; }
                case 20:
                    { t.kind = 33; break; }
                case 21:
                    { t.kind = 34; break; }
                case 22:
                    { t.kind = 36; break; }
                case 23:
                    { t.kind = 38; break; }
                case 24:
                    { t.kind = 41; break; }
                case 25:
                    { t.kind = 44; break; }
                case 26:
                    recEnd = pos; recKind = 12;
                    if (ch == '=') { AddCh(); goto case 5; }
                    else { t.kind = 12; break; }
                case 27:
                    recEnd = pos; recKind = 26;
                    if (ch == '*') { AddCh(); goto case 7; }
                    else { t.kind = 26; break; }
                case 28:
                    recEnd = pos; recKind = 40;
                    if (ch == ')') { AddCh(); goto case 8; }
                    else { t.kind = 40; break; }
                case 29:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'n' || ch >= 'p' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'o') { AddCh(); goto case 32; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 30:
                    recEnd = pos; recKind = 35;
                    if (ch == '=') { AddCh(); goto case 22; }
                    else { t.kind = 35; break; }
                case 31:
                    recEnd = pos; recKind = 37;
                    if (ch == '=') { AddCh(); goto case 23; }
                    else { t.kind = 37; break; }
                case 32:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'f' || ch >= 'h' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'g') { AddCh(); goto case 33; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 33:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == '.') { AddCh(); goto case 34; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 34:
                    if (ch == 'I') { AddCh(); goto case 9; }
                    else if (ch == 'B') { AddCh(); goto case 12; }
                    else { goto case 0; }

            }
            t.val = new String(tval, 0, tlen);
            return t;
        }

        private void SetScannerBehindT()
        {
            buffer.Pos = t.pos;
            NextCh();
            line = t.line; col = t.col; charPos = t.charPos;
            for (int i = 0; i < tlen; i++) NextCh();
        }

        // get the next token (possibly a token already seen during peeking)
        public Token Scan()
        {
            if (tokens.next == null)
            {
                return NextToken();
            }
            else
            {
                pt = tokens = tokens.next;
                return tokens;
            }
        }

        // peek for the next token, ignore pragmas
        public Token Peek()
        {
            do
            {
                if (pt.next == null)
                {
                    pt.next = NextToken();
                }
                pt = pt.next;
            } while (pt.kind > maxT); // skip pragmas

            return pt;
        }

        // make sure that peeking starts at the current scan position
        public void ResetPeek() { pt = tokens; }

    } // end Scanner
}