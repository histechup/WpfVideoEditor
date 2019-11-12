using Xunit;

namespace WpfVideoEditor
{
    public class MyExtensionsTest_FormatFileSize
    {
        [Fact]
        public void FormatFileSize_0B() => Assert.Equal("0 B", MyExtensions.FormatFileSize(0));

        [Fact]
        public void FormatFileSize_1B() => Assert.Equal("1 B", MyExtensions.FormatFileSize(1));

        [Fact]
        public void FormatFileSize_123_123B() => Assert.Equal("123 B", MyExtensions.FormatFileSize(123));

        [Fact]
        public void FormatFileSize_1023_1023B() => Assert.Equal("1023 B", MyExtensions.FormatFileSize(1023));

        [Fact]
        public void FormatFileSize_1024_1KB()
        {
            Assert.Equal("1 KB", MyExtensions.FormatFileSize(1024));
            Assert.Equal("1 KB", MyExtensions.FormatFileSize(1024.Pow(1)));
        }

        [Fact]
        public void FormatFileSize_1MB() => Assert.Equal("1 MB", MyExtensions.FormatFileSize(1024.Pow(2)));

        [Fact]
        public void FormatFileSize_1GB() => Assert.Equal("1 GB", MyExtensions.FormatFileSize(1024.Pow(3)));

        [Fact]
        public void FormatFileSize_1TB() => Assert.Equal("1 TB", MyExtensions.FormatFileSize(1024L.Pow(4)));

        [Fact]
        public void FormatFileSize_1024TB()
        {
            Assert.Equal("2 TB", MyExtensions.FormatFileSize(1024L.Pow(4) * 2));
            FormatFileSizeTest("2 TB", 1024L.Pow(4) * 2);
        }

        private void FormatFileSizeTest(string expected, long bytesCount)
        {
            Assert.Equal(expected, MyExtensions.FormatFileSize(bytesCount));
        }
    }
}
