using System;
using System.Diagnostics;
using System.Linq;
using TermColor;

namespace TermColor.Demo {
    class Program {
        static void Main(string[] args) {

            Console.Title = "TermColor demo";
            Console.CursorVisible = false;
            Console.ResetColor();
            Console.Clear();

            var term = new Terminal(20, 10);
            term.ForegroundColor = Color24.Turquoise;
            term.WriteLine("Hello, world!");
            term.ForegroundColor = Color24.RosyBrown;
            term.WriteLine("Press enter to continue to next demo...");
            
            term.Flush();

            Console.ReadLine();

            ColorSpaceDemo.Run();

            Console.Clear();
            Console.SetCursorPosition(0, 0);

            ShapesDemo.Run();
        }
    }
}
