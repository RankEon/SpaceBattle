using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SpaceInvaders
{
    class Graphics
    {
        private GameLoop _gameLoop;
        private GameCanvas _gameCanvas;

        protected BitmapImage[] _explosionImgBitmap = new BitmapImage[6];
        private Image[] _explosionImg = new Image[6];

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gl">GameLoop -object</param>
        public Graphics(GameLoop gl)
        {
            _gameLoop = gl;
            _gameCanvas = gl.GetCanvas();
            LoadExplosionImages();
        }

        /// <summary>
        /// Loads explosion image sequence.
        /// </summary>
        private void LoadExplosionImages()
        {
            for (int i = 0; i < _explosionImg.Length; i++)
            {
                _explosionImgBitmap[i] = new BitmapImage(new Uri($"Resources\\explosion_{i+1}.png", UriKind.Relative));
                _explosionImg[i] = new Image();
                _explosionImg[i].Source = _explosionImgBitmap[i];
                _explosionImg[i].Width = _explosionImgBitmap[i].Width;
                _explosionImg[i].Height = _explosionImgBitmap[i].Height;
            }
        }

        /// <summary>
        /// Draws enemies on canvas.
        /// </summary>
        public void DrawEnemies()
        {
            Enemy[,] Enemies = _gameLoop.GetEnemyArr();

            // Clear canvas.
            _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _gameCanvas.EnemyCanvas.Children.Clear();
            }), DispatcherPriority.Normal, null);

            // Draw enemies in parallel.
            Parallel.For(0, 5, i =>
            {
                _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (Enemies[i, j].IsAlive == true)
                        {
                            Canvas.SetLeft(Enemies[i, j].GetImage(), Enemies[i, j].Coordinates.X);
                            Canvas.SetTop(Enemies[i, j].GetImage(), Enemies[i, j].Coordinates.Y);
                            _gameCanvas.EnemyCanvas.Children.Add(Enemies[i, j].GetImage());
                        }
                    }
                }), DispatcherPriority.Normal, null);
            });
        }

        /// <summary>
        /// Draws the player ship image (i.e. the hero).
        /// </summary>
        public void DrawHero()
        {
            Hero hero = _gameLoop.GetHero();

            _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _gameCanvas.PlayerCanvas.Children.Clear();
                Canvas.SetLeft(hero.GetImage(), hero.Coordinates.X);
                Canvas.SetTop(hero.GetImage(), hero.Coordinates.Y);
                _gameCanvas.PlayerCanvas.Children.Add(hero.GetImage());
            }), DispatcherPriority.Normal, null);
        }

        /// <summary>
        /// Draws the hero ammo to the canvas.
        /// </summary>
        public void DrawHeroAmmo()
        {
            List<Ammo> ammo = _gameLoop.GetHeroAmmo();

            if (!ammo.Any())
            {
                return;
            }

            // Draw all of the fired hero ammo to the canvas.
            _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _gameCanvas.HeroAmmoCanvas.Children.Clear();

                for (int i = 0; i < ammo.Count; i++)
                {
                    Canvas.SetLeft(ammo[i].GetImage(), ammo[i].Coordinates.X);
                    Canvas.SetTop(ammo[i].GetImage(), ammo[i].Coordinates.Y);
                    _gameCanvas.HeroAmmoCanvas.Children.Add(ammo[i].GetImage());
                }
            }), DispatcherPriority.Normal, null);
        }

        /// <summary>
        /// Draws enemy ammo to the canvas.
        /// </summary>
        public void DrawEnemyAmmo()
        {
            List<Ammo> ammo = _gameLoop.GetEnemyAmmo();

            if (!ammo.Any())
            {
                //_gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    _gameCanvas.EnemyAmmoCanvas.Children.Clear();
                //}), DispatcherPriority.Normal, null);
                return;
            }

            // Draw all fired ammo to the canvas.
            _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _gameCanvas.EnemyAmmoCanvas.Children.Clear();

                foreach (var bullet in ammo)
                {
                    Canvas.SetLeft(bullet.GetImage(), bullet.Coordinates.X);
                    Canvas.SetTop(bullet.GetImage(), bullet.Coordinates.Y);
                    _gameCanvas.EnemyAmmoCanvas.Children.Add(bullet.GetImage());
                }

            }), DispatcherPriority.Normal, null);
        }

        /// <summary>
        /// Clear all ammo effects.
        /// </summary>
        public void ClearAmmoEffectsCanvas()
        {
            _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _gameCanvas.AmmoEffectsCanvas.Children.Clear();
            }), DispatcherPriority.Normal, null);
        }

        /// <summary>
        /// Draw the ammo explosion sequence.
        /// </summary>
        /// <param name="coordinates">Coordinates of the explosion</param>
        /// <param name="width">Width of the explosion effect box.</param>
        /// <param name="height">Height of the explosion effect box.</param>
        /// <param name="repeats">Repeats to run the sequence (default=10)</param>
        public void DrawAmmoExplosion(Point coordinates, double width, double height, [Optional, DefaultParameterValue(10)] int repeats)
        {
            _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _gameCanvas.AmmoEffectsCanvas.Children.Clear();

                for (int i = 0; i < _explosionImg.Length; i++)
                {
                    Canvas.SetLeft(_explosionImg[i], coordinates.X);
                    Canvas.SetTop(_explosionImg[i], coordinates.Y);
                    _gameCanvas.AmmoEffectsCanvas.Children.Add(_explosionImg[i]);
                    Thread.Sleep(1);                  
                }
            }), DispatcherPriority.Normal, null);
        }

        /// <summary>
        /// Draws the explosion over the hero ship.
        /// </summary>
        /// <param name="coordinates">Coordinates of the explosion</param>
        /// <param name="width">Width of the explosion effect box.</param>
        /// <param name="height">Height of the explosion effect box.</param>
        /// <param name="repeats">Repeats to run the sequence (default=10)</param>
        /// <returns></returns>
        public bool DrawHeroExplosion(Point coordinates, double width, double height, [Optional, DefaultParameterValue(10)] int repeats)
        {
            _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _gameCanvas.AmmoEffectsCanvas.Children.Clear();

                for (int round = 0; round < repeats; round++)
                {
                    double el_width = width;
                    double el_height = height;

                    for (int i = 0; i < 10; i++)
                    {
                        Ellipse el = new Ellipse();
                        el.Width = el_width;
                        el.Height = el_height;
                        el.Fill = (i < 5) ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.LightYellow);
                        Canvas.SetLeft(el, coordinates.X);
                        Canvas.SetTop(el, coordinates.Y);
                        _gameCanvas.AmmoEffectsCanvas.Children.Add(el);

                        el_width += 4;
                        el_height += 4;
                    }
                }
            }), DispatcherPriority.Normal, null);

            return true;
        }
    }
}
