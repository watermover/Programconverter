using System;
using System.IO;
using System.Collections;

namespace TranslatePrograms.Models
{
    public class ScannerPythonToPascal:IScanner
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

        static ScannerPythonToPascal()
        {
            start = new Hashtable(128);
            for (int i = 65; i <= 82; ++i) start[i] = 1;
            for (int i = 84; i <= 90; ++i) start[i] = 1;
            for (int i = 97; i <= 104; ++i) start[i] = 1;
            for (int i = 106; i <= 122; ++i) start[i] = 1;
            for (int i = 48; i <= 57; ++i) start[i] = 2;
            start[59] = 3;
            start[123] = 4;
            start[125] = 5;
            start[61] = 45;
            start[44] = 6;
            start[40] = 46;
            start[58] = 8;
            start[83] = 47;
            start[105] = 48;
            start[35] = 33;
            start[41] = 34;
            start[43] = 35;
            start[45] = 36;
            start[33] = 38;
            start[60] = 49;
            start[62] = 50;
            start[42] = 42;
            start[47] = 43;
            start[37] = 44;
            start[Buffer.EOF] = -1;

        }

        public ScannerPythonToPascal(string fileName)
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

        public ScannerPythonToPascal(Stream s)
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
                case "using": t.kind = 3; break;
                case "System": t.kind = 4; break;
                case "class": t.kind = 6; break;
                case "var": t.kind = 9; break;
                case "true": t.kind = 11; break;
                case "false": t.kind = 12; break;
                case "def": t.kind = 14; break;
                case "Scanner": t.kind = 17; break;
                case "in": t.kind = 18; break;
                case "new": t.kind = 19; break;
                case "String": t.kind = 21; break;
                case "ss": t.kind = 22; break;
                case "print": t.kind = 25; break;
                case "if": t.kind = 28; break;
                case "else": t.kind = 29; break;
                case "while": t.kind = 30; break;
                case "not": t.kind = 33; break;
                case "or": t.kind = 40; break;
                case "and": t.kind = 44; break;
                default: break;
            }
        }

        Token NextToken()
        {
            while (ch == ' ' ||
                ch == 10 || ch == 13
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
                    { t.kind = 5; break; }
                case 4:
                    { t.kind = 7; break; }
                case 5:
                    { t.kind = 8; break; }
                case 6:
                    { t.kind = 13; break; }
                case 7:
                    { t.kind = 15; break; }
                case 8:
                    { t.kind = 16; break; }
                case 9:
                    if (ch == 'S') { AddCh(); goto case 10; }
                    else { goto case 0; }
                case 10:
                    if (ch == 'y') { AddCh(); goto case 11; }
                    else { goto case 0; }
                case 11:
                    if (ch == 's') { AddCh(); goto case 12; }
                    else { goto case 0; }
                case 12:
                    if (ch == 't') { AddCh(); goto case 13; }
                    else { goto case 0; }
                case 13:
                    if (ch == 'e') { AddCh(); goto case 14; }
                    else { goto case 0; }
                case 14:
                    if (ch == 'm') { AddCh(); goto case 15; }
                    else { goto case 0; }
                case 15:
                    if (ch == '.') { AddCh(); goto case 16; }
                    else { goto case 0; }
                case 16:
                    if (ch == 'i') { AddCh(); goto case 17; }
                    else { goto case 0; }
                case 17:
                    if (ch == 'n') { AddCh(); goto case 18; }
                    else { goto case 0; }
                case 18:
                    if (ch == ')') { AddCh(); goto case 19; }
                    else { goto case 0; }
                case 19:
                    if (ch == ';') { AddCh(); goto case 20; }
                    else { goto case 0; }
                case 20:
                    { t.kind = 20; break; }
                case 21:
                    if (ch == 'n') { AddCh(); goto case 22; }
                    else { goto case 0; }
                case 22:
                    if (ch == 'e') { AddCh(); goto case 23; }
                    else { goto case 0; }
                case 23:
                    if (ch == 'x') { AddCh(); goto case 24; }
                    else { goto case 0; }
                case 24:
                    if (ch == 't') { AddCh(); goto case 25; }
                    else { goto case 0; }
                case 25:
                    if (ch == 'L') { AddCh(); goto case 26; }
                    else { goto case 0; }
                case 26:
                    if (ch == 'i') { AddCh(); goto case 27; }
                    else { goto case 0; }
                case 27:
                    if (ch == 'n') { AddCh(); goto case 28; }
                    else { goto case 0; }
                case 28:
                    if (ch == 'e') { AddCh(); goto case 29; }
                    else { goto case 0; }
                case 29:
                    if (ch == '(') { AddCh(); goto case 30; }
                    else { goto case 0; }
                case 30:
                    if (ch == ')') { AddCh(); goto case 31; }
                    else { goto case 0; }
                case 31:
                    if (ch == ';') { AddCh(); goto case 32; }
                    else { goto case 0; }
                case 32:
                    { t.kind = 23; break; }
                case 33:
                    { t.kind = 24; break; }
                case 34:
                    { t.kind = 27; break; }
                case 35:
                    { t.kind = 31; break; }
                case 36:
                    { t.kind = 32; break; }
                case 37:
                    { t.kind = 34; break; }
                case 38:
                    if (ch == '=') { AddCh(); goto case 39; }
                    else { goto case 0; }
                case 39:
                    { t.kind = 35; break; }
                case 40:
                    { t.kind = 37; break; }
                case 41:
                    { t.kind = 39; break; }
                case 42:
                    { t.kind = 41; break; }
                case 43:
                    { t.kind = 42; break; }
                case 44:
                    { t.kind = 43; break; }
                case 45:
                    recEnd = pos; recKind = 10;
                    if (ch == '=') { AddCh(); goto case 37; }
                    else { t.kind = 10; break; }
                case 46:
                    recEnd = pos; recKind = 26;
                    if (ch == ')') { AddCh(); goto case 7; }
                    else { t.kind = 26; break; }
                case 47:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'b' || ch >= 'd' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'c') { AddCh(); goto case 51; }
                    else { t.kind = 1; t.val   = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 48:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'm' || ch >= 'o' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'n') { AddCh(); goto case 52; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 49:
                    recEnd = pos; recKind = 36;
                    if (ch == '=') { AddCh(); goto case 40; }
                    else { t.kind = 36; break; }
                case 50:
                    recEnd = pos; recKind = 38;
                    if (ch == '=') { AddCh(); goto case 41; }
                    else { t.kind = 38; break; }
                case 51:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'b' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'a') { AddCh(); goto case 53; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 52:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == '.') { AddCh(); goto case 21; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 53:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'm' || ch >= 'o' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'n') { AddCh(); goto case 54; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 54:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'm' || ch >= 'o' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'n') { AddCh(); goto case 55; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 55:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'e') { AddCh(); goto case 56; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 56:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'q' || ch >= 's' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'r') { AddCh(); goto case 57; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 57:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == '(') { AddCh(); goto case 9; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }

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