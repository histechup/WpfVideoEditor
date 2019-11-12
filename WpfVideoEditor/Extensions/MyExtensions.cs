using System;
using System.IO;

namespace WpfVideoEditor
{
    public static class MyExtensions
    {
        /// <summary>
        /// returns a pretty (human readable) file lenght 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string LengthAsPrettyString(this FileInfo file) => FormatFileSize(file?.Length ?? throw new ArgumentNullException(nameof(file)));

        private static readonly string[] FileSizeMagnitudes = { "B", "KB", "MB", "GB", "TB" };

        /// <summary>
        /// Returns formated file size
        /// </summary>
        /// <param name="bytesCount"></param>
        /// <returns></returns>
        public static string FormatFileSize(long bytesCount)
        {
            double i = bytesCount;
            var order = 0;
            while (i >= 1024 && order < FileSizeMagnitudes.Length - 1)
            {
                ++order;
                i /= 1024;
            }

            return $"{i:0.##} {FileSizeMagnitudes[order]}";
        }

        /// <summary>
        /// Returns a specified number raised to the specified power
        /// </summary>
        /// <param name="bas"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static int Pow(this int bas, int exp)
        {
            if (exp == 0)
            {
                return 1;
            }
            var i = bas;
            while (--exp > 0)
            {
                i *= bas;
            }
            return i;
        }


        /// <summary>
        /// Returns a specified number raised to the specified power
        /// </summary>
        /// <param name="bas"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static long Pow(this long bas, long exp)
        {
            if (exp == 0)
            {
                return 1;
            }
            var i = bas;
            while (--exp > 0)
            {
                i *= bas;
            }
            return i;
        }

    }
}
