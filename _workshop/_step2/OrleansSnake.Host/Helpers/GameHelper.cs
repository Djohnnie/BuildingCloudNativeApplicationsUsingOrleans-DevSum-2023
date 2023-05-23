using OrleansSnake.Contracts;

namespace OrleansSnake.Host.Helpers;

public class GameHelper
{
    private Orientation _orientation;

    public Orientation GetOrientation()
    {
        return _orientation;
    }

    public void SetOrientation(Orientation orientation)
    {
        _orientation = orientation;
    }
}