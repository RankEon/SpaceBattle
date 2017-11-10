using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpaceInvaders
{
    /// <summary>
    /// Game object base class.
    /// </summary>
    class GameObject
    {
        protected Point _coordinates = new Point();
        protected BitmapImage _gameObjectBitmap;
        protected Image _gameObjectImage = new Image();
        protected bool _isAlive = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameObject()
        {
            _isAlive = true;
        }

        /// <summary>
        /// Constructor with initial coordinates.
        /// </summary>
        /// <param name="point">Initial coordinates.</param>
        public GameObject(Point point)
        {
            _coordinates.X = point.X;
            _coordinates.Y = point.Y;
            _isAlive = true;
        }

        /// <summary>
        /// Sets the image for the object.
        /// </summary>
        /// <param name="path">Path to the image (relative).</param>
        public void SetImage(string path)
        {
            _gameObjectBitmap = new BitmapImage(new Uri(path, UriKind.Relative));
            _gameObjectImage.Source = _gameObjectBitmap;
            _gameObjectImage.Width = _gameObjectImage.Width;
            _gameObjectImage.Height = _gameObjectImage.Height;
        }

        /// <summary>
        /// Set/Get whether the object is alive.
        /// </summary>
        public bool IsAlive
        {
            get { return _isAlive; }
            set { _isAlive = value; }
        }

        /// <summary>
        /// Gets the object image.
        /// </summary>
        /// <returns>Image of the object.</returns>
        public Image GetImage()
        {
            return _gameObjectImage;
        }

        /// <summary>
        /// Set/Get object's coordinates.
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
