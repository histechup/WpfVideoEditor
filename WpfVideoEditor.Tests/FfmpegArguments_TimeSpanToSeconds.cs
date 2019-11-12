using WpfVideoEditor.Ffmpeg;
using System;
using Xunit;

namespace WpfVideoEditor
{
    public class FfmpegArguments_TimeSpanToSeconds
    {
        [Fact]
        public void TimeSpanToSeconds_null() => Assert.Equal("0.000", FfmpegArguments.TimeSpanToSeconds(null));
        [Fact]
        public void TimeSpanToSeconds_0() => Assert.Equal("0.000", FfmpegArguments.TimeSpanToSeconds(TimeSpan.FromMilliseconds(0)));
        [Fact]
        public void TimeSpanToSeconds_1() => Assert.Equal("0.001", FfmpegArguments.TimeSpanToSeconds(TimeSpan.FromMilliseconds(1)));
        [Fact]
        public void TimeSpanToSeconds_10() => Assert.Equal("0.010", FfmpegArguments.TimeSpanToSeconds(TimeSpan.FromMilliseconds(10)));
        [Fact]
        public void TimeSpanToSeconds_11() => Assert.Equal("0.011", FfmpegArguments.TimeSpanToSeconds(TimeSpan.FromMilliseconds(11)));
        [Fact]
        public void TimeSpanToSeconds_111() => Assert.Equal("0.111", FfmpegArguments.TimeSpanToSeconds(TimeSpan.FromMilliseconds(111)));
        [Fact]
        public void TimeSpanToSeconds_1111() => Assert.Equal("1.111", FfmpegArguments.TimeSpanToSeconds(TimeSpan.FromMilliseconds(1111)));
        [Fact]
        public void TimeSpanToSeconds_11111() => Assert.Equal("11.111", FfmpegArguments.TimeSpanToSeconds(TimeSpan.FromMilliseconds(11111)));
        [Fact]
        public void TimeSpanToSeconds_12034() => Assert.Equal("12.034", FfmpegArguments.TimeSpanToSeconds(TimeSpan.FromMilliseconds(12034)));
    }
}
