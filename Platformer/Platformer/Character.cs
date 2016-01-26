using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Linq;

namespace Platformer
{
    public abstract class Character : IGameObject
    {
        protected readonly Dictionary<string, Animation> Animations;

        public Animation CurrentAnimation { get; protected set; }
        public Animation PreviousAnimation { get; protected set; }

        protected Vector2 position = Vector2.Zero;
        public Vector2 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public enum DirectionEnum
        {
            Left = -1,
            Right = 1
        };
        public DirectionEnum Direction = DirectionEnum.Right;

        public bool IsAlive { set; get; }
        public bool MarkedForDelete { set; get; }

        private Rectangle _rectangle = Rectangle.Empty;
        public virtual Rectangle AABB
        {
            get
            {
                return new Rectangle(
                ((int)Position.X - ((int)CurrentAnimation.FrameSize.X / 2)) + CurrentAnimation.BoundingBox.X,
                ((int)Position.Y - ((int)CurrentAnimation.FrameSize.Y / 2)) + CurrentAnimation.BoundingBox.Y,
                CurrentAnimation.BoundingBox.Width,
                CurrentAnimation.BoundingBox.Height);
            }
        }

        public Texture2D Texture
        {
            get
            {
                return CurrentAnimation.Texture;
            }
        }

        public bool CheckForCollision { set; get; }

        public Rectangle Rectangle
        {
            get { return _rectangle; }
            set { _rectangle = value; }
        }

        private readonly ContentManager _content;

        protected Character(ContentManager manager, Rectangle rectangle = new Rectangle())
        {
            _content = manager;
            Rectangle = rectangle;
            Animations = new Dictionary<string, Animation>();
            IsAlive = true;
            CheckForCollision = true;
        }

        public virtual void Load(string xmlPath)
        {
            var element = XElement.Load(xmlPath);
            var xanimations = element.Elements();

            foreach (var animation in xanimations)
            {
// ReSharper disable PossibleNullReferenceException
                string name = animation.Element("Name").Value;

                int fps = int.Parse(animation.Element("FramesPerSecond").Value);
                string fileName = animation.Element("FileName").Value;
                bool looping = bool.Parse(animation.Element("Looping").Value);

                int frames = 0;
                var rects = new List<Rectangle>();
                foreach (XElement frame in animation.Descendants("Clips"))
                {
                    int x = int.Parse(frame.Element("x").Value);
                    int y = int.Parse(frame.Element("y").Value);
                    int w = int.Parse(frame.Element("w").Value);
                    int h = int.Parse(frame.Element("h").Value);

                    var rect = new Rectangle(x, y, w, h);
                    rects.Add(rect);
                    frames++;
                }
// ReSharper restore PossibleNullReferenceException
                var anim = new Animation(_content.Load<Texture2D>(fileName), rects, name)
                {
                    FramesPerSecond = fps,
                    Looping = looping
                };
                anim.Finish += AnimationFinish;

                Animations.Add(name, anim);
            }
        }

        protected abstract void AnimationFinish(object sender, AnimationFinishEventArgs e);

        public virtual void Update(GameTime time)
        {
            CurrentAnimation.Update(time);
        }

        public virtual void Draw(SpriteBatch batch)
        {
            var flip = Direction == DirectionEnum.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            CurrentAnimation.Draw(batch, position, flip);
        }

        protected abstract void Kill();

        public virtual bool CheckCollision(ICollidable src)
        {
            return Collision.CheckCollision(src, this);
        }
    }
}
