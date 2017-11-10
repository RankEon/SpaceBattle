using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpaceInvaders
{
    class Hero : GameObject
    {
        /// <summary>
        /// Player (Hero) -object constructor, inherited from GameObject
        /// </summary>
        public Hero() : base()
        {
            SetImage(@"Resources\Spaceship.png");

            _coordinates.X = 500;
            _coordinates.Y = 500;

            _gameObjectImage.Height = 50;
            _gameObjectImage.Width = 60;
        }
    }
}
