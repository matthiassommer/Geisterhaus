using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Geisterhaus
{
    public class Ghost
    {
        public bool IsActive;

        // indicates if a ghost is selectable
        public bool isSelectable;
        public bool wasSelectable = false;

        // Punkte bei erfüllter Geste
        public const int GEST_SPIRALE = 0;
        public const int GEST_KREIS = 1;
        public const int GEST_DREIECK = 2;
        public const int GEST_VIERECK = 3;
        public const int FALSE_GEST = 4;
        public int gest; // 0=Spirale ; 1= Kreis; 2= Dreieck; 3 = Viereck;

        public Vector2 target { get; set; } // Ziel auf das er sich zubewegt
        public int moving = 1; // 1== geistert in seinem quadranten; 2= muss sich ausserhalb der Anzeige befinden; 3 = geistert in der Mitte;
        public int quadrant; // 1= linksoben,2=rechtsoben,3=linksunten,4=rechtsunten
        private Geisterhaus engine;
        Random randomizer = new Random();
        private GhostPath gp;
        public Boolean selected = false;
        //private bool wasSelected = false;

        int posX, posY, tX, tY;

        public Ghost(GameScene Game_, Geisterhaus engine,  bool isMenueGhost)
        {
            this.engine = engine;
            gp = new GhostPath(this, engine, isMenueGhost);
        }
        
        public void init(int sw, int sh)
        {
            IsActive = true;

            if (quadrant == 1) { posX = -30; posY = -30; }
            if (quadrant == 2) { posX = sw + 30; posY = -30; }
            if (quadrant == 3) { posX = -30; posY = sh + 30; }
            if (quadrant == 4) { posX = sw + 30; posY = sh + 30; }

            tX = posX;
            tY = posY;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (gp.isMenueGhost)
            {
                spriteBatch.Draw(engine.ghostTexture, gp.getCurrentPosition(engine.slowDown), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
            }
            // normal (not selectable ghost)
            else if (!isSelectable && !wasSelectable)
            {
                switch (this.gest)
                {
                    case GEST_SPIRALE: spriteBatch.Draw(engine.ghostTextureSpirale, gp.getCurrentPosition(engine.slowDown), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;
                    case GEST_VIERECK: spriteBatch.Draw(engine.ghostTextureViereck, gp.getCurrentPosition(engine.slowDown), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;
                    case GEST_KREIS: spriteBatch.Draw(engine.ghostTextureKreis, gp.getCurrentPosition(engine.slowDown), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;
                    case GEST_DREIECK: spriteBatch.Draw(engine.ghostTextureDreieck, gp.getCurrentPosition(engine.slowDown), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;
                }

            }
            // selectable ghost
            else// if (!wasSelectable)
            {
                ///TODO change scale function to selectable
                switch (this.gest)
                {
                    case GEST_SPIRALE: spriteBatch.Draw(engine.ghostTextureSpirale, gp.animateToScreenCenter(), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;
                    case GEST_VIERECK: spriteBatch.Draw(engine.ghostTextureViereck, gp.animateToScreenCenter(), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;
                    case GEST_KREIS: spriteBatch.Draw(engine.ghostTextureKreis, gp.animateToScreenCenter(), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;
                    case GEST_DREIECK: spriteBatch.Draw(engine.ghostTextureDreieck, gp.animateToScreenCenter(), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;
                    /*case FALSE_GEST: spriteBatch.Draw(engine.ghost_red, gp.animateToScreenCenter(), null, Color.White, 0, new Vector2(0, 0), gp.getCurrentScale(), SpriteEffects.None, 1);
                        break;*/
                }

            }
        }
    }
}