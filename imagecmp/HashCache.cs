using System;
using Utilites;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imagecmp
{
    public class HashCache
    {
        private static long[] FromByteArray(byte[] bs)
        {
            int ct = bs.Length / sizeof(long);
            long[] outp = new long[ct];

            for (int i = 0; i < ct; i++)
            {
                outp[i] = BitConverter.ToInt64(bs, i * sizeof(long));
            }

            return outp;
        }

        private static byte[] ToByteArray(long[] bs)
        {
            int ct = bs.Length * sizeof(long);
            byte[] outp = new byte[ct];

            for (int i = 0; i < bs.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(bs[i]);

                for (int k = 0; k < bytes.Length; k++)
                {
                    outp[i * sizeof(long) + k] = bytes[k];
                }
            }

            return outp;
        }

        private FileInfo file;

        public HashCache(FileInfo file)
        {
            this.file = file;

            if (file.Exists)
            { // Load File
                FileStream fstr = file.Open(FileMode.Open, FileAccess.Read);
                var binr = new BinaryReader(fstr);

                while (! (fstr.Position >= fstr.Length-1))
                {
                    string fname = binr.ReadString();
                    uint bits = binr.ReadUInt32();
                    ushort hashlen = binr.ReadUInt16();
                    byte[] hashbs = binr.ReadBytes(hashlen);
                    long[] hash = FromByteArray(hashbs);

                    Cache(fname, bits, hash);
                }

                binr.Close();
            }
        }

        public void ChangeFile(FileInfo newf)
        {
            file = newf;
        }

        public void WriteCache()
        {
            FileStream fstr = file.Open(FileMode.Create, FileAccess.Write);
            var binr = new BinaryWriter(fstr);

            foreach (var key in cache.Keys)
            {
                HashData hdat = new HashData();

                bool chk = cache.TryGetValue(key, out hdat);

                if (!chk)
                {
                    Console.WriteLine("Unexpected 'No value for key' when iterating (element {0})".SFormat(key));
                    continue;
                }


                binr.Write(key);
                binr.Write(hdat.bits);

                byte[] hashbs = ToByteArray(hdat.hash);
                ushort hashlen = (ushort) hashbs.Length;

                binr.Write(hashlen);
                binr.Write(hashbs);

            }

            binr.Close();
        }

        private struct HashData
        {
            public uint bits;
            public long[] hash;
        }

        private Dictionary<string, HashData> cache = new Dictionary<string, HashData>();

        public void Cache(string filen, uint bits, long[] hash)
        {
            cache[filen] = new HashData() { bits = bits, hash = hash };
        }

        public bool HasFile(string filen)
        {
            return cache.ContainsKey(filen);
        }

        public string[] Filenames
        {
            get { return cache.Keys.ToArray(); }
        }

        public bool TryGetHashBits(string filen, out uint bits)
        {
            bool ret = true;

            ret = cache.TryGetValue(filen, out HashData data);

            if (!ret)
            {
                bits = uint.MinValue;
                return ret;
            }

            bits = data.bits;

            return ret;
        }

        public bool TryGetHash(string filen, out long[] hash)
        {
            bool ret = true;
            
            ret = cache.TryGetValue(filen, out HashData data);

            if (!ret)
            {
                hash = new long[] { };
                return ret;
            }

            hash = data.hash;

            return ret;
        }

        public void RemoveFile(string fname)
        {
            cache.Remove(fname);
        }
    }
}
