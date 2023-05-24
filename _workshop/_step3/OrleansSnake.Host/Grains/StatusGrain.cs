namespace OrleansSnake.Host.Grains;

public interface IStatusGrain : IGrainWithGuidKey
{
    Task<string> GetStatus();
}

public class StatusGrain : Grain, IStatusGrain
{
    public Task<string> GetStatus()
    {
        return Task.FromResult("Hello OrleansSnake");
    }
}