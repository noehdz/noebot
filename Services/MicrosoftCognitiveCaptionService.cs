namespace ImageCaption.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using Microsoft.ProjectOxford.Vision;
    using Microsoft.ProjectOxford.Vision.Contract;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Xml.Linq;

    /// <summary>
    /// A wrapper around the Microsoft Cognitive Computer Vision API Service.
    /// <remarks>
    /// This class makes use of the Microsoft Computer Vision SDK.
    /// SDK: https://github.com/Microsoft/ProjectOxford-ClientSDK/blob/master/Vision/Windows/ClientLibrary"
    /// </remarks>
    /// </summary>
    public class MicrosoftCognitiveCaptionService : ICaptionService
    {
        /// <summary>
        /// Microsoft Computer Vision API key.
        /// </summary>
        private static readonly string ApiKey = WebConfigurationManager.AppSettings["MicrosoftVisionApiKey"];
        private static string respuesta = "";

        /// <summary>
        /// Microsoft Computer Vision API Endpoint.
        /// </summary>
        private static readonly string ApiEndpoint = WebConfigurationManager.AppSettings["MicrosoftVisionApiEndpoint"];

        /// <summary>
        /// The set of visual features we want from the Vision API.
        /// </summary>
        private static readonly VisualFeature[] VisualFeatures = { VisualFeature.Description };

        /// <summary>
        /// Gets the caption of an image URL.
        /// <remarks>
        /// This method calls <see cref="IVisionServiceClient.AnalyzeImageAsync(string, string[])"/> and
        /// returns the first caption from the returned <see cref="AnalysisResult.Description"/>
        /// </remarks>
        /// </summary>
        /// <param name="url">The URL to an image.</param>
        /// <returns>Description if caption found, null otherwise.</returns>
        public async Task<string> GetCaptionAsync(string url)
        {
            var client = new VisionServiceClient(ApiKey, ApiEndpoint);
            var result = await client.AnalyzeImageAsync(url, VisualFeatures);
            return ProcessAnalysisResult(result);
        }

        /// <summary>
        /// Gets the caption of the image from an image stream.
        /// <remarks>
        /// This method calls <see cref="IVisionServiceClient.AnalyzeImageAsync(Stream, string[])"/> and
        /// returns the first caption from the returned <see cref="AnalysisResult.Description"/>
        /// </remarks>
        /// </summary>
        /// <param name="stream">The stream to an image.</param>
        /// <returns>Description if caption found, null otherwise.</returns>
        public async Task<string> GetCaptionAsync(Stream stream)
        {
            var client = new VisionServiceClient(ApiKey, ApiEndpoint);
            var result = await client.AnalyzeImageAsync(stream, VisualFeatures);
            string texto = ProcessAnalysisResult(result);
            await TranslateText(texto);
            return "Al parecer enviaste una fotografia de: "+respuesta;
        }

        /// <summary>
        /// Processes the analysis result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The caption if found, error message otherwise.</returns>
        private static string ProcessAnalysisResult(AnalysisResult result)
        {
            string message = result?.Description?.Captions.FirstOrDefault()?.Text;
            return string.IsNullOrEmpty(message) ?
                        "Algo que no puedo entender" :
                        message;
        }

        private async static Task TranslateText(string texto)
        {
            string host = "https://api.microsofttranslator.com";
            string path = "/V2/Http.svc/Translate";

            // NOTE: Replace this example key with a valid subscription key.
            string key = "650e9375325c40a79d385a95d2a1217c";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string> (texto, "es-mx")
            };
                string uri = host + path + "?to=" + "es-mx"+ "&text=" + System.Net.WebUtility.UrlEncode(texto);
                HttpResponseMessage response = await client.GetAsync(uri);
                string result = await response.Content.ReadAsStringAsync();
                // NOTE: A successful response is returned in XML. You can extract the contents of the XML as follows.
                // var content = XElement.Parse(result).Value;
                respuesta = XElement.Parse(result).Value;
        }

    }

}