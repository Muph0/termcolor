using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TermColor.Test {
    public class TerminalTest {

        [Theory]
        [InlineData(13, 20)]
        [InlineData(22, 15)]
        [InlineData(1, 1)]
        [InlineData(1, 200)]
        [InlineData(200, 1)]
        public void BufferSizeWorks(int width, int height) {
            Win32TermBuffer.EnableVTProcessingThrows = false;
            Terminal term = new(width, height);

            Assert.Equal(term.Width, width);
            Assert.Equal(term.Height, height);
        }

        [Theory]
        [InlineData(-13, 20)]
        [InlineData(22, -15)]
        [InlineData(0, 0)]
        [InlineData(0, 200)]
        [InlineData(200, 0)]
        public void BufferSizeNonpositiveThrows(int width, int height) {
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                new Terminal(width, height);
            });
        }

        
    }
}
