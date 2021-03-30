using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using UniCompiler.CSharpCompiler;
using UniCompiler.Logging;
using UniCompiler.PreProcessing;
using UniCompiler.Utilities;
using UniCompiler.XamlGeneration;

namespace UniCompiler
{
    public class Compiler
    {
        public static bool ExecuteCSharpCompiler(CompilerOptions programArgs, List<WorkflowDocument> documents)
        {
            ConcurrentBag<ResourceDescription> resources = new ConcurrentBag<ResourceDescription>();
            var libraryRequest = LibraryRequest(programArgs, documents, resources);

            LibraryFactory libraryFactory = new LibraryFactory();
            var syntaxTrees = libraryFactory.Build(libraryRequest);
            ObjectFactoryCache.ClearCache();
            ActivityFactoryCache.ClearCache();
            LibraryFactoryCache.ClearCache();
            documents.Clear();

            MemoryStream memoryStream = new MemoryStream();
            try
            {
                string outputFilePath =
                    Path.Combine(libraryRequest.OutputFolder, libraryRequest.LibraryName + ".dll");
                libraryFactory.Compile(syntaxTrees, libraryRequest, memoryStream);
                File.WriteAllBytes(outputFilePath, memoryStream.ToArray());
            }
            catch
            {
                return false;
            }
            finally
            {
                if (memoryStream != null)
                {
                    ((IDisposable)memoryStream).Dispose();
                }
            }
            return true;
        }

        private static GenerateLibraryRequest LibraryRequest(CompilerOptions programArgs, List<WorkflowDocument> documents, ConcurrentBag<ResourceDescription> resources)
        {
            var activitiesToCompile = new ConcurrentQueue<Lazy<GenerateActivityRequest>>(
                documents.Select(doc => new Lazy<GenerateActivityRequest>(() => GenerateActivityRequest(programArgs, resources, doc))));

            return new GenerateLibraryRequest
            {
                LibraryName = programArgs.LibraryName,
                RootPath = programArgs.PathToCompile,
                AssemblyVersion = GetVersion(programArgs.Version).ToString(),
                AssemblyResources = resources,
                OutputFolder = programArgs.OutputFolder,
                PathsToClassNames = documents.ToDictionary(d => d.DocumentAbsolutePath, d => (d.ClassNamespace, d.ClassName), StringComparer.InvariantCultureIgnoreCase),
                ActivitiesToCompile = activitiesToCompile
            };
        }

        private static GenerateActivityRequest GenerateActivityRequest(CompilerOptions programArgs, ConcurrentBag<ResourceDescription> resources,
            WorkflowDocument doc)
        {
            string fileText = File.ReadAllText(doc.DocumentAbsolutePath);
            ActivityBuilder activityBuilder = ActivityBuilderLoader.LoadFromString(fileText);
            if (programArgs.NeedsCurrentWorkingDirectoryResolve)
            {
                activityBuilder.Implementation =
                    activityBuilder.Implementation.DecorateWithCurrentDirectoryResolver(doc.ClassNamespace + "." + doc.ClassName + ", " + programArgs.LibraryName);
            }

            if (programArgs.IncludeOriginalXaml)
            {
                string resourceName = doc.ClassNamespace + "." + doc.ClassName + "_original.xaml";
                Func<Stream> dataProvider = () => new MemoryStream(Encoding.UTF8.GetBytes(fileText));
                resources.Add(new ResourceDescription(resourceName, dataProvider, isPublic: false));
            }

            string descriptionAttribute =
                WorkflowInformationHelper.SerializeAsTypeInformation(
                    WorkflowInformationHelper.ExtractInformation(activityBuilder));

            return new GenerateActivityRequest
            {
                AbsolutePath = doc.DocumentAbsolutePath,
                ActivityBuilder = activityBuilder,
                BrowsableAttribute = doc.PublishActivity,
                ClassName = doc.ClassName,
                ClassNamespace = doc.ClassNamespace,
                DisplayNameAttribute = Path.GetFileNameWithoutExtension(doc.DocumentPath),
                CategoryAttribute = doc.Category,
                DescriptionAttribute = descriptionAttribute,
                CompileExpressions = (programArgs.ExpressionLanguage == "C#")
            };
        }

        private static Version GetVersion(string versionString)
        {
            if (!VersionUtils.TryParse(versionString, out Version version))
            {
                return new Version();
            }
            return version;
        }
    }

    public static class VersionUtils
    {
        public static bool TryParse(string versionString, out Version version)
        {
            int num = versionString.IndexOfAny(new char[2]
            {
                '-',
                '+'
            });
            return Version.TryParse((num != -1) ? versionString.Substring(0, num) : versionString, out version);
        }
    }
}
