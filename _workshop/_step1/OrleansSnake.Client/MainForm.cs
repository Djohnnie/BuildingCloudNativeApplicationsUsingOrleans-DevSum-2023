using OrleansSnake.Client.Enums;
using OrleansSnake.Client.Renderers;
using OrleansSnake.Contracts;
using Orientation = OrleansSnake.Contracts.Orientation;
using Timer = System.Windows.Forms.Timer;

namespace OrleansSnake.Client
{
    internal partial class MainForm : Form
    {
        private static Color ClearColor = Color.FromArgb(63, 50, 102);

        private readonly Timer _timer = new Timer();
        private readonly Random _random = new();
        private bool _closing;

        private SnakeGameState _snakeGameState = SnakeGameState.MainMenu;
        public SnakeGameState SnakeGameState
        {
            get => _snakeGameState;
            set
            {
                _snakeGameState = value;
                RefreshGameState();
            }
        }

        private GameState _gameState;
        public GameState GameState
        {
            get => _gameState;
            set
            {
                _gameState = value;
                Invalidate();
            }
        }

        public MainForm()
        {
            _timer.Interval = 250;
            _timer.Tick += Timer_Tick;
            _timer.Enabled = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            GameState = new GameState
            {
                Width = 24,
                Height = 14,
                Players = new List<PlayerState>
                {
                    new PlayerState
                    {
                        PlayerName = "Player 1",
                        IsReady = true,
                        Snake = new Snake
                        {
                            Orientation = Orientation.East,
                            Coordinates = new List<Coordinates>
                            {
                                new Coordinates { X = 2, Y = 2 },
                                new Coordinates { X = 3, Y = 2 },
                                new Coordinates { X = 4, Y = 2 },
                            }
                        }
                    }
                },
                Food = new Food
                {
                    Bites = new List<Bite>
                    {
                        new Bite { X = 5, Y = 5 }
                    }
                }
            };

            InitializeComponent();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (GameState != null)
            {
                foreach (var playerState in GameState.Players)
                {

                    var snake = playerState.Snake;

                    Func<Coordinates, Coordinates> moveFunc = coordinates => coordinates;

                    switch (snake.Orientation)
                    {
                        case Orientation.North:
                            moveFunc = coordinates =>
                            {
                                var newY = coordinates.Y - 1 < 0 ? GameState.Height - 1 : coordinates.Y - 1;
                                return coordinates with { Y = newY };
                            };

                            break;
                        case Orientation.East:
                            moveFunc = coordinates =>
                            {
                                var newX = coordinates.X + 1 >= GameState.Width ? 0 : coordinates.X + 1;
                                return coordinates with { X = newX };
                            };

                            break;
                        case Orientation.South:
                            moveFunc = coordinates =>
                            {
                                var newY = coordinates.Y + 1 >= GameState.Height ? 0 : coordinates.Y + 1;
                                return coordinates with { Y = newY };
                            };

                            break;
                        case Orientation.West:
                            moveFunc = coordinates =>
                            {
                                var newX = coordinates.X - 1 < 0 ? GameState.Width - 1 : coordinates.X - 1;
                                return coordinates with { X = newX };
                            };

                            break;
                    }

                    var newCoordinates = new List<Coordinates>(snake.Coordinates.Count);

                    for (int i = 0; i < snake.Coordinates.Count; i++)
                    {
                        if (i == 0)
                        {
                            newCoordinates.Add(moveFunc(snake.Coordinates[0]));
                        }
                        else
                        {
                            newCoordinates.Add(snake.Coordinates[i - 1]);
                        }
                    }

                    playerState.Snake = playerState.Snake with { Coordinates = newCoordinates };

                }
            }

            Invalidate();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SnakeGameState = SnakeGameState.Game;
        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_closing)
            {
                _closing = true;

                e.Cancel = true;

                Close();
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (SnakeGameState == SnakeGameState.Game && GameState != null)
            {
                e.Graphics.RenderWorld(ClientRectangle, GameState);
                e.Graphics.RenderSnakes(ClientRectangle, GameState);
                e.Graphics.RenderFood(ClientRectangle, GameState);
            }
            else
            {
                e.Graphics.Clear(ClearColor);
            }
        }

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (SnakeGameState == SnakeGameState.Game)
            {
                var currentPlayerState = _gameState.Players.SingleOrDefault(x => x.PlayerName == "Player 1");
                var currentOrientation = currentPlayerState != null ? currentPlayerState.Snake.Orientation : Orientation.North;
                var newOrientation = currentOrientation;

                switch (e.KeyCode)
                {
                    case Keys.Z:
                    case Keys.Up:
                        if (currentOrientation != Orientation.South)
                        {
                            newOrientation = Orientation.North;
                        }
                        break;
                    case Keys.D:
                    case Keys.Right:
                        if (currentOrientation != Orientation.West)
                        {
                            newOrientation = Orientation.East;
                        }
                        break;
                    case Keys.S:
                    case Keys.Down:
                        if (currentOrientation != Orientation.North)
                        {
                            newOrientation = Orientation.South;
                        }
                        break;
                    case Keys.Q:
                    case Keys.Left:
                        if (currentOrientation != Orientation.East)
                        {
                            newOrientation = Orientation.West;
                        }
                        break;
                }

                currentPlayerState.Snake = currentPlayerState.Snake with { Orientation = newOrientation };
            }
        }

        private void newGameButton_Click(object sender, EventArgs e)
        {
            SnakeGameState = SnakeGameState.NewGame;
        }

        private void joinGameButton_Click(object sender, EventArgs e)
        {
            SnakeGameState = SnakeGameState.JoinGame;
        }

        private async void lobbyButton_Click(object sender, EventArgs e)
        {
            var gameCode = string.Empty;



            SnakeGameState = SnakeGameState.GameLobby;
        }

        private async void readyButton_Click(object sender, EventArgs e)
        {
            readyButton.Visible = false;
        }

        private void RefreshGameState()
        {
            mainMenuPanel.Visible = SnakeGameState == SnakeGameState.MainMenu;
            joinPanel.Visible = SnakeGameState == SnakeGameState.NewGame || SnakeGameState == SnakeGameState.JoinGame;
            gameCodeTextBox.Visible = gameCodeLabel.Visible = SnakeGameState == SnakeGameState.JoinGame;
            lobbyButton.Text = SnakeGameState == SnakeGameState.NewGame ? "CREATE GAME" : "JOIN GAME";
            lobbyPanel.Visible = SnakeGameState == SnakeGameState.GameLobby;
        }
    }
}