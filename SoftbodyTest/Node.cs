using System.Numerics;

namespace SoftbodyTest;

public class Node : ICloneable
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Acceleration { get; set; }
    public float Mass { get; set; }
    public Dictionary<int, Vector2> Neighbours { get; set; } = new Dictionary<int, Vector2>();
    public Node(Vector2 position, float mass)
    {
        Position = position;
        Mass = mass;
    }

    public object Clone()
    {
        var res = new Node(Position, Mass);
        res.Neighbours = Neighbours.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        res.Acceleration = Acceleration;
        res.Velocity = Velocity;
        res.Position = Position;
        
        return res;
    }
}