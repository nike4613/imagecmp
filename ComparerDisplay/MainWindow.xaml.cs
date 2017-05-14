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
        public List<Pair<string>> Sames;
        public int index = 0;
        public string TitleFormat;

        public MainWindow()
        {
            InitializeComponent();

            TitleFormat = Title;

            keepl.Click += Keepl_Click;
            keepr.Click += Keepr_Click;
            keepboth.Click += Keepboth_Click;
        }

        private void Keepboth_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Keepr_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Keepl_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal void SetSames(List<Pair<string>> sameObjects, string rlt)
        {
            Sames = sameObjects;
            relto = rlt;

            var pair = Sames[index++];

            Title = TitleFormat.SFormat(pair.First, pair.Last);
            imagel.Source = new BitmapImage(new Uri(Path.GetFullPath(Path.Combine(relto, pair.First))));
            imager.Source = new BitmapImage(new Uri(Path.GetFullPath(Path.Combine(relto, pair.Last))));
        }
    }
}
