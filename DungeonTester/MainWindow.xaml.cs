using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DungeonTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //The most important part
        DungeonGenerator dungeon = new DungeonGenerator();

        //ushort[,] displayContext;
        

        public MainWindow()
        {
            InitializeComponent();

            //generate filler data context
            try
            {
                displayContext = new ushort[ushort.Parse(this.FloorWidthTextBox.Text),
                    ushort.Parse(this.FloorHeightTextBox.Text)];
            }
            catch
            {
                Console.WriteLine("MainWindow.xaml.cs::MainWindow() -- displayContext failure");
            }
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateImage(object sender, MouseEventArgs e)
        {

        }

        /// <summary>
        /// Load a grid for Selected Floor
        /// </summary>
        /// <param name="sender"></param>
        private void LoadFloorButton_Click(object sender, RoutedEventArgs e)
        {
            int floorSelected;

            if (MainDataGrid.IsInitialized)
            {
                //Might have to make an image ahead of time, then draw on that.
                //Seriously might be easier to just make a spreadsheet and color that
                // An issue with being able to output as an image?

                //Get Image Dimensions
                //double imgWidth = MainImage.Width;
                //double imgHeight = MainImage.Height;
                //int imgBorder = 5;
                //int cellSize = 10;
                //double gridWidth = imgWidth / cellSize;
                //double gridHeight = imgHeight / cellSize;

                //DrawingVisual drawingVisual = new DrawingVisual();
                /*using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    //Make a loop to draw each grid entry in turn
                    //For now, make a grid that is resized as the image gets invalidated
                    for (int ix = imgBorder; ix < MainImage.Width; ix+=cellSize)
                    {
                        for (int iy = imgBorder; iy < MainImage.Height; iy+=cellSize)
                        {
                            drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(ix, iy, cellSize, cellSize));
                        }
                    }
                }*/

                //RenderTargetBitmap tempbmp = new RenderTargetBitmap(500, 500, 100, 90, PixelFormats.Pbgra32);
                //tempbmp.Render(drawingVisual);

                //MainImage.Source = tempbmp;
            }
            
        }

        private void _Run_Click(object sender, RoutedEventArgs e)
        {
            this.dungeon.setupDungeon();
            this.dungeon.Generate();
        }

        private void MainDataGrid_Initialized(object sender, EventArgs e)
        {
            //Load blank grid using default values in text fields
            MainDataGrid.Items.Add(displayContext);
            //MainDataGrid.DataContext = displayContext;

            //MainDataGrid
        }
    }
}
