using DataTableActivity.Dialog;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Data;
using System.Windows;

namespace DataTableActivity
{
    public partial class BuildDataTableDesigner
    {
        public BuildDataTableDesigner()
        {
            InitializeComponent();
        }

        private void DataTableBuild(object sender, System.Windows.RoutedEventArgs e)
        {
            string text = base.ModelItem.Properties["TableInfo"].ComputedValue as string;
            DataTable dataTable = new DataTable();
            try
            {
                BuildDataTable.ReadDataTableFromXML(text, dataTable);
            }
            catch (Exception arg_2F_0)
            {
                //Trace.TraceError(arg_2F_0.Message);
                //UniMessageBox.Show(UiPath_System_Activities_Design.DataTableNotReadExceptionMessage, UiPath_System_Activities_Design.ErrorMessageBox_Title, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            BuildDataTableDialog buildDataTableDialog = new BuildDataTableDialog(dataTable, base.ModelItem);
            buildDataTableDialog.ShowDialog();
            if (buildDataTableDialog.SaveTable)
            {
                base.ModelItem.Properties["TableInfo"].SetValue(buildDataTableDialog.DataTableXmlSchema);
            }
        }
    }
}