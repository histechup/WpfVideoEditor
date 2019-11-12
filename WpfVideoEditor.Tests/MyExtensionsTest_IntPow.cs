using Xunit;

namespace WpfVideoEditor
{
    public class MyExtensionsTest_IntPow
    {
        [Fact]
        public void FormatFileSize_0() => Assert.Equal(0, 0.Pow(1));
        [Fact]
        public void FormatFileSize_0Pow0_1() => Assert.Equal(1, 0.Pow(0));
        [Fact]
        public void FormatFileSize_1Pow0_1() => Assert.Equal(1, 1.Pow(0));
        [Fact]
        public void FormatFileSize_999Pow0_1() => Assert.Equal(1, 999.Pow(0));
        [Fact]
        public void FormatFileSize_0Pow1() => Assert.Equal(0, 0.Pow(1));
        [Fact]
        public void FormatFileSize_1Pow1() => Assert.Equal(1, 1.Pow(1));
        [Fact]
        public void FormatFileSize_10Pow1() => Assert.Equal(10, 10.Pow(1));
        [Fact]
        public void FormatFileSize_0Pow2() => Assert.Equal(0, 0.Pow(2));
        [Fact]
        public void FormatFileSize_1Pow2() => Assert.Equal(1, 1.Pow(2));
        [Fact]
        public void FormatFileSize_10Pow2() => Assert.Equal(100, 10.Pow(2));
        [Fact]
        public void FormatFileSize_1024Pow2() => Assert.Equal(1024 * 1024, 1024.Pow(2));
    }
}
