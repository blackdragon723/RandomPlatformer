using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Platformer
{
    interface IGameObject : ICollidable, IPosition
    {
        void Draw(SpriteBatch batch);
        void Update(GameTime time);
    }
}
