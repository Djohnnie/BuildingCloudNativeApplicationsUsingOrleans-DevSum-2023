using OrleansSnake.Host.Hubs;
using OrleansSnake.Contracts;
using System.Diagnostics;
using OrleansSnake.Host.Helpers;

namespace OrleansSnake.Host.Workers;

public class TickerWorker : BackgroundService
{
    private readonly TickerHub _tickerHub;
    private readonly GameHelper _gameHelper;
    private readonly GameState _gameState;

    public TickerWorker(
        TickerHub tickerHub,
        GameHelper gameHelper)
    {
        _tickerHub = tickerHub;
        _gameHelper = gameHelper;

        _gameState = new GameState
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

        _gameHelper.SetOrientation(Orientation.East);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000);

        while (!stoppingToken.IsCancellationRequested)
        {
            var stopWatch = Stopwatch.StartNew();

            foreach (var playerState in _gameState.Players)
            {
                var snake = playerState.Snake;

                Func<Coordinates, Coordinates> moveFunc = coordinates => coordinates;

                var orientation = _gameHelper.GetOrientation();

                switch (orientation)
                {
                    case Orientation.North:
                        moveFunc = coordinates =>
                        {
                            var newY = coordinates.Y - 1 < 0 ? _gameState.Height - 1 : coordinates.Y - 1;
                            return coordinates with { Y = newY };
                        };

                        break;
                    case Orientation.East:
                        moveFunc = coordinates =>
                        {
                            var newX = coordinates.X + 1 >= _gameState.Width ? 0 : coordinates.X + 1;
                            return coordinates with { X = newX };
                        };

                        break;
                    case Orientation.South:
                        moveFunc = coordinates =>
                        {
                            var newY = coordinates.Y + 1 >= _gameState.Height ? 0 : coordinates.Y + 1;
                            return coordinates with { Y = newY };
                        };

                        break;
                    case Orientation.West:
                        moveFunc = coordinates =>
                        {
                            var newX = coordinates.X - 1 < 0 ? _gameState.Width - 1 : coordinates.X - 1;
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

                playerState.Snake = playerState.Snake with { Coordinates = newCoordinates, Orientation = orientation };

            }

            await _tickerHub.SendGameState(_gameState);

            stopWatch.Stop();
            await Task.Delay(Math.Max(0, 250 - (int)stopWatch.ElapsedMilliseconds), stoppingToken);
        }
    }
}