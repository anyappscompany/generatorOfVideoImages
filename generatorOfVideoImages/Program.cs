using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using Gif.Components;
using System.Threading;
using System.Diagnostics;
using BytescoutImageToVideo;
using Splicer;
using Splicer.Timeline;
using Splicer.Renderer;

namespace generatorOfVideoImages
{
    class Program
    {
        static string line;
        static List<string> kw = new List<string>();
        static void Main(string[] args)
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "uploads");
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);

            StreamReader fkwords = new StreamReader(@"kwords.txt");
            while ((line = fkwords.ReadLine()) != null)
            {
                kw.Add(line);
                Console.WriteLine("Added: " + line);
            }
            using (var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "video/upload.csv", false, Encoding.GetEncoding("utf-8")))
            {
                //sw.WriteLine("Title,Description,Tags,Category,Private,Path");
                foreach (string slovo in kw)
                {
                    Console.WriteLine("Current Word: " + slovo);

                    if (downloadImages(slovo) > 0)
                    {
                        // создание gif из скачаных
                        if (createTmpGifImage(slovo))
                        {
                            /*
Title,Description,Tags,Category,Private,Path
Title One,The first test video,"water",Education,TRUE,c:\videos\file2one.avi
Title Two,The second test video,"humor",Entertainment,TRUE,c:\videos\filetwo.avi
                             * 
                             Gaming 
Vlog 
Pranks 
Sketch 
Music 
Parody 
Comedy 
Informative 
Animal 
Food 
Sport 
Reviews 
Babies 
Fail 
Make up/fashion tutorials 
Music cover videos 
Action 
Art tutorials 
Reaction
                             * 
                             */
                            sw.WriteLine(FirstUpper(slovo) + "05452" + FirstUpper(slovo) + "05452" + FirstUpper(slovo) + "05452" + "People" + "05452" + "TRUE" + "05452" + FirstUpper(slovo) + ".wmv");
                            //загрузка на сервис
                            Console.WriteLine("Картинка GIF создана");                            

                        }
                        else
                        {
                            Console.WriteLine("Ошибка создания GIF картинки");
                            
                            continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при загрузке картинок");                        
                        continue;
                    }



                }
            }

            Console.WriteLine("Создание видео завершено");
            Console.ReadKey();
        }

        private static string GET(string Url)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.Stream stream = resp.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }        
        private static int downloadImages(string kw, string start = "0")
        {
            int count = 0;
            // удалить файлы из папки uploads
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "uploads\\" + kw);
            //foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            //foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            label1:
            //https://www.google.com.ua/search?q=c+sharp+get+запрос
            string imjJSON = GET("https://ajax.googleapis.com/ajax/services/search/images?v=1.0&q=" + kw + "&rsz=8&start=" + start + "");
            
            //System.IO.File.WriteAllText(@"WriteText.txt", imjJSON);
            Regex newReg = new Regex("unescapedUrl\":\"(?<val>.*?)\",\"url");
            MatchCollection matches = newReg.Matches(imjJSON);
            if (matches.Count > 0)
            {
                List<string> imgUrls = new List<string>();
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"WriteLines2.txt", true))
                {
                    foreach (Match mat in matches)
                    {
                        string e = Path.GetExtension(mat.Groups["val"].Value);
                        if (e == ".png" || e == ".jpg" || e == ".gif" || e == ".jpeg")
                        {
                            file.WriteLine(mat.Groups["val"].Value);
                            //Console.WriteLine(mat.Groups["val"].Value);
                            imgUrls.Add(mat.Groups["val"].Value);
                        }
                    }
                }
                Console.WriteLine("Get total Images: " + imgUrls.Count());

                // скачивание картинок
                foreach (string urlkartinka in imgUrls)
                {
                    Console.WriteLine(urlkartinka);
                    string filname;
                    try
                    {
                        filname = Path.GetFileName(Uri.UnescapeDataString(urlkartinka).Replace("/", "\\"));
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }

                    WebClient webClient = new WebClient();
                    //Console.WriteLine("ot " + urlkartinka + "v " + directory + "**" + filname); Console.WriteLine(directory + @"\" + filname);
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "uploads\\" + kw);
                    try
                    {
                        webClient.DownloadFile(urlkartinka, directory + @"\" + filname);
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }
                }
                return matches.Count;
            }
            
            if (count < 5)
            {
                count++;
                Thread.Sleep(3000);
                goto label1;
            }
            return 0;
        }
        private static bool createTmpGifImage(string kw)
        {
            /*DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "uploads\\" + kw);
            Random rnd1 = new Random();
            AnimatedGifEncoder e = new AnimatedGifEncoder();


            e.Start(AppDomain.CurrentDomain.BaseDirectory + "\\uploads\\tmp.gif");
            
            e.SetRepeat(0);
            e.SetDelay(1000);
            e.SetSize(640, 480);
            foreach (System.IO.FileInfo file in directory.GetFiles())
            {
                Console.WriteLine(file.Name);
                Image newImage = ScaleImage(Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "uploads\\" + kw + "\\" + file.Name), 640, 480); //Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "uploads\\" + kw + "\\" + file.Name);
                e.AddFrame(newImage);               
            }
          
            
            
            
            e.Finish();
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\uploads\\tmp.gif")) { return true; }*/
           
            Console.Write("Converting JPG slides into video, please wait..");
 
// Create BytescoutImageToVideoLib.ImageToVideo object instance
ImageToVideo converter = new ImageToVideo();
 
// Activate the component
converter.RegistrationName = "demo";
converter.RegistrationKey = "demo";
 
// Add images and set the duration for every slide
Slide slide;
DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "uploads\\" + kw);
foreach (System.IO.FileInfo file in directory.GetFiles())
{
    try
    {
        slide = (Slide)converter.AddImageFromFileName(AppDomain.CurrentDomain.BaseDirectory + "uploads\\" + kw + "\\" + file.Name);
        slide.Duration = 3000; // 3000ms = 3s
    }
    catch(Exception ex)
    {
        //
    }
}

/*slide = (Slide) converter.AddImageFromFileName("..\\..\\..\\..\\slide1.jpg"); 
slide.Duration = 3000; // 3000ms = 3s

slide = (Slide) converter.AddImageFromFileName("..\\..\\..\\..\\slide2.jpg"); 
slide.Duration = 3000;

slide = (Slide) converter.AddImageFromFileName("..\\..\\..\\..\\slide3.jpg"); 
slide.Duration = 3000;*/



 
// Set output video size
converter.OutputWidth = 640;
 
converter.OutputHeight = 480;
 
// Set output video file name
converter.OutputVideoFileName = AppDomain.CurrentDomain.BaseDirectory + "video/" + FirstUpper(kw) + ".wmv";

// Run the conversion
converter.RunAndWait();
 
Console.WriteLine("Conversion is done. Press any key to continue..");
 
//Console.ReadKey();
 
// Open the result video file in default media player
//Process.Start(AppDomain.CurrentDomain.BaseDirectory + "video/" + FirstUpper(kw) + ".wmv");

            return true;
        }
 
        

        private static string FirstUpper(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
       

        static Image ScaleImage(Image source, int width, int height)
        {

            Image dest = new Bitmap(width, height);
            using (Graphics gr = Graphics.FromImage(dest))
            {
                gr.FillRectangle(Brushes.White, 0, 0, width, height);  // Очищаем экран
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                float srcwidth = source.Width;
                float srcheight = source.Height;
                float dstwidth = width;
                float dstheight = height;

                if (srcwidth <= dstwidth && srcheight <= dstheight)  // Исходное изображение меньше целевого
                {
                    int left = (width - source.Width) / 2;
                    int top = (height - source.Height) / 2;
                    gr.DrawImage(source, left, top, source.Width, source.Height);
                }
                else if (srcwidth / srcheight > dstwidth / dstheight)  // Пропорции исходного изображения более широкие
                {
                    float cy = srcheight / srcwidth * dstwidth;
                    float top = ((float)dstheight - cy) / 2.0f;
                    if (top < 1.0f) top = 0;
                    gr.DrawImage(source, 0, top, dstwidth, cy);
                }
                else  // Пропорции исходного изображения более узкие
                {
                    float cx = srcwidth / srcheight * dstheight;
                    float left = ((float)dstwidth - cx) / 2.0f;
                    if (left < 1.0f) left = 0;
                    gr.DrawImage(source, left, 0, cx, dstheight);
                }

                return dest;
            }
        }
    }
}
