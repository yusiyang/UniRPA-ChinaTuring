using System;
using System.Activities;
using System.Activities.Presentation.Annotations;
using System.Activities.XamlIntegration;
using System.Collections.Concurrent;
using System.IO;
using System.Xaml;

namespace UniCompiler.CSharpCompiler
{
    class ActivityBuilderLoader
    {
        private static readonly ConcurrentBag<XamlSchemaContext> XamlSchemaContextsPool;

        static ActivityBuilderLoader()
        {
            XamlSchemaContextsPool = new ConcurrentBag<XamlSchemaContext>();
            Annotation.GetAnnotationText(new object());
        }

        public static ActivityBuilder LoadFromFile(string file)
        {
            return LoadFromString(File.ReadAllText(file));
        }

        public static ActivityBuilder LoadFromString(string content)
        {
            XamlSchemaContext result;
            XamlSchemaContext xamlSchemaContext = XamlSchemaContextsPool.TryTake(out result) ? result : new XamlSchemaContext();
            try
            {
                object obj;
                using (StringReader textReader = new StringReader(content))
                {
                    using (XamlXmlReader innerReader = new XamlXmlReader(textReader, xamlSchemaContext))
                    {
                        XamlReader xamlReader = ActivityXamlServices.CreateBuilderReader(innerReader, xamlSchemaContext);
                        obj = XamlServices.Load(xamlReader);
                        xamlReader.Close();
                    }
                }
                return (ActivityBuilder)obj;
            }
            catch (Exception ex)
            {
                string text = ex.Message;
                if (ex.InnerException != null)
                {
                    text = text + " " + ex.InnerException.Message;
                }

                throw;
            }
            finally
            {
                XamlSchemaContextsPool.Add(xamlSchemaContext);
            }
        }
	}
}
