using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using TranslatePrograms.Controllers;

namespace TranslatePrograms.Models
{
    public class ExportPdf
    {
        public string CreatePDFList(string folder, List<Lang> langs)
        {
            iTextSharp.text.Document doc = new iTextSharp.text.Document();
            string path = Path.Combine(folder, Guid.NewGuid().ToString() + ".pdf");
            doc.SetMargins(0f, 0f, 2f, 0f);
            PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            //Открываем документ
            doc.Open();
            BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);
            PdfPTable table = new PdfPTable(1);
            PdfPCell cell = new PdfPCell(new Phrase("Конвертирование языков программирования", font));
            cell.Colspan = 0;
            cell.HorizontalAlignment = 1;
            cell.Border = 0;
            table.AddCell(cell);
            doc.Add(table);
            PdfPTable ctable = new PdfPTable(4);
            for (int j = 0; j < langs.Count; j++)
            {
                AddHeaderCell(ctable, langs[j].name, font);
            }
            for (int j = 0; j < langs.Count; j++)
            {
                AddTextCell(ctable, langs[j].text, font);
            }
            doc.Add(ctable);
            doc.Close();
            return path;
        }
        public string CreatePDFLang(string folder, Lang lang)
        {
            iTextSharp.text.Document doc = new iTextSharp.text.Document();
            string path = Path.Combine(folder, Guid.NewGuid().ToString() + ".pdf");
            doc.SetMargins(0f, 0f, 2f, 0f);
            PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            //Открываем документ
            doc.Open();
            BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);
            PdfPTable table = new PdfPTable(1);
            PdfPCell cell = new PdfPCell(new Phrase("Код "+ lang.name, font));
            cell.Colspan = 0;
            cell.HorizontalAlignment = 1;
            cell.Border = 0;
            table.AddCell(cell);
            doc.Add(table);
            table = new PdfPTable(1);
            cell = new PdfPCell(new Phrase(lang.text, font));
            cell.Colspan = 0;
            cell.HorizontalAlignment = 0;
            cell.Border = 0;
            table.AddCell(cell);
            doc.Add(table);
            doc.Close();
            return path;
        }

        void AddHeaderCell(PdfPTable table, string text, iTextSharp.text.Font font)
        {
            font.Size = 9;
            PdfPCell cell = new PdfPCell(new Phrase(new Phrase(text, font)));
            cell.HorizontalAlignment = 1;
            cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
        }
        void AddTextCell(PdfPTable table, string text, iTextSharp.text.Font font)
        {
            font.Size = 9;
            PdfPCell cell = new PdfPCell(new Phrase(new Phrase(text, font)));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
        }
    }
}