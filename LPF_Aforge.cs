using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;                  
using AForge.Imaging.ComplexFilters;    // for FFT
using AForge.Imaging.Filters;           // for Smoothing
using AForge.Math;

namespace winForm1
{​
    public partial class Form1 : Form
    {​

        Bitmap image;       // Global Bitmap

        private bool _drawRect = false;
        private bool _drawCircle = false;

        public Form1()
        {​
            InitializeComponent();
            pictureBox1.Paint += new PaintEventHandler(this.pictureBox1_Paint);

            // load image
            image =
                AForge.Imaging.Image.FromFile(@"C:\Users\user\Pictures\lena512.jpg");

        }​

        // Calc FFT2 when button --------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {​
            // create complex image
            ComplexImage complexImage = ComplexImage.FromBitmap(image);
            // do forward Fourier transformation
            complexImage.ForwardFourierTransform();
            // get complex image as bitmat
            Bitmap fourierImage = complexImage.ToBitmap();

            pictureBox1.Image = fourierImage;
        }​

        // show Original Image --------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {​
            pictureBox2.Image = image;
           
        }​

        // form exit ------------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {​
            this.Close();
        }​

        // circle drawing button ------------------------------------------
        private void button3_Click(object sender, EventArgs e)
        {​
            _drawRect = false;
            _drawCircle = true;

            pictureBox1.Invalidate();

            MessageBox.Show("Good~!");

        }​

        //
        private void pictureBox1_Click(object sender, EventArgs e)
        {​
           
        }​

        // picBox Paint event ------------------------------------------------
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {​
            if (_drawRect){​
                Graphics g = e.Graphics;
                // use g to do your drawing
                Rectangle rect = new Rectangle(100, 100, 10, 10);
                e.Graphics.DrawRectangle(new Pen(Color.Green, 20), rect);
            }​
            if (_drawCircle)
            {​
                Graphics g = e.Graphics;
                // use g to do your drawing
                Rectangle rect = new Rectangle(108, 108, 40, 40);
                e.Graphics.DrawEllipse(new Pen(Color.Red, 1), rect);
            }​
        }​

        // Calc LPF with FFT2 ----------------------------------------------
        private void button4_Click(object sender, EventArgs e)
        {​
            // create complex image
            ComplexImage complexImage = ComplexImage.FromBitmap(image);
            // do forward Fourier transformation
            complexImage.ForwardFourierTransform();
            // create filter
            FrequencyFilter filter = new FrequencyFilter(new IntRange(0, 20));
            // apply filter
            filter.Apply(complexImage);
            // do backward Fourier transformation
            complexImage.BackwardFourierTransform();
            // get complex image as bitmat
            Bitmap fourierImage = complexImage.ToBitmap();

            pictureBox1.Image = fourierImage;
        }​// button4

    }​// class Form1
}​// namespace

