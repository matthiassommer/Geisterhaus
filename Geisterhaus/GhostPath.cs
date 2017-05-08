using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Geisterhaus
{
    class GhostPath
    {
        public const int TOP_LEFT = 0;
        public const int TOP_RIGHT = 1;
        public const int BOTTOM_LEFT = 2;
        public const int BOTTOM_RIGHT = 3;

        private Random randomizer = new Random();
        //private Vector2[] controlPoints;
        private List<Vector2> l_controlPoints;
        private const int controlPointCount = 20;
        private List<Vector2> l_positions;
        private int positionIndex = 0;

        Geisterhaus engine;

        //time for scaling set between 10 and 100
        int scaleWhith;

        int scaleIndex = 10;

        bool increase = true;

        //set from 0.1 to 1
        float scaleHeight;
        //float selectableScale = 3.0f;

        int slowDownIndex = 0;
        int slowDown = 6;
        bool animatetedToScreenCenter = false;

        int animateToCenterSteps = 100;
        int animateToCenterIndex = 0;
        float animateToScreenCenterTargetScale = 1.5f;

        Vector2 castlePosition;

        public bool isBackAtPosition = false;

        //int animateToCasteSteps = 100;
        //int animateToCastleIndex = 0;

        public bool isMenueGhost;

        private Ghost ghost;

        NaturalCubic nc;

        /// <summary>
        /// generates for a specific ghost a path for animation
        /// </summary>
        /// <param name="whichGhost"></param>
        /// <param name="engine"></param>
        public GhostPath(Ghost ghost, Geisterhaus engine, bool isMenueGhost)
        {
            this.ghost = ghost;
            this.isMenueGhost = isMenueGhost;
            this.engine = engine;
            castlePosition = new Vector2(engine.screenWidth / 2, 30);
            l_controlPoints = new List<Vector2>();
            for (int i = 0; i < controlPointCount; i++) l_controlPoints.Add(randomPos());
            nc = new NaturalCubic();
            l_positions = nc.getPositionPath(l_controlPoints);
            // time for scaling
            scaleWhith = randomizer.Next(100, 400);
            if(isMenueGhost) scaleHeight = ((float)randomizer.Next(5, 10)) / 10.0f;
            else scaleHeight = 0.3f + ((float)randomizer.Next(0, 3)) / 10.0f;
        }

        /*
        public Vector2 animateToCastle()
        {
            Vector2 centerPosition =  new Vector2(engine.screenWidth / 2 - engine.ghostTextureDreieck.Width * animateToScreenCenterTargetScale / 2, engine.screenHeight / 2 - engine.ghostTextureDreieck.Height * animateToScreenCenterTargetScale / 2);
            Vector2 targetPosition = new Vector2(engine.gamePlayerArea.X + engine.castle1.Width / 2, 20 + engine.castle1.Height);

            Vector2 position = new Vector2();

            position.X = centerPosition.X + Math.Abs(targetPosition.X - centerPosition.X) / animateToCasteSteps * animateToCastleIndex;
            position.Y = centerPosition.Y - Math.Abs(targetPosition.Y - centerPosition.Y) / animateToCasteSteps * animateToCastleIndex;

            if(animateToCastleIndex < animateToCenterSteps) animateToCastleIndex++;

            return position;
        }*/
 

        private Vector2 randomPos()
        {
            int textureWidth = engine.ghostTextureDreieck.Width;
            int textureHeight = engine.ghostTextureDreieck.Height;
            return random(0, engine.screenWidth - textureWidth, 0, engine.screenHeight - textureHeight);
        }

        /// <summary>
        /// generiert einen zufälligen Startpunkt für die Geister
        /// </summary>
        /// <param name="s1"> min x </param>
        /// <param name="s2"> max x  </param>
        /// <param name="t1"> min y</param>
        /// <param name="t2"> max y</param>
        /// <returns></returns>
        private Vector2 random(int minX, int maxX, int minY, int maxY)
        {
            int rand1 = (int)randomizer.Next(minX, maxX);
            int rand2 = (int)randomizer.Next(minY, maxY);
            return new Vector2((float)rand1, (float)rand2);
        }

        private void initNewPositions()
        {
            Vector2 last = l_controlPoints[l_controlPoints.Count - 1];
            l_controlPoints.Clear();
            l_controlPoints.Add(last);
            for (int i = 0; i < controlPointCount-1; i++) l_controlPoints.Add(randomPos());
            l_positions = nc.getPositionPath(l_controlPoints);
            positionIndex = 0;
        }

        


        /// <summary>
        /// returns the current position
        /// </summary>
        /// <returns></returns>
        public Vector2 getCurrentPosition(int slowDown)
        {
            this.slowDown = slowDown;

            if (positionIndex + 1 >= l_positions.Count) initNewPositions();
            
            Vector2 currentPosition = l_positions[positionIndex];
            Vector2 nextPosition = l_positions[positionIndex + 1];

            /*
            if (positionIndex + 1 >= positions.Length) nextPosition = positions[0];
            else nextPosition = positions[positionIndex + 1];
            //
            if (positionIndex + 1 >= l_positions.Count) nextPosition = l_positions[0];
            else nextPosition = l_positions[positionIndex + 1];
            */

            float dx = nextPosition.X - currentPosition.X;
            float dy = nextPosition.Y - currentPosition.Y;

            slowDownIndex++;
            if (slowDownIndex > slowDown)
            {
                slowDownIndex = 0;
                positionIndex++;
                if (positionIndex > l_positions.Count - 1) positionIndex = 0;
                return getCurrentPosition(slowDown);
            }
            Vector2 cPosition = new Vector2(currentPosition.X + (dx / slowDown) * slowDownIndex, currentPosition.Y + (dy / slowDown) * slowDownIndex);
            return cPosition;
        }

        public float getCurrentScale()
        {
            // normal scale animation in positiv direction
            if (increase && !animatetedToScreenCenter && !ghost.isSelectable)
            {
                scaleIndex++;
                if (scaleIndex > scaleWhith) increase = false;
                return (scaleHeight / scaleWhith) * scaleIndex;
            }
            // normal scale animation in negativ direction
            else if (!animatetedToScreenCenter && !ghost.isSelectable)
            {
                scaleIndex--;
                if (scaleIndex < 10) increase = true;
                return (scaleHeight / scaleWhith) * scaleIndex;
            }
            else //if (ghost.isSelectable)
            {
                /*
                float currentScale = (scaleHeight / scaleWhith) * scaleIndex;
                animateToCenterIndex++;
                return (animateToScreenCenterTargetScale - currentScale) / animateToCenterSteps * animateToCenterIndex;
                 * */
                return animateToScreenCenterTargetScale;
            }
            // animation to screen center
            /*else
            {
                float currentScale = (scaleHeight / scaleWhith) * scaleIndex;
                return (animateToScreenCenterTargetScale - currentScale) / animateToCenterSteps * animateToCenterIndex;
            }*/
            /*
            scaleIndex++;
            float currentScale = (scaleHeight / scaleWhith) * scaleIndex;
            return currentScale;
            float currentScale = 1.0f + 0.5f*(float)Math.Sin(0.0005f * engine.gameScene.PlayedTime.Milliseconds);
            return currentScale;
            */

        }

        /// <summary>
        /// animates linear to screen center from current ghost position
        /// </summary>
        /// <returns></returns>
        public Vector2 animateToScreenCenter()
        {
            Vector2 currentPosition = l_positions[positionIndex];
            Vector2 targetPosition = new Vector2(engine.screenWidth / 2 - engine.ghostTextureDreieck.Width * animateToScreenCenterTargetScale / 2, engine.screenHeight / 2 - engine.ghostTextureDreieck.Height * animateToScreenCenterTargetScale / 2);
            Vector2 animationPosition = new Vector2();
            
            //animate in left direction
            if (targetPosition.X > currentPosition.X)
            {
                animationPosition.X = currentPosition.X - Math.Abs(targetPosition.X - currentPosition.X) / animateToCenterSteps * animateToCenterIndex;
            }
            //animate in right direction
            else
            {
                animationPosition.X = currentPosition.X + Math.Abs(targetPosition.X - currentPosition.X) / animateToCenterSteps * animateToCenterIndex;
            }

            //animate in down direction
            if (targetPosition.Y > currentPosition.Y)
            {
                animationPosition.Y = currentPosition.Y + Math.Abs(targetPosition.Y - currentPosition.Y) / animateToCenterSteps * animateToCenterIndex;
            }
            //animate in down direction
            else
            {
                animationPosition.Y = currentPosition.Y + Math.Abs(targetPosition.Y - currentPosition.Y) / animateToCenterSteps * animateToCenterIndex;
            }

            if (animateToCenterIndex < animateToCenterSteps) animateToCenterIndex++;
            else isBackAtPosition = false;
            //return animationPosition;


            return targetPosition;

            /*
            animatetedToScreenCenter = true;
            increase = true;
            Vector2 currentPosition = l_positions[positionIndex];
            /*
            Vector2 targetPosition = new Vector2(engine.screenWidth / 2 - (engine.ghostTexture.Width / 2) * animateToScreenCenterTargetScale,
                                                 engine.screenHeight / 2 - (engine.ghostTexture.Height / 2) * animateToScreenCenterTargetScale - 100);
            */
            /*
            Vector2 targetPosition = new Vector2(engine.screenWidth / 2, engine.screenHeight / 2);
            Vector2 animationPosition = new Vector2();

            animationPosition.X = currentPosition.X - Math.Abs(targetPosition.X - currentPosition.X) / animateToCenterSteps * animateToCenterIndex;
            animationPosition.Y = currentPosition.Y + Math.Abs(targetPosition.Y - currentPosition.Y) / animateToCenterSteps * animateToCenterIndex;
            
            if (animateToCenterIndex < animateToCenterSteps) animateToCenterIndex++;
            else isBackAtPosition = false;
            return animationPosition;
             * */
        }

        /*
        public Vector2 animateBackToPosition()
        {
            if (animatetedToScreenCenter)
            {
                animatetedToScreenCenter = false;
                animateToCenterIndex = 0;
            }
            
            //increase = false;
            Vector2 targetPosition = l_positions[positionIndex];
            Vector2 currentPosition = new Vector2(engine.screenWidth / 2 - (engine.ghostTexture.Width / 2) * animateToScreenCenterTargetScale,
                                                 engine.screenHeight / 2 - (engine.ghostTexture.Height / 2) * animateToScreenCenterTargetScale);
            Vector2 animationPosition = new Vector2();

            if (this.whichGhost == TOP_LEFT)
            {
                animationPosition.X = currentPosition.X - Math.Abs(targetPosition.X - currentPosition.X) / animateToCenterSteps * animateToCenterIndex;
                animationPosition.Y = currentPosition.Y - Math.Abs(targetPosition.Y - currentPosition.Y) / animateToCenterSteps * animateToCenterIndex;
            }
            else if (this.whichGhost == TOP_RIGHT)
            {
                animationPosition.X = currentPosition.X + Math.Abs(targetPosition.X - currentPosition.X) / animateToCenterSteps * animateToCenterIndex;
                animationPosition.Y = currentPosition.Y - Math.Abs(targetPosition.Y - currentPosition.Y) / animateToCenterSteps * animateToCenterIndex;
            }
            else if (this.whichGhost == BOTTOM_LEFT)
            {
                animationPosition.X = currentPosition.X - Math.Abs(targetPosition.X - currentPosition.X) / animateToCenterSteps * animateToCenterIndex;
                animationPosition.Y = currentPosition.Y + Math.Abs(targetPosition.Y - currentPosition.Y) / animateToCenterSteps * animateToCenterIndex;
            }
            else if (this.whichGhost == BOTTOM_RIGHT)
            {
                animationPosition.X = currentPosition.X + Math.Abs(targetPosition.X - currentPosition.X) / animateToCenterSteps * animateToCenterIndex;
                animationPosition.Y = currentPosition.Y + Math.Abs(targetPosition.Y - currentPosition.Y) / animateToCenterSteps * animateToCenterIndex;
            }
            if (animateToCenterIndex < animateToCenterSteps) animateToCenterIndex++;
            else isBackAtPosition = true;
            return animationPosition;
        }
         * */

    }
}
