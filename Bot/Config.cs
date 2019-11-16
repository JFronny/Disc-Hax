using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Bot
{
    static partial class Config
    {
        public static void Save()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream st = new FileStream("config.dat", FileMode.OpenOrCreate);
            bf.Serialize(st, data);
            st.Dispose();
        }
        public static void Load()
        {
            if (File.Exists("config.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream st = new FileStream("config.dat", FileMode.OpenOrCreate);
                _data = (Data)bf.Deserialize(st);
                st.Dispose();
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
