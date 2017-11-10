using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SpaceInvaders
{
    /// <summary>
    /// Ammo direction enumeration.
    /// </summary>
    public enum direction
    {
        UP = 1,
        DOWN = 2
    }

    class Ammo
    {
        private direction _dir;
        private Point _coordinates;
        private int _ammoSpeed;
        protected BitmapImage _ammoBitmap;
        protected Image _ammoImage;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dir">Ammo direction (up/down)</param>
        /// <param name="coordinates">Initial coordinates</param>
        public Ammo(direction dir, Point coordinates)
        {
            _ammoImage = new Image();
            _dir = dir;
            _coordinates = coordinates;
            _ammoSpeed = 1;

            _ammoBitmap = (_dir.Equals(direction.UP))
                             ? new BitmapImage(new Uri(@"Resources\Bullet_down.png", UriKind.Relative))
                             : new BitmapImage(new Uri(@"Resources\Bullet_up.png", UriKind.Relative));

            _ammoImage.Source = _ammoBitmap;
            _ammoImage.Width = _ammoImage.Width;
            _ammoImage.Height = _ammoImage.Height;
        }

        /// <summary>
        /// Get ammo image.
        /// </summary>
        /// <returns>Ammo Image.</returns>
        public Image GetImage()
        {
            return _ammoImage;
        }

        /// <summary>
        /// Set/Get ammo speed.
        /// </summary>
        public int AmmoSpeed
        {
            get
            {
                return _ammoSpeed;
            }
            set
            {
                _ammoSpeed = value;
            }
        }

        /// <summary>
        /// Update ammo coordinates.
        /// </summary>
        /// <param name="amount"></param>
        public void Move(double amount)
        {
            if(_dir.Equals(direction.UP))
            {
                _coordinates.Y -= amount;
            }            
            else if(_dir.Equals(direction.DOWN))
            {
                _coordinates.Y += amount;
            }
        }

        /// <summary>
        /// Get ammo direction.
        /// </summary>
        /// <returns>Direction enmumeration (UP/DOWN)</returns>
        public direction GetDirection()
        {
            return _dir;
        }

        /// <summary>
        /// Set/Get Ammo coordinates.
        /// </summary>
        public Point Coordinates
        {
            get
            {
                return _coordinates;
            }

            set
            {
                _coordinates.X = value.X;
                _coordinates.Y = value.Y;
            }
        }

    }
}
