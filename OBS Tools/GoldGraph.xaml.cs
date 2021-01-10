using OBS_Tools.Interfaces;
using RiotSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LCUSharp;
using System.Net.Http;
using RiotSharp.Endpoints.SummonerEndpoint;
using LCUSharp.Websocket;
using System.Windows.Threading;
using System.Net;
using Newtonsoft.Json;
using RiotSharp.Endpoints.StaticDataEndpoint.Champion;
using System.Drawing.Imaging;
using System.Drawing;
using IronOcr;
using System.Runtime.InteropServices;
using LiveCharts;

namespace OBS_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GoldGraph : Window
    {
        public string Basepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"TextFiles\");
        public string Imagepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"Images\");
        public RiotApi Api = RiotApi.GetDevelopmentInstance(GlobalValues.RiotAPIKey, 20, 100);
        public LeagueClientApi LCUApi;
        public List<string> SummonerNames = new List<string>();
        private DispatcherTimer Timer = new DispatcherTimer();
        public State State;
        public CSAction Actions;
        public string LatestVersion;
        public ChampionListStatic Champions;

        public double GoldBlue;
        public double GoldRed;

        public ChartValues<double> Values { get; set; }
        public string[] Labels { get; set; }

        public GoldGraph()
        {
            InitializeComponent();

            Timer.Interval = TimeSpan.FromMinutes(2);
            Timer.Tick += TimerTick;

            Values = new ChartValues<double> { 0, 0.5, 2, 0.8, 4, 3.6, 1, -2, 2, 1.4, -2.3, -4, -6, -7.5, -10.8 };

            GoldDifference.Labels = new[] { $"{Values.Min()}", "0", $"{Values.Max()}" };

            Minutes.Labels = new[] { "0", "2", "4", "6", "8", "10", "12", "14", "16", "18", "20", "22", "24", "26", "28", "30", "32", "34", "36", "38", "40", "42", "44", "46", "48", "50" };

            DataContext = this;

            //Timer.Start();
            //TimerTick(null, null);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            CaptureScreen("GoldBlueTeam.png", 760, 18);
            CaptureScreen("GoldRedTeam.png", 1142, 18);
            ReadScreen("GoldBlueTeam.png");
            ReadScreen("GoldRedTeam.png");

            Values.Add(GoldBlue - GoldRed);
        }

        public void CaptureScreen(string fileName, int x, int y)
        {
            using ( Bitmap bmp = new Bitmap(50, 20))
            {
                var bitmap = Contrast(bmp);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    String filename = fileName;
                    Opacity = .0;
                    g.CopyFromScreen(x, y, 0, 0, bitmap.Size);
                    bitmap.Save(Imagepath + "TeamGold\\" + filename);
                    Opacity = 1;
                }

            }
        }

        public void ReadScreen(string image)
        {
            string result;
            var cr = new IronTesseract();
            using (var input = new OcrInput(Imagepath + $"TeamGold\\{image}"))
            {
                input.DeNoise();
                result = cr.Read(input).Text;
            }

            int index = 0;
            double gold = 0;
            char[] tmpGoldValue = result.ToCharArray();
            char[] goldValue = new char[5];

            foreach (char c in tmpGoldValue)
            {
                if (c != ' ')
                {
                    goldValue[index] = c;
                    index++;
                }
            }

            if (double.TryParse(goldValue[0].ToString() + goldValue[1].ToString(), out double resultV))
            {
                gold += resultV;
            }

            if (double.TryParse(goldValue[3].ToString(), out double resultV2))
            {
                gold += resultV2 / 10;
            }

            if (image == "GoldBlueTeam.png")
                GoldBlue = gold;
            else if (image == "GoldRedTeam.png")
                GoldRed = gold;
        }

        public static Bitmap Contrast(Bitmap sourceBitmap)
        {
            int threshold = 100;
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                        sourceBitmap.Width, sourceBitmap.Height),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            double contrastLevel = Math.Pow((100.0 + threshold) / 100.0, 2);


            double blue = 0;
            double green = 0;
            double red = 0;


            for (int k = 0; k + 4 < pixelBuffer.Length; k += 4)
            {
                blue = ((((pixelBuffer[k] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                green = ((((pixelBuffer[k + 1] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                red = ((((pixelBuffer[k + 2] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                if (blue > 255)
                { blue = 255; }
                else if (blue < 0)
                { blue = 0; }


                if (green > 255)
                { green = 255; }
                else if (green < 0)
                { green = 0; }


                if (red > 255)
                { red = 255; }
                else if (red < 0)
                { red = 0; }


                pixelBuffer[k] = (byte)blue;
                pixelBuffer[k + 1] = (byte)green;
                pixelBuffer[k + 2] = (byte)red;
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);


            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                        resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);


            Marshal.Copy(pixelBuffer, 0, resultData.Scan0, pixelBuffer.Length);
            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }
    }
}
