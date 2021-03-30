using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinApi.User32;

namespace Plugins.Shared.Library.WindowsAPI
{
    public class IMEScope:IDisposable
    {
        //输入法是否存在于语言布局中
        private bool _isLayoutAvailable;

        //要切换的输入法
        private string _language;

        //编辑器的句柄
        private IntPtr _hwnd;

        private IntPtr _hkl;

        //原始输入布局
        private IntPtr _sourceLayout;

        public IMEScope(IntPtr hwnd,string language)
        {
            _language = language;
            _hwnd = hwnd;
            LoadLanguage(); 
        }

        /// <summary>
        /// 切换或载入输入法
        /// </summary>
        private void LoadLanguage()
        {
            IntPtr curProcess = IntPtr.Zero;
            var curThread = User32Methods.GetWindowThreadProcessId(_hwnd, curProcess);
            _sourceLayout = IMEHelper.GetKeyboardLayout(curThread);

            _isLayoutAvailable =  IMEHelper.IsLayoutAvailable(_language);

            _hkl= IMEHelper.ChangeToLanguage(_hwnd, _language);
            
        }

        /// <summary>
        /// 创建一个IME编辑上下文
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static IMEScope BeginEdit(IntPtr hwnd,string language)
        {
            var scope= new IMEScope(hwnd, language);
            return scope;
        }

        public void Dispose()
        {
            if (!_isLayoutAvailable)
            {
                IMEHelper.UnloadKeyboardLayout(_hkl);
            }
            else
            {
                IMEHelper.ChangeToLanguage(_hwnd, _sourceLayout);
                //IMEHelper.ActivateKeyboardLayout(IMEHelper.HKL_PREV, (uint)KLF.KLF_SETFORPROCESS);
            }
        }
    }
}
