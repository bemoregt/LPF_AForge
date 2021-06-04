using System;
using System.Drawing;
using System.Windows.Forms;
//
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
//
using Accord.Imaging;

namespace SpectralSaliencyMap
{​​
    // 스펙샐리 클래스
    public class SaliencyMap
    {​​
        // 이미지 사이즈
        private static int IMAGE_SIZE_X = 512;
        private static int IMAGE_SIZE_Y = 512;
        // 할일없는 생성자
        public SaliencyMap()
        {​​
        }​​
        // 
        public static Bitmap GetTransferImage(Bitmap srcImage, int imageSize)
        {​​
            // 2의 n승으로 변경
            if ((imageSize & (imageSize - 1)) != 0)
                throw new ArgumentException("이미지 사이즈는 2의 n승이여야 합니다.");
            else
            {​​
                IMAGE_SIZE_X = imageSize;
                IMAGE_SIZE_Y = imageSize;
            }​​
            // 2차원 푸리에변환.
            AForge.Imaging.ComplexImage fft2Image = GetFFT2(srcImage);
            // 어레이 초기화
            double[,] logAmp = GetLAmp(fft2Image);
            double[,] phase = GetPhase(fft2Image);
            // 스펙트럴 레지듀얼 함수 호출
            Bitmap sr = GetSR(logAmp);
            // 샐리언시맵 산출 함수 호출
            Bitmap salMap = GetSalMap(sr, phase);
            // 가우시안 블러링
            var filt2 = new Accord.Imaging.Filters.GaussianBlur(2.5, 9);
            filt2.ApplyInPlace(salMap);
            // 컨트라스트 스트레칭
            var filtNorm = new Accord.Imaging.Filters.ContrastStretch();
            filtNorm.ApplyInPlace(salMap);
            return salMap;
        }​​
        // 2차원 푸리에변환 함수
        public static AForge.Imaging.ComplexImage GetFFT2(Bitmap srcImage)
        {​​
            // 일단, 컬러 이미지용으로
            var complexImage = 
                AForge.Imaging.ComplexImage.FromBitmap(ResizeImage(ToGrayScale(srcImage)));
            // 2차원 푸리에변환
            AForge.Math.FourierTransform.FFT2(
                complexImage.Data, AForge.Math.FourierTransform.Direction.Forward);
            return complexImage;
        }​​
        // 진폭복소영상에 로그 취하기
        private static double[,] GetLAmp(AForge.Imaging.ComplexImage fft2Image)
        {​​
            // 크기가 동일한 복소이미지 생성
            AForge.Imaging.ComplexImage complexImage = fft2Image;
            // 빈 어레이 생성
            double[,] logAmpl = new double[IMAGE_SIZE_X, IMAGE_SIZE_Y];
            // 엘리먼트별로 로그 취하기
            for (int x = 0; x < IMAGE_SIZE_X; x++)
            {​​
                for (int y = 0; y < IMAGE_SIZE_Y; y++)
                {​​
                    // abs (z) = sqrt (x^2 + y^2)
                    // 따라서, 우리는 Complex.Magnitude로 구할 수 있다.
                    logAmpl[x, y] = Math.Log(complexImage.Data[x, y].Magnitude);
                }​​
            }​​
            return logAmpl;
        }​​
        // 위상 2차원어레이 산출 ------------------------------------------------------------
        private static double[,] GetPhase(AForge.Imaging.ComplexImage fft2Image)
        {​​
            // 복소 이미지 생성
            AForge.Imaging.ComplexImage complexImage = fft2Image;
            // 위상 어레이 추출
            double[,] phase = new double[IMAGE_SIZE_X, IMAGE_SIZE_Y];
            for (int x = 0; x < IMAGE_SIZE_X; x++)
            {​​
                for (int y = 0; y < IMAGE_SIZE_Y; y++)
                {​​
                    phase[x, y] = complexImage.Data[x, y].Phase;
                }​​
            }​​
            return phase;
        }​​
        // Spectral Residual 계산하기 -------------------------------------------------
        private static Bitmap GetSR(double[,] logAmp)
        {​​
            // 어레이2 비트맵
            Bitmap amp = logAmp.ToBitmap();
            // 패스트 스무딩
            Accord.Imaging.Filters.FastBoxBlur imfilter = new Accord.Imaging.Filters.FastBoxBlur(3, 3);
            Bitmap smoothAmp = imfilter.Apply(amp);
            // 차영상 구하기
            AForge.Imaging.Filters.Subtract filter = new AForge.Imaging.Filters.Subtract(smoothAmp);
            Bitmap specResi = filter.Apply(amp);
            
            return specResi;
        }​​
        // 샐리언시맵 산출 -------------------------------------------------------------
        private static Bitmap GetSalMap(Bitmap sr, double[,] phase)
        {​​
            AForge.Imaging.ComplexImage totalSpec = AForge.Imaging.ComplexImage.FromBitmap(sr);
            
            // exp(SR + 1i * Phase)
            for (int x = 0; x < IMAGE_SIZE_X; x++)
            {​​
                for (int y = 0; y < IMAGE_SIZE_Y; y++)
                {​​
                    totalSpec.Data[x, y].Im = phase[x, y];
                    totalSpec.Data[x, y] = Complex.Exp(totalSpec.Data[x, y]);
                }​​
            }​​
            totalSpec.ForwardFourierTransform();
            // .^2
            for (int x = 0; x < IMAGE_SIZE_X; x++)
            {​​
                for (int y = 0; y < IMAGE_SIZE_Y; y++)
                {​​
                    totalSpec.Data[x, y] =
                        Complex.Multiply(totalSpec.Data[x, y], totalSpec.Data[x, y]);
                }​​
            }​​
            // shift
            Complex[,] shifted = Shift(totalSpec.Data, IMAGE_SIZE_X, IMAGE_SIZE_Y);
            for (int x = 0; x < IMAGE_SIZE_X; x++)
                for (int y = 0; y < IMAGE_SIZE_Y; y++)
                    totalSpec.Data[x, y] = shifted[x, y];
            return totalSpec.ToBitmap();
        }​​
        // FFTShift ------------------------------------------------------------------
        public static Complex[,] Shift(Complex[,] data, int width, int height)
        {​​
            // 초기화
            var shifted = new Complex[width, height];
            // 사분면 바꾸기
            for (int x = 0; x <= width / 2 - 1; x++)
            {​​ 
                for (int y = 0; y <= height / 2 - 1; y++)
                {​​
                    shifted[x, y] = data[width / 2 - x, height / 2 - y];
                    shifted[x + width / 2, y] = data[width - 1 - x, height / 2 - y];
                    shifted[x, y + height / 2] = data[width / 2 - x, height -1 - y];
                    shifted[x + height / 2, y + height / 2] = data[width -1 - x, height - 1 - y];
                }​​
            }​​
            return shifted;
        }​​
        // 컬러2그레이 변환 함수 ---------------------------------------------------------
        public static Bitmap ToGrayScale(Bitmap srcImage)
        {​​
            // 컬러채널별 가중치 할당.
            Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
            return filter.Apply(srcImage);
        }​​
        // 리사이징 함수, 전역상수 크기로 -------------------------------------------------
        public static Bitmap ResizeImage(Bitmap srcImage)
        {​​
            ResizeBilinear filter = new ResizeBilinear(IMAGE_SIZE_X, IMAGE_SIZE_Y);
            return filter.Apply(srcImage);
        }​​
    }​​
}​​
