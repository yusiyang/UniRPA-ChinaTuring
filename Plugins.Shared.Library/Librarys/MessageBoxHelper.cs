using Plugins.Shared.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Plugins.Shared.Library.Librarys
{
    public static class MessageBoxHelper
    {
        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Common.RunInUI(() => UniMessageBox.Show(messageText, caption, button, icon));
        }

        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle)
        {
            return Common.RunInUI(() => UniMessageBox.Show(messageText, caption, button, icon, messageBoxStyle));
        }

        public static MessageBoxResult Show(string messageText)
        {
            return Common.RunInUI(() => UniMessageBox.Show(messageText));
        }

        public static MessageBoxResult Show(string messageText, string caption)
        {
            return Common.RunInUI(() => UniMessageBox.Show(messageText,caption));
        }

        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Common.RunInUI(() => UniMessageBox.Show(messageText, caption, button, icon, defaultResult));
        }

        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            return Common.RunInUI(() => UniMessageBox.Show(messageText, caption, button, icon, defaultResult, messageBoxStyle));
        }
    }
}
