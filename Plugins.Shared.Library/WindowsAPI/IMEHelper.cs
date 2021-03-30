using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinApi.User32;

namespace Plugins.Shared.Library.WindowsAPI
{
    [Flags]
    public enum KLF : uint
    {
        KLF_ACTIVATE = 0x00000001,
        KLF_SUBSTITUTE_OK = 0x00000002,
        KLF_REORDER = 0x00000008,
        KLF_REPLACELANG = 0x00000010,
        KLF_NOTELLSHELL = 0x00000080,
        KLF_SETFORPROCESS = 0x00000100,
        KLF_SHIFTLOCK = 0x00010000,
        KLF_RESET = 0x40000000
    }

    public class IMEHelper
    {
        public const string HKL_ENGLISH_US = "04090409";

        public const string HKL_CHINESE_SIMPLIFIED = "08040804";

        private const uint KLF_ACTIVATE = 1;

        public const uint HKL_NEXT = 1;

        public const uint HKL_PREV = 0;

        [DllImport("user32.dll")] 
        static extern uint GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

        [DllImport("user32.dll")] 
        static extern bool GetKeyboardLayoutName(StringBuilder pwszKLID);

        [DllImport("user32.dll")] 
        static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        public static extern bool UnloadKeyboardLayout(IntPtr hkl);

        [DllImport("user32.dll")]
        public static extern uint ActivateKeyboardLayout(uint hkl, uint Flags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetKeyboardLayout([In] uint idThread);

        /// <summary>
        /// 切换输入法为中文
        /// </summary>
        /// <param name="hwnd"></param>
        public static void ChangeToChinese(IntPtr hwnd)
        {
            ChangeToLanguage(hwnd, HKL_CHINESE_SIMPLIFIED);
        }
        
        /// <summary>
        /// 切换输入法为英文
        /// </summary>
        /// <param name="hwnd"></param>
        public static void ChangeToEnglish(IntPtr hwnd)
        {
            ChangeToLanguage(hwnd,HKL_ENGLISH_US);    
        }

        /// <summary>
        /// 切换输入法
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static IntPtr ChangeToLanguage(IntPtr hwnd,string language)
        {
            var hkl = LoadKeyboardLayout(language, KLF_ACTIVATE);
            User32Methods.PostMessage(hwnd, (uint)WM.INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);
            return hkl;
        }

        /// <summary>
        /// 切换输入法
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hkl"></param>
        public static void ChangeToLanguage(IntPtr hwnd,IntPtr hkl)
        {
            User32Methods.PostMessage(hwnd, (uint)WM.INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);
        }

        /// <summary>
        /// 是否已存在该键盘布局
        /// </summary>
        /// <param name="layoutId"></param>
        /// <returns></returns>
        public static bool IsLayoutAvailable(string layoutId)
        {
            uint nElements = GetKeyboardLayoutList(0, null);
            IntPtr[] ids = new IntPtr[nElements];
            GetKeyboardLayoutList(ids.Length, ids);
            for (var index = 0; index < ids.Length; index++)
            {
                if (ids[index].ToString("X16").Substring(8).Equals(layoutId)) return true;
            }
            return false;
        }
    }
}
