using System;
using System.Windows.Data;
using UniStudio.Community.ViewModel;

namespace UniStudio.Community.Librarys
{
  class ActiveDocumentConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is DocumentViewModel)
        return value;

      return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is DocumentViewModel)
        return value;

      return Binding.DoNothing;
    }
  }
}
