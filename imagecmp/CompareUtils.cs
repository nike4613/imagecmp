using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Utilites;

namespace imagecmp
{
    public class CompareUtils
    {

        public static long[] GetHash(Bitmap bmpSource, uint bits=256)
        {
            int sz = (int) Math.Floor(Math.Sqrt(bits));

            uint ict = (uint) Math.Floor(bits / 8d / sizeof(long));

            long[] res = new long[ict];
            
            //create new image with 16x16 pixel
            Bitmap bmpMin = new Bitmap(bmpSource, new Size(sz,sz));
            var avg = CalculateAverageLightness(bmpMin);

            int k = 0;
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduce colors to true / false                
                    if (bmpMin.GetPixel(i, j).GetBrightness() < avg)
                        res[k / 8 / sizeof(long)] |= (long) Math.Pow(2, k % (8 * sizeof(long)));
                    k++;
                }
            }
            return res;
        }

        public static Bitmap GetHashBitmap(Bitmap src, uint bits = 256)
        {
            int sz = (int)Math.Floor(Math.Sqrt(bits));
            
            long[] res = GetHash(src,bits);

            var bmp = new Bitmap(sz,sz);

            int k = 0;
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    long val = res[k / 8 / sizeof(long)];
                    bool b = (val & (long) Math.Pow(2, k % (8 * sizeof(long)))) > 0;
                    bmp.SetPixel(i, j, b ? Color.White : Color.Black);
                    k++;
                }
            }

            return bmp;
        }

        public static double CalculateAverageLightness(Bitmap bm)
        {
            double lum = 0;
            var tmpBmp = new Bitmap(bm);
            var width = bm.Width;
            var height = bm.Height;
            var bppModifier = bm.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var srcData = tmpBmp.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
            var stride = srcData.Stride;
            var scan0 = srcData.Scan0;

            //Luminance (standard, objective): (0.2126*R) + (0.7152*G) + (0.0722*B)
            //Luminance (perceived option 1): (0.299*R + 0.587*G + 0.114*B)
            //Luminance (perceived option 2, slower to calculate): sqrt( 0.241*R^2 + 0.691*G^2 + 0.068*B^2 )

            unsafe
            {
                byte* p = (byte*)(void*)scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        lum += (0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                    }
                }
            }

            tmpBmp.UnlockBits(srcData);
            tmpBmp.Dispose();
            var avgLum = lum / (width * height);


            return avgLum / 255.0;
        }

        public static List<string> ImageExtensions = new List<string>();
        public static void LoadImageExtensions()
        {
            if (ImageExtensions.Count != 0) return;
            foreach (var kv in MimeTypeMap.List.MappingList.Mappings)
            {
                foreach (var mime in kv.Value)
                {
                    if (mime.Contains("image"))
                    {
                        ImageExtensions.Add(kv.Key);
                        break;
                    }
                }
            }
        }

        public static int LoadImageHashes(string rootdir, HashCache cache, uint bits=32*32)
        {
            LoadImageExtensions();

            string[] files = Directory.GetFiles(rootdir,"*",SearchOption.AllDirectories);

            int iters = 0;

            foreach (string file in files)
            {
                var fi = new FileInfo(file);
                if (!ImageExtensions.Contains(fi.Extension)) continue;

                string stoname = Utils.GetRelativePath(fi.FullName, Path.GetFullPath(rootdir)).UrlDecode();

                if (cache.HasFile(stoname)) continue;

                //CompareUtils.GetHashBitmap(new Bitmap(file), 32 * 32).Save(Path.Combine(odir, fi.Name + ".png"));

                long[] hsh = GetHash(new Bitmap(file), bits);

                cache.Cache(stoname, bits, hsh);
                iters++;
            }

            return iters;
        }

        public static List<Pair<string>> CompareImages(HashCache cache, uint diffq=10, bool debug = false)
        {
            List<Pair<string>> outp = new List<Pair<string>>();

            var files = cache.Filenames;

            for (int i = 0; i < files.Length; i++) 
            {
                for (int j = i+1; j < files.Length; j++)
                {
                    string key = files[i];
                    string kez = files[j];

                    cache.TryGetHash(key, out long[] hsha);
                    cache.TryGetHashBits(key, out uint bita);

                    cache.TryGetHash(kez, out long[] hshb);
                    cache.TryGetHashBits(key, out uint bitb);

                    if (bita != bitb)
                    {
                        if (debug)
                            Console.WriteLine("Bits in hash inequal for {0} and {1}.".SFormat(key, kez));
                        continue;
                    }

                    long[] xord = new long[hsha.Length];
                    for (int k = 0; k < hsha.Length; k++)
                    {
                        xord[k] = hsha[k] ^ hshb[k];
                    }

                    uint diffs = 0;
                    for (int k = 0; k < xord.Length; k++)
                    {
                        diffs += xord[k].SetBits();
                    }

                    if (debug)
                        Console.WriteLine("Diffs in {0} and {1}: {2}".SFormat(key, kez, diffs));

                    if (diffs <= diffq)
                        outp.Add(new Pair<string>(key,kez));
                }
            }

            return outp;
        }
    }
}
