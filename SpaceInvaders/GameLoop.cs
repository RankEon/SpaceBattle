using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace SpaceInvaders
{
    /// <summary>
    /// Enumeration for determining enemy swarm direction
    /// </summary>
    public enum enemySwarmDirection
    {
        LEFT = 1,
        RIGHT = 2
    }

    /// <summary>
    /// Gameloop
    /// </summary>
    class GameLoop
    {
        // Constants
        const int ENEMY_ROWS = 5;
        const int ENEMY_COLS = 10;
        const int ENEMY_SIMULTANEOUS_AMMO_CAP = 3;

        // Main Canvas
        private GameCanvas _gameCanvas;

        // Graphics
        private Graphics _graphics;

        // Game Objects
        private Enemy[,] Enemies = new Enemy[ENEMY_ROWS, ENEMY_COLS];
        private enemySwarmDirection _enemySwarmDirection = enemySwarmDirection.RIGHT;
        private Cover[,] Covers = new Cover[2, 6];
        private Hero Player;
        private List<Ammo> HeroAmmo = new List<Ammo>();
        private List<Ammo> EnemyAmmo = new List<Ammo>();
        private double HeroContinuousMovePx = 0;
        private int HeroLives = 3;

        // Threading related
        private volatile bool _gameRunning;
        private int _updateSpeed = 2; // ms (avg. 55 fps).
        private bool _gamePaused = false;

        // Randomizer
        Random Randomizer = new Random();

        // EnemyAmmoData
        private int enemyAmmosFired = 0;

        // Score (todo/improvement idea: might be better if this would be bind to textbox instead)
        private int score = 0;
        private int _enemiesDestroyed = 0;
        private int _difficultyLevel = 0;

        // Explosion effects related.
        Thread effectLoopThread;
        protected BitmapImage[] _explosionImgBitmap = new BitmapImage[6];
        private Image[] _explosionImg = new Image[6];

        /// <summary>
        /// GameLoop constructor
        /// </summary>
        /// <param name="canvas">Canvas -object</param>
        public GameLoop(GameCanvas canvas)
        {
            for (int i = 0; i < _explosionImg.Length; i++)
            {
                _explosionImgBitmap[i] = new BitmapImage(new Uri($"Resources\\explosion_{i + 1}.png", UriKind.Relative));
                _explosionImg[i] = new Image();
                _explosionImg[i].Source = _explosionImgBitmap[i];
                _explosionImg[i].Width = _explosionImgBitmap[i].Width;
                _explosionImg[i].Height = _explosionImgBitmap[i].Height;
            }

            score = 0;
            _gameCanvas = canvas;
            _graphics = new Graphics(this);
            SetEnemies();

            Player = new Hero();

            _graphics.DrawEnemies();
            _graphics.DrawHero();

            _enemiesDestroyed = 0;
            _difficultyLevel = 0;
        }

        /// <summary>
        /// Moves the hero -object and controls the boundaries
        /// </summary>
        /// <param name="amount">Amount of pixels to move</param>
        public void MoveHero(double amount)
        {
            double currentX = Player.Coordinates.X;
            double currentY = Player.Coordinates.Y;

            if(currentX > -5 && amount < 0)
            {
                Player.Coordinates = new Point(currentX + amount, currentY);
                _graphics.DrawHero();
            }
            else if(currentX < 935 && amount > 0)
            {
                Player.Coordinates = new Point(currentX + amount, currentY);
                _graphics.DrawHero();
            }
        }

        public void SetHeroContinuousMovePx(double amount)
        {
            HeroContinuousMovePx = amount;
        }

        public void ToggleGamePause()
        {
            _gamePaused = !_gamePaused;

            if(_gamePaused)
            {
                ShowMessage("P A U S E D . . .");
            }
            else
            {
                ShowMessage("");
            }
        }

        public void ShowMessage(string message)
        {
            _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _gameCanvas.tbMessage.Text = message;
            }), DispatcherPriority.Normal, null);
        }

        public void HeroFireAmmo()
        {
            HeroAmmo.Add(new Ammo(direction.UP, new Point(Player.Coordinates.X + 23, Player.Coordinates.Y - 31)));
        }

        public Enemy[,] GetEnemyArr()
        {
            return Enemies;
        }

        public Hero GetHero()
        {
            return Player;
        }

        public void SetEnemyArr(Enemy[,] enemies)
        {
            Enemies = enemies;
        }

        public GameCanvas GetCanvas()
        {
            return _gameCanvas;
        }

        public List<Ammo> GetHeroAmmo()
        {
            return HeroAmmo;
        }

        public List<Ammo> GetEnemyAmmo()
        {
            return EnemyAmmo;
        }

        public async void DoWork()
        {
            _gameRunning = true;

            while(_gameRunning)
            {

                double enemyY = FindLowestEnemyYCoordinate();

                if (HeroLives > 0 && _enemiesDestroyed == 50)
                {
                    ReviveEnemies();
                    _difficultyLevel += 2;
                }
                else if(HeroLives <= 0 || enemyY > 460)
                {
                    await _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        _gameCanvas.tbMessage.Text = " :-( G A M E  O V E R :-( ";
                    }), DispatcherPriority.Normal, null);
                    _gameRunning = false;

                    await Task.Delay(5000);
                }

                if (!_gamePaused)
                {
                    MoveHero(HeroContinuousMovePx);
                    _graphics.DrawEnemies();
                    _graphics.DrawHeroAmmo();
                    _graphics.DrawEnemyAmmo();

                    // Handle player ammo
                    if (HeroAmmo.Any())
                    {
                        List<int> removeBullets = new List<int>();

                        // Loop through active ammunition
                        Parallel.For(0, HeroAmmo.Count, i =>
                        {
                            if (HeroAmmo[i].Coordinates.Y - 2 < 0)
                            {
                                removeBullets.Add(i);
                            }
                            else
                            {
                                HeroAmmo[i].Move(6);

                                // Check if ammo was hit to enemy (we get boolean value and enemy coordinates).
                                var result = CheckAmmoHit(HeroAmmo[i].Coordinates);

                                // If ammo was hit to an enemy, handle effects
                                if (result.Item1 == true)
                                {
                                    _enemiesDestroyed++;

                                    // Start explosion effects thread
                                    ExplosionEffectsThread expThread = new ExplosionEffectsThread(result.Item2, 40, 40, _gameCanvas, _explosionImg, 1);
                                    effectLoopThread = new Thread(expThread.explosionEffectsDoWork);
                                    effectLoopThread.SetApartmentState(ApartmentState.STA);
                                    effectLoopThread.Start();

                                    // Add this bullet to removable list.
                                    removeBullets.Add(i);

                                    // Update player score on screen.
                                    _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        score = Convert.ToInt32(_gameCanvas.tbScore.Text.ToString()) + 1;
                                        _gameCanvas.tbScore.Text = score.ToString();
                                    }), DispatcherPriority.Normal, null);
                                }
                            }
                        });

                        for (int i = 0; i < removeBullets.Count; i++)
                        {
                            if (i < HeroAmmo.Count)
                                HeroAmmo.RemoveAt(removeBullets[i]);
                        }
                    }

                    // Handle enemy ammo
                    if (EnemyAmmo.Any())
                    {
                        List<int> removeBullets = new List<int>();

                        // Loop through active ammunition
                        Parallel.For(0, EnemyAmmo.Count, i =>
                        {
                            if (EnemyAmmo[i].Coordinates.Y - 2 >= 600)
                            {
                                removeBullets.Add(i);
                            }
                            else if (EnemyAmmo[i].Coordinates.Y - 2 >= Player.Coordinates.Y &&
                                    EnemyAmmo[i].Coordinates.X > Player.Coordinates.X + 8 &&
                                    EnemyAmmo[i].Coordinates.X < Player.Coordinates.X + 52)
                            {
                                // Update player live statistics on screen.
                                _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    HeroLives = Convert.ToInt32(_gameCanvas.tbPlayerLives.Text.ToString()) - 1;
                                    _gameCanvas.tbPlayerLives.Text = HeroLives.ToString();
                                }), DispatcherPriority.Normal, null);

                                // Start explosion effects thread
                                ExplosionEffectsThread heroExpThread = new ExplosionEffectsThread(Player.Coordinates, 40, 60, _gameCanvas, _explosionImg, 1);
                                effectLoopThread = new Thread(heroExpThread.explosionEffectsDoWork);
                                effectLoopThread.SetApartmentState(ApartmentState.STA);
                                effectLoopThread.Start();

                                // Add this bullet to removable list.
                                removeBullets.Add(i);
                            }
                            else
                            {
                                EnemyAmmo[i].Move(EnemyAmmo[i].AmmoSpeed);
                            }
                        });

                        // Remove bullets from the active list
                        for (int i = 0; i < removeBullets.Count; i++)
                        {
                            EnemyAmmo.RemoveAt(removeBullets[i]);
                            enemyAmmosFired--;
                        }
                    }

                    // Move enemies
                    MoveEnemies();

                    // Fire enemy ammo if simultaneous ammo cap is not reached.
                    if (enemyAmmosFired < ENEMY_SIMULTANEOUS_AMMO_CAP)
                    {
                        await _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
                         {
                             EnemyFireAmmo();
                         }), DispatcherPriority.Normal, null);
                    }
                } // if (!_gamePaused)

                // Small delay to the while loop
                await Task.Delay(_updateSpeed);

                // Clear ammo effects
                if (!_gamePaused)
                {
                    _graphics.ClearAmmoEffectsCanvas();
                }
            }

            // After the game is finished, go back to the main menu.
            await _gameCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                NavigationService nav = NavigationService.GetNavigationService(_gameCanvas);
                nav.Navigate(new MainMenu());
            }), DispatcherPriority.Normal, null);
        }

        /// <summary>
        /// Find the lowest enemy Y -coordinate from the active 
        /// enemies.
        /// </summary>
        /// <returns>Lowest Y -coordinate</returns>
        private double FindLowestEnemyYCoordinate()
        {
            double enemyYCoord = 0;

            for (int row = 0; row < ENEMY_ROWS; row++)
            {
                for (int col = 0; col < ENEMY_COLS; col++)
                {
                    if(Enemies[row, col].IsAlive == true && Enemies[row, col].Coordinates.Y > enemyYCoord)
                    {
                        enemyYCoord = Enemies[row, col].Coordinates.Y;
                    }
                }
            }

            return enemyYCoord;
        }

        /// <summary>
        /// Revives enemies, after all enemies have been destroyed and next, more difficult,
        /// round is started.
        /// </summary>
        private void ReviveEnemies()
        {
            double x = 0;
            double y = 50;

            for (int row = 0; row < ENEMY_ROWS; row++)
            {
                x = 50;

                for (int col = 0; col < ENEMY_COLS; col++)
                {
                    Enemies[row, col].IsAlive = true;
                    Enemies[row, col].Coordinates = new Point(x, y);
                    x += 20 + 40;
                }

                y += 20 + 40;
            }

            _enemiesDestroyed = 0;
        }

        /// <summary>
        /// Fire enemy ammo. Randomly select an enemy from the available enemies and
        /// fire ammo, if the ammo cap is not reached.
        /// </summary>
        private void EnemyFireAmmo()
        {
            if(enemyAmmosFired < ENEMY_SIMULTANEOUS_AMMO_CAP)
            {
                List<Point> enemyCoordinates = new List<Point>();

                for (int row = 0; row < ENEMY_ROWS; row++)
                {
                    for (int col = 0; col < ENEMY_COLS; col++)
                    {
                        if(Enemies[row, col].IsAlive)
                        {
                            enemyCoordinates.Add(new Point(Enemies[row, col].Coordinates.X + 20, Enemies[row, col].Coordinates.Y + 40));
                        }
                    }
                }

                int coordIndex = 0;

                if (enemyCoordinates.Any())
                {
                    coordIndex = Randomizer.Next(0, enemyCoordinates.Count);
                    EnemyAmmo.Add(new Ammo(direction.DOWN, new Point(enemyCoordinates[coordIndex].X, enemyCoordinates[coordIndex].Y)));
                    EnemyAmmo.Last().AmmoSpeed = Randomizer.Next(1, 6);
                    enemyAmmosFired++;
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Checks whether player ammo has hit the enemy.
        /// </summary>
        /// <param name="ammoCoordinates">Current coordinates of the ammunition</param>
        /// <returns>Tuple, containing whether ammo has hit and the coordinates, where it hit.</returns>
        private Tuple<bool, Point> CheckAmmoHit(Point ammoCoordinates)
        {
            for (int row = ENEMY_ROWS - 1; row >= 0; row--)
            {
                for (int col = 0; col < ENEMY_COLS; col++)
                {
                    if (Enemies[row, col].IsAlive == true &&
                       ammoCoordinates.X >= Enemies[row, col].Coordinates.X &&
                       ammoCoordinates.X <= Enemies[row, col].Coordinates.X + 45)
                    {
                        if(ammoCoordinates.Y <= Enemies[row, col].Coordinates.Y + 20)
                        {
                            Enemies[row, col].IsAlive = false;
                            return new Tuple<bool, Point>(true, Enemies[row, col].Coordinates);
                        }
                    }
                }
            }

            return new Tuple<bool, Point>(false, new Point(0, 0));
        }

        /// <summary>
        /// Moves the enemies on the canvas.
        /// </summary>
        private void MoveEnemies()
        {
            int GetMovementSpeed()
            {
                int speed = 1;

                // Set the movement speed, based on the destroyed
                // enemies (and difficulty level).
                switch (_enemiesDestroyed)
                {
                    case int n when (n <= 50 && n > 40):
                        speed = 6 + _difficultyLevel;
                        break;
                    case int n when (n <= 40 && n > 30):
                        speed = 5 + _difficultyLevel;
                        break;
                    case int n when (n <= 30 && n > 20):
                        speed = 4 + _difficultyLevel;
                        break;
                    case int n when (n <= 20 && n > 10):
                        speed = 3 + _difficultyLevel;
                        break;
                    case int n when (n <= 20 && n > 10):
                        speed = 2 + _difficultyLevel;
                        break;
                    case int n when (n <= 10):
                        speed = 1 + _difficultyLevel;
                        break;
                    default:
                        speed = 1 + _difficultyLevel;
                        break;
                }

                return speed;
            }

            int movePx = GetMovementSpeed();
            int movePy = 0;
            double maxX = 0;
            double minX = 1000;

            // Find maximum X- and Y -coordinates from the active enemy
            // swarm.
            for (int rows = 0; rows < ENEMY_ROWS; rows++)
            {
                for (int cols = 0; cols < ENEMY_COLS; cols++)
                {
                    if(Enemies[rows, cols].IsAlive == true)
                    {
                        if (Enemies[rows, cols].Coordinates.X > maxX)
                        {
                            maxX = Enemies[rows, cols].Coordinates.X;
                        }
                        else if (Enemies[rows, cols].Coordinates.X < minX)
                        {
                            minX = Enemies[rows, cols].Coordinates.X;
                        }
                    }
                }
            }

            // Update the new X-coordinate and if we have reached the left/right boundary
            // of the canvas, also move the swarm downwards on the Y-axle.
            if (_enemySwarmDirection == enemySwarmDirection.RIGHT)
            {
                if(maxX >= 940)
                {
                    movePx = movePx * (-1);
                    movePy = 10;
                    _enemySwarmDirection = enemySwarmDirection.LEFT;
                }
            }
            else if (_enemySwarmDirection == enemySwarmDirection.LEFT)
            {
                if (minX <= 0)
                {
                    movePy = 10;
                    _enemySwarmDirection = enemySwarmDirection.RIGHT;
                }
                else
                {
                    movePx = movePx * (-1);
                }
            }

            // Update enemy coordinates to the enemy swarm objects.
            Parallel.For(0, ENEMY_ROWS, row =>
            {
                Parallel.For(0, ENEMY_COLS, col =>
                {
                    Enemies[row, col].MoveX(movePx);
                    Enemies[row, col].MoveY(movePy);
                });                
            });
        }

        /// <summary>
        /// Sets the initial placement of the enemy swarm on the canvas and loads
        /// the ship images.
        /// </summary>
        public void SetEnemies()
        {
            string imagePath = "";
            double x = 0;
            double y = 50;

            for (int i = 0; i < 5; i++)
            {
                x = 50;

                for (int j = 0; j < 10; j++)
                {
                    Enemies[i, j] = new Enemy(new Point(x, y));

                    switch (i)
                    {
                        case 0:
                            {
                                imagePath = @"Resources\Enemy_Hi.png";
                                break;
                            }
                        case 1:
                        case 2:
                            {
                                imagePath = @"Resources\Enemy_Mid.png";
                                break;
                            }
                        case 3:
                        case 4:
                            {
                                imagePath = @"Resources\Enemy_Low.png";
                                break;
                            }

                        default:
                            break;
                    }

                    Enemies[i, j].SetImage(imagePath);

                    x += 20 + 40;
                }

                y += 20 + 40;
                //x = 0;
            }
        }
    }
}
