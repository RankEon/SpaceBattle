using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpaceInvaders
{
    class Enemy : GameObject
    {
        private int enemySpacing = 20; //px

        /// <summary>
        /// Constructor
        /// </summary>
        public Enemy() : base()
        {

        }

        /// <summary>
        /// Constructor with initial coordinates.
        /// </summary>
        /// <param name="point">Initial coordinates</param>
        public Enemy(Point point) : base(point)
        {
        }

        /// <summary>
        /// Move amount of pixels in X -axle.
        /// </summary>
        /// <param name="xVal">Amount to move.</param>
        public void MoveX(double xVal)
        {
            _coordinates.X += xVal;
        }

        /// <summary>
        /// Move amount of pixels in Y -axle.
        /// </summary>
        /// <param name="yVal">Amount to move.</param>
        public void MoveY(double yVal)
        {
            _coordinates.Y += yVal;
        }

    }
}
