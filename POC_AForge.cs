// Calc POC button ---------------------------------------------------
private void button5_Click(object sender, EventArgs e)
{​
    // 이미지들 로드
    Bitmap fix1 = (Bitmap)pictureBox2.Image;
    Bitmap shift1 = (Bitmap)pictureBox1.Image;
    // 그레이 이미지화.
    fix1 = AForge.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(fix1) ;
    shift1 = AForge.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(shift1);
    //pictureBox1.Image = fix1;
    //pictureBox2.Image = shift1;
    // 복소 이미지버퍼화.
    AForge.Imaging.ComplexImage fixedCplx = AForge.Imaging.ComplexImage.FromBitmap(fix1) ;
    int height = fixedCplx.Data.GetLength(0);
    int width = fixedCplx.Data.GetLength(1);
    AForge.Imaging.ComplexImage shiftedCplx = AForge.Imaging.ComplexImage.FromBitmap(shift1);
    // 
    AForge.Imaging.ComplexImage SonCplx = AForge.Imaging.ComplexImage.FromBitmap(shift1);
    AForge.Imaging.ComplexImage MomCplx = AForge.Imaging.ComplexImage.FromBitmap(shift1);
    // 2D FFT
    fixedCplx.ForwardFourierTransform();
    shiftedCplx.ForwardFourierTransform();
    //pictureBox1.Image = fixedCplx.ToBitmap();
    // 복소영상의 모든 픽셀에 대해.
    for (int y = 0; y < height; y++)
    {​
        for (int x = 0; x < width; x++)
        {​
            //Calculating elementwise complex conjugate of the 
            //shifted image 2d vector
            // 복소공액 영상 계산
            shiftedCplx.Data[y, x].Im = -1 * shiftedCplx.Data[y, x].Im;
            //Elementwise multiplication to obtain cross power spectrum
            // 푸리에 분자곱
            SonCplx.Data[y, x].Re =
                fixedCplx.Data[y, x].Re * shiftedCplx.Data[y, x].Re -
                fixedCplx.Data[y, x].Im * shiftedCplx.Data[y, x].Im;
            SonCplx.Data[y, x].Im =
                fixedCplx.Data[y, x].Re * shiftedCplx.Data[y, x].Im +
                shiftedCplx.Data[y, x].Re * fixedCplx.Data[y, x].Im;
            // 이미지를  F × G *
            //dstRe[i, j] = srcRe1[i, j] * srcRe2[i, j] - srcIm1[i, j] * srcIm2[i, j];
            //dstIm[i, j] = srcRe1[i, j] * srcIm2[i, j + srcRe2[i, j] * srcIm1[i, j];
        }​
    }​
    double Spectrum = 0;
    // 복소영상의 모든 픽셀에 대해.
    for (int y = 0; y < height; y++)
    {​
        for (int x = 0; x < width; x++)
        {​
            //Elementwise normalization
            Spectrum = Math.Sqrt(SonCplx.Data[y, x].Re *
                SonCplx.Data[y, x].Re + 
                SonCplx.Data[y, x].Im * SonCplx.Data[y, x].Im);
            // 푸리에 분자곱 나누기 분모곱 절대값
            MomCplx.Data[y, x].Re =
                SonCplx.Data[y, x].Re / Spectrum;
            MomCplx.Data[y, x].Im =
                SonCplx.Data[y, x].Im / Spectrum;
            // 절대 값을 계산하고 그 값으로 나누면 | F × G * |
            //double Spectrum = Math.Sqrt(dstRe[i, j] * dstRe[i, j] + dstIm[i, j] * dstIm[i, j]);
            //dstRe[i, j] = dstRe[i, j] / spectrum;
            //dstIm[i, j] = dstIm[i, j] / spectrum;
        }​
    }​
    // 푸리에 역변환.
    //MomCplx.BackwardFourierTransform();
    MomCplx.ForwardFourierTransform();
    // get complex image as bitmat
    Bitmap bmpFour = MomCplx.ToBitmap();
    // Brighten filter
    //BrightnessCorrection filter = new BrightnessCorrection(5);
    // apply the filter
    //filter.ApplyInPlace(fourierImage);
    // Median filter
    //GaussianBlur filter = new GaussianBlur();
    // apply the filter
    //filter.ApplyInPlace(bmpFour);
    pictureBox3.Image = bmpFour;
    MessageBox.Show("IFFT, Success~");
    // collect statistics
    AForge.Imaging.ImageStatistics his = 
        new AForge.Imaging.ImageStatistics(bmpFour);
    // get gray histogram (for grayscale image)
    //Histogram histogram = his.Gray;
    //MessageBox.Show("Gray Max = " + histogram.Max.ToString());
}​
 