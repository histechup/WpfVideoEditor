using WpfVideoEditor.Models;
using System;
using System.IO;

namespace WpfVideoEditor.Ffmpeg
{
    /// <summary>
    /// Ffmpeg Arguments
    /// </summary>
    public class FfmpegArguments
    {
        private static class Arg
        {
            public static readonly string NeverOverwriteTarget = "-n";
            public static readonly string OverwriteTarget = "-y";
            public static readonly string StartTime = "-ss";
            public static readonly string EndTime = "-to";
            public static readonly string Duration = "-t";
        }

        private static string GetExtractFilepath(FileInfo sourceFilePath, int fromMs, int toMs) => sourceFilePath.FullName + $"_{fromMs:D}-{toMs:D}{sourceFilePath.Extension}";

        public FileInfo SourceFile { get; set; }
        public FileInfo TargetFile { get; set; }
        public TimeSpan? FromTime { get; set; }
        public TimeSpan? ToTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public bool? OverwriteTargetFile { get; set; }
        public VideoFormat ContainerFormat { get; set; }
        public VideoCodec VideoCodec { get; set; }
        public AudioCodec AudioCodec { get; set; }

        internal FfmpegArguments(FileInfo sourceFilePath, int fromMs, int toMs)
        {
            SourceFile = sourceFilePath;
            TargetFile = new FileInfo(GetExtractFilepath(sourceFilePath, fromMs, toMs));
            FromTime = TimeSpan.FromMilliseconds(fromMs);
            ToTime = TimeSpan.FromMilliseconds(toMs);
        }

        public override string ToString()
        {
            var overwriteFlag = OverwriteTargetFile ?? false ? Arg.OverwriteTarget : Arg.NeverOverwriteTarget;
            var fromS = TimeSpanToSeconds(FromTime);
            var toS = TimeSpanToSeconds(ToTime);
            var extractArguments = @$"{overwriteFlag} {Arg.StartTime} {fromS} {Arg.EndTime} {toS}";
            var targetArguments = GetTargetArguments();

            return @$"{extractArguments} -i ""{SourceFile.FullName}"" {targetArguments} ""{TargetFile.FullName}{GetContainerFileExtension()}""";
        }

        private object GetTargetArguments() => $"{GetVideoArguments()} {GetAudioArguments()} {GetContainerFormatArguments()}";

        private string GetVideoArguments()
        {
            if (VideoCodec == VideoCodec.FromSource)
            {
                return string.Empty;
            }
            return VideoCodec switch
            {
                VideoCodec.AV1 => "-c:v libaom-av1 -strict experimental",
                VideoCodec.VP9 => "-c:v libvpx-vp9",
                VideoCodec.H264 => "-c:v libx264",
                VideoCodec.H265 => "-c:v libx265",
                VideoCodec.Copy => "-c:v copy",
                VideoCodec.Drop => "-vn",
                _ => throw new NotImplementedException($"The ffmpeg parameter creation for this video codec is not implemented yet. {Enum.GetName(typeof(VideoCodec), VideoCodec)}"),
            };
        }

        private string GetAudioArguments()
        {
            if (AudioCodec == AudioCodec.FromSource)
            {
                return string.Empty;
            }
            return AudioCodec switch
            {
                AudioCodec.AAC => "-c:a aac",
                AudioCodec.Opus => "-c:a libopus",
                AudioCodec.MP3 => "-c:a libmp3lame",
                AudioCodec.Copy => "-c:a copy",
                AudioCodec.Drop => "-an",
                _ => throw new NotImplementedException($"The ffmpeg parameter creation for this video codec is not implemented yet. {Enum.GetName(typeof(VideoCodec), VideoCodec)}"),
            };
        }

        private string GetContainerFileExtension()
        {
            if (ContainerFormat == VideoFormat.FromSource)
            {
                return string.Empty;
            }
            return ContainerFormat switch
            {
                VideoFormat.MP4 => ".mp4",
                VideoFormat.MKV => ".mkv",
                VideoFormat.WEBM => ".webm",
                _ => throw new NotImplementedException($"The ffmpeg parameter creation for this container type is not implemented yet. {Enum.GetName(typeof(VideoFormat), ContainerFormat)}"),
            };
        }

        private string GetContainerFormatArguments()
        {
            return ContainerFormat switch
            {
                VideoFormat.MP4 => "-movflags +faststart",
                _ => string.Empty,
            };
        }

        public static string TimeSpanToSeconds(TimeSpan? ts)
        {
            var ms = ts.HasValue ? ts.Value.TotalMilliseconds / 1000 : 0;
            return FormattableString.Invariant($"{ms:0.000}");
        }
    }
}
