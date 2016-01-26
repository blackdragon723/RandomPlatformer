using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    public interface ICollidable
    {
        //bool CheckCollision(ICollidable src, bool pixelPerfect = false);
        //bool PixelPerfectCollision(ICollidable src);
        Rectangle AABB { get; }
        Texture2D Texture { get; }
        bool CheckForCollision { set; get; }
    }
}
