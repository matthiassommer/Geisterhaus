using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Geisterhaus
{
    class PlayerMenue : GameScene
    {
        int MenuEntrySelected;
        float Color1;
        float Color3;
        float Color2;

        public PlayerMenue(Geisterhaus Game_)
            : base(Game_)
        {
            Initialize();
        }

        public void Initialize()
        {
            MenuEntrySelected = 2;
            Color1 = 1f;
            Color2 = 0.5f;
            Color3 = 0.5f;
        }

        /*
        public override void StartMusic()
        {
            MediaPlayer.Stop();
            MediaPlayer.Volume = 0.3f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(engine.MenuMusic);
        }
        */

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            engine.screenRectangle = new Rectangle(0, 0, engine.screenWidth, engine.screenHeight);

            // Menue
            spriteBatch.Draw(engine.spielerEinblenden,
                                new Vector2(engine.screenRectangle.Width / 2 - engine.spielerEinblenden.Width / 2, 50),
                                new Color(Color1, Color1, Color1, 1f));

            
            
            //TODO: wii und pda grafik im menü einbauen statt start
            //spriteBatch.Draw(engine.pdaTexture,
            //spriteBatch.Draw(engine.wiiTexture,
            spriteBatch.Draw(engine.spielerNichtEinblenden,
                                new Vector2(engine.screenRectangle.Width / 2 - engine.spielerNichtEinblenden.Width / 2, 250),
                                new Color(Color2, Color2, Color2, 1f));
        }

        // Aktualisiert Menü-Farben
        public override void Update(GameTime GameTime_)
        {
            switch (MenuEntrySelected)
            {
                case 1: Color1 = MathHelper.Lerp(Color1, 1f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    Color2 = MathHelper.Lerp(Color2, 0.5f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    Color3 = MathHelper.Lerp(Color3, 0.5f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    break;
                case 2: Color1 = MathHelper.Lerp(Color1, 0.5f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    Color2 = MathHelper.Lerp(Color2, 1f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    Color3 = MathHelper.Lerp(Color3, 0.5f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    break;
                case 3: Color1 = MathHelper.Lerp(Color1, 0.5f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    Color2 = MathHelper.Lerp(Color2, 0.5f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    Color3 = MathHelper.Lerp(Color3, 1f, 0.01f * GameTime_.ElapsedGameTime.Milliseconds);
                    break;
            }
        }

        // Hier wird der Tastaturinput im Menu gehandled.
        // Key_ übergibt der Methode die neu gedrückte Taste
        public override void HandleInput(Keys Key_)
        {
            if (Key_ == Keys.Up)
            {
                MenuEntrySelected--;
                if (MenuEntrySelected < 1)
                    MenuEntrySelected = 2;
                PlaySound(0);
            }
            if (Key_ == Keys.Down)
            {
                MenuEntrySelected++;
                if (MenuEntrySelected > 2)
                    MenuEntrySelected = 1;
                PlaySound(0);
            }

            if (Key_ == Keys.Enter)
            {

                DLLTools.start();
                switch (MenuEntrySelected)
                {
                    case 1:
                        //DLLTools.start();
                        //DLLTools.
                        engine.StartNewGame();
                        break;
                    case 2:
                        engine.Highscore();
                        break;
                }
            }

            if (Key_ == Keys.Escape)
                this.engine.Exit();
        }
    }
}
