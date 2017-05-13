﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imagecmp
{
    class Program
    {
        static void Main(string[] args)
        {
            var idir = @"Z:\Users\aaron\Pictures\Saved Pictures\sortbythelols\hent\";

            if (args.Length > 0) idir = args[0];

            string[] files = Directory.GetFiles(idir);

            var csh = new HashCache(new FileInfo("cache.hash"));

            /*int iters = 0;

            foreach (string file in files)
            {
                var fi = new FileInfo(file);
                if (fi.Name == "index.php") continue;

                CompareUtils.GetHashBitmap(new Bitmap(file), 32 * 32).Save(Path.Combine(odir,fi.Name+".png"));
                iters++;
            }*/

            int iters = CompareUtils.LoadImageHashes(idir, csh);

            csh.WriteCache();

            Console.WriteLine(iters);

            // compare all
            var same = CompareUtils.CompareImages(csh);

            var strm = new StreamWriter(new FileInfo("same.txt").OpenWrite());

            foreach (var p in same)
            {
                strm.WriteLine(p.First + ";=;" + p.Last);
            }

            strm.Close();
        }
    }
}
