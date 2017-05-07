using ProductionSupportPIP.Models;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.IO;
using System.Diagnostics;
using HtmlAgilityPack;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NotesFor.HtmlToOpenXml;
using System.Linq;
using System.Collections.Generic;

namespace ProductionSupportPIP.Controllers
{
    public class HomeController : Controller
    {  
        private static List<String> csiidArray = new List<String>();
        private static List<String> articlenameArray = new List<string>();
        private static List<String> filenameArray = new List<String>();
        private static List<String> filecontentArray = new List<String>();
        private int arrayCount = 0;

        [ValidateInput(false)]
        public ActionResult Index(String editor1, String CSIID, String ArticleTitle, Nullable<int> ID, String updateBtn)
        {
            FileNameContentModel model = new FileNameContentModel();
            
            if (Request.Form["generateBtn"] != null)
            {
                GenerateWordDocument();
                model.SuccessMessage = filecontentArray.Count() + " article(s) have been generated and saved in Downloads folder";

                csiidArray.Clear();
                articlenameArray.Clear();
                filenameArray.Clear();
                filecontentArray.Clear();
                arrayCount = 0;
            }
            else if (ID.HasValue)
            {
                model.RetrieveCSIID = csiidArray[ID.GetValueOrDefault()];
                model.RetrieveArticleName = articlenameArray[ID.GetValueOrDefault()];
                model.RetrieveFileContent = filecontentArray[ID.GetValueOrDefault()];

                model.FileNameArray = filenameArray;
                model.FileContentArray = filecontentArray;
                model.UpdateBool = true;
                model.CurrentUpdateID = ID.GetValueOrDefault();
            }
            //else if (!String.IsNullOrEmpty(CSIID) && !String.IsNullOrEmpty(ArticleTitle))
            else if (Request.Form["saveBtn"] != null)
            {
                String filename = CSIID + " - " + ArticleTitle;

                // Converting the input into proper HTML
                String encoded = HttpUtility.HtmlEncode(editor1);
                Session["Encoded"] = encoded;

                // Initialize StringWriter
                StringWriter stringWriter = new StringWriter();

                // Initialize HTMLWriter
                HtmlTextWriter writer = new HtmlTextWriter(stringWriter);

                // Write HTML elements
                writer.RenderBeginTag(HtmlTextWriterTag.Html); // Begin HTML
                writer.RenderBeginTag(HtmlTextWriterTag.Head); // Begin Head
                writer.Write("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>");
                writer.RenderEndTag(); // End Head
                writer.RenderBeginTag(HtmlTextWriterTag.Body);
                /* ========== Write User Input =============== */
                writer.Write(editor1);
                /* ========== /Write User Input =============== */
                writer.RenderEndTag(); // End Body
                writer.RenderEndTag(); // End HTML

                String output = stringWriter.ToString();
                Debug.WriteLine(output);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(output);

                csiidArray.Add(CSIID);
                articlenameArray.Add(ArticleTitle);
                filenameArray.Add(filename);
                filecontentArray.Add(output);
                
                model.FileNameArray = filenameArray;
                model.FileContentArray = filecontentArray;
                model.ArrayCount = arrayCount;
                arrayCount++;
            }
            else if (Request.Form["updateBtn"] != null)
            {
                int updateId = Convert.ToInt32(updateBtn);

                String filename = CSIID + " - " + ArticleTitle;
                String output = GenerateHTML(editor1);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(output);

                csiidArray[updateId] = CSIID;
                articlenameArray[updateId] = ArticleTitle;
                filenameArray[updateId] = filename;
                filecontentArray[updateId] = output;

                model.FileNameArray = filenameArray;
                model.FileContentArray = filecontentArray;
                model.ArrayCount = arrayCount;
                model.SuccessMessage = filename + " has ben updated";
            }
            else if (Request.Form["cancelBtn"] != null)
            {
                model.FileNameArray = filenameArray;
                model.FileContentArray = filecontentArray;
            }
            
            return View(model);
        }

        public String GenerateHTML(String editor1)
        {
            // Converting the input into proper HTML
            String encoded = HttpUtility.HtmlEncode(editor1);
            Session["Encoded"] = encoded;

            // Initialize StringWriter
            StringWriter stringWriter = new StringWriter();

            // Initialize HTMLWriter
            HtmlTextWriter writer = new HtmlTextWriter(stringWriter);

            // Write HTML elements
            writer.RenderBeginTag(HtmlTextWriterTag.Html); // Begin HTML
            writer.RenderBeginTag(HtmlTextWriterTag.Head); // Begin Head
            writer.Write("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>");
            writer.RenderEndTag(); // End Head
            writer.RenderBeginTag(HtmlTextWriterTag.Body);
            /* ========== Write User Input =============== */
            writer.Write(editor1);
            /* ========== /Write User Input =============== */
            writer.RenderEndTag(); // End Body
            writer.RenderEndTag(); // End HTML

            return stringWriter.ToString();
        }

        public static void GenerateWordDocument()
        {
            FileNameContentModel model = new FileNameContentModel();
        
            for (int i = 0; i < filecontentArray.Count(); i++)
            {
                String output = filecontentArray[i];
                String filedirectory = @"C:\Users\Patrick\Desktop\Joscelin\data\" + filenameArray[i] + ".docx";

                byte[] byteOutput = HtmlToWord(output);
                using (Stream file = System.IO.File.OpenWrite(filedirectory))
                {
                    file.Write(byteOutput, 0, byteOutput.Length);
                }
            }

        }

        public static byte[] HtmlToWord(String html)
        {
            using (MemoryStream generatedDocument = new MemoryStream())
            {
                using (WordprocessingDocument package = WordprocessingDocument.Create(generatedDocument, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = package.MainDocumentPart;
                    if (mainPart == null)
                    {
                        mainPart = package.AddMainDocumentPart();
                        new Document(new Body()).Save(mainPart);
                    }

                    HtmlConverter converter = new HtmlConverter(mainPart);
                    Body body = mainPart.Document.Body;

                    var paragraphs = converter.Parse(html);
                    for (int i = 0; i < paragraphs.Count; i++)
                    {
                        body.Append(paragraphs[i]);
                    }

                    mainPart.Document.Save();
                }
     
                return generatedDocument.ToArray();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}