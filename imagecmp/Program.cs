using System;
using Utilites;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.Diagnostics;

namespace imagecmp
{
    class Program
    {
        private static IExitSignal OnExit;

        private static Process SubProc;

        static Program()
        {
            OnExit = new WinExitSignal();
            OnExit.Exit += OnExit_Exit;
        }

        private static void OnExit_Exit(object sender, EventArgs e)
        {
            SubProc.CloseMainWindow();
            SubProc.WaitForExit();
            SubProc.Close();
        }

        static void Main(string[] args)
        {
            RelDir = ".";

            if (args.Length > 0) RelDir = args[0];
            
            int iters = LoadCachedFiles();
            CompareHashes(false);
            int code = ProcessHumanFilter();
            DeleteRelevantFiles(false);

            Console.WriteLine(code);

            Console.ReadKey();
        }

        private static string RelDir;
        private static HashCache Cache;

        private static List<Pair<string>> Sames;

        private static int LoadCachedFiles(string cachefile="cache.hash")
        {
            Cache = new HashCache(new FileInfo(cachefile));

            int iters = CompareUtils.LoadImageHashes(RelDir, Cache);

            foreach (string file in Cache.Filenames)
            {
                var fulf = Path.GetFullPath(Path.Combine(RelDir, file));

                if (!new FileInfo(fulf).Exists)
                {
                    Cache.RemoveFile(file);
                }
            }

            Cache.WriteCache();

            return iters;
        }

        private static void CompareHashes(bool debug = false)
        {
            Sames = CompareUtils.CompareImages(Cache);

            if (debug)
            {
                var strm = new StreamWriter(new FileInfo("same.txt").OpenWrite());

                foreach (var p in Sames)
                {
                    strm.WriteLine(p.First + ":=" + p.Last);
                }

                strm.Close();
            }
        }

        private static List<Pair<Pair<string>>> HumanProccesedList;

        private static int ProcessHumanFilter()
        {
            var proc = Utils.ProcessCommandLine("ComparerDisplay.exe");

            SubProc = proc;

            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardInput = true;

            proc.Start();

            const string sep = ";;";

            var sout = proc.StandardOutput;
            var sin = proc.StandardInput;

            sin.WriteLine(RelDir);

            sin.WriteLine(sep);

            foreach (var p in Sames)
            {
                sin.WriteLine(p.First + sep + p.Last);
            }

            sin.Close();

            int Digits = int.Parse(sout.ReadLine(),System.Globalization.NumberStyles.HexNumber);
            string prefix = sout.ReadLine();

            Console.WriteLine("Digits: " + Digits);

            List<Pair<Pair<string>>> result = new List<Pair<Pair<string>>>();

            while (!proc.HasExited)
            {
                string line = sout.ReadLine();
                if (line == null) continue;

                if (!line.StartsWith(prefix))
                    Console.WriteLine(line);
                else
                {
                    line = line.Substring(1);

                    var parts = line.Split(':');

                    string sindex = parts[0];

                    if (sindex.Length != Digits)
                    {
                        Console.WriteLine("Incorrect index length {0}! Should be {1}!".SFormat(sindex.Length,Digits));
                        continue;
                    }
                    
                    int index = int.Parse(sindex, System.Globalization.NumberStyles.HexNumber)-1;

                    var pair = Sames[index];

                    Pair<Pair<string>> cel = new Pair<Pair<string>>();

                    cel.First = new Pair<string>(parts[1].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                    if (cel.First.Last != pair.First)
                    {
                        Console.WriteLine("Incorrect first element '{3}' at index {0}! Should be '{1}'!".SFormat(index, pair.First, cel.First.Last));
                        continue;
                    }

                    cel.Last = new Pair<string>(parts[2].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                    if (cel.Last.Last != pair.Last)
                    {
                        Console.WriteLine("Incorrect second element '{3}' at index {0}! Should be '{1}'!".SFormat(index, pair.Last, cel.Last.Last));
                        continue;
                    }

                    result.Add(cel);
                }
            }

            HumanProccesedList = result;

            return proc.ExitCode;
        }

        private static void DeleteRelevantFiles(bool debug = false)
        {
            StreamWriter strm = null;
            if (debug)
                strm = new StreamWriter(new FileInfo("delete.txt").OpenWrite());

            foreach (var pa in HumanProccesedList)
            {
                foreach (var p in pa)
                {
                    if (debug)
                        strm.WriteLine("'{1}': {0}".SFormat(p.First,p.Last));

                    switch (p.First)
                    {
                        case "del":
                            Cache.RemoveFile(p.Last);

                            new FileInfo(Path.GetFullPath(Path.Combine(RelDir, p.Last))).Delete();

                            if (debug)
                                strm.WriteLine("'{0}' deleted".SFormat(p.Last));
                            break;
                        case "keep":
                        default:
                            break;
                    }
                }
            }

            if (debug)
                strm.Close();

            Cache.WriteCache();
        }
    }
}
