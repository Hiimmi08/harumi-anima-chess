public struct Neighbors
{
    public int South;
    public int North;
    public int East;
    public int West;

    public Neighbors(int south, int north, int east, int west)
    {
        this.South = south;
        this.North = north;
        this.East = east;
        this.West = west;
    }
}