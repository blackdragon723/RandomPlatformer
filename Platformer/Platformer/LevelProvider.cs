namespace Platformer
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework;

    public class LevelProvider
    {
        private readonly List<Map> _maps = new List<Map>();
        private Map _currentMap;
        private readonly ContentManager _contentManager;
        private readonly Player _player;
        private readonly Camera2D _camera;

        public LevelProvider(ContentManager manager, Player p, Camera2D cam)
        {
            _contentManager = manager;
            _player = p;
            _camera = cam;
        }

        public void LoadMaps(GraphicsDevice gd, params string[] mapFiles)
        {
            foreach (string mapFile in mapFiles)
            {
                var m = new Map();
                m.Load(_contentManager, mapFile, gd);
                _maps.Add(m);
            }

            _currentMap = _maps.First();
        }

        public void Update(GameTime dt)
        {
            _currentMap.Update(dt, _player);

            if (_currentMap.Completed)
            {
                int index = _maps.FindIndex(x => x == _currentMap);
                _currentMap = (Map)_maps[++index].Clone();
                _player.Respawn(new Vector2(200, 200));
            }
        }

        public void Draw(SpriteBatch b)
        {
            _currentMap.DrawClouds(b);
            _currentMap.DrawTiles(b, _camera);
        }
    }
}
