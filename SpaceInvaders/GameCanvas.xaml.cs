using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpaceInvaders
{
    /// <summary>
    /// Interaction logic for GameCanvas.xaml
    /// </summary>
    public partial class GameCanvas : Page
    {
        private Enemy[,] Enemies = new Enemy[5,10];
        private Cover[,] Covers = new Cover[2, 6];
        private Hero Player = new Hero();
        private GameLoop gameLoop;
        Thread gameLoopThread;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameCanvas()
        {
            InitializeComponent();
            gameLoop = new GameLoop(this);
            gameLoopThread = new Thread(gameLoop.DoWork);
            gameLoopThread.SetApartmentState(ApartmentState.STA);
            gameLoopThread.Start();
        }

        /// <summary>
        /// Draws enemies inintially.
        /// </summary>
        public void DrawEnemies()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (Enemies[i, j].IsAlive == true)
                    {
                        Canvas.SetLeft(Enemies[i, j].GetImage(), Enemies[i, j].Coordinates.X);
                        Canvas.SetTop(Enemies[i, j].GetImage(), Enemies[i, j].Coordinates.Y);
                        EnemyCanvas.Children.Add(Enemies[i, j].GetImage());
                    }
                }
            }
        }

        /// <summary>
        /// Handles keypresses.
        /// </summary>
        private void GameCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.Right) && Keyboard.IsKeyDown(Key.Space))
            {
                gameLoop.SetHeroContinuousMovePx(5);
                gameLoop.HeroFireAmmo();
            }
            else if(Keyboard.IsKeyDown(Key.Left) && Keyboard.IsKeyDown(Key.Space))
            {
                gameLoop.SetHeroContinuousMovePx(-5);
                gameLoop.HeroFireAmmo();
            }
            else if(Keyboard.IsKeyDown(Key.Right))
            {
                gameLoop.SetHeroContinuousMovePx(5);
            }
            else if(Keyboard.IsKeyDown(Key.Left))
            {
                gameLoop.SetHeroContinuousMovePx(-5);
            }
            else if(Keyboard.IsKeyDown(Key.Space))
            {
                gameLoop.HeroFireAmmo();
            }
            else if(Keyboard.IsKeyDown(Key.P))
            {
                gameLoop.ToggleGamePause();
            }
        }

        /// <summary>
        /// Handles key up -events.
        /// </summary>
        private void GameCanvas_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                gameLoop.SetHeroContinuousMovePx(0);

                if (Keyboard.IsKeyDown(Key.Right))
                {
                    gameLoop.SetHeroContinuousMovePx(5);
                }
                else if (Keyboard.IsKeyDown(Key.Left))
                {
                    gameLoop.SetHeroContinuousMovePx(-5);
                }
            }
        }

        /// <summary>
        /// Invoked when the canvas is loaded and sets the 
        /// event handlers for listening keypresses.
        /// </summary>
        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.PreviewKeyDown += GameCanvas_PreviewKeyDown;
            window.PreviewKeyUp += GameCanvas_PreviewKeyUp;
        }
    }
}
