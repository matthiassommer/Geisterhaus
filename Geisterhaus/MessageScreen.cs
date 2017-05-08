using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Geisterhaus
{
    class MessageScreen
    {
        GameplayScene game;
        public float Alpha { get; set; }
        public bool Active { get; set; }
        Texture2D tex;
        Vector2 position;
        Vector2 origin;

        // Die Klasse MessageScreen stellt die Mitteilungsfenster dar (Spieler hat verloren, etc.)
        public MessageScreen(GameplayScene Game_, Texture2D Texture_, Vector2 Position_)
        {
            game = Game_;
            tex = Texture_;
            position = Position_;
            origin = new Vector2(tex.Width / 2, tex.Height / 2);
            Active = true;
        }

        // Aktualisiert die Transparenz des Mitteilungsfensters
        public virtual void Update(GameTime GameTime_)
        {
            if (Active)
                Alpha = MathHelper.Lerp(Alpha, 1f, 0.005f * GameTime_.ElapsedGameTime.Milliseconds);
            else
            {
                Alpha = MathHelper.Lerp(Alpha, 0f, 0.005f * GameTime_.ElapsedGameTime.Milliseconds);
                if (Alpha < 0.01f)
                {
                    Alpha = 0;
                    Active = false;
                }
            }
        }

        // Zeichnet das Mitteilungsfenster
        public virtual void Draw(SpriteBatch SpriteBatch, GameTime gameTime)
        {
            if (Alpha > 0)
                SpriteBatch.Draw(tex, position, null, new Color(1f, 1f, 1f, Alpha), 0, origin, 1, SpriteEffects.None, 0f);
        }
    }
}
