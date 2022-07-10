using FibonacciHeap;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

///Algorithms Project
///Intelligent Scissors
///

namespace IntelligentScissors
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }

    /// <summary>
    /// Holds the edge energy between 
    ///     1. a pixel and its right one (X)
    ///     2. a pixel and its bottom one (Y)
    /// </summary>
    public struct Vector2D
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        public static List<KeyValuePair<int, double>>[] adjacencyList;
        public static Vector2D[] pixelEnergies;
        public static Dictionary<int, double> cost;
        public static Dictionary<int, int> parent;
        public static int numOfPixels, start, end, w, h;
        public static Dictionary<int, FibonacciHeapNode<int, double>> fheapnode;
        public static FibonacciHeap<int, double> qu;
        static int imageWidth;
        public static bool init = false;
        public static double s(int x, int y, int x1, int y1)
        {
            return Math.Abs(x - x1) + Math.Abs(y - y1);
        }

        public static void dijkstra(int point, int finish)//O(VLog(V)+E)
        {
            if (!init)
            {
                fheapnode = new Dictionary<int, FibonacciHeapNode<int, double>>(numOfPixels);
                qu = new FibonacciHeap<int, double>(0);
                cost = new Dictionary<int, double>(numOfPixels);
                parent = new Dictionary<int, int>(numOfPixels);
                int z = 0;
                for (int j = start + w; j < end; j += imageWidth)//O(V)
                {
                    for (int i = j - w; i < j; i++)
                    {


                        cost[i] = (double.MaxValue);
                        parent[i] = (-1);
                        FibonacciHeapNode<int, double> node = new FibonacciHeapNode<int, double>(i, cost[i]);
                        fheapnode[i] = (node);
                        qu.Insert(node);//O(1)
                    }
                    z++;
                }

                cost[point] = 0;
                qu.DecreaseKey(fheapnode[point], cost[point]);//O(1)
            }



            init = true;
            while (!qu.IsEmpty())//itr v
            {

                FibonacciHeapNode<int, double> u = qu.Min();//O(1)
                if (u.Data == finish)
                    return;
                u = qu.RemoveMin();//O(Log(V))
                foreach (var x in adjacencyList[u.Data])
                {
                    //if (cost[u.Data] + x.Value+s(x.Key%imageWidth, x.Key / imageWidth, finish % imageWidth, finish / imageWidth) < cost[x.Key])
                    if (cost.ContainsKey(x.Key))
                        if (cost[u.Data] + x.Value < cost[x.Key])
                        {

                            // cost[x.Key] = x.Value + cost[u.Data]+ s(x.Key % imageWidth, x.Key / imageWidth, finish % imageWidth, finish / imageWidth);
                            cost[x.Key] = x.Value + cost[u.Data];
                            qu.DecreaseKey(fheapnode[x.Key], cost[x.Key]);//O(1)

                            parent[x.Key] = u.Data;


                        }
                }
            }




        }
        // Checks if new pixel coordinates are valid
        private static bool check(int rowIndex, int columnIndex, RGBPixel[,] ImageMatrix)//O(1)
        {
            return rowIndex >= 0 && rowIndex < GetHeight(ImageMatrix)
                && columnIndex >= 0 && columnIndex < GetWidth(ImageMatrix);
        }
        // Constructs the adjacency list for an image
        public static void initAdjacencyList(RGBPixel[,] ImageMatrix)//O(V)
        {

            int imageHeight = GetHeight(ImageMatrix);
            imageWidth = GetWidth(ImageMatrix);
            numOfPixels = imageHeight * imageWidth;
            adjacencyList = new List<KeyValuePair<int, double>>[numOfPixels];
            // These were reversed *** (matters for the first sample case)
            int[] rowsOffset = { -1, 0, 1, 0 };
            int[] columnsOffset = { 0, -1, 0, 1 };
            pixelEnergies = new Vector2D[numOfPixels];
            for (int i = 0; i < numOfPixels; i++)//O(V)
            {

                // 20 x 10 image
                // 100 ?
                // 100 / 20 = 10  row index=y
                // 100 % 20 = 0  columns=x
                // 10 * 10 + 0
                // 4-Connectivity
                adjacencyList[i] = new List<KeyValuePair<int, double>>(4);
                // These were wrong *** (doesn't matter for the first sample case)
                int pixelRowIndex = i / imageWidth;
                int pixelColumnIndex = i % imageWidth;
                // These were flipped x == pixelColumnIndex and y == pixelRowIndex *** (matters for the first sample case??)
                Vector2D energies = CalculatePixelEnergies(pixelColumnIndex, pixelRowIndex, ImageMatrix);
                pixelEnergies[i] = energies;
                for (int j = 0; j < 4; j++)
                {
                    if (check(pixelRowIndex + rowsOffset[j], pixelColumnIndex + columnsOffset[j], ImageMatrix))
                    {
                        // From (x, y) to [0 , numOfPixels[
                        int adjacentPixelIndex = (pixelRowIndex + rowsOffset[j]) * imageWidth + (pixelColumnIndex + columnsOffset[j]);
                        if (j == 0)
                        {
                            adjacencyList[i].Add(new KeyValuePair<int, double>(adjacentPixelIndex, pixelEnergies[adjacentPixelIndex].Y));
                        }
                        else if (j == 1)
                        {
                            adjacencyList[i].Add(new KeyValuePair<int, double>(adjacentPixelIndex, pixelEnergies[adjacentPixelIndex].X));
                        }
                        else if (j == 2)
                        {
                            adjacencyList[i].Add(new KeyValuePair<int, double>(adjacentPixelIndex, energies.Y));
                        }
                        else
                        {
                            adjacencyList[i].Add(new KeyValuePair<int, double>(adjacentPixelIndex, energies.X));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)//O(V)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[0];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[2];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)//O(1)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)//O(1)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Calculate edge energy between
        ///     1. the given pixel and its right one (X)
        ///     2. the given pixel and its bottom one (Y)
        /// </summary>
        /// <param name="x">pixel x-coordinate</param>
        /// <param name="y">pixel y-coordinate</param>
        /// <param name="ImageMatrix">colored image matrix</param>
        /// <returns>edge energy with the right pixel (X) and with the bottom pixel (Y)</returns>
        public static Vector2D CalculatePixelEnergies(int x, int y, RGBPixel[,] ImageMatrix)//O(1)
        {
            if (ImageMatrix == null) throw new Exception("image is not set!");

            Vector2D gradient = CalculateGradientAtPixel(x, y, ImageMatrix);

            double gradientMagnitude = Math.Sqrt(gradient.X * gradient.X + gradient.Y * gradient.Y);
            double edgeAngle = Math.Atan2(gradient.Y, gradient.X);
            double rotatedEdgeAngle = edgeAngle + Math.PI / 2.0;

            Vector2D energy = new Vector2D();
            energy.X = 1 / Math.Abs(gradientMagnitude * Math.Cos(rotatedEdgeAngle));
            energy.Y = 1 / Math.Abs(gradientMagnitude * Math.Sin(rotatedEdgeAngle));
            if (double.IsInfinity(energy.X))
                energy.X = 1e16;
            if (double.IsInfinity(energy.Y))
                energy.Y = 1e16;
            return energy;
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)//O(V)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[0] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[2] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


        #region Private Functions
        /// <summary>
        /// Calculate Gradient vector between the given pixel and its right and bottom ones
        /// </summary>
        /// <param name="x">pixel x-coordinate</param>
        /// <param name="y">pixel y-coordinate</param>
        /// <param name="ImageMatrix">colored image matrix</param>
        /// <returns></returns>
        private static Vector2D CalculateGradientAtPixel(int x, int y, RGBPixel[,] ImageMatrix)//O(1)
        {
            Vector2D gradient = new Vector2D();

            RGBPixel mainPixel = ImageMatrix[y, x];
            double pixelGrayVal = 0.21 * mainPixel.red + 0.72 * mainPixel.green + 0.07 * mainPixel.blue;

            if (y == GetHeight(ImageMatrix) - 1)
            {
                //boundary pixel.
                for (int i = 0; i < 3; i++)
                {
                    gradient.Y = 0;
                }
            }
            else
            {
                RGBPixel downPixel = ImageMatrix[y + 1, x];
                double downPixelGrayVal = 0.21 * downPixel.red + 0.72 * downPixel.green + 0.07 * downPixel.blue;

                gradient.Y = pixelGrayVal - downPixelGrayVal;
            }

            if (x == GetWidth(ImageMatrix) - 1)
            {
                //boundary pixel.
                gradient.X = 0;

            }
            else
            {
                RGBPixel rightPixel = ImageMatrix[y, x + 1];
                double rightPixelGrayVal = 0.21 * rightPixel.red + 0.72 * rightPixel.green + 0.07 * rightPixel.blue;

                gradient.X = pixelGrayVal - rightPixelGrayVal;
            }


            return gradient;
        }


        #endregion
    }
}
