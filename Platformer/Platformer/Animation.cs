using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Runtime.CompilerServices;

namespace Platformer
{
    public delegate void OnFinishEventHandler(object sender, AnimationFinishEventArgs e);
    public sealed class Animation
    {
        internal readonly Texture2D Texture;
        public Color Color = Color.White;
        public SpriteEffects Effects = SpriteEffects.None;

        public Vector2 Origin;
        public Vector2 FrameSize;

        public float Rotation = 0f;
        public float Scale = 1f;

        private List<Rectangle> Clips;
        private List<Rectangle> BoundingBoxes;
        public Rectangle BoundingBox
        {
            get { return BoundingBoxes.ElementAt(FrameIndex); }
        }

        private int FrameIndex = 0;
        private int NumFrames;

        private float timeElapsed;
        private float timePerFrame = 1 / 20f;
        public int FramesPerSecond
        {
            set { timePerFrame = (1f / value); }
        }

        public bool Finished = false;
        public bool Looping = false;
        public int LoopingFor { set; get; }
        private int LoopIteration = 0;

        public string Name { get; set; }

        public Animation()
        {
        }

        public Animation(Texture2D texture, int frames, string name)
        {
            this.Texture = texture;
            NumFrames = frames;
            Name = name;

            int width = this.Texture.Width / frames;

            Clips = new List<Rectangle>(frames);
            for (int i = 0; i < frames; i++)
            {
                Rectangle r = new Rectangle(i * width, 0, width, Texture.Height);
                Clips.Add(r);
            }

            Rectangle rec = Clips[0];
            FrameSize = new Vector2(width, Texture.Height);

            GenerateBoundingBoxes();
        }

        public Animation(Texture2D texture, List<Rectangle> rects, string name)
        {
            this.Texture = texture;
            NumFrames = rects.Count();
            Name = name;

            Clips = rects;
            
            FrameSize = new Vector2(rects[0].Width, rects[0].Height);

            GenerateBoundingBoxes();
        }

        public void GenerateBoundingBoxes()
        {
            BoundingBoxes = new List<Rectangle>();
            foreach (Rectangle clip in Clips)
            {
                Rectangle r = Collision.CalculateLocalBounds(Texture, clip);
                BoundingBoxes.Add(r);
            }
        }

        // Messy but it works
        public Animation(Animation copy)
        {
            Texture = copy.Texture;
            Color = copy.Color;
            Origin = copy.Origin;
            Rotation = copy.Rotation;
            Scale = copy.Scale;
            Effects = copy.Effects;
            Clips = copy.Clips;
            FrameIndex = 0;
            Finished = false;
            NumFrames = copy.NumFrames;
            Name = copy.Name;
            timeElapsed = 0;
            Looping = copy.Looping;
            timePerFrame = copy.timePerFrame;
            Finish = copy.Finish;
            FrameSize = copy.FrameSize;
            LoopingFor = copy.LoopingFor;
            BoundingBoxes = copy.BoundingBoxes;
        }

        public void Draw(SpriteBatch batch, Vector2 position)
        {
            Origin = new Vector2((Texture.Width / NumFrames) / 2, Texture.Height / 2);
            batch.Draw(Texture, position, Clips[FrameIndex], Color,
                Rotation, Origin, Scale, Effects, 0f);
        }

        public void Draw(SpriteBatch batch, Vector2 position, SpriteEffects effects)
        {
            Origin = new Vector2((Texture.Width / NumFrames) / 2, Texture.Height / 2);
            batch.Draw(Texture, position, Clips[FrameIndex], Color,
                Rotation, Origin, Scale, effects, 0f);
        }

        public void Update(GameTime time)
        {
            timeElapsed += (float)time.ElapsedGameTime.TotalSeconds;

            if (timeElapsed >= timePerFrame)
            {
                if (FrameIndex < Clips.Count - 1)
                    FrameIndex++;
                else if (Looping || LoopIteration < LoopingFor)
                {
                    FrameIndex = 0;
                    LoopIteration++;
                }
                else
                {
                    Finished = true;
                    OnFinish(new AnimationFinishEventArgs { Time = time });
                }

                timeElapsed = 0;
            }
        }

        public event OnFinishEventHandler Finish;
        private void OnFinish(AnimationFinishEventArgs e)
        {
            if (Finish != null)
                Finish(this, e);
        }
    }

    public class AnimationFinishEventArgs : EventArgs
    {
        public GameTime Time { set; get; }
    }
}
