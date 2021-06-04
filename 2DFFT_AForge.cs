using System;
using System.Drawing;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Imaging.ComplexFilters;

namespace tt
{​​
    class MainClass
    {​​
        public static void Main(string[] args)
        {​​
            // load image
            Bitmap image = AForge.Imaging.Image.FromFile(@"/Users/kerb/rose.jpg");

            // create complex image
            ComplexImage complexImage = ComplexImage.FromBitmap(image);
            // do forward Fourier transformation
            complexImage.ForwardFourierTransform();
            // get complex image as bitmat
            Bitmap fourierImage = complexImage.ToBitmap();
            //Bitmap fourierImage = complexImage.ForwardFourierTransform()
                                              
            // save image 
            fourierImage.Save(@"/Users/kerb/filtered_space2.jpg");

            Console.Write("Good~!");
        }​​
    }​​
}​​