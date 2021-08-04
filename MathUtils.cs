using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermColor {

    /// <summary>
    /// Helper for 
    /// </summary>
    internal static class MathUtils {
        public static float Clamp(this float x, float min, float max) {
            return MathF.Min(max, MathF.Max(min, x));
        }
    }
}
