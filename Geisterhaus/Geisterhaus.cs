using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Geisterhaus
{
    public class Geisterhaus : Microsoft.Xna.Framework.Game
    {
        public Song MenuMusic;
        public Song Wind;
        public List<SoundEffect> GameSounds = new List<SoundEffect>();

        byte fadeDirection = 2;
        float fadeAlpha;
        int destinationGameScene;

        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public GraphicsDevice device;

        //Bluetooth PC<->PDA
        public BTC btc;

        // Gibt an, in welchem Zustand sich das Spiel befindet (Menu oder Spiel)
        public int currentGameScene;
        public GameplayScene gameScene;
        Menue menueScreen;
        public Highscore highscore;

        KeyboardState keybState = new KeyboardState();
        KeyboardState lastKBState = new KeyboardState();

        public int screenWidth;
        public int screenHeight;
        public Rectangle screenRectangle = new Rectangle(0, 0, 0, 0);
        public int sw = 0;
        public int sh = 0;
        public Vector2 baseScreenSize;

#if DEBUG
        bool fullscreen = false;
#else
        bool fullscreen = true;
#endif

        public SpriteFont font;

        public Texture2D foregroundTexture;
        public Texture2D backgroundTexture;

        public Texture2D ghostTexture;
        public Texture2D ghost_red;
        public Texture2D ghostTextureDreieck;
        public Texture2D ghostTextureViereck;
        public Texture2D ghostTextureKreis;
        public Texture2D ghostTextureSpirale;

        public Texture2D moonTexture;
        public Texture2D treeTexture;
        public Texture2D graveTexture;

        public Texture2D castleTexture;
        public Texture2D castle1;
        public Texture2D castle2;
        public Texture2D castle3;
        public Texture2D castle4;
        public Texture2D castle5;
        public Texture2D castle6;
        public Texture2D castle7;
        public Texture2D castle8;
        
        public Texture2D startTexture;
        public Texture2D winTexture;
        //public Texture2D pdaTexture;
        //public Texture2D wiiTexture;
        public Texture2D quitTexture;
        public Texture2D highscoreTexture;
        public Texture2D gameoverTexture;
        public Texture2D blank;
        public Texture2D weiter;

        public Texture2D dummy;
        public Texture2D onePixel;
        public Texture2D coin;
        public Texture2D coinGray;

        public Texture2D spielerEinblenden;
        public Texture2D spielerNichtEinblenden;

        Random randomizer = new Random();

        public int slowDown = 4;

        public float gamePlayerAreaPerentage = 1.0f / 4.0f;
        public bool keepOriginalHigh = false;
        public Rectangle gamePlayerArea;

        public Geisterhaus()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = fullscreen;
            if (fullscreen)
            {
                baseScreenSize.X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                baseScreenSize.Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                baseScreenSize.X = 1024;
                baseScreenSize.Y = 768;
                graphics.PreferredBackBufferWidth = 1024;
                graphics.PreferredBackBufferHeight = 768;
            }
            Content.RootDirectory = "Content";
            Window.Title = "Geisterhaus";
            gameScene = new GameplayScene(this);
            
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            setScene(0);        
            base.Initialize();

            BTConnect.Release();

            btc = new BTC(this);

            btc.connect();
        }

        // LoadContent will be called once per game and is the place to load
        // all of your content.
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;

            screenWidth = (int)baseScreenSize.X;
            screenHeight = (int)baseScreenSize.Y;

            backgroundTexture = Content.Load<Texture2D>(@"Textures\background");
            ghostTexture = Content.Load<Texture2D>(@"Textures\Geist");
            ghost_red = Content.Load<Texture2D>(@"Textures\Geist_red");
            ghostTextureDreieck = Content.Load<Texture2D>(@"Textures\geist_dreieck");
            ghostTextureViereck = Content.Load<Texture2D>(@"Textures\geist_viereck");
            ghostTextureKreis = Content.Load<Texture2D>(@"Textures\geist_kreis");
            ghostTextureSpirale = Content.Load<Texture2D>(@"Textures\geist_spirale");
            weiter = Content.Load<Texture2D>(@"Textures\weiter");

            castleTexture = Content.Load<Texture2D>(@"Textures\huette0");
            castle1 = Content.Load<Texture2D>(@"Textures\huette1");
            castle2 = Content.Load<Texture2D>(@"Textures\huette2");
            castle3 = Content.Load<Texture2D>(@"Textures\huette3");
            castle4 = Content.Load<Texture2D>(@"Textures\huette4");
            castle5 = Content.Load<Texture2D>(@"Textures\huette5");
            castle6 = Content.Load<Texture2D>(@"Textures\huette6");
            castle7 = Content.Load<Texture2D>(@"Textures\huette7");
            castle8 = Content.Load<Texture2D>(@"Textures\huette8");

            coin = Content.Load<Texture2D>(@"Textures\coin");
            coinGray = Content.Load<Texture2D>(@"Textures\coin_gray");

            startTexture = Content.Load<Texture2D>(@"Textures\start");
            //pdaTexture = Content.Load<Texture2D>(@"Textures\pda");
            //wiiTexture = Content.Load<Texture2D>(@"Textures\wii");
            quitTexture = Content.Load<Texture2D>(@"Textures\beenden");
            highscoreTexture = Content.Load<Texture2D>(@"Textures\highscore");
            gameoverTexture = Content.Load<Texture2D>(@"Textures\gameover");
            winTexture = Content.Load<Texture2D>(@"Textures\gewonnen");

            dummy = Content.Load<Texture2D>(@"Textures\dummy");
            onePixel = Content.Load<Texture2D>(@"Textures\onePixel");
            blank = Content.Load<Texture2D>(@"Textures\blank");

            font = Content.Load<SpriteFont>("myFont");

            MenuMusic = Content.Load<Song>(@"Sounds\Wood");
            Wind = Content.Load<Song>(@"Sounds\wind");
            GameSounds.Add(Content.Load<SoundEffect>(@"Sounds\WOODCUT"));
            GameSounds.Add(Content.Load<SoundEffect>(@"Sounds\lightning_strike_01"));
            GameSounds.Add(Content.Load<SoundEffect>(@"Sounds\laugh"));
            GameSounds.Add(Content.Load<SoundEffect>(@"Sounds\ghost_die"));
            
            menueScreen = new Menue(this);
            //gameScene = new GameplayScene(this);
            highscore = new Highscore(this);
        }

        protected override void UnloadContent()
        {
        }

        // checking for collisions, gathering input, and playing audio.
        protected override void Update(GameTime gameTime)
        {
            ProcessKeyboard();
            switch (currentGameScene)
            {
                case 0: menueScreen.Update(gameTime);
                    break;
                case 1: gameScene.Update(gameTime);
                    break;
                case 2: highscore.Update(gameTime);
                    break;
            }

            switch (fadeDirection)
            {
                case 1:
                    fadeAlpha = MathHelper.Lerp(fadeAlpha, 1f, 0.003f * gameTime.ElapsedGameTime.Milliseconds);
                    if (fadeAlpha > 0.95f)
                    {
                        fadeAlpha = 1f;
                        currentGameScene = destinationGameScene;
                        fadeDirection = 2;
                        MediaPlayer.Stop();
                    }
                    MediaPlayer.Volume = 1f - fadeAlpha;
                    break;
                case 2:
                    fadeAlpha = MathHelper.Lerp(fadeAlpha, 0f, 0.003f * gameTime.ElapsedGameTime.Milliseconds);
                    if (fadeAlpha < 0.05f)
                    {
                        fadeAlpha = 0f;
                        fadeDirection = 0;
                        if (currentGameScene == 0)
                            menueScreen.StartMusic();
                        if (currentGameScene == 1)
                        {
                            castleTexture = Content.Load<Texture2D>(@"Textures\huette0");
                            gameScene.StartMusic();
                        }
                    }
                    MediaPlayer.Volume = 0.3f - fadeAlpha;
                    break;
            }

            base.Update(gameTime);
        }

        public void setScene(int i)
        {
            if (i == destinationGameScene) return;
            if (fadeAlpha != 0) return;
            destinationGameScene = i;
            fadeDirection = 1;
        }

        public void Highscore()
        {
            setScene(2);
            highscore.Initialize();
        }

        public void StartNewGame()
        {
            setScene(1);
            menueScreen.Initialize();
            gameScene.Initialize();
        }

        public void QuitGame()
        {
            Exit();
        }

        // This is called when the game should draw itself.
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            Vector3 screenScalingFactor;

            float horScaling = (float)device.PresentationParameters.BackBufferWidth / baseScreenSize.X;
            float verScaling = (float)device.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
            screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            Matrix globalTransformation = Matrix.CreateScale(screenScalingFactor);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, globalTransformation);
            switch (currentGameScene)
            {
                case 0: menueScreen.Draw(spriteBatch, gameTime);
                    break;
                case 1: gameScene.Draw(spriteBatch, gameTime);
                    break;
                case 2: highscore.Draw(spriteBatch, gameTime);
                    break;
            }
            if (fadeAlpha > 0)
                spriteBatch.Draw(blank, screenRectangle, new Color(0f, 0f, 0f, fadeAlpha));
            spriteBatch.End();

            base.Draw(gameTime);
        }

        // Prüft, ob die aktuell gedrückte Taste im letzten Frame auch schon gedrückt war
        // key beinhaltet die aktuell gedrückte Taste
        bool IsNewInput(Keys Key_)
        {
            return (keybState.IsKeyDown(Key_) && lastKBState.IsKeyUp(Key_));
        }

        private void ProcessKeyboard()
        {
            if (fadeDirection != 0) return;
            lastKBState = keybState;
            keybState = Keyboard.GetState(0);
            switch (currentGameScene)
            {
                case 0:
                    if (IsNewInput(Keys.Up))
                        menueScreen.HandleInput(Keys.Up);
                    if (IsNewInput(Keys.Down))
                        menueScreen.HandleInput(Keys.Down);
                    if (IsNewInput(Keys.Enter))
                        menueScreen.HandleInput(Keys.Enter);
                    if (IsNewInput(Keys.Escape))
                        menueScreen.HandleInput(Keys.Escape);
                    break;
                case 1:
                    if (IsNewInput(Keys.Enter))
                        gameScene.HandleInput(Keys.Enter);
                    if (IsNewInput(Keys.Back))
                        gameScene.HandleInput(Keys.Back);
                    if (IsNewInput(Keys.Escape))
                        gameScene.HandleInput(Keys.Escape);
                    if (IsNewInput(Keys.I))
                        slowDown++;
                    if (IsNewInput(Keys.O))
                        if (slowDown > 0) slowDown--;
                    if (IsNewInput(Keys.G))
                        gameScene.HandleInput(Keys.G);
                    break;
                case 2:
                    if (IsNewInput(Keys.Enter))
                        highscore.HandleInput(Keys.Enter);
                    if (IsNewInput(Keys.Escape))
                        highscore.HandleInput(Keys.Escape);
                    break;
            }
        }
    }
}
