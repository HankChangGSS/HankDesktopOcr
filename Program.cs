using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using Microsoft.ProjectOxford.Vision.Contract;
using System.IO;

namespace ConsoleApplication1
{
    public class OcrResultsHelper
    {
        //this code is revised from http://studyhost.blogspot.com/2016/08/microsoft-cognitive-services-2-vision.html
        public static string GetOcrResults(string subscriptionKey, string apiRoot, byte[] filebody, string languageCodes)
        {
            string result = string.Empty;

            OcrResults OcrResults = default(OcrResults);

            //建立VisionServiceClient
            var visionClient = new Microsoft.ProjectOxford.Vision.VisionServiceClient(subscriptionKey, apiRoot);

            using (var ms = new MemoryStream(filebody, true))
            {
                //以繁體中文辨識
                //OcrResults = visionClient.RecognizeTextAsync(ms, LanguageCodes.AutoDetect).Result;
                OcrResults = visionClient.RecognizeTextAsync(ms, languageCodes).Result;
            }

            //抓取每一區塊的辨識結果
            foreach (var Region in OcrResults.Regions)
            {
                //抓取每一行
                foreach (var line_loopVariable in Region.Lines)
                {
                    var line = line_loopVariable;
                    dynamic aline = "";
                    //抓取每一個字
                    foreach (var Word_loopVariable in line.Words)
                    {
                        var Word = Word_loopVariable;
                        //顯示辨識結果
                        aline += Word.Text;
                    }

                    //加換行
                    result += aline + "\n";
                }
            }

            return result;
        }
    }

    class Program
    {
        static void ValidateFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        static void Main(string[] args)
        {
			//設定檔
            string apiRoot = @"https://southeastasia.api.cognitive.microsoft.com/vision/v1.0";
			string languageCode = "zh-Hant";

            string apiKey = ConfigurationManager.AppSettings["apiKey"]; //Manage keys from Azure Computer Vision
            string folderImgSource = ConfigurationManager.AppSettings["DirectoryImgSource"];
            string folderImgMoveTo = ConfigurationManager.AppSettings["DirectoryImgMoveTo"];
            string folderText = ConfigurationManager.AppSettings["DirectoryText"];
            int timeout = Convert.ToInt32(ConfigurationManager.AppSettings["SleepSeconds"]) * 1000;

			//確認資料夾存在
            ValidateFolder(folderImgSource);
            ValidateFolder(folderImgMoveTo);
            ValidateFolder(folderText);

			//讀取資料夾中的所有圖片檔
			//1.圖片轉為文字
			//2.轉完後寫入文字檔
			//3.成功後將原始圖片檔搬移
            foreach (string path in Directory.GetFiles(folderImgSource))
            {
                try
                {
                    var fileBody = File.ReadAllBytes(path);
                    var ocrText = OcrResultsHelper.GetOcrResults(apiKey, apiRoot, fileBody, languageCode);

                    File.WriteAllText(path.Replace(folderImgSource, folderText) + ".txt", ocrText, Encoding.UTF8);
                    File.Move(path, path.Replace(folderImgSource, folderImgMoveTo));
                    Console.WriteLine(ocrText);
                    System.Threading.Thread.Sleep(timeout);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /*
         public static class LanguageCodes
        {
            public const string AutoDetect = "unk";
            public const string ChineseSimplified = "zh-Hans";
            public const string ChineseTraditional = "zh-Hant";
            public const string Czech = "cs";
            public const string Danish = "da";
            public const string Dutch = "nl";
            public const string English = "en";
            public const string Finnish = "fi";
            public const string French = "fr";
            public const string German = "de";
            public const string Greek = "el";
            public const string Hungarian = "hu";
            public const string Italian = "it";
            public const string Japanese = "ja";
            public const string Korean = "ko";
            public const string Norwegian = "nb";
            public const string Polish = "pl";
            public const string Portuguese = "pt";
            public const string Russian = "ru";
            public const string Spanish = "es";
            public const string Swedish = "sv";
            public const string Turkish = "tr";
        }
        */
    }
}
