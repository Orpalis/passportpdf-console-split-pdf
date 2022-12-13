using PassportPDF.Api;
using PassportPDF.Client;
using PassportPDF.Model;

namespace PDFSplitter
{

    public class PDFSplitter
    {
        static async Task Main(string[] args)
        {
            GlobalConfiguration.ApiKey = "YOUR-PASSPORT-CODE";

            PassportManagerApi apiManager = new();
            PassportPDFPassport passportData = await apiManager.PassportManagerGetPassportInfoAsync(GlobalConfiguration.ApiKey);

            if (passportData == null)
            {
                throw new ApiException("The Passport number given is invalid, please set a valid passport number and try again.");
            }
            else if (passportData.IsActive is false)
            {
                throw new ApiException("The Passport number given not active, please go to your PassportPDF dashboard and active your plan.");
            }

            string uri = "https://passportpdfapi.com/test/multiple_pages.pdf";

            DocumentApi api = new();

            Console.WriteLine("Loading document into PassportPDF...");
            DocumentLoadResponse document = await api.DocumentLoadFromURIAsync(new LoadDocumentFromURIParameters(uri));
            Console.WriteLine("Document loaded.");

            PDFApi pdfApi = new();

            Console.WriteLine("Splitting PDF into single-page documents..");

            PdfExtractPageResponse pdfExtractPageResponse = await pdfApi.ExtractPageAsync(new PdfExtractPageParameters(document.FileId, "*")
            {
                ExtractAsSeparate = true
            });

            if (pdfExtractPageResponse.Error is not null)
            {
                throw new ApiException(pdfExtractPageResponse.Error.ExtResultMessage);
            }
            else
            {
                Console.WriteLine("Splitting PDF document is done.");
            }

            // Download every page as a separate document
            Console.WriteLine("Downloading single page documents..");
            try
            {
                for(int i=0; i< pdfExtractPageResponse.FileIds.Count; i++)
                {
                    var pageId = pdfExtractPageResponse.FileIds[i];

                    PdfSaveDocumentResponse savePageResponse = await pdfApi.SaveDocumentAsync(new PdfSaveDocumentParameters(pageId));

                    string savePath = Path.Join(Directory.GetCurrentDirectory(), "extracted_page_" + (i+1).ToString() + ".pdf");

                    File.WriteAllBytes(savePath, savePageResponse.Data);

                    Console.WriteLine("Done downloading extracted page. Document has been saved in : {0}", savePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not download single pages! : {0}", ex.Message);
            }

        }
    }
}


