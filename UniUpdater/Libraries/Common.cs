using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using log4net;

namespace UniUpdater.Libraries
{
    public class Common
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetProgramVersion()
        {
            FileVersionInfo myFileVersion = FileVersionInfo.GetVersionInfo(App.Instance.StudioPath);
            return myFileVersion.FileVersion;
        }

        public static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }


        public static string GetDirectoryNameWithoutSuffixFormat(string name, string pattern = @"(.*) \([0-9]+\)")
        {
            Match match = Regex.Match(name, pattern);
            if (match.Success)
            {
                if (match.Groups.Count == 2)
                {
                    name = match.Groups[1].Value;
                }
            }

            return name;
        }

        public static string GetFileNameWithoutSuffixFormat(string name, string pattern = @"(.*) \([0-9]+\)")
        {
            var ext = Path.GetExtension(name);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);

            Match match = Regex.Match(fileNameWithoutExt, pattern);
            if (match.Success)
            {
                if (match.Groups.Count == 2)
                {
                    fileNameWithoutExt = match.Groups[1].Value;
                }
            }

            return fileNameWithoutExt + ext;
        }

        public static string GetValidDirectoryName(string path, string name, string suffix_format = " ({0})", int begin_index = 2)
        {
            var validName = name;
            if (Directory.Exists(path + @"\" + name))
            {
                for (int i = begin_index; ; i++)
                {
                    var format_i = string.Format(suffix_format, i);
                    if (!Directory.Exists(path + @"\" + name + format_i))
                    {
                        validName = name + format_i;
                        break;
                    }
                }
            }

            return validName;
        }

        public static string GetValidFileName(string path, string name, string prefix_format = "", string suffix_format = " ({0})", int begin_index = 2)
        {
            var ext = Path.GetExtension(path + @"\" + name);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(path + @"\" + name);

            var validName = name;
            if (File.Exists(path + @"\" + name))
            {
                if (File.Exists(path + @"\" + fileNameWithoutExt + prefix_format + ext))
                {
                    for (int i = begin_index; ; i++)
                    {
                        var format_i = string.Format(prefix_format + suffix_format, i);
                        if (!File.Exists(path + @"\" + fileNameWithoutExt + format_i + ext))
                        {
                            validName = fileNameWithoutExt + format_i + ext;
                            break;
                        }
                    }
                }
                else
                {
                    validName = fileNameWithoutExt + prefix_format + ext;
                }


            }

            return validName;
        }


        public static bool DeleteDir(string dir)//递归删除目录
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                di.Delete(true);
            }
            catch (Exception err)
            {
                return false;
            }

            return true;
        }

        public static bool IsStringInFile(string fileName, string searchString)
        {
            return File.ReadAllText(fileName).Contains(searchString);
        }


        public static string MakeRelativePath(string baseDir, string filePath)
        {
            if (string.IsNullOrEmpty(baseDir)) throw new ArgumentNullException("baseDir");
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");

            if (!baseDir.EndsWith(@"\") && !baseDir.EndsWith(@"/"))
            {
                baseDir += @"\";
            }

            Uri fromUri = new Uri(baseDir);
            Uri toUri = new Uri(filePath);

            if (fromUri.Scheme != toUri.Scheme) { return filePath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }


        /// <summary>
        /// 计算文件的MD5校验
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string filePath)
        {
            FileStream file=null;
            try
            {
                file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch
            {
                throw;
            }
            finally
            {
                file?.Close();
            }
        }

        ///<summary>
        /// MD5加密函数
        ///</summary>
        ///<param name="toCryString"></param>
        ///<returns></returns>
        public static string MD5(string text, int length = 32)
        {
            string strEncryt = string.Empty;
            MD5CryptoServiceProvider hashmd5;
            hashmd5 = new MD5CryptoServiceProvider();
            strEncryt = BitConverter.ToString(hashmd5.ComputeHash(Encoding.Default.GetBytes(text.ToCharArray()))).Replace("-", "").ToLower();
            //将所有字母变小写
            if (length == 16)
            {
                strEncryt = strEncryt.Substring(8, 16).ToLower();
            }

            return strEncryt;
        }




        ////////////////////////////////////////////////////////////////////////////

        [DllImport("shell32.dll")]
        public extern static IntPtr ShellExecute(IntPtr hwnd,
                                                 string lpOperation,
                                                 string lpFile,
                                                 string lpParameters,
                                                 string lpDirectory,
                                                 int nShowCmd
                                                );
        public enum ShowWindowCommands
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }

        public static void ShellExecute(string lpFile, string lpParameters = "")
        {
            ShellExecute(IntPtr.Zero, "open", lpFile, lpParameters, null, (int)Common.ShowWindowCommands.SW_SHOW);
        }

        ////////////////////////////////////////////////////////////////////////////

        public static string GetFileNameFromUrl(string url)
        {
            string[] strs = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var originFileName = "";
            if (strs.Length > 0)
            {
                originFileName = strs[strs.Length - 1];
            }

            return originFileName;
        }

        /// <summary>
        /// 遍历一个目录所有的子目录及文件，通过函数进行判断，如果返回true，则直接返回，标明已经确认某个条件成立
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public delegate bool CheckDeleage(object item, object param);
        public static bool DirectoryChildrenForEach(DirectoryInfo di, CheckDeleage checkFun, object param = null)
        {
            //当前目录文件夹遍历
            DirectoryInfo[] dis = di.GetDirectories();
            for (int j = 0; j < dis.Length; j++)
            {
                DirectoryInfo diItem = dis[j];
                if (checkFun != null && checkFun(diItem, param))
                {
                    return true;
                }

                if (DirectoryChildrenForEach(diItem, checkFun, param))
                {
                    return true;
                }
            }

            //当前目录文件遍历
            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                FileInfo fiItem = fis[i];

                if (checkFun != null && checkFun(fiItem, param))
                {
                    return true;
                }
            }


            return false;

        }

        /// <summary>
        /// 复制文件夹中的所有内容
        /// </summary>
        /// <param name="sourceDirPath">源文件夹目录</param>
        /// <param name="targetDirPath">目标文件夹目录</param>
        public static void Copy(string sourceDirPath, string targetDirPath)
        {            
            if (!Directory.Exists(targetDirPath))
            {
                Directory.CreateDirectory(targetDirPath);
            }

            string[] files = Directory.GetFiles(sourceDirPath);
            foreach (string file in files)
            {
                try
                {
                    string pFilePath = targetDirPath + "\\" + Path.GetFileName(file);
                    if (File.Exists(pFilePath) && GetMD5HashFromFile(file) == GetMD5HashFromFile(pFilePath))
                    {
                        continue;
                    }
                    File.Copy(file, pFilePath, true);
                }
                catch(Exception ex)
                {
                    Logger.Error(ex, logger);
                }
            }

            string[] dirs = Directory.GetDirectories(sourceDirPath);
            foreach (string dir in dirs)
            {
                Copy(dir, targetDirPath + "\\" + Path.GetFileName(dir));
            }
        }
    }
}
