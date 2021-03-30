using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace UniCompiler.PreProcessing
{
	internal class WfDocUtils
	{
		private static readonly CSharpCodeProvider _codeProvider = new CSharpCodeProvider();

		private const int GenerateMaxRetries = 10;

		public static string GenerateValidIdentifier(string name)
		{
			string value = Regex.Replace(name, "[^\\w_]", "_");
			value = _codeProvider.CreateValidIdentifier(value);
			int num = 0;
			bool flag;
			while (!(flag = _codeProvider.IsValidIdentifier(value)) && num < 10)
			{
				value = "_" + value;
				num++;
			}
			if (!flag)
			{
				//throw new CSharpCompilationException(string.Format(Resources.WfDocUtils_GenerateValidIdentifier_Unable_to_generate_valid_identifier_for__0_, name));
				throw new Exception("");
			}
			return value;
		}

		public static char[] GetInvalidFileCharacters(string fileName)
		{
			string text = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
			List<char> list = new List<char>();
			string text2 = text;
			for (int i = 0; i < text2.Length; i++)
			{
				char item = text2[i];
				if (fileName.Contains(item.ToString()))
				{
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		public static string ComputeClassName(string fileName)
		{
			return GenerateValidIdentifier(Path.GetFileNameWithoutExtension(fileName));
		}

		public static string NormalizeDirectorySeparator(string fileName)
		{
			return fileName.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
		}

		public static string GetNormalizedDirectoryFullPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return path;
			}
			string text = NormalizeDirectorySeparator(Path.GetFullPath(path));
			if (text.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				text = text.Substring(0, text.Length - 1);
			}
			return text;
		}

		public static string ComputeQualifiedName(string fileName, string rootPath, string rootNameSpace, bool escapeRootNamespace = true)
		{
			rootPath = NormalizeDirectorySeparator(rootPath);
			fileName = NormalizeDirectorySeparator(fileName);
			string input = fileName;
			string text = escapeRootNamespace ? GenerateValidIdentifier(rootNameSpace) : rootNameSpace;
			string str = Regex.Escape(Path.GetFileName(fileName));
			string text2 = Regex.Escape(Path.DirectorySeparatorChar.ToString());
			string str2 = Regex.Escape(rootPath);
			string pattern = "^" + str2 + text2 + "?";
			string[] array = Regex.Split(Regex.Replace(Regex.Replace(input, pattern, string.Empty), text2 + "*" + str + "$", string.Empty), text2);
			foreach (string text3 in array)
			{
				if (!string.IsNullOrWhiteSpace(text3))
				{
					string str3 = GenerateValidIdentifier(text3);
					text = text + "." + str3;
				}
			}
			return text;
		}

		public static string GetCorrectCaseFileRelativePath(string rootPath, string invokedWorkflowFile, string warningCategory)
		{
			try
			{
				string fullPath = Path.GetFullPath(Path.Combine(rootPath, invokedWorkflowFile));
				string text = Path.GetFullPath(rootPath);
				if (!text.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					text = $"{text}{Path.DirectorySeparatorChar}";
				}
				Uri uri = new Uri(GetProperFilePathCapitalization(fullPath));
				return NormalizeDirectorySeparator(Uri.UnescapeDataString(new Uri(text).MakeRelativeUri(uri).OriginalString));
			}
			catch (Exception)
			{
				//Logger.WriteLine(string.Format(Resources.WfDocUtils_GetCorrectCaseFilePath, invokedWorkflowFile), warningCategory, TraceEventType.Warning);
				return invokedWorkflowFile;
			}
		}

		private static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
		{
			DirectoryInfo parent = dirInfo.Parent;
			if (parent == null)
			{
				return dirInfo.Name;
			}
			return Path.Combine(GetProperDirectoryCapitalization(parent), parent.GetDirectories(dirInfo.Name)[0].Name);
		}

		private static string GetProperFilePathCapitalization(string filename)
		{
			FileInfo fileInfo = new FileInfo(filename);
			DirectoryInfo directory = fileInfo.Directory;
			return Path.Combine(GetProperDirectoryCapitalization(directory), directory.GetFiles(fileInfo.Name)[0].Name);
		}
	}

}
