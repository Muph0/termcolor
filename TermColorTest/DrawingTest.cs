using System;
using TermColor;
using Xunit;

using TermColor.Drawing;
using System.IO;

namespace TermColor.Test {
    public class DrawingTest {

        [Theory]
        [InlineData(6, 4, 1, 1, 3, 3, "      \n" +
                                      " xxx  \n" +
                                      " xxx  \n" +
                                      " xxx  \n")]
        public void RectangleFilled(int width, int height, int startX, int startY, int endX, int endY, string expected) {

            var buf = new TextOnlyBuffer(width, height);
            buf.FillRectangle<Color4>(startX, startY, endX, endY, 'x', ConsoleColor.Red, ConsoleColor.Red);

            var sw = new StringWriter();
            buf.Flush(sw);

            Assert.Equal(expected, sw.ToString());
        }

        [Fact]
        public void TestsRunInDebug() {
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            Assert.True(debug);
        }
    }
}
