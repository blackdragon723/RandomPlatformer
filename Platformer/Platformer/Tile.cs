using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Platformer
{
    class Tile :  IReactsWithPlayer
    {
        public readonly int GID;
        public readonly int LayerNumber;
        public Texture2D Texture { get; set; }
        public Rectangle Clip { set; get; }
        private readonly Rectangle LocalAABB;
        public bool FlagForRemove = false;
        public float Scale = 1f;

        public Vector2 Position { get; set; }
        public Rectangle AABB
        {
            get
            {
                return new Rectangle(
                    (int)Position.X + LocalAABB.X, (int)Position.Y + LocalAABB.Y,
                    LocalAABB.Width, LocalAABB.Height);
            }
        }

        public bool CheckForCollision { set; get; }

        public Tile(Vector2 position, Texture2D texture, Rectangle clip, int id, int layerNumer)
        {
            Position = position;
            Texture = texture;
            GID = id;
            Clip = clip;
            LocalAABB = Collision.CalculateLocalBounds(Texture, Clip);
            CheckForCollision = true;
            LayerNumber = layerNumer;
        }

       // delegate int del();

        public void Draw(SpriteBatch batch, Vector2 position)
        {
            batch.Draw(Texture, position, Clip, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
        }

        public void Update(GameTime time)
        {
            throw new NotImplementedException();
        }

        public virtual void CheckCollision(Player player)
        {
            if (Collision.CheckCollision(this, player))
            {
                OnPlayerCollision(player);
            }
        }

        public virtual void OnPlayerCollision(Player player)
        {
            Rectangle collisionArea = Rectangle.Intersect(player.AABB, this.AABB);

            float distanceY = collisionArea.Height;
            float distanceX = collisionArea.Left - (collisionArea.Left + collisionArea.Width);

            Vector2 depth = player.AABB.GetIntersectionDepth(this.AABB);

            if (depth != Vector2.Zero)
            {
                float absDepthX = Math.Abs(depth.X);
                float absDepthY = Math.Abs(depth.Y);

                if (absDepthY < absDepthX)
                {
                    var pos = new Vector2(player.Position.X, player.Position.Y + depth.Y + (player.AABB.Center.Y - player.Position.Y));
                    player.Position = pos;
                    player.CollidedWithSurface(false);
                    player.OnGround = true;
                }
                else
                {
                    player.Position = new Vector2(player.Position.X + depth.X, player.Position.Y);
                    player.CollidedWithSurface(true);
                }
            }
        }
    }
}
