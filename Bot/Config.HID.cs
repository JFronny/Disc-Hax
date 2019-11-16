using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Bot.Properties;

namespace Bot
{
    partial class Config
    {
        public class HID
        {

            private static byte[] _fingerPrint;
            public static byte[] Value()
            {
                if (_fingerPrint == null)
                {
                    string fingerprint_tmp = "";
                    Resources.HIDClasses.Split('\r').Select(s =>
                    {
                        if (s.StartsWith("\n"))
                            s = s.Remove(0, 1);
                        return s.Split(':');
                    }).ToList().ForEach(s =>
                    {
                        using (ManagementClass mc = new ManagementClass(s[0]))
                        {
                            using (ManagementObjectCollection moc = mc.GetInstances())
                            {
                                ManagementBaseObject[] array = moc.OfType<ManagementBaseObject>().ToArray();
                                for (int j = 0; j < array.Length; j++)
                                {
                                    if ((s.Length > 2) && array[j][s[2]].ToString() != "True") continue;
                                    try
                                    {
                                        fingerprint_tmp += array[j][s[1]].ToString();
                                        break;
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    });
                    using (MD5 sec = new MD5CryptoServiceProvider())
                    {
                        byte[] bt = Encoding.ASCII.GetBytes(fingerprint_tmp);
                        _fingerPrint = sec.ComputeHash(bt);
                    }
                }
                return _fingerPrint;
            }
        }
    }
}

