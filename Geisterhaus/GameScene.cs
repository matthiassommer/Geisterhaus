using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Geisterhaus
{
    public class GameScene
    {
        protected Geisterhaus engine;
        protected float alpha = 0f;
        public bool Active { get; set; }

        public GameScene(Geisterhaus Engine_)
        {
            engine = Engine_;
        }

        public virtual void StartMusic()
        {
        }

        public void PlaySound(int Index_)
        {
            if (Index_ >= engine.GameSounds.Count) return;
            engine.GameSounds[Index_].Play(0.3f, 0.0f, 0.0f);
        }

        public virtual void Draw(SpriteBatch SpriteBatch_, GameTime gameTime)
        {
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void HandleInput(Keys key)
        {
        }
    }
}
