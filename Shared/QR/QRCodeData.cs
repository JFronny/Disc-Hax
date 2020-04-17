using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace QRCoder
{
    public class QRCodeData : IDisposable
    {
        public List<BitArray> ModuleMatrix { get; set; }

        public QRCodeData(int version)
        {
            Version = version;
            int size = ModulesPerSideFromVersion(version);
            ModuleMatrix = new List<BitArray>();
            for (int i = 0; i < size; i++)
                ModuleMatrix.Add(new BitArray(size));
        }
#if NETFRAMEWORK || NETSTANDARD2_0
        public QRCodeData(string pathToRawData, Compression compressMode) : this(File.ReadAllBytes(pathToRawData), compressMode)
        {
        }
#endif
        
        class Stream4Methods
        {
            public static void CopyTo(Stream input, Stream output)
            {
                byte[] buffer = new byte[16 * 1024];
                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) output.Write(buffer, 0, bytesRead);
            }
        }
        
        public QRCodeData(byte[] rawData, Compression compressMode)
        {
            List<byte> bytes = new List<byte>(rawData);

            //Decompress
            if (compressMode == Compression.Deflate)
            {
                using MemoryStream input = new MemoryStream(bytes.ToArray());
                using MemoryStream output = new MemoryStream();
                using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress)) Stream4Methods.CopyTo(dstream, output);
                bytes = new List<byte>(output.ToArray());
            }
            else if (compressMode == Compression.GZip)
            {
                using MemoryStream input = new MemoryStream(bytes.ToArray());
                using MemoryStream output = new MemoryStream();
                using (GZipStream dstream = new GZipStream(input, CompressionMode.Decompress)) Stream4Methods.CopyTo(dstream, output);
                bytes = new List<byte>(output.ToArray());
            }

            if (bytes[0] != 0x51 || bytes[1] != 0x52 || bytes[2] != 0x52)
                throw new Exception("Invalid raw data file. Filetype doesn't match \"QRR\".");

            //Set QR code version
            int sideLen = bytes[4];
            bytes.RemoveRange(0, 5);
            Version = (sideLen - 21 - 8) / 4 + 1;

            //Unpack
            Queue<bool> modules = new Queue<bool>(8 * bytes.Count);
            foreach (byte b in bytes)
            {
                BitArray bArr = new BitArray(new[] { b });
                for (int i = 7; i >= 0; i--) modules.Enqueue((b & (1 << i)) != 0);
            }

            //Build module matrix
            ModuleMatrix = new List<BitArray>(sideLen);
            for (int y = 0; y < sideLen; y++)
            {
                ModuleMatrix.Add(new BitArray(sideLen));
                for (int x = 0; x < sideLen; x++) ModuleMatrix[y][x] = modules.Dequeue();
            }

        }

        public byte[] GetRawData(Compression compressMode)
        {
            List<byte> bytes = new List<byte>();

            //Add header - signature ("QRR")
            bytes.AddRange(new byte[]{ 0x51, 0x52, 0x52, 0x00 });

            //Add header - rowsize
            bytes.Add((byte)ModuleMatrix.Count);

            //Build data queue
            Queue<int> dataQueue = new Queue<int>();
            foreach (BitArray row in ModuleMatrix)
            foreach (object module in row)
                dataQueue.Enqueue((bool)module ? 1 : 0);
            for (int i = 0; i < 8 - (ModuleMatrix.Count * ModuleMatrix.Count) % 8; i++) dataQueue.Enqueue(0);

            //Process queue
            while (dataQueue.Count > 0)
            {
                byte b = 0;
                for (int i = 7; i >= 0; i--) b += (byte)(dataQueue.Dequeue() << i);
                bytes.Add(b);
            }
            byte[] rawData = bytes.ToArray();

            //Compress stream (optional)
            if (compressMode == Compression.Deflate)
            {
                using MemoryStream output = new MemoryStream();
                using (DeflateStream dstream = new DeflateStream(output, CompressionMode.Compress)) dstream.Write(rawData, 0, rawData.Length);
                rawData = output.ToArray();
            }
            else if (compressMode == Compression.GZip)
            {
                using MemoryStream output = new MemoryStream();
                using (GZipStream gzipStream = new GZipStream(output, CompressionMode.Compress, true)) gzipStream.Write(rawData, 0, rawData.Length);
                rawData = output.ToArray();
            }
            return rawData;
        }

#if NETFRAMEWORK || NETSTANDARD2_0
        public void SaveRawData(string filePath, Compression compressMode)
        {
            File.WriteAllBytes(filePath, GetRawData(compressMode));
        }
#endif

        public int Version { get; private set; }

        private static int ModulesPerSideFromVersion(int version)
        {
            return 21 + (version - 1) * 4;
        }

        public void Dispose()
        {
            ModuleMatrix = null;
            Version = 0;

        }

        public enum Compression
        {
            Uncompressed,
            Deflate,
            GZip
        }
    }
}
