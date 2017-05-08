using System;
using System.Threading;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
namespace Geisterhaus
{
    class WiiListener : CallasOsc.Listener
    {
        GameplayScene target;
        public WiiListener(GameplayScene t)
        {
            target = t;
        }

        public override void recv_event(String id, float time, float dur, int num /* # der Klassen */, String[] event_names /* kreis,dreieck,etc.*/, float[] event_values /* wahrscheinlichkeiten der events*/)
        {
            String tmp = event_names[0];
            if (tmp.Equals("spirale")) { target.handleWiiInput(0); } // Spirale  
            if (tmp.Equals("kreis")) { target.handleWiiInput(1); } // kreis
            if (tmp.Equals("dreieck")) { target.handleWiiInput(2); } // dreieck
            if (tmp.Equals("viereck")) { target.handleWiiInput(3); } // viereck                   
        }
    }

    public class GameplayScene : GameScene
    {
        public bool GameOver { get; set; }
        MessageScreen messageScreen;

        GameTime gameTime;
        string PlayedTimeString;
        private System.Timers.Timer timer;
        int remainTime;
        TimeSpan startTimeToDestroy;

        private const int timeTillGhostAppears = 3;

        public int score = 0;

        public Ghost[] ghosts;
        public const int numberOfGhosts = 8;
        public int remainingGhosts = 0;
        int coinCount = 0;

        private const int selectableTime = 10;
        private int selectableGhost = -1;
        Random random;

        /// <summary>
        /// flag that indicates if ghost a ghost is in center or not
        /// currently every "selectableTime"  a ghost is selectable for "selectableTime"
        /// see attribute: selectableTime
        /// </summary>
        bool select = true;

        // Wii access data
        int port = 1111;
        CallasOsc.Receiver receiver;

        public GameplayScene(Geisterhaus Engine_)
            : base(Engine_)
        {
        }

        public void Initialize()
        {
            GameOver = false;
            messageScreen = null;
            engine.screenWidth = (int)engine.baseScreenSize.X;
            engine.screenHeight = (int)engine.baseScreenSize.Y;

            float gpa_y = 0;
            float gpa_width = engine.screenWidth * engine.gamePlayerAreaPerentage;
            float gpa_height = engine.screenHeight;
            float gpa_x = engine.screenWidth / 2.0f - gpa_width / 2.0f;
            engine.gamePlayerArea = new Rectangle((int)gpa_x, (int)gpa_y, (int)gpa_width, (int)gpa_height);

            PlayedTimeString = selectableTime.ToString();

            InitGhosts();
            init_Wii();

            score = 0;
            remainingGhosts = 0;
            coinCount = 0;

            //an PDA Spielstart übermitteln
            engine.btc.msg1(-2);
        }

        public void init_Wii()
        {
            WiiListener listener = new WiiListener(this);
            receiver = new CallasOsc.Receiver(port, listener.receive);
            receiver.start();
        }

        public override void StartMusic()
        {
            MediaPlayer.Stop();
            MediaPlayer.Volume = 0.3f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(engine.Wind);
        }

        // Zeichnet das komplette Spielfeld
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            engine.screenRectangle = new Rectangle(0, 0, engine.screenWidth, engine.screenHeight);
            //CreateForeground();
            if (!GameOver)
            {
                spriteBatch.Draw(engine.backgroundTexture, new Rectangle(0, 0, engine.screenWidth, engine.screenHeight), Color.White);

                DrawCastle(spriteBatch);

                DrawDummy(spriteBatch);

                DrawGhosts(spriteBatch, gameTime);
                //DrawMarker(spriteBatch, engine.sw, engine.sh);
                DrawText(spriteBatch);

                drawCoins(spriteBatch);
            }
            else
            {
                if (messageScreen != null)
                    messageScreen.Draw(spriteBatch, gameTime);
                return;
            }
            if (messageScreen != null)
                messageScreen.Draw(spriteBatch, gameTime);

        }

        private void DrawCastle(SpriteBatch spriteBatch)
        {
            // draw rectangle to indicate game item area
            //spriteBatch.Draw(engine.onePixel, engine.gamePlayerArea, Color.PaleTurquoise);
            if (engine.keepOriginalHigh) spriteBatch.Draw(engine.castleTexture, new Rectangle(engine.gamePlayerArea.X, 80, engine.gamePlayerArea.Width, engine.castleTexture.Height), Color.White);
            else
            {
                float sa = (float)engine.castleTexture.Width / (float)engine.gamePlayerArea.Width;
                float newHeight = engine.castleTexture.Height / sa;
                spriteBatch.Draw(engine.castleTexture, new Rectangle(engine.gamePlayerArea.X, 20, engine.gamePlayerArea.Width, (int)newHeight), Color.White);
            }

        }

        private void DrawDummy(SpriteBatch spriteBatch)
        {
            if (engine.keepOriginalHigh)
            {
                Vector2 dummyPosition = new Vector2((engine.screenWidth / 2 - engine.dummy.Width / 2), (engine.screenHeight - engine.dummy.Height));
                spriteBatch.Draw(engine.dummy, new Rectangle(engine.gamePlayerArea.X, (int)dummyPosition.Y, engine.gamePlayerArea.Width, engine.dummy.Height), Color.White);
            }
            else
            {
                float sa = (float)engine.dummy.Width / (float)engine.gamePlayerArea.Width;
                float newHight = engine.dummy.Height / sa;
                spriteBatch.Draw(engine.dummy, new Rectangle(engine.gamePlayerArea.X, engine.screenHeight - (int)newHight, engine.gamePlayerArea.Width, (int)newHight), Color.White);
            }
        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(engine.font, "Punkte: " + score, new Vector2(engine.screenWidth - 250, 20), Color.White);
            if (!select) spriteBatch.DrawString(engine.font, "Zeit: " + PlayedTimeString, new Vector2(engine.screenWidth - 250, 60), Color.White);
        }

        private void DrawGhosts(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (Ghost g in ghosts)
            {
                if (g.IsActive)
                {
                    g.Draw(spriteBatch, gameTime);
                }
            }
        }

        private void drawCoins(SpriteBatch spriteBatch)
        {
            int x, y;
            int coinSize = 50;
            int spaceX = 10;
            int spaceY = 10;
            int line = 2;
            /*
            spriteBatch.Draw(engine.onePixel, new Rectangle(engine.screenWidth - 250 - spaceX / 2, 100 - line / 2, coinSize * 4 + spaceX * 4, line), Color.LightGray);
            spriteBatch.Draw(engine.onePixel, new Rectangle(engine.screenWidth - 250 - spaceX / 2, 100 - line / 2 + coinSize + spaceY, coinSize * 4 + spaceX * 4, line), Color.LightGray);
            spriteBatch.Draw(engine.onePixel, new Rectangle(engine.screenWidth - 250 - spaceX / 2, 100 - line / 2 + 2 * (coinSize + spaceY), coinSize * 4 + spaceX * 4, line), Color.LightGray);
            for (int i = 0; i < 5; i++)
            {
                x = engine.screenWidth - 250 + i * (coinSize + spaceX) - spaceX / 2;
                y = 100 - line / 2;
                int width = line;
                int height = 2 * (coinSize + spaceY);
                spriteBatch.Draw(engine.onePixel, new Rectangle(x, y, width, height), Color.LightGray) ;
            }
             */
            for (int i = 0; i < 8; i++)
            {
                // 4 coins fit in one row
                int f = i / 4;
                y = 100 + spaceX / 2 + f * (spaceY + coinSize);
                x = engine.screenWidth - 250 + (i % 4) * (spaceX + coinSize);
                spriteBatch.Draw(engine.coinGray, new Rectangle(x, y, coinSize, coinSize), Color.White);
            }
            for (int i = 0; i < coinCount; i++)
            {
                // 4 coins fit in one row
                int f = i / 4;
                y = 100 + spaceX / 2 + f * (spaceY + coinSize);
                x = engine.screenWidth - 250 + (i % 4) * (spaceX + coinSize);
                spriteBatch.Draw(engine.coin, new Rectangle(x, y, coinSize, coinSize), Color.White);
            }

            if (coinCount >= 8)
                win();
        }

        private void CreateForeground()
        {
            Color[] foregroundColors = new Color[engine.screenWidth * engine.screenHeight];
            Color c = new Color(100, 10, 20, 128);

            for (int x = 0; x < engine.screenWidth; x++)
                for (int y = 0; y < engine.screenHeight; y++)
                    foregroundColors[x + y * engine.screenWidth] = c;

            engine.foregroundTexture = new Texture2D(engine.device, engine.screenWidth, engine.screenHeight, 1, TextureUsage.None, SurfaceFormat.Color);
            engine.foregroundTexture.SetData(foregroundColors);
        }

        // Aktualisiert den Spielbildschirm
        public override void Update(GameTime gameTime)
        {
            this.gameTime = gameTime;
            if (messageScreen == null)
            {
                // total time to destroy a ghost in seconds
                int time = (int)(timer.Interval / 1000);
                //time left since ghost is destroyable in seconds
                remainTime = time - (int)(gameTime.TotalGameTime.Subtract(startTimeToDestroy).TotalSeconds);

                PlayedTimeString = remainTime.ToString();
            }
            if (messageScreen != null)
            {
                messageScreen.Update(gameTime);
                if (messageScreen.Alpha == 0f)
                    messageScreen = null;
            }
        }

        private void InitGhosts()
        {
            timer = new System.Timers.Timer(timeTillGhostAppears * 1000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            ghosts = new Ghost[numberOfGhosts];
            for (int i = 0; i < numberOfGhosts; i++)
            {
                createSingleGhost(i);
                // we need to wait for some milliseconds because the randomizer needs some time
                Thread.Sleep(90);
            }
            timer.Start();

        }

        /// <summary>
        /// selects a random ghost out of all ghosts
        /// </summary>
        private void selectRandomGhost(bool destroyedLastGhost)
        {
            if (!GameOver)
            {
                if (select)
                {
                    if (random == null) random = new Random(gameTime.TotalRealTime.Milliseconds);
                    selectableGhost = random.Next(0, numberOfGhosts - 1);
                    startTimeToDestroy = gameTime.TotalGameTime;
                    ghosts[selectableGhost].isSelectable = true;
                    select = false;
                }
                else
                {
                    if (selectableGhost > -1)
                    {
                        if (!destroyedLastGhost) lostGhost();
                        else coinCount++;
                        createSingleGhost(selectableGhost);
                        ghosts[selectableGhost].isSelectable = false;
                    }
                    select = true;
                }
            }

        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timer.Interval == timeTillGhostAppears * 1000) timer.Interval = 1000 * selectableTime;
            selectRandomGhost(false);
        }

        private int getRandomGesture()
        {
            Random random = new Random();
            return random.Next(0, 4);
        }

        //create new ghost if a ghost was catched or lost
        void createSingleGhost(int i)
        {
            if (!GameOver)
            {
                ghosts[i] = new Ghost(this, engine, false);
                ghosts[i].init(engine.screenWidth, engine.screenHeight);
                ghosts[i].gest = getRandomGesture();
            }
        }

        private void lostGhost()
        {
            remainingGhosts++;
            PlaySound(3);
            switch (remainingGhosts)
            {
                case 1: engine.castleTexture = engine.castle1; break;
                case 2: engine.castleTexture = engine.castle2; break;
                case 3: engine.castleTexture = engine.castle3; break;
                case 4: engine.castleTexture = engine.castle4; break;
                case 5: engine.castleTexture = engine.castle5; break;
                case 6: engine.castleTexture = engine.castle6; break;
                case 7: engine.castleTexture = engine.castle7; break;
                case 8: engine.castleTexture = engine.castle8; break;
            }
            if (remainingGhosts == 8)
                die();
        }

        void die()
        {
            //Game over tex anzeigen, highscore speichern
            messageScreen = new MessageScreen(this, engine.gameoverTexture, new Vector2(engine.baseScreenSize.X / 2, engine.baseScreenSize.Y / 2));
            engine.highscore.saveNewHighscore(score, remainingGhosts);
            PlaySound(1);
            gameEnd();
            GameOver = true;
        }

        void win()
        {
            messageScreen = new MessageScreen(this, engine.winTexture, new Vector2(engine.baseScreenSize.X / 2, engine.baseScreenSize.Y / 2));
            engine.highscore.saveNewHighscore(score, remainingGhosts);
            GameOver = true;
            gameEnd();
        }

        public void handleWiiInput(int message)
        {
            if (!GameOver)
            {
                // right gesture
                if (ghosts[selectableGhost].gest == message)
                {
                    score += remainTime;
                    PlaySound(2);
                    selectRandomGhost(true);

                    //An PDA: Geste richtig ausgeführt
                    engine.btc.msg1(-4);
                }
                else
                {
                    /*Ghost g = ghosts[selectableGhost];
                    int oldGest = g.gest;
                    g.gest = 4;
                    Thread.Sleep(1000);
                    g.gest = oldGest;*/

                    //An PDA: Geste falsch ausgeführt
                    engine.btc.msg1(-3);
                }
            }
        }

        public void gameEnd()
        {
            timer.Stop();
            //GAME-OVER-Message an PDA
            engine.btc.msg1(-1);
            //Wii-Receiver stoppen für Neustart des Spiels
            receiver.stop();
        }

        public override void HandleInput(Keys Key_)
        {
            bool HandleGamePlayKeys = true;
            if ((messageScreen != null) && (messageScreen.Active))
                HandleGamePlayKeys = false;

            if (HandleGamePlayKeys)
            {
                if (Key_ == Keys.G)
                {
                    handleWiiInput(ghosts[selectableGhost].gest);
                }
            }
            if (Key_ == Keys.Enter)
            {
                if (messageScreen != null)
                {
                    if (messageScreen.Active)
                    {
                        messageScreen.Active = false;
                        if (GameOver)
                        {
                            engine.setScene(2);
                        }
                    }
                }
            }
            if (Key_ == Keys.Escape)
            {
                engine.setScene(0);
                gameEnd();
            }
            if (Key_ == Keys.Back)
                //handleWiiInput(1);
                lostGhost();
        }
    }
}