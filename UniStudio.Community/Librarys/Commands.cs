using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniStudio.Community.Librarys
{
    public static class Commands {

        public static ICommand ClosedCommand { get; private set; }

        public static ICommand DragMoveCommand { get; private set; }

        public static ICommand OpenChildCommand { get; private set; }

        static Commands() { 

            ClosedCommand = new RelayCommand<Window>((win) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    win.Close();
                });
            });

            DragMoveCommand = new RelayCommand<Window>((win) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    win.DragMove();
                });
            });

            OpenChildCommand = new RelayCommand<Type>((type) =>
            {
                if (Activator.CreateInstance(type) is Window window)
                {
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();
                }
            });
        }
    }
}
