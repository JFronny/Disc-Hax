using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Misc;

namespace Shared.Config
{
    public static class TokenManager
    {
        static string _token;
        static readonly string containerFile = Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\key.secure";
        public static string Token
        {
            get {
                if (string.IsNullOrWhiteSpace(_token))
                {
                    if (File.Exists(containerFile))
                        using (MemoryStream ms = new MemoryStream(HID.DecryptLocal(File.ReadAllBytes(containerFile))))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            _token = (string)bf.Deserialize(ms);
                        }
                    else
                        Token = Interaction.InputBox("Please enter your Token");
                }
                return _token;
            }
            set {
                _token = value;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, _token);
                    File.WriteAllBytes(containerFile, HID.EncryptLocal(ms.ToArray()));
                }
            }
        }

        //HID.DecryptLocal and HID.EncryptLocal are NOT PORTABLE!
        //The HID is used to prevent accidental leaking of your token
    }
}
