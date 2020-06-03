using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using TranslatePrograms.Models;

namespace TranslatePrograms.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
        public ActionResult Index1()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [System.Web.Http.HttpPost]
        public JsonResult allTranslate([FromBody]List<Lang> langs)
        {
            Lang lang1 = langs.Where(x => x.text != String.Empty).First();
            if (lang1.text != null)
                langs.ForEach(x =>
                {
                    x.text = x.name != lang1.name ? CodeConverter.ConvertCode(lang1.text, lang1.name, x.name) : lang1.text;
                });
            return Json(langs, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.HttpPost]
        public JsonResult Translate([FromBody]Translang tl)
        {
            //return Json(tl.text1+$" - Переведен из языка {tl.lang1} в язык {tl.lang2}", JsonRequestBehavior.AllowGet);
            return Json(CodeConverter.ConvertCode(tl.text1, tl.lang1, tl.lang2), JsonRequestBehavior.AllowGet);
        }



        [System.Web.Http.HttpPost]
        [ValidateInput(false)]
        public FileResult downloadmany(string Csharp, string Pascal, string Python, string Cpp)//([FromBody]List<Lang> l)
        {
            List<Lang> l = new List<Lang>();
            l.Add(new Lang { name = "C#", text = Csharp });
            l.Add(new Lang { name = "Pascal", text = Pascal });
            l.Add(new Lang { name = "Python", text = Python });
            l.Add(new Lang { name = "C++", text = Cpp });
            ExportPdf pdf = new ExportPdf();
            string folder = Server.MapPath("~/Downloads/");
            //string folder = @"C:\pdfBatchPassport\";
            string path = pdf.CreatePDFList(folder, l);
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_type = "application/pdf";
            string file_name = "convertorcode.pdf";
            return File(mas, file_type, file_name);
        }

        [System.Web.Http.HttpPost]
        [ValidateInput(false)]
        public FileResult downloadone(string pname, string ptext)//([FromBody]List<Lang> l)
        {
            Lang l = new Lang { name = pname, text = ptext };
            ExportPdf pdf = new ExportPdf();
            string folder = Server.MapPath("~/Downloads/");
            //string folder = @"C:\pdfBatchPassport\";
            string path = pdf.CreatePDFLang(folder, l);
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_type = "application/pdf";
            string file_name = "code.pdf";
            return File(mas, file_type, file_name);
        }


        [System.Web.Http.HttpPost]
        public JsonResult Upload(HttpPostedFileBase upload)
        {
            if (upload != null)
            {
                // получаем имя файла
                string fileName = System.IO.Path.GetFileName(upload.FileName);
                // сохраняем файл в папку Files в проекте
              upload.SaveAs(Server.MapPath("~/Files/" + fileName));
            }
            return Json(/*CodeConverter.ConvertCode(tl.text1, tl.lang1, tl.lang2)*/"", JsonRequestBehavior.AllowGet);
        }

    }

    public class Translang
    {
        public string lang1 { get; set; }
        public string lang2 { get; set; }
        public string text1 { get; set; }
    }
    public class Lang
    {
        public string name { get; set; }
        public string text { get; set; }
    }
    //enum langs
    //{
    //    Python=1,
    //    CSharp,
    //    CPlPl,
    //    JavaScript
    //}
}
