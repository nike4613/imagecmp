using Utilites;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ComparerDisplay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string RelPath = "";

        public static List<Pair<string>> SameObjects = new List<Pair<string>>();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var strm = new StreamWriter(new FileInfo("inp.txt").OpenWrite());

            string line = "";

            RelPath = Console.ReadLine();

            string sep = Console.ReadLine();

            while ((line=Console.ReadLine()) != null && line != "")
            {
                var splits = line.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries);

                SameObjects.Add(new Pair<string>(splits));
            }

            foreach (var p in SameObjects)
            {
                strm.WriteLine(p);
            }

            strm.Close();

            StartMain();
        }

        private void StartMain()
        {
            var win = new MainWindow();

            win.SetSames(SameObjects, RelPath);

            win.Show();
        }
    }
}
