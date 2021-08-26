using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermColor.Demo {
    class ColorSpaceDemo {
        public static void Run() {
            int termWidth = 0;
            int termHeight = 0;

            int consoleW = -1;
            int consoleH = -1;

            Terminal[] terminals = null;

            void on_resize() {
                Console.CursorVisible = false;
                consoleW = Console.WindowWidth;
                consoleH = Console.WindowHeight;
                termWidth = consoleW / 2 - 1;
                termHeight = consoleH / 2 - 1;
                termWidth -= termWidth % 2;
                terminals = new Terminal[4].Select((v, i) => new Terminal(termWidth, termHeight, ColorMode.Plain4bit + i)).ToArray();
            }


            var times = new double[] { 0, 0, 0, 0 };
            Stopwatch stopky = new(), time = new();
            time.Start();

            while (time.Elapsed.TotalSeconds < 48) {

                if (consoleW != Console.WindowWidth || consoleH != Console.WindowHeight) {
                    on_resize();
                    Console.Clear();
                }

                Console.SetCursorPosition(0, 0);

                for (int i = 0; i < 4; i++) {

                    var terminal = terminals[i];
                    terminal.Clear();

                    float seconds = time.ElapsedMilliseconds / 1000.0f;


                    for (int y = 0; y < terminal.Height - 1; y++) {
                        for (int x = 0; x < terminal.Width; x += 2) {
                            float a = (float)x / (terminal.Width - 1), b = 1f - (float)y / (terminal.Height - 2);

                            if (((int)seconds / 24) % 2 == 0) {
                                terminal.BackgroundColor = new ColorHSV(60 * seconds, a, b);
                            } else {
                                terminal.BackgroundColor = new ColorRGB(a, b, MathF.Sin(seconds / 2) / 2 + 0.5f);
                            }

                            terminal.Write("  ");
                        }
                    }

                    for (int x = 0; x < terminal.Width; x++) {
                        terminal.BackgroundColor = new ColorHSV((float)x / terminal.Width * 360f, 1f, 1f);
                        terminal.Write(' ');
                    }

                    terminal.ResetColor();
                    terminal.SetCursorPosition(1, 1);
                    terminal.WriteLine($" {terminal.ColorMode} ");
                    terminal.CursorLeft++;
                    terminal.WriteLine($" {terminal.Buffer.GetType().Name} ");
                    terminal.CursorLeft++;
                    terminal.WriteLine($" {times[i]:F3} ms ");

                    stopky.Restart();

                    terminal.Flush((i % 2) * (terminal.Width + 1), (i / 2) * (terminal.Height + 1));

                    stopky.Stop();

                    double q = 0.05;
                    times[i] = (1 - q) * times[i] + q * (stopky.Elapsed.TotalSeconds * 1000);
                }
            }
        }
    }
}
