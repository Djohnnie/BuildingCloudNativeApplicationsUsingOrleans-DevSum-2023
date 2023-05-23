using Microsoft.AspNetCore.SignalR;
using OrleansSnake.Contracts;
using OrleansSnake.Host.Helpers;
using GameState = OrleansSnake.Contracts.GameState;

namespace OrleansSnake.Host.Hubs;

public class TickerHub : Hub
{
    private readonly GameHelper _gameHelper;

    public TickerHub(GameHelper gameHelper)
    {
        _gameHelper = gameHelper;
    }

    public async Task SendGameState(GameState gameState)
    {
        if (Clients != null)
        {
            await Clients.All.SendAsync("ReceiveGameState", gameState);
        }
    }

    public async Task Turn(Orientation orientation)
    {
        _gameHelper.SetOrientation(orientation);
    }
}