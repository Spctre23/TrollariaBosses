using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollariaBosses.Helper;

public class MathUtils
{
    public static int ScaleRange(int origMin, int origMax, int newMin, int newMax, int value)
    {
        double scale = (double)(newMax - newMin) / (origMax - origMin);
        return (int)(newMin + ((value - origMin) * scale));
    }
}
