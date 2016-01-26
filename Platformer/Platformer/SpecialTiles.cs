using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    class Spike : Tile
    {
        public Spike(Vector2 position, Texture2D texture, Rectangle clip, int id, int layerNumber)
            : base(position, texture, clip, id, layerNumber)
        {
        }

        public override void OnPlayerCollision(Player player)
        {
            if (player.RecentlyDamaged)
                return;

            Rectangle collisionArea = Rectangle.Intersect(player.AABB, this.AABB);
            bool yCollision = false;
            if (collisionArea.Width > collisionArea.Height)
                yCollision = true;

            Vector2 depth = player.AABB.GetIntersectionDepth(this.AABB);
            if (yCollision)
            {
                var origin = new Vector2(AABB.Center.X, AABB.Center.Y);
                var destination = new Vector2(AABB.Center.X, AABB.Center.X + (Texture.Height / 2));
                var normal = (destination - origin);
                normal.Normalize();
                var reflected = player.Velocity - 2 * normal * (player.Velocity * normal);
                player.Velocity = reflected;
                player.Damage();
            }
            else
            {
                player.Position = new Vector2(player.Position.X + depth.X, player.Position.Y);
                player.CollidedWithSurface(true);
            }
        }
    }

    class Coin : Tile
    {
        public Coin(Vector2 position, Texture2D texture, Rectangle clip, int id, int layerNumber)
            : base(position, texture, clip, id, layerNumber)
        {
        }

        public override void OnPlayerCollision(Player player)
        {
            player.CoinsCollected++;
            this.FlagForRemove = true;
        }
    }

    class ExitPortal : Tile
    {
        public bool Triggered = false;

        public ExitPortal(Vector2 position, Texture2D texture)
            : base(position, texture, new Rectangle(0, 0, 102, 102), -1, 5)
        {
        }

        public override void CheckCollision(Player player)
        {
            if (Collision.CheckCollision(player, this))
            {
                AABB.GetIntersectionDepth(player.AABB);
                if (player.AABB.GetIntersectionDepth(this.AABB).X > 30)
                {
                    OnPlayerCollision(player);
                }
            }
        }

        public override void OnPlayerCollision(Player player)
        {
            Triggered = true;
        }
    }
}
