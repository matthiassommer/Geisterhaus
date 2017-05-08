using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Geisterhaus
{
    public class Highscore : GameScene
    {
        struct highscore
        {
            public String score;
            public int remainingGhosts;
        }

        float c = 1f;
        int maxScores = 6;
        highscore[] hs;

        textdatei t = new textdatei();

        public Highscore(Geisterhaus Game_)
            : base(Game_)
        {
            Initialize();
        }

        public void Initialize()
        {
            hs = new highscore[maxScores];
            readHighscore();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            engine.screenRectangle = new Rectangle(0, 0, engine.screenWidth, engine.screenHeight);
            spriteBatch.Draw(engine.highscoreTexture,
                                new Vector2(engine.screenRectangle.Width / 2 - engine.highscoreTexture.Width / 2, 50),
                                new Color(c, c, c, 1f));
            spriteBatch.DrawString(engine.font, "Uebrige Zeit", new Vector2(engine.screenWidth / 3 - 50, 250), Color.White);
            spriteBatch.DrawString(engine.font, "Verlorene Geister", new Vector2(engine.screenWidth / 3 + 150, 250), Color.White);

            for (int i = 1; i < maxScores; i++)
            {
                spriteBatch.DrawString(engine.font, hs[i].score, new Vector2(engine.screenWidth / 3 - hs[i].score.Length, 250 + i * 50), Color.White);
                spriteBatch.DrawString(engine.font, ""+hs[i].remainingGhosts, new Vector2(engine.screenWidth / 2, 250 + i * 50), Color.White);
            }

            spriteBatch.Draw(engine.weiter,
                                new Vector2(engine.screenRectangle.Width / 2, maxScores*50+280),
                                new Color(c, c, c, 1f));
        }

        public override void Update(GameTime GameTime_)
        {
            readHighscore();
        }

        public void saveNewHighscore(int score, int remainingGhosts)
        {
            for (int i = 1; i < maxScores; i++)
            {
                int sc = Convert.ToInt32(hs[i].score);
                int rg = hs[i].remainingGhosts;
                int tmpMaxScore = maxScores - 1;

                if (sc < score)
                {
                    rotateHighscore(i, tmpMaxScore, score, remainingGhosts);
                    return;
                }
                else if (sc == score && rg < remainingGhosts)
                {
                    rotateHighscore(i, tmpMaxScore, score, remainingGhosts);
                    return;
                }
                else if (sc == score && rg > remainingGhosts)
                {
                    i++;
                    rotateHighscore(i, tmpMaxScore, score, remainingGhosts);
                    return;
                }
            }
        }

        void rotateHighscore(int i, int tmpMaxScore, int score, int remainingGhosts)
        {
            for (; tmpMaxScore > i; tmpMaxScore--)
            {
                int tmp = tmpMaxScore - 1;
                t.WriteLine(tmpMaxScore, hs[tmp].score + " " + hs[tmp].remainingGhosts);
            }
            t.WriteLine(i, score + " " + remainingGhosts);
            return;
        }

        void readHighscore()
        {
            for (int i = 1; i < maxScores; i++)
            {
                String s = t.ReadLine(i);
                String[] words = s.Split(' ');
                highscore h;
                h.score = words[0];
                
                h.remainingGhosts = Convert.ToInt32(words[1]);
                hs[i] = h;
            }
        }

        public override void HandleInput(Keys Key_)
        {
            if (Key_ == Keys.Escape)
                engine.setScene(0);
            if (Key_ == Keys.Enter)
                engine.setScene(0);
        }
    }
}
