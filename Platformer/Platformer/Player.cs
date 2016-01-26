using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Platformer
{
    public sealed class Player : Character
    {
        public enum PlayerState
        {
            Idle,
            Walk,
            Jump,
            Dead
        };
        private readonly Stack<PlayerState> _states = new Stack<PlayerState>();

        public Vector2 Velocity = Vector2.Zero;
        private Vector2 _acceleration = Vector2.Zero;
        private Vector2 _deltaMovement = Vector2.Zero;
        private readonly Vector2 _gravity = new Vector2(0f, -26);
        private readonly Vector2 _friction = new Vector2(13.39f, 0);
        private Vector2 _force = Vector2.Zero;
        private const float Mass = 25f;

        private float _remainingJumpIterations;
        public bool Jumping
        {
            get { return _states.First() == PlayerState.Jump; }
        }
        private bool CanDoubleJump { set; get; }
        private readonly Stopwatch _doubleJumpTimer = new Stopwatch();

        private int _hp = 2;
        public int Lives = 3;
        public bool RecentlyDamaged = false;
        private readonly Stopwatch _recentlyDamagedTimer = new Stopwatch();

        public int CoinsCollected = 0;

        private readonly Effect _flashWhite;
        private readonly Stopwatch _flashTimer = new Stopwatch();

        public bool OnGround { set; get; }

        public Player(ContentManager manager, Vector2 position)
            : base(manager)
        {
            Position = position;
            _flashWhite = manager.Load<Effect>("tintColor");
        }

        public override void Load(string xmlPath)
        {
            base.Load(xmlPath);

            PushState(PlayerState.Idle);
        }

        public void HandleKeyboard(KeyboardState state)
        {
            if (!IsAlive)
                return;

            Keys[] pressed = state.GetPressedKeys();

            foreach (Keys k in pressed)
            {
                switch (k)
                {
                    case Keys.Space:
                        Jump();
                        break;

                    case Keys.D:
                    case Keys.Right:
                        if (_states.First() != PlayerState.Jump)
                        {
                            PushState(PlayerState.Walk);
                            Direction = DirectionEnum.Right;
                            _force.X = 2000;
                        }
                        else
                        {
                            Direction = DirectionEnum.Right;
                            _force.X += 50;
                        }
                        break;

                    case Keys.A:
                    case Keys.Left:
                        if (_states.First() != PlayerState.Jump)
                        {
                            PushState(PlayerState.Walk);
                            Direction = DirectionEnum.Left;
                            _force.X = -2000;
                        }
                        else
                        {
                            _force.X -= 50;
                            Direction = DirectionEnum.Left;
                        }
                        break;
                }
            }

            if (_states.First() == PlayerState.Walk && (state.IsKeyUp(Keys.A) && state.IsKeyUp(Keys.D)))
            {
                PopState();
                _force.X = 0;
            }
        }

        public override void Update(GameTime time)
        {
            KeyboardState state = Keyboard.GetState();
            var dt = (float)time.ElapsedGameTime.TotalSeconds;

            HandleKeyboard(state);
            CurrentAnimation.Update(time);

            if (_remainingJumpIterations > 0)
            {
                _force.Y -= 1000;
                _remainingJumpIterations--;
            }
            else
            {
                _force.Y = 0;
            }

            _acceleration = (_force - (Mass * (_gravity))) / Mass;

            if (OnGround && !Jumping)
                _acceleration.Y = 0;

            Velocity += _acceleration - (_friction * Velocity) * dt;

            Velocity = Vector2.Clamp(Velocity, new Vector2(-400f, -500f), new Vector2(400f, 500f));

            _deltaMovement = Velocity * dt + (_acceleration * ((float)Math.Pow(dt, 2)) / 2); // x = x0 + v0 * t + 1/2(a * t^2)

            Position += _deltaMovement;
            OnGround = false;

            if (!IsAlive)
            {
                position.Y += 150 * dt;
                _force.X = 0;
            }

            if (_recentlyDamagedTimer.ElapsedMilliseconds > 2000)
            {
                RecentlyDamaged = false;
            }

            base.Update(time);
        }

        public override bool CheckCollision(ICollidable src)
        {
            bool success = base.CheckCollision(src);
            if (success)
            {
                var obj = src as IReactsWithPlayer;
                if (obj != null) obj.OnPlayerCollision(this);
            }

            return success;
        }

        private void StateChanged(PlayerState newState)
        {
            if (PreviousAnimation != null)
                PreviousAnimation = CurrentAnimation;
                
            string key = newState.ToString();
            CurrentAnimation = new Animation(Animations[key]);
        }

        private void PopState()
        {
            if (_states.Count > 1)
            {
                _states.Pop();
                StateChanged(_states.First());
            }
        }

        private void PushState(PlayerState state)
        {
            if (!_states.Contains(state))
            {
                _states.Push(state);
                StateChanged(state);
            }
        }

        public void Jump()
        {
            if (!Jumping)
            {
                PushState(PlayerState.Jump);
                _remainingJumpIterations = 5;
                _doubleJumpTimer.Start();
            }
            else if (CanDoubleJump && _doubleJumpTimer.ElapsedMilliseconds > 350)
            {
                _remainingJumpIterations += 5;
                CanDoubleJump = false;
                _doubleJumpTimer.Reset();
            }
        }

        public void CollidedWithSurface(bool x)
        {
            if (x)
            {
                _force.X = 0;
            }
            else
            {
                Velocity.Y = 0;
                _force.Y = 0;

                _remainingJumpIterations = 0;

                if (_states.First() == PlayerState.Jump)
                {
                    PopState();
                }

                OnGround = true;

                CanDoubleJump = true;
                _doubleJumpTimer.Reset();
            }
        }

        protected override void Kill()
        {
            _states.Clear();
            PushState(PlayerState.Dead);
            Lives--;
            IsAlive = false;
            CheckForCollision = false;
            RecentlyDamaged = false;
            _recentlyDamagedTimer.Reset();
            _flashTimer.Reset();
        }

        public void Damage()
        {
            if (!RecentlyDamaged)
            {
                if (--_hp == 0)
                {
                    Kill();
                    _hp = 3;
                    return;
                }
                RecentlyDamaged = true;
                _recentlyDamagedTimer.Restart();
                _flashTimer.Restart();
            }
        }

        public void Respawn(Vector2 pos)
        {
            IsAlive = true;
            Lives = 3;
            Velocity = Vector2.Zero;
            CoinsCollected = 0;
            _states.Clear();
            PushState(PlayerState.Idle);
            Position = pos;
            CheckForCollision = true;
        }

        protected override void AnimationFinish(object sender, AnimationFinishEventArgs e)
        {
            PopState();
        }

        public void Draw(SpriteBatch batch, Camera2D camera)
        {
            if (RecentlyDamaged && IsAlive)
                if (_flashTimer.ElapsedMilliseconds > 100)
                {
                    batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _flashWhite, camera.View);
                    _flashTimer.Restart();
                }
                else
                    batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.View);
            else
                batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.View);

            Draw(batch);

            batch.End();
        }
    }
}
