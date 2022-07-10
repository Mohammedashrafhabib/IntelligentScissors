using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

using System.Windows.Forms;


namespace IntelligentScissors
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        Point point;
        Point endpoint;
        int w;
        Stopwatch stopwatch;
        List<int> anchorpoints = new List<int>();

        int shortest_path_value = 0, base_value = 0;
        List<KeyValuePair<int, RGBPixel>> imageorg = new List<KeyValuePair<int, RGBPixel>>();
        RGBPixel[,] ImageMatrix;
        // RGBPixel[,] ImageMatrix2;
        string testdir;
        private void btnOpen_Click(object sender, EventArgs e)//O(V)
        {
            //     MessageBox.Show(point.X.ToString());
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                testdir = Path.GetDirectoryName(OpenedFilePath);
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);//O(V)
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            stopwatch = Stopwatch.StartNew();
            //ImageOperations.dijkstra(1695577, 2055619);
            ImageOperations.initAdjacencyList(ImageMatrix);//O(V)
            stopwatch.Stop();
            MessageBox.Show(stopwatch.Elapsed.ToString());





        }
        #region automated testing
        private void testgraph()//O(E)
        {
            Vector2D[] x = ImageOperations.pixelEnergies;
            List<KeyValuePair<int, double>>[] adjacencyList = ImageOperations.adjacencyList;
            // File.AppendText("testenergies.txt");
            StreamWriter f = new StreamWriter("testgraph.txt", append: false);
            DialogResult d = MessageBox.Show("sample?", "test type", MessageBoxButtons.YesNo);
            if (d == DialogResult.Yes)
            {
                int index = 0;
                f.WriteLine("The constructed graph");
                f.WriteLine("");
                foreach (List<KeyValuePair<int, double>> a in adjacencyList)//O(E)
                {
                    f.WriteLine(" The  index node" + index);
                    f.WriteLine("Edges");
                    foreach (KeyValuePair<int, double> z in a)
                    {
                        f.WriteLine("edge from   " + index + "  To  " + z.Key.ToString() + "  With Weights  " + z.Value);

                    }
                    f.WriteLine("\n\n");
                    index++;
                }
                f.Close();

                //Open the browsed image and display it

                string OpenedFilePath = (!(testdir[testdir.Length - 1].ToString().Equals("1"))) ? testdir + @"\" + "output" + testdir[testdir.Length - 1] + ".txt" : testdir + @"\" + "output.txt";
                StreamReader trueoutput = new StreamReader(OpenedFilePath);
                StreamReader output = new StreamReader(OpenedFilePath);
                int wrong = 0;
                while (true)//O(E)
                {
                    string s = trueoutput.ReadLine();
                    string s1 = output.ReadLine();
                    if (s != null)
                    {
                        if (s.Equals(s1))
                        {
                            continue;
                        }
                        else
                        {

                            wrong++;
                            break;
                        }
                    }
                    else
                        break;
                }
                output.Close();
                trueoutput.Close();
                MessageBox.Show(wrong == 0 ? "test completed successfully :)" : "wrong lines=" + wrong.ToString() + "\n SAD");

            }
            else if (d == DialogResult.No)
            {
                int index = 0;
                f.WriteLine("Constructed Graph: (Format: node_index|edges:(from, to, weight)... )");

                foreach (List<KeyValuePair<int, double>> a in adjacencyList)//O(E)
                {
                    f.Write(index + "|edges:");


                    foreach (KeyValuePair<int, double> z in a)
                    {
                        f.Write("(" + index + "," + z.Key.ToString() + "," + z.Value + ")");

                    }
                    f.WriteLine("");
                    index++;
                }
                f.WriteLine("Graph construction took: " + stopwatch.Elapsed.TotalSeconds.ToString());
                f.Close();

                //Open the browsed image and display it
                string OpenedFilePath = (!testdir[testdir.Length - 1].ToString().Equals("1")) ? testdir + @"\" + "output" + testdir[testdir.Length - 1] + ".txt" : testdir + @"\" + "output.txt";
                StreamReader trueoutput = new StreamReader(OpenedFilePath);
                StreamReader output = new StreamReader(OpenedFilePath);
                int wrong = 0;
                while (true)//O(V)
                {
                    string s = trueoutput.ReadLine();
                    string s1 = output.ReadLine();
                    if (s != null)
                    {
                        if (s.Equals(s1))
                        {
                            continue;
                        }
                        else
                        {

                            wrong++;
                            break;
                        }
                    }
                    else
                        break;
                }
                output.Close();
                trueoutput.Close();
                MessageBox.Show(wrong == 0 ? "test completed successfully :)" : "wrong lines=" + wrong.ToString() + "\n SAD");

            }

        }


        private void btnGaussSmooth_Click(object sender, EventArgs e)//O(E)
        {
            //double sigma = double.Parse(txtGaussSigma.Text);
            //int maskSize = (int)nudMaskSize.Value;
            //ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            //ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            testgraph();//O(E)
        }
        #endregion
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)//O(V)
        {
            MouseEventArgs ee = (MouseEventArgs)e;
            point = ee.Location;
            w = ImageOperations.GetWidth(ImageMatrix);
            anchorpoints.Add(point.Y * w + point.X);

            int start = (point.Y) * w + (point.X);
            Point p = pictureBox1.Location;
            
            ImageOperations.init = false;
            base_value = shortest_path_value;
            imageorg.Clear();//O(V) 
            

        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)//O(VLog(V)+E)
        {
            endpoint = e.Location;
            shortest_path_value = base_value;

            if (!point.IsEmpty)
            {
                if (imageorg.Count > 0)
                {
                    while (imageorg.Count > 0)//O(V)
                    {
                        int index = imageorg[0].Key;
                        int x = index % w, y = index / w;

                        //ImageMatrix[y, x].blue = imageorg[0].Value.blue;
                        //ImageMatrix[y, x].green = imageorg[0].Value.green;
                        //ImageMatrix[y, x].red = imageorg[0].Value.red;
                        ((Bitmap)pictureBox1.Image).SetPixel(x, y, Color.FromArgb(imageorg[0].Value.blue, imageorg[0].Value.green, imageorg[0].Value.red));
                        imageorg.RemoveAt(0);
                    }
                    // ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                }
                int start = (point.Y) * w + (point.X);
                int end = (endpoint.Y) * w + (endpoint.X);
                if (ImageOperations.GetHeight(ImageMatrix) < Math.Abs(panel1.Height) && ImageOperations.GetWidth(ImageMatrix) < Math.Abs(panel1.Width))
                {
                    ImageOperations.start = 0;
                    ImageOperations.end = ImageOperations.numOfPixels;
                    ImageOperations.w = ImageOperations.GetWidth(ImageMatrix);
                    ImageOperations.h = ImageOperations.GetHeight(ImageMatrix);
                }
                else
                {
                    if(ImageOperations.start!= Math.Abs(pictureBox1.Location.X) + w * Math.Abs(pictureBox1.Location.Y))
                        ImageOperations.init = false;
                    ImageOperations.start = Math.Abs(pictureBox1.Location.X) + w * Math.Abs(pictureBox1.Location.Y) ;
                    ImageOperations.end = Math.Abs(pictureBox1.Location.X) + panel1.Width + w * (Math.Abs(pictureBox1.Location.Y) + panel1.Height) ;
                    ImageOperations.w = panel1.Width;
                    ImageOperations.h = panel1.Height;
                }

                if (!ImageOperations.init)
                    ImageOperations.dijkstra(start, end);//O(VLog(V)+E)
                if (ImageOperations.parent != null && ImageOperations.parent.ContainsKey(end))
                {
                    if (ImageOperations.parent[end] == -1 && end != start)
                        ImageOperations.dijkstra(start, end);//O(VLog(V)+E)
                    if (ImageOperations.parent[start] != -1)
                        ImageOperations.dijkstra(start, end);//O(VLog(V)+E)
                }
                else
                {

                    ImageOperations.dijkstra(start, end);//O(VLog(V)+E)

                }
                Dictionary<int, int> pp = ImageOperations.parent;


                while (true)//O(V)
                {
                    int x = end % w;
                    int y = end / w;
                    if (!pp.ContainsKey(end))
                        break;
                    if (end == -1)
                    {
                        break;

                    }
                    else
                    {

                        shortest_path_value += 1;
                        RGBPixel pixel = new RGBPixel();
                        pixel.blue = ((Bitmap)pictureBox1.Image).GetPixel(x, y).R;
                        pixel.green = ((Bitmap)pictureBox1.Image).GetPixel(x, y).G;
                        pixel.red = ((Bitmap)pictureBox1.Image).GetPixel(x, y).B;
                        imageorg.Add(new KeyValuePair<int, RGBPixel>(end, pixel));

                        ((Bitmap)pictureBox1.Image).SetPixel(x, y, Color.Red);


                    }

                    end = pp[end];

                }

                pictureBox1.Refresh();
                txtGaussSigma.Text = shortest_path_value.ToString();



            }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)//O(VLog(V)+E)
        {
            MouseEventArgs ee = (MouseEventArgs)e;
            point = ee.Location;
            w = ImageOperations.GetWidth(ImageMatrix);
            anchorpoints.Add(point.Y * w + point.X);

            int start = (point.Y) * w + (point.X);

            ImageOperations.init = false;
            if (anchorpoints.Count > 1)

            {
                int end = (point.Y) * w + (point.X);
                Dictionary<int, int> pp = ImageOperations.parent;
                imageorg.Clear();//O(V)
                while (true)//O(V)
                {
                    int x = end % w;
                    int y = end / w;

                    if (end == -1)
                    {
                        break;

                    }
                    else
                    {

                        ((Bitmap)pictureBox1.Image).SetPixel(x, y, Color.Red);


                    }

                    end = pp[end];

                }
            }

            int end1 = anchorpoints[0];
            ImageOperations.start = Math.Abs(pictureBox1.Location.X) + w * Math.Abs(pictureBox1.Location.Y);
            ImageOperations.end = Math.Abs(pictureBox1.Location.X) + panel1.Width + w * (Math.Abs(pictureBox1.Location.Y) + panel1.Height);
            ImageOperations.w = panel1.Width;
            ImageOperations.h = panel1.Height;
            if (!ImageOperations.init)
                ImageOperations.dijkstra(start, end1);//O(VLog(V)+E)
            if (ImageOperations.parent != null && ImageOperations.parent.ContainsKey(end1))
            {
                if (ImageOperations.parent[end1] == -1 && end1 != start)
                    ImageOperations.dijkstra(start, end1);//O(VLog(V)+E)
                if (ImageOperations.parent[start] != -1)
                    ImageOperations.dijkstra(start, end1);//O(VLog(V)+E)
            }
            else
            {
                ImageOperations.dijkstra(start, end1);//O(VLog(V)+E)
            }
            Dictionary<int, int> pp1 = ImageOperations.parent;
            while (true)//O(V)
            {
                int x = end1 % w;
                int y = end1 / w;

                if (end1 == -1)
                {
                    break;

                }
                else
                {

                    ((Bitmap)pictureBox1.Image).SetPixel(x, y, Color.Red);


                }

                end1 = pp1[end1];

            }
            point.X = 0;
            point.Y = 0;
            anchorpoints = new List<int>();
            pictureBox1.Refresh();



        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {

        }
    }
}