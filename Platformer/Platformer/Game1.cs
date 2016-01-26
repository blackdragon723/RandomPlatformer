using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Platformer
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        SpriteBatch _spriteBatch;

        LevelProvider _levelProvider;

        private SpriteFont _font;

        Player _player;

        private Camera2D _camera;
        private HUD _hud;

        public Game1()
        {
            var graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1440,
                PreferredBackBufferHeight = 900
            };
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _camera = new Camera2D(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _player = new Player(Content, new Vector2(400, 430));
            _player.Load("Animations.xml");

            _font = Content.Load<SpriteFont>("font");

            Debug.Assert(GraphicsDevice != null, "GraphicsDevice != null");
// ReSharper disable once PossibleLossOfFraction
            _camera.MinPosition = new Vector2(x: GraphicsDevice.Viewport.Width / 2, y: 0);
            //Camera.MaxPosition = new Vector2(map.Dimensions.X - GraphicsDevice.Viewport.Width / 2, map.Dimensions.Y / 2 - 35);
            _camera.MaxPosition = new Vector2(5000, 200);

            _camera.TrackingBody = _player;

            _levelProvider = new LevelProvider(Content, _player, _camera);
            _levelProvider.LoadMaps(GraphicsDevice, "hills.tmx");

            _hud = new HUD(Content, _player);
            _hud.Initialize();
            _hud.SetMapWidth(1000);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // Skip first loop due to divide by zero
            if (Math.Abs(gameTime.ElapsedGameTime.TotalSeconds) < (1f / 60f))
                return;

            _player.Update(gameTime);
            _levelProvider.Update(gameTime);

            if (!_player.IsAlive)
            {
                _camera.TrackingBody = null;
                if (_player.Position.Y > 800)
                {
                    _player.Respawn(new Vector2(400, 400));
                    _camera.TrackingBody = _player;
                }
            }

            _camera.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(208, 244, 247));

            _spriteBatch.Begin();
            _levelProvider.Draw(_spriteBatch);
            _spriteBatch.End();

            /*
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Camera.View);
            map.DrawClouds(spriteBatch);
            spriteBatch.End();
            */

            _player.Draw(_spriteBatch, _camera);

            _spriteBatch.Begin();
#if DEBUG
            _spriteBatch.DrawString(_font, string.Format("Position X: {0}     Y: {1}", _player.Position.X, _player.Position.Y), new Vector2(10, 10), Color.Black);
            _spriteBatch.DrawString(_font, string.Format("Velocity: {0}, {1}", (int)_player.Velocity.X, (int)_player.Velocity.Y), new Vector2(10, 30), Color.Black);
#endif
            _hud.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
