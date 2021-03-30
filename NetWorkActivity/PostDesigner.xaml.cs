using Plugins.Shared.Library;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Activities;
using Microsoft.VisualBasic.Activities;
using System.Activities.Expressions;
using System.Windows.Forms;

namespace NetWorkActivity
{
    public partial class PostDesigner
    {
        public PostDesigner()
        {
            InitializeComponent();
        }
        private void PathSelect(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
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
