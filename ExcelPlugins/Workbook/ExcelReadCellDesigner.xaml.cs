using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;


namespace ExcelPlugins
{
    // ActivityDesigner1.xaml 的交互逻辑
    public partial class ExcelReadCellDesigner
    {
        public ExcelReadCellDesigner()
        {
            InitializeComponent();
        }
        private void IcoPath_Loaded(object sender, RoutedEventArgs e)
        {
            icoPath.ImageSource = new BitmapImage(new Uri(CommonVariable.getPropertyValue("icoPath", ModelItem)));
        }

        private void PathSelect(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xls files (*.xls)|*.xls|xlsx files (*.xlsx)|*.xlsx|xlsm files(启用宏)|*.xlsm|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fName = openFileDialog.FileName;
                List<ModelProperty> PropertyList = ModelItem.Properties.ToList();
                ModelProperty _property = PropertyList.Find((ModelProperty property) => property.Name.Equals("PathUrl"));
                InArgument<string> pathValue = fName;
                _property.SetValue(pathValue);
            }
        }
    }
}
