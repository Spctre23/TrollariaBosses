using Microsoft.Xna.Framework;

namespace TrollariaBosses.Helper;

public class VectorUtils
{
    public static Vector2 GetRandomVectorWithinRange(Vector2 pos, int range)
    {
        int x = new Random().Next((int)pos.X - range, (int)pos.X + range);
        int y = new Random().Next((int)pos.Y - range, (int)pos.Y + range);

        return new (x, y);
    }
}
