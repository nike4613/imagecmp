using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utilites;

namespace ComparerDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string relto = "";
        public List<Pair<string>> Sames = new List<Pair<string>>();
        public int index = 0;
        public string TitleFormat;
        public Pair<string> Current;

        public int Digits = 1;

        public MainWindow()
        {
            InitializeComponent();

            TitleFormat = Title;

            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;

            keepl.Click += Keepl_Click;
            keepr.Click += Keepr_Click;
            keepboth.Click += Keepboth_Click;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            { // Perform default (assume same and choose biggest)
                if (CurL.PixelHeight == CurR.PixelHeight &&
                    CurL.PixelWidth == CurR.PixelWidth) // If same size, pick one with shortest name
                    if (Current.First.Length < Current.Last.Length)
                        KeepThese(ButtonSide.Left);
                    else
                        KeepThese(ButtonSide.Right);
                else
                    if (CurL.PixelHeight > CurR.PixelHeight &&
                        CurL.PixelWidth > CurR.PixelWidth) // Otherwise pick biggest one
                    KeepThese(ButtonSide.Left);
                else
                    KeepThese(ButtonSide.Right);
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        { 
            if (e.Key == Key.Left)
                KeepThese(ButtonSide.Left);
            if (e.Key == Key.Right)
                KeepThese(ButtonSide.Right);
            if (e.Key == Key.Down || e.Key == Key.Up)
                KeepThese(ButtonSide.Both);
        }

        private void Keepboth_Click(object sender, RoutedEventArgs e)
        {
            KeepThese(ButtonSide.Both);
        }

        private void Keepr_Click(object sender, RoutedEventArgs e)
        {
            KeepThese(ButtonSide.Right);
        }

        private void Keepl_Click(object sender, RoutedEventArgs e)
        {
            KeepThese(ButtonSide.Left);
        }

        private const string prefix = "!";
        private const string inform = ":{0};{1};";
        private void KeepThese(ButtonSide side)
        {
            string keepform = inform.SFormat("keep", "{0}");
            string delform = inform.SFormat("del", "{0}");

            string keep = prefix + index.ToString("X"+Digits);

            if ((side & ButtonSide.Left) == ButtonSide.Left)
                keep += keepform.SFormat(Current.First);
            else
                keep += delform.SFormat(Current.First);

            if ((side & ButtonSide.Right) == ButtonSide.Right)
                keep += keepform.SFormat(Current.Last);
            else
                keep += delform.SFormat(Current.Last);

            Console.WriteLine(keep);

            LoadNextPair();
        }

        internal void SetSames(List<Pair<string>> sameObjects, string rlt)
        {
            Sames = sameObjects;
            relto = rlt;

            Digits = (int)Math.Ceiling(Math.Log(Sames.Count, 16));

            Console.WriteLine(Digits.ToString("X"));
            Console.WriteLine(prefix);

            LoadNextPair();
        }

        private void LoadNextPair()
        {
            if (index == Sames.Count)
            {
                Application.Current.Shutdown();
                return;
            }

            Current = Sames[index++];
            ViewPair();
        }

        private BitmapImage CurL;
        private BitmapImage CurR;

        private void ViewPair()
        {
            const string restext = "({0}x{1})";

            Title = TitleFormat.SFormat(Current.First, Current.Last);

            CurL = new BitmapImage(new Uri(Path.GetFullPath(Path.Combine(relto, Current.First))));
            imagel.Source = CurL;
            LImgL.Content = restext.SFormat(CurL.PixelWidth, CurL.PixelHeight);

            CurR = new BitmapImage(new Uri(Path.GetFullPath(Path.Combine(relto, Current.Last))));
            imager.Source = CurR;
            RImgL.Content = restext.SFormat(CurR.PixelWidth, CurR.PixelHeight);

        }
    }

    internal enum ButtonSide
    {
        Left=1,
        Right=2,
        Both=Left|Right
    }
}
