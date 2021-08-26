using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TermColor.Drawing;

namespace TermColor.Demo {
    class ShapesDemo {
        public static void Run() {

            Stopwatch s = new();

            Terminal term = new Terminal();
            term.ColorMode = ColorMode.Plain8bit;
            var black = Color24.Black.ToHSV();

            int lineCount = term.Width + term.Height;

            s.Start();
            while (s.ElapsedMilliseconds < lineCount * 10 + 1000) {

                term.Clear();

                for (int i = 0; i < lineCount; i++) {
                    var color = new ColorHSV((float)i / lineCount * 360, 1f, 1f);

                    if (i < term.Width) {
                        if (s.ElapsedMilliseconds > i * 10) {
                            term.DrawLine(0, 0, i, term.Height - 1, ' ', black, color);
                        }
                    } else {
                        if (s.ElapsedMilliseconds > i * 10) {
                            term.DrawLine(0, 0, term.Width - 1, lineCount - i - 1, ' ', black, color);
                        }
                    }
                }

                term.Flush();
            }

            term.Clear();
            {
                int x = 0, y = 0;
                int w = 10, h = 10;

                int dirX = 1;
                int dirY = 1;

                s.Restart();
                while (s.ElapsedMilliseconds < 10000) {

                    term.Clear();

                    term.DrawRectangle<IColor>(x, y, x + w, y + h, ' ', black, Color24.CornflowerBlue);

                    if (x + dirX < 0 || x + dirX + w >= term.Width) {
                        dirX *= -1;
                    }
                    if (y + dirY < 0 || y + dirY + h >= term.Height) {
                        dirY *= -1;
                    }

                    x += dirX;
                    y += dirY;

                    term.Flush();
                }
            }
        }
    }
}
