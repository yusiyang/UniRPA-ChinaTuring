using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public class DirectoryOperation
    {
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
                string pFilePath = targetDirPath + "\\" + Path.GetFileName(file);
                File.Copy(file, pFilePath, true);
            }

            string[] dirs = Directory.GetDirectories(sourceDirPath);
            foreach (string dir in dirs)
            {
                Copy(dir, targetDirPath + "\\" + Path.GetFileName(dir));
            }
        }

        /// <summary>
        /// 删除文件夹下的所有文件与子文件夹
        /// </summary>
        /// <param name="file"></param>
        public static void DeleteDir(string file)
        {
            //去除文件夹和子文件的只读属性
            //去除文件夹的只读属性
            System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
            fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

            //去除文件的只读属性
            System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

            //判断文件夹是否还存在
            if (Directory.Exists(file))
            {
                foreach (string f in Directory.GetFileSystemEntries(file))
                {
                    if (File.Exists(f))
                    {
                        //如果有子文件删除文件
                        File.Delete(f);
                    }
                    else
                    {
                        //循环递归删除子文件夹
                        DeleteDir(f);
                    }
                }

                //删除空文件夹
                Directory.Delete(file);
            }
        }
    }
}
