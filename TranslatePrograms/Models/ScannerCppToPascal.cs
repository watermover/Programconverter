using System;
using System.IO;
using System.Collections;

namespace TranslatePrograms.Models
{


    public class ScannerCppToPascal:IScanner
    {
        const char EOL = '\n';
        const int eofSym = 0; /* pdt */
        const int maxT = 44;
        const int noSym = 44;


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

        static ScannerCppToPascal()
        {
            start = new Hashtable(128);
            for (int i = 65; i <= 90; ++i) start[i] = 1;
            for (int i = 97; i <= 102; ++i) start[i] = 1;
            for (int i = 104; i <= 106; ++i) start[i] = 1;
            for (int i = 108; i <= 111; ++i) start[i] = 1;
            for (int i = 113; i <= 122; ++i) start[i] = 1;
            for (int i = 48; i <= 57; ++i) start[i] = 2;
            start[35] = 3;
            start[60] = 60;
            start[59] = 27;
            start[61] = 61;
            start[44] = 28;
            start[40] = 62;
            start[123] = 30;
            start[112] = 63;
            start[107] = 64;
            start[103] = 65;
            start[125] = 41;
            start[47] = 66;
            start[42] = 67;
            start[41] = 48;
            start[43] = 49;
            start[45] = 50;
            start[33] = 68;
            start[62] = 69;
            start[124] = 55;
            start[37] = 57;
            start[38] = 58;
            start[Buffer.EOF] = -1;

        }

        public ScannerCppToPascal(string fileName)
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

        public ScannerCppToPascal(Stream s)
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
                case "static": t.kind = 7; break;
                case "true": t.kind = 9; break;
                case "false": t.kind = 10; break;
                case "void": t.kind = 12; break;
                case "any": t.kind = 16; break;
                case "int": t.kind = 20; break;
                case "bool": t.kind = 21; break;
                case "if": t.kind = 26; break;
                case "else": t.kind = 28; break;
                case "while": t.kind = 29; break;
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
                    if (ch == 'i') { AddCh(); goto case 4; }
                    else { goto case 0; }
                case 4:
                    if (ch == 'n') { AddCh(); goto case 5; }
                    else { goto case 0; }
                case 5:
                    if (ch == 'c') { AddCh(); goto case 6; }
                    else { goto case 0; }
                case 6:
                    if (ch == 'l') { AddCh(); goto case 7; }
                    else { goto case 0; }
                case 7:
                    if (ch == 'u') { AddCh(); goto case 8; }
                    else { goto case 0; }
                case 8:
                    if (ch == 'd') { AddCh(); goto case 9; }
                    else { goto case 0; }
                case 9:
                    if (ch == 'e') { AddCh(); goto case 10; }
                    else { goto case 0; }
                case 10:
                    { t.kind = 3; break; }
                case 11:
                    if (ch == 't') { AddCh(); goto case 12; }
                    else { goto case 0; }
                case 12:
                    if (ch == 'd') { AddCh(); goto case 13; }
                    else { goto case 0; }
                case 13:
                    if (ch == 'i') { AddCh(); goto case 14; }
                    else { goto case 0; }
                case 14:
                    if (ch == 'o') { AddCh(); goto case 15; }
                    else { goto case 0; }
                case 15:
                    if (ch == '.') { AddCh(); goto case 16; }
                    else { goto case 0; }
                case 16:
                    if (ch == 'h') { AddCh(); goto case 17; }
                    else { goto case 0; }
                case 17:
                    if (ch == '>') { AddCh(); goto case 18; }
                    else { goto case 0; }
                case 18:
                    { t.kind = 4; break; }
                case 19:
                    if (ch == 'o') { AddCh(); goto case 20; }
                    else { goto case 0; }
                case 20:
                    if (ch == 'n') { AddCh(); goto case 21; }
                    else { goto case 0; }
                case 21:
                    if (ch == 'i') { AddCh(); goto case 22; }
                    else { goto case 0; }
                case 22:
                    if (ch == 'o') { AddCh(); goto case 23; }
                    else { goto case 0; }
                case 23:
                    if (ch == '.') { AddCh(); goto case 24; }
                    else { goto case 0; }
                case 24:
                    if (ch == 'h') { AddCh(); goto case 25; }
                    else { goto case 0; }
                case 25:
                    if (ch == '>') { AddCh(); goto case 26; }
                    else { goto case 0; }
                case 26:
                    { t.kind = 5; break; }
                case 27:
                    { t.kind = 6; break; }
                case 28:
                    { t.kind = 11; break; }
                case 29:
                    { t.kind = 13; break; }
                case 30:
                    { t.kind = 14; break; }
                case 31:
                    if (ch == 'r') { AddCh(); goto case 32; }
                    else { goto case 0; }
                case 32:
                    if (ch == 'e') { AddCh(); goto case 33; }
                    else { goto case 0; }
                case 33:
                    if (ch == 's') { AddCh(); goto case 34; }
                    else { goto case 0; }
                case 34:
                    if (ch == 's') { AddCh(); goto case 35; }
                    else { goto case 0; }
                case 35:
                    { t.kind = 15; break; }
                case 36:
                    if (ch == ')') { AddCh(); goto case 37; }
                    else { goto case 0; }
                case 37:
                    { t.kind = 17; break; }
                case 38:
                    if (ch == ')') { AddCh(); goto case 39; }
                    else { goto case 0; }
                case 39:
                    if (ch == ';') { AddCh(); goto case 40; }
                    else { goto case 0; }
                case 40:
                    { t.kind = 18; break; }
                case 41:
                    { t.kind = 19; break; }
                case 42:
                    { t.kind = 22; break; }
                case 43:
                    { t.kind = 23; break; }
                case 44:
                    if (ch == 'd') { AddCh(); goto case 45; }
                    else { goto case 0; }
                case 45:
                    if (ch == '"') { AddCh(); goto case 46; }
                    else { goto case 0; }
                case 46:
                    if (ch == ',') { AddCh(); goto case 47; }
                    else { goto case 0; }
                case 47:
                    { t.kind = 24; break; }
                case 48:
                    { t.kind = 25; break; }
                case 49:
                    { t.kind = 30; break; }
                case 50:
                    { t.kind = 31; break; }
                case 51:
                    { t.kind = 33; break; }
                case 52:
                    { t.kind = 34; break; }
                case 53:
                    { t.kind = 36; break; }
                case 54:
                    { t.kind = 38; break; }
                case 55:
                    if (ch == '|') { AddCh(); goto case 56; }
                    else { goto case 0; }
                case 56:
                    { t.kind = 39; break; }
                case 57:
                    { t.kind = 42; break; }
                case 58:
                    if (ch == '&') { AddCh(); goto case 59; }
                    else { goto case 0; }
                case 59:
                    { t.kind = 43; break; }
                case 60:
                    recEnd = pos; recKind = 35;
                    if (ch == 's') { AddCh(); goto case 11; }
                    else if (ch == 'c') { AddCh(); goto case 19; }
                    else if (ch == '=') { AddCh(); goto case 53; }
                    else { t.kind = 35; break; }
                case 61:
                    recEnd = pos; recKind = 8;
                    if (ch == '=') { AddCh(); goto case 51; }
                    else { t.kind = 8; break; }
                case 62:
                    recEnd = pos; recKind = 27;
                    if (ch == ')') { AddCh(); goto case 29; }
                    else { t.kind = 27; break; }
                case 63:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'q' || ch >= 's' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'r') { AddCh(); goto case 70; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 64:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'e') { AddCh(); goto case 71; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 65:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'e') { AddCh(); goto case 72; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 66:
                    recEnd = pos; recKind = 41;
                    if (ch == '*') { AddCh(); goto case 42; }
                    else { t.kind = 41; break; }
                case 67:
                    recEnd = pos; recKind = 40;
                    if (ch == '/') { AddCh(); goto case 43; }
                    else { t.kind = 40; break; }
                case 68:
                    recEnd = pos; recKind = 32;
                    if (ch == '=') { AddCh(); goto case 52; }
                    else { t.kind = 32; break; }
                case 69:
                    recEnd = pos; recKind = 37;
                    if (ch == '=') { AddCh(); goto case 54; }
                    else { t.kind = 37; break; }
                case 70:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'h' || ch >= 'j' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'i') { AddCh(); goto case 73; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 71:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'x' || ch == 'z') { AddCh(); goto case 1; }
                    else if (ch == 'y') { AddCh(); goto case 74; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 72:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 's' || ch >= 'u' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 't') { AddCh(); goto case 75; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 73:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'm' || ch >= 'o' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'n') { AddCh(); goto case 76; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 74:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == '"') { AddCh(); goto case 36; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 75:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'b' || ch >= 'd' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'c') { AddCh(); goto case 77; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 76:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 's' || ch >= 'u' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 't') { AddCh(); goto case 78; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 77:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'g' || ch >= 'i' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'h') { AddCh(); goto case 79; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 78:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'e' || ch >= 'g' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == 'f') { AddCh(); goto case 80; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 79:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == '(') { AddCh(); goto case 38; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 80:
                    recEnd = pos; recKind = 1;
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') { AddCh(); goto case 1; }
                    else if (ch == '(') { AddCh(); goto case 81; }
                    else { t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t; }
                case 81:
                    if (ch == '"') { AddCh(); goto case 82; }
                    else { goto case 0; }
                case 82:
                    if (ch == 'p') { AddCh(); goto case 31; }
                    else if (ch == '%') { AddCh(); goto case 44; }
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