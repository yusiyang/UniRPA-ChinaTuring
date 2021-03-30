using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace UniStudio.Community.Librarys
{
    public static class RegUtil
    {
        static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(unchecked((int)0x80000004));
        static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));
        static readonly IntPtr HKEY_DYN_DATA = new IntPtr(unchecked((int)0x80000006));

        // 获取操作Key值句柄 
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out IntPtr phkResult);

        //创建或打开Key值
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegCreateKeyEx(IntPtr hKey, string lpSubKey, int reserved, string type, int dwOptions, int REGSAM, IntPtr lpSecurityAttributes, out IntPtr phkResult,
                                                 out int lpdwDisposition);

        //关闭注册表转向（禁用特定项的注册表反射）
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegDisableReflectionKey(IntPtr hKey);

        //使能注册表转向（开启特定项的注册表反射）
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegEnableReflectionKey(IntPtr hKey);

        //获取Key值（即：Key值句柄所标志的Key对象的值）
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, out uint lpType, System.Text.StringBuilder lpData, ref uint lpcbData);

        //设置Key值
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegSetValueEx(IntPtr hKey, string lpValueName, uint unReserved, uint unType, byte[] lpData, uint dataCount);

        //关闭Key值
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        public static IntPtr TransferKeyName(string keyName)
        {
            IntPtr ret = IntPtr.Zero;
            switch (keyName)
            {
                case "HKEY_CLASSES_ROOT":
                    ret = HKEY_CLASSES_ROOT;
                    break;
                case "HKEY_CURRENT_USER":
                    ret = HKEY_CURRENT_USER;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    ret = HKEY_LOCAL_MACHINE;
                    break;
                case "HKEY_USERS":
                    ret = HKEY_USERS;
                    break;
                case "HKEY_PERFORMANCE_DATA":
                    ret = HKEY_PERFORMANCE_DATA;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    ret = HKEY_CURRENT_CONFIG;
                    break;
                case "HKEY_DYN_DATA":
                    ret = HKEY_DYN_DATA;
                    break;
                default:
                    ret = HKEY_LOCAL_MACHINE;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 设置64位注册表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="subKey"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Set64BitRegistryKey(string key, string subKey, string name, string value)
        {
            int STANDARD_RIGHTS_ALL = (0x001F0000);
            int KEY_QUERY_VALUE = (0x0001);
            int KEY_SET_VALUE = (0x0002);
            int KEY_CREATE_SUB_KEY = (0x0004);
            int KEY_ENUMERATE_SUB_KEYS = (0x0008);
            int KEY_NOTIFY = (0x0010);
            int KEY_CREATE_LINK = (0x0020);
            int SYNCHRONIZE = (0x00100000);
            int KEY_WOW64_64KEY = (0x0100);
            int REG_OPTION_NON_VOLATILE = (0x00000000);
            int KEY_ALL_ACCESS = (STANDARD_RIGHTS_ALL | KEY_QUERY_VALUE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY | KEY_ENUMERATE_SUB_KEYS
                                 | KEY_NOTIFY | KEY_CREATE_LINK) & (~SYNCHRONIZE);

            int ret = 0;
            try
            {
                //将Windows注册表主键名转化成为不带正负号的整形句柄（与平台是32或者64位有关）
                IntPtr hKey = TransferKeyName(key);

                //声明将要获取Key值的句柄 
                IntPtr pHKey = IntPtr.Zero;

                //获得操作Key值的句柄
                int lpdwDisposition = 0;
                ret = RegCreateKeyEx(hKey, subKey, 0, "", REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS | KEY_WOW64_64KEY, IntPtr.Zero, out pHKey, out lpdwDisposition);
                if (ret != 0)
                {
                    return ret;
                }

                //关闭注册表转向（禁止特定项的注册表反射）
                RegDisableReflectionKey(pHKey);

                //设置访问的Key值
                uint REG_SZ = 1;
                byte[] data = Encoding.Unicode.GetBytes(value);

                RegSetValueEx(pHKey, name, 0, REG_SZ, data, (uint)data.Length);

                //打开注册表转向（开启特定项的注册表反射）
                RegEnableReflectionKey(pHKey);

                RegCloseKey(pHKey);
            }
            catch (Exception ex)
            {
                return -1;
            }

            return ret;
        }

        public static void SetRegistryKey(string key, string subKey, string name, string value)
        {
            if (System.IntPtr.Size == 8)
            {
                // 写SOFTWARE\Huawei\VirtualDesktopAgent，需要关闭注册表重定向，再写64位路径的注册表
                int ret = RegUtil.Set64BitRegistryKey(key, subKey, name, value);
                if (ret != 0)
                {
                }
            }

            try
            {
                Microsoft.Win32.Registry.SetValue(key + "\\" + subKey, name, value);
            }
            catch (Exception ex)
            {
            }
        }

        public static string GetRegistryValue(string path, string key)
        {
            RegistryKey regkey = null;
            try
            {
                regkey = Registry.LocalMachine.OpenSubKey(path);
                if (regkey == null)
                {
                    return null;
                }

                object val = regkey.GetValue(key);
                if (val == null)
                {
                    return null;
                }

                return val.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (regkey != null)
                {
                    regkey.Close();
                }
            }
        }
    }
}
