using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platformer
{
    public interface IReactsWithPlayer : ICollidable
    {
        void OnPlayerCollision(Player player);
    }
}
