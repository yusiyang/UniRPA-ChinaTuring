using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WordPlugins
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WordCreateDesigner
    {

        public WordCreateDesigner()
        {
            InitializeComponent();
            //TextBox textBox = urlTextBox;
            //textBox.KeyDown += new KeyEventHandler(textBox1_KeyUp);
        }


        
        private void IcoPath_Loaded(object sender, RoutedEventArgs e)
        {
            icoPath.ImageSource = new BitmapImage(new Uri(CommonVariable.getPropertyValue("icoPath", ModelItem)));
        }

        private void PathSelect(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
