namespace UI
{
    public delegate void SpaceStateChangeDelegate(SpaceState state, ushort spaceID);

    public enum SpaceState
    {
        StolenLost,
        StolenGained,
        EmptyGained,
    }
}