using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace TranslatePrograms.Models
{
    public class ParserManager
    {
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static IParser CreateParser(string text, string lang1, string lang2)
        {
            Stream s = GenerateStreamFromString(text);
            if (lang1 == "Pascal")
            {
                if (lang2 == "C#")
                    return new ParserPascalToCSharp(new ScannerPascalToCSharp(s));
                else if (lang2 == "Python")
                    return new ParserPascalToPython(new ScannerPascalToPython(s));
                else if (lang2 == "C++")
                    return new ParserPascalToCpp(new ScannerPascalToCpp(s));
            }
            else if (lang1 == "C#" && lang2 == "Pascal")
            {
               // if (lang2 == "Pascal" && lang2 == "Pascal")
                    return new ParserCSharpToPascal(new ScannerCSharpToPascal(s));
            }
            else if (lang1 == "Python" && lang2 == "Pascal")
            {
             //   if (lang2 == "Pascal")
                    return new ParserPythonToPascal(new ScannerPythonToPascal(s));
            }
            else if (lang1 == "C++" && lang2 == "Pascal")
            {
               // if (lang2 == "Pascal")
                    return new ParserCppToPascal(new ScannerCppToPascal(s));
            }
            else
            {
                return CreateParser(CodeConverter.ConvertCode(text, lang1, "Pascal"), "Pascal", lang2);
            }
            throw new Exception($"не существующие языки {lang1} или {lang2}");
            //return new ParserPascalToCSharp(new ScannerPascalToCSharp(s));
        }
    }
}