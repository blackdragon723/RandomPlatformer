using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    static class Collision
    {
        private delegate int del();
        public static Rectangle CalculateLocalBounds(Texture2D obj, Rectangle Clip)
        {
            Color[] bits = new Color[Clip.Width * Clip.Height];
            obj.GetData(0, Clip, bits, 0, bits.Length);
            Color[,] data = new Color[Clip.Width, Clip.Height];

            for (int x = 0; x < Clip.Width; x++)
            {
                for (int y = 0; y < Clip.Height; y++)
                {
                    data[x, y] = bits[x + y * Clip.Width];
                }
            }

            Rectangle aabb = new Rectangle();
            del FindFirstX = () =>
            {
                for (int x = 0; x < Clip.Width; x++)
                {
                    for (int y = 0; y < Clip.Height; y++)
                    {
                        if (data[x, y].A == 0)
                            continue;

                        return x;
                    }
                }
                return 0;
            };
            aabb.X = FindFirstX();

            del FindFirstY = () =>
            {
                for (int y = 0; y < Clip.Height; y++)
                {
                    for (int x = 0; x < Clip.Width; x++)
                    {
                        if (data[x, y].A == 0)
                            continue;

                        return y;
                    }
                }
                return 0;
            };
            aabb.Y = FindFirstY();

            del FindWidth = () =>
            {
                for (int x = Clip.Width - 1; x > 0; x--)
                {
                    for (int y = Clip.Height - 1; y > 0; y--)
                    {
                        if (data[x, y].A == 0)
                            continue;

                        return x - aabb.X + 1;
                    }
                }
                return 0;
            };
            aabb.Width = FindWidth();

            del FindHeight = () =>
            {
                for (int y = Clip.Height - 1; y > 0; y--)
                {
                    for (int x = Clip.Width - 1; x > 0; x--)
                    {
                        if (data[x, y].A == 0)
                            continue;

                        return y - aabb.Y + 1;
                    }
                }
                return 0;
            };
            aabb.Height = FindHeight();

            return aabb;
        }
        public static bool CheckCollision(ICollidable src, ICollidable target)
        {
            if (src.CheckForCollision && target.CheckForCollision)
            {
                return target.AABB.Intersects(src.AABB);
            }

            return false;
        }

        public static bool CheckCollision(Rectangle a, Rectangle b)
        {
            return a.Intersects(b);
        }

        public static Vector2 GetIntersectionDepth(this Rectangle rectA, Rectangle rectB)
        {
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }
    }
}
