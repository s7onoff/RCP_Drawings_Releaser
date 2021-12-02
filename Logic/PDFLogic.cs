using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microsoft.Office.Interop.Word;


namespace RCP_Drawings_Releaser
{
    class PDFLogic
    {
        public static void MergeFiles(List<string> files, string path, string filename)
        {
            var resultFile = new PdfDocument();

            foreach (var file in files.Where(file => File.Exists(file)))
            {
                switch (Path.GetExtension(file).ToLower())
                {
                    case ".pdf":
                    {
                        AddPdf(file, resultFile);
                        break;
                    }
                    case ".doc":
                    {
                        AddPdf(CreatePdfFromWord(file), resultFile);
                        break;
                    }
                    case ".docx":
                    {
                        AddPdf(CreatePdfFromWord(file), resultFile);
                        break;
                    }
                    case ".rtf":
                    {
                        AddPdf(CreatePdfFromWord(file), resultFile);
                        break;
                    }
                }
            }

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    // ignored
                }
            }
            resultFile.Save(Path.Combine(path, filename));
        }

        private static void AddPdf(string file, PdfDocument exportFile)
        {
            if (!File.Exists(file)) return;
            var pdfDocument = PdfReader.Open(Path.Combine(file), PdfDocumentOpenMode.Import);
            foreach (var pdfPage in pdfDocument.Pages)
            {
                exportFile.AddPage(pdfPage);
            }
        }

        private static string CreatePdfFromWord(string file)
        {
            var appWord = new Application();

            var pdfFile = "";

            if (appWord.Documents == null) return "";
            try
            {
                var wordDocument = appWord.Documents.Open(file);
                var originDirectory = Path.GetDirectoryName(file);

                var exportDirectory = ChooseDirectory(originDirectory);
                var pdfDocName = string.Concat(Path.GetFileNameWithoutExtension(file), ".pdf");
                pdfFile = Path.Combine(exportDirectory, pdfDocName);
                if (wordDocument != null)
                {                                         
                    wordDocument.ExportAsFixedFormat(pdfFile,   
                        WdExportFormat.wdExportFormatPDF);
                    wordDocument.Close();
                }
                
                appWord.Quit();
            }
            catch (COMException)
            {
                //
            }

            return pdfFile;
        }

        private static string ChooseDirectory(string originDirectory)
        {
            if (Directory.Exists(originDirectory))
            {
                bool checker;
                try
                {
                    try
                    {
                        File.Delete(Path.Combine(originDirectory, "_"));
                    }
                    catch
                    {
                        // ignored
                    }

                    File.Create(Path.Combine(originDirectory, "_"),32,FileOptions.DeleteOnClose);
                    checker = true;
                }
                catch
                {
                    checker = false;
                }
                
                return checker ? originDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }
        
    }
}
