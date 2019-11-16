using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Security.Cryptography;

namespace Bot
{
    static partial class Config
    {
        public static void Save()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, _data);
            File.WriteAllBytes("config.dat", ProtectedData.Protect(ms.ToArray(), HID.Value(), DataProtectionScope.CurrentUser));
            ms.Dispose();
        }
        public static void Load()
        {
            if (File.Exists("config.dat"))
            {
                using (MemoryStream ms = new MemoryStream(ProtectedData.Unprotect(File.ReadAllBytes("config.dat"), HID.Value(), DataProtectionScope.CurrentUser)))
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

        internal class Protected
        {
            protected byte[] Protect(byte[] data) => ProtectedData.Protect(data, HID.Value(), DataProtectionScope.CurrentUser);
            protected byte[] Unprotect(byte[] data) => ProtectedData.Unprotect(data, HID.Value(), DataProtectionScope.CurrentUser);
        }
    }
}
