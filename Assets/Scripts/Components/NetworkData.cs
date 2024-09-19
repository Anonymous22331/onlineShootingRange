using System.Numerics;

public struct NetworkData
{
    public int Id;
    public Vector3 Position;
    public Quaternion Rotation;
}

public struct EnemyNetworkData
{
    public int Health;
    public int Id;
    public Vector3 Position;
    public int Type;
}
