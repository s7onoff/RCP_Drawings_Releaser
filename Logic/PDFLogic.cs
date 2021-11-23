using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;


namespace RCP_Drawings_Releaser
{
    class PDFLogic
    {
        
        public static void MergeDocsTest()
        {
            var doc3 = new PdfDocument();
            var doc1 = PdfReader.Open("C:\\Users\\s77\\Downloads\\Ticket_passenger_SERGEY_SEMENOV_VDVP19.pdf", PdfDocumentOpenMode.Import);
            var doc2 = PdfReader.Open(Path.Combine("C:\\Users\\s77\\Downloads", "500.225.00.00-WD-KM.02.13 XX.12.02.003.Warehouse. Column Plan.rev.000 (1).pdf"), PdfDocumentOpenMode.Import);
            
            doc3.AddPage(doc1.Pages[0]);
            doc3.AddPage(doc2.Pages[0]);

            doc3.Save("C:\\Users\\s77\\Downloads\\export.pdf");

        }
    }
}
