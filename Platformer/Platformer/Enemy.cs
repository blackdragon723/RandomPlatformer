using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Platformer
{
    public sealed class Enemy : Character, IReactsWithPlayer
    {
        private struct PatrolPath
        {
            public Vector2 start;
            public Vector2 end;
        }
        public bool Patrols { set; get; }
        private PatrolPath patrolPath;
        private Vector2 PatrolTarget;

        public Enemy(ContentManager manager, Vector2 position)
            : base(manager)
        {
            Position = position;
            CheckForCollision = true;
        }

        public override void Load(string xmlPath)
        {
            base.Load(xmlPath);
            Animations["Alive"].Looping = true;
            CurrentAnimation = new Animation(Animations["Alive"]);
        }

        public override void Update(GameTime time)
        {
            if (Patrols)
            {
                float distance;
                Vector2.Distance(ref position, ref PatrolTarget, out distance);
                if (distance < 0.5f)
                {
                    if (PatrolTarget == patrolPath.start)
                        PatrolTarget = patrolPath.end;
                    else
                        PatrolTarget = patrolPath.start;

                    if (Direction == DirectionEnum.Left)
                        Direction = DirectionEnum.Right;
                    else
                        Direction = DirectionEnum.Left;
                }

                position.X += (float)Direction * 150f * (float)time.ElapsedGameTime.TotalSeconds;
            }

            if (!IsAlive)
            {
                position.Y += 350f * (float)time.ElapsedGameTime.TotalSeconds;
            }

            if (position.Y > 600)
                MarkedForDelete = true;

            base.Update(time);
        }

        protected override void AnimationFinish(object sender, AnimationFinishEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SetPatrol(Vector2 end)
        {
            patrolPath.start = Position;
            patrolPath.end = end;
            Patrols = true;

            PatrolTarget = end;

            Direction = patrolPath.start.X < patrolPath.end.X ? DirectionEnum.Right : DirectionEnum.Left;
        }

        public void OnPlayerCollision(Player player)
        {
            if (player.AABB.Y + player.AABB.Height - 10 < AABB.Y)
            {
                Console.WriteLine("Enemy Killed!");
                this.Kill();
            }
            else
            {
                Console.WriteLine("Player Killed");
                player.Damage();
            }

            var origin = new Vector2(AABB.Center.X, AABB.Center.Y);
            var destination = new Vector2(AABB.Center.X, AABB.Center.X + (Texture.Height / 2));
            var normal = (destination - origin);
            normal.Normalize();
            var reflected = player.Velocity - 2 * normal * (player.Velocity * normal);
            player.Velocity = reflected;
        }

        protected override void Kill()
        {
            IsAlive = false;
            
            CurrentAnimation = new Animation(Animations["Dead"]);
        }
    }
}
