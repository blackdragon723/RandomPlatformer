using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using C3.XNA;

namespace Platformer
{
    class HUD
    {
        private ContentManager contentManager;
        private Texture2D livesTexture;
        private Texture2D coinTexture;
        private SpriteFont font;
        private Player playerRef;
        private float mapWidth;
        public HUD(ContentManager content, Player player)
        {
            contentManager = content;
            playerRef = player;
        }

        public void Initialize()
        {
            livesTexture = contentManager.Load<Texture2D>("idle");
            coinTexture = contentManager.Load<Texture2D>("coin_gold");
            font = contentManager.Load<SpriteFont>("hud_font");
        }

        public void SetMapWidth(float width)
        {
            mapWidth = width;
        }

        public void Update(float dt)
        {
        }

        public void Draw(SpriteBatch b)
        {
            // Get starting position
            float startingPos = 1280 - ((25 + 5) * playerRef.Lives);

            for (int i = 0; i < playerRef.Lives; i++)
            {
                float position = startingPos + ((25 + 25) * i);
                b.Draw(livesTexture, new Vector2(position, 15), null, Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            }

            b.Draw(coinTexture, new Vector2(1180, 90), null, Color.White, 0f, Vector2.Zero, 0.85f, SpriteEffects.None, 0f);
            b.DrawString(font, playerRef.CoinsCollected.ToString(), new Vector2(1240, 95), Color.Black);

            float mapProgression = (playerRef.Position.X / mapWidth);
            b.FillRectangle(new Vector2(540, 10), new Vector2(400, 20), Color.WhiteSmoke);
            b.FillRectangle(new Vector2(540 + (400 * mapProgression), 10), new Vector2(5, 20), Color.Black);
        }
    }
}
