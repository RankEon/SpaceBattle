using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SpaceInvaders
{
    /// <summary>
    /// Thread to play the explosion effects sequence.
    /// </summary>
    class ExplosionEffectsThread
    {
        private Point _coordinates;
        private int _width;
        private int _height;
        private int _repeats;
        GameCanvas _canvas;
        private Image[] _explosionImg = new Image[6];

        /// <summary>
        /// Constructor of the explosion effect thread.
        /// </summary>
        /// <param name="coordinates">Coordinates of the explosion</param>
        /// <param name="width">Width of the explosion effect box.</param>
        /// <param name="height">Heigth of the explosion effect box.</param>
        /// <param name="canvas">Game canvas object.</param>
        /// <param name="explosionImages">Explosion image sequence</param>
        /// <param name="repeats">Repeats to run the sequence (default=10)</param>
        public ExplosionEffectsThread(Point coordinates, int width, int height, GameCanvas canvas, Image[] explosionImages, [Optional, DefaultParameterValue(10)] int repeats)
        {
            _coordinates = coordinates;
            _width = width;
            _height = height;
            _canvas = canvas;
            _repeats = repeats;
            _explosionImg = explosionImages;
        }

        /// <summary>
        /// Worker thread to play the enemy explosion effect.
        /// </summary>
        public async void explosionEffectsDoWork()
        {
            for (int j = 0; j < 5; j++)
            {
                await _canvas.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _canvas.AmmoEffectsCanvas.Children.Clear();

                    for (int i = 0; i < _explosionImg.Length; i++)
                    {
                        Canvas.SetLeft(_explosionImg[i], _coordinates.X);
                        Canvas.SetTop(_explosionImg[i], _coordinates.Y - 40);
                        _canvas.AmmoEffectsCanvas.Children.Add(_explosionImg[i]);
                    }
                }), DispatcherPriority.Normal, null);

                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Worker thread to play the player ship explosion effect.
        /// </summary>
        public async void explosionDamageEffectToHeroShipDoWork()
        {
            for (int j = 0; j < 5; j++)
            {
                await _canvas.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _canvas.HeroDamageEffectsCanvas.Children.Clear();

                    for (int i = 0; i < _explosionImg.Length; i++)
                    {
                        Canvas.SetLeft(_explosionImg[i], _coordinates.X);
                        Canvas.SetTop(_explosionImg[i], _coordinates.Y - 40);
                        _canvas.HeroDamageEffectsCanvas.Children.Add(_explosionImg[i]);
                    }
                }), DispatcherPriority.Normal, null);

                await Task.Delay(1);
            }
        }
    }
}
