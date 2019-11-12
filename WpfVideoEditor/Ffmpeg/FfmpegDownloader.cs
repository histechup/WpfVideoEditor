using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace WpfVideoEditor
{
    /// <summary>
    /// Ffmpeg downloader
    /// </summary>
    class FfmpegDownloader : IDisposable
    {
        private static readonly string Version = "20190620-86f04b9";
        private static readonly Uri DownloadUrl = new Uri($"https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-{Version}-win64-static.zip");

        public static void Download()
        {
            using var downloader = new FfmpegDownloader();
            downloader.DoDownload();
        }

        private readonly HttpClient HttpClient = new HttpClient();

        private FfmpegDownloader()
        {
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        private void DoDownload()
        {
            var res = DownloadZip();
            using var zip = new ZipArchive(res, ZipArchiveMode.Read);
            var entry = zip.GetEntry($"ffmpeg-{Version}-win64-static/bin/ffmpeg.exe");
            entry.ExtractToFile("ffmpeg.exe", overwrite: false);
            //zip.GetEntry("ffmpeg-20190612-caabe1b-win64-static")
        }

        private Stream DownloadZip()
        {
            var res = HttpClient.GetAsync(DownloadUrl).Result;
            return res.Content.ReadAsStreamAsync().Result;
        }
    }
}
