using System;
using System.Collections.Generic;
using System.Text;
using IronOcr;

namespace OBS_Tools
{
    public class ImageReader
    {
        public string ReadImage()
        {
            string result = new IronTesseract().Read(@"C:\Users\JanPhilippThies\Desktop\cs.png").Text;
            return result;
        }
    }
}
