using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Security.Cryptography;
using Misc;

namespace Bot
{
    static partial class Config
    {
        public static void Save()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, _data);
            File.WriteAllBytes("config.dat", HID.EncryptLocal(ms.ToArray()));
            ms.Dispose();
        }
        public static void Load()
        {
            if (File.Exists("config.dat"))
            {
                using (MemoryStream ms = new MemoryStream(HID.DecryptLocal(File.ReadAllBytes("config.dat"))))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    _data = (Data)bf.Deserialize(ms);
                }
            }
            else
            {
                _data = new Data();
                Save();
            }
        }
        private static Data _data;

        public static Data data
        {
            get {
                if (_data == null)
                    Load();
                return _data;
            }
        }
    }
}
