using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace Platformer
{
    public class Cloud
    {
        public Cloud(Texture2D tex, Vector2 pos)
        {
            Texture = tex;
            Position = pos;
        }

        public Texture2D Texture;
        public Vector2 Position;

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0);
        }
    }

    public class Map : ICloneable
    {
        private readonly List<Tile> Tiles = new List<Tile>();
        public readonly List<Cloud> Clouds = new List<Cloud>();
        private Dictionary<int, Rectangle> TileClips = new Dictionary<int, Rectangle>();

        private Texture2D TileSetImage;

        public bool Completed = false;

        private GraphicsDevice Device;
        private RenderTarget2D BackBuffer;
        private SpriteBatch SpriteBatch;
        
        public Vector2 Dimensions { private set; get; }
        private Vector2 ViewportDimensions;
        //list of enemies etc etc

        public Map()
        {

        }

        public void Load(ContentManager manager, string mapFile, GraphicsDevice device) //string enemies etc
        {
            ViewportDimensions = new Vector2(device.Viewport.Width, device.Viewport.Height);

            Device = device;
            BackBuffer = new RenderTarget2D(Device, Device.Viewport.Width, Device.Viewport.Height, true, Device.DisplayMode.Format, DepthFormat.Depth24);
            SpriteBatch = new SpriteBatch(Device);

            XDocument doc = XDocument.Load(mapFile);
            var map = doc.Root;
            var width = int.Parse(map.Attribute("width").Value);
            var height = int.Parse(map.Attribute("height").Value);
            Dimensions = new Vector2(width * 70, height * 70);

            TileSetImage = manager.Load<Texture2D>("tiles_spritesheet");
            int[][] imageDimensions = new int[TileSetImage.Width / 70][];
            for (int i = 0; i < imageDimensions.Length; i++)
            {
                imageDimensions[i] = new int[TileSetImage.Height / 70];
            }

            var tileClips = new Dictionary<int, Rectangle>();
            int tileId = 1;

            for (int j = 0; j < imageDimensions[0].Length; j++)
            {
                for (int i = 0; i < imageDimensions.Length; i++)
                {
                    var rect = new Rectangle(i * (70 + 1), j * (70 + 1), 70, 70);
                    TileClips.Add(tileId, rect);
                    tileId++;
                }
            }

            var layers = map.Elements("layer");

            int k = 1;
            foreach (var layer in layers)
            {
                LoadMapLayer(k, layer);
                k++;
            }

            OptimizeCollisions();
            //GenerateClouds(manager, 5);
            FillScreen(device);
        }

        delegate Tile CreateTileBasedOnLayer(int x, int y, int id);
        private void LoadMapLayer(int layerNumer, XElement xml)
        {
            CreateTileBasedOnLayer CreateTile;
            switch (layerNumer)
            {
                case 1:
                    CreateTile = (xPos, yPos, id) =>
                        {
                            return new Tile(new Vector2(xPos * 70, yPos * 70), TileSetImage, TileClips[id], id, layerNumer);
                        };
                    break;

                case 2:
                    CreateTile = (xPos, yPos, id) =>
                        {
                            var tile = new Tile(new Vector2(xPos * 70, yPos * 70), TileSetImage, TileClips[id], id, layerNumer);
                            tile.CheckForCollision = false;
                            return tile;
                        };
                    break;

                case 3:
                    CreateTile = (xPos, yPos, id) =>
                        {
                            var tile = new Spike(new Vector2(xPos * 70, yPos * 70), TileSetImage, TileClips[id], id, layerNumer);
                            return tile;
                        };
                    break;

                case 4:
                    CreateTile = (xPos, yPos, id) =>
                        {
                            var tile = new Coin(new Vector2(xPos * 70, yPos * 70), TileSetImage, TileClips[id], id, layerNumer);
                            return tile;
                        };
                    break;

                default:
                    CreateTile = (xpos, yPos, id) => null;
                    MessageBox.Show("Layer Delegate Doesn't exist");
                    Environment.Exit(1);
                    break;
            }

            int x = 0, y = 0;
            foreach (var position in xml.Element("data").Elements("tile"))
            {
                int id = int.Parse(position.Attribute("gid").Value);
                if (id != 0)
                {
                    var tile = CreateTile(x, y, id);
                    Tiles.Add(tile);
                }

                if (++x == (int)Dimensions.X / 70)
                {
                    x = 0;
                    ++y;
                }
            }
        }

        private void OptimizeCollisions(int groundLevel = 1, int mapHeight = 10)
        {
            int threshold = (mapHeight - groundLevel) * 70;

            var noCollision = from t in Tiles
                              where t.Position.Y > threshold
                              select t;

            foreach (var t in noCollision)
            {
                t.CheckForCollision = false;
            }
        }

        private void FillScreen(GraphicsDevice device)
        {
            for (int i = 0; i < Dimensions.X; i += 70)
            {
                Tile t = new Tile(new Vector2(i, Dimensions.Y), TileSetImage, TileClips[12], 12, 1);
                t.CheckForCollision = false;
                Tiles.Add(t);
            }
        }

        private delegate bool BoolDelegate(int x, int y);
        private void GenerateClouds(ContentManager manager, int numClouds)
        {
            Random random = new Random();
            Texture2D[] CloudTextures = { manager.Load<Texture2D>("cloud_1"), manager.Load<Texture2D>("cloud_2"), manager.Load<Texture2D>("cloud_3") };

            BoolDelegate CheckExisting = (int x, int y) =>
                {
                    Vector2 pos = new Vector2(x, y);
                    foreach (Cloud c in Clouds)
                    {
                        if (Vector2.Distance(pos, c.Position) < 500)
                            return true;
                    }

                    return false;
                };

            Vector2 minPos = new Vector2(-150, 100);
            Vector2 maxPos = new Vector2(Dimensions.X + 150, 150);

            int genX = random.Next((int)minPos.X, (int)maxPos.X);
            int genY = random.Next((int)minPos.Y, (int)maxPos.Y);

            for (int i = 0; i < numClouds; i++)
            {
                while (CheckExisting(genX, genY))
                {
                    genX = random.Next((int)minPos.X, (int)maxPos.X);
                    genY = random.Next((int)minPos.Y, (int)maxPos.Y);
                }

                Clouds.Add(new Cloud(CloudTextures[random.Next(0, 2)], new Vector2(genX, genY)));
            }
        }

        public void Update(GameTime time, Player player)
        {
            if (player.AABB.X < 0)
            {
                player.Position =  new Vector2(player.AABB.Width / 2, player.Position.Y);
            }
            else if (player.AABB.Right > Dimensions.X)
            {
                player.Position = new Vector2(Dimensions.X - player.AABB.Width / 2, player.Position.Y);
            }

            Collision(player);

            /*
            var portal = Tiles.Find(x => x is ExitPortal) as ExitPortal;
            if (portal.Triggered)
            {
                Completed = true;
            }
            */
            Tiles.RemoveAll(x => x.FlagForRemove == true);
        }

        public void Collision(Player player)
        {
            var near = from t in Tiles
                       where Vector2.Distance(player.Position, t.Position) < 150 && t.CheckForCollision
                       select t;

            foreach (Tile t in near)
            {
                t.CheckCollision(player);
            }
        }

        public void DrawTiles(SpriteBatch batch, Camera2D camera)
        {
            Device.SetRenderTarget(BackBuffer);
            Device.Clear(new Color(208, 244, 247));
            SpriteBatch.Begin();

            Vector2 LastTilePositon = Vector2.Zero;

            var tiles = GetViewableTiles(camera);

            for (int i = 1; i < 6; i++)
            {
                var layer = from t in tiles
                            where t.LayerNumber == i
                            select t;

                foreach (Tile t in layer)
                {
                    var position = camera.ConvertWorldToScreen(t.Position);
                    t.Draw(SpriteBatch, position);
                }
            }
        
            SpriteBatch.End();
            Device.SetRenderTarget(null);

            batch.Draw(BackBuffer, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }

        public void DrawClouds(SpriteBatch batch)
        {
            foreach (Cloud c in Clouds)
            {
                c.Draw(batch);
            }
        }

        private List<Tile> GetViewableTiles(Camera2D camera)
        {
            Rectangle viewableArea = new Rectangle((int)camera.Position.X - (int)(ViewportDimensions.X / 2),
                                                (int)camera.Position.Y - (int)(ViewportDimensions.Y / 2),
                                                (int)ViewportDimensions.X, (int)ViewportDimensions.Y);
            viewableArea.Inflate(200, 100);

            var viewableTiles = from tile in Tiles
                                where tile.AABB.Intersects(viewableArea)
                                select tile;

            return viewableTiles.ToList();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
