using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Rlgl;

namespace SoftbodyTest;

class Program
{
    private const int WindowWidth = 800;
    private const int WindowHeight = 600;

    private const float DeltaTime = 1.0f / 60.0f;

    private const float K_N = 500f;
    private const float L_N = 2f;

    private const float L_D = 2.8f;
    private const float K_D = 500f;

    private const float NodeRadius = .1f;

    private static int draggedNode = -1;

    private static List<Node> _nodes = new List<Node>();
    private static List<Node> _newNodesState = new List<Node>();

    private static Texture2D _texture; 
    
    private static float[,] _texCoords = {
        {0, 0},
        {0, 1},
        {1, 1},
        {1, 0}
    };

    private static Vector2[] _nodesPositions =
    {
        new Vector2(1, 1),
        new Vector2(1, 2),
        new Vector2(2, 2),
        new Vector2(2, 1)
    };
    
    static void Main(string[] args)
    {
        Raylib.InitWindow(WindowWidth, WindowHeight, "Softbody Test");
        initPhysics();
        _texture = Raylib.LoadTexture("./dia-de-la-risa.jpg");
        Raylib.SetTargetFPS(60);
        while (!Raylib.WindowShouldClose())
        {
            // Update state
            var mP = Raylib.GetMousePosition() / 100;
            

            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                foreach(var node in _nodes)
                {
                    if (Vector2.Distance(mP, node.Position) <= NodeRadius)
                    {
                        draggedNode = _nodes.IndexOf(node);
                        break;
                    }
                }
            }

            if (Raylib.IsMouseButtonUp(MouseButton.Left))
            {
                draggedNode = -1;
            }
            
            updateNodes();
            if (draggedNode != -1)
            {
                _nodes[draggedNode].Position = mP;
            }

            if (Raylib.IsKeyDown(KeyboardKey.R))
            {
                initPhysics();
            }

            if (Raylib.IsKeyDown(KeyboardKey.V))
            {
                resetVelocities();
            }
            // Render
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);
            drawNodes();
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }

    private static void initPhysics()
    {
        _nodes = new List<Node>();
        _newNodesState = new List<Node>();
        for (int i = 0; i < 4; i++)
        {
            _nodes.Add(new Node(_nodesPositions[i], 1.0f));
        }

        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int n = 0; n < _nodes.Count; n++)
            {
                if (n != i)
                {
                    if (Math.Abs(i - n) < 1)
                    {
                        _nodes[i].Neighbours.Add(n, new Vector2(L_N, K_N));
                    }
                    else
                    {
                        _nodes[i].Neighbours.Add(n, new Vector2(L_D, K_D));
                    }
                }
                
            }
        }
    }

    private static void drawNodes()
    {
        SetTexture(_texture.Id);
        Begin(DrawMode.Quads);
        Color4f(1.0f, 1.0f, 1.0f, 1.0f);
        DisableBackfaceCulling();
        
        foreach (var node in _nodes)
        {
            var ni = _nodes.IndexOf(node);
            TexCoord2f(_texCoords[ni, 0], _texCoords[ni, 1]);
            Vertex2f(node.Position.X * 100, node.Position.Y * 100);
        }
        End();
        SetTexture(0);
        EnableBackfaceCulling();

        foreach (var node in _nodes)
        {
            Raylib.DrawCircle((int)(node.Position.X * 100), (int)(node.Position.Y * 100), 10, Color.Red);
            foreach (var neighbour in node.Neighbours)
            {
                var nodeRenderPos = node.Position * 100;
                var neighbourRenderPos = _nodes[neighbour.Key].Position * 100;
                Raylib.DrawLine((int)nodeRenderPos.X, (int)nodeRenderPos.Y, (int)neighbourRenderPos.X, (int)neighbourRenderPos.Y, Color.Green);
            }    
        }
        
    }

    private static void updateNodes()
    {
        copyList(_nodes, _newNodesState);
        for (int i = 0; i < _nodes.Count; i++)
        {
            var force = computeForce(_nodes[i]);
            var acceleration = computeAcceleration(force, _nodes[i]);
            var velocity = computeVelocity(acceleration, _nodes[i]);
            var position = computeNewPosition(velocity, acceleration, _nodes[i]);
            
            _newNodesState[i].Position = position;
            _newNodesState[i].Velocity = velocity;
            _newNodesState[i].Acceleration = acceleration;
        }

        copyList(_newNodesState, _nodes);
    }

    private static Vector2 computeAcceleration(Vector2 force, Node node)   // sum F = ma; => a = sum F / m
    {
        var acceleration = force / node.Mass;

        return acceleration;
    }

    private static Vector2 computeVelocity(Vector2 acceleration, Node node)
    {
        var velocity = node.Velocity + acceleration * DeltaTime;
        
        return velocity;
    }

    private static Vector2 computeForce(Node node)
    {
        Vector2 force = Vector2.Zero;

        foreach (var neighbour in node.Neighbours)
        {
            var dist = Vector2.Distance(node.Position, _nodes[neighbour.Key].Position);
            var forceVector = Vector2.Normalize(_nodes[neighbour.Key].Position - node.Position);    // From node TO neighbour
            var dx = dist - neighbour.Value.X;
            force += dx * forceVector * neighbour.Value.Y;  // In Newtons       // K is Y and L is X
        }
        return force;
    }

    private static Vector2 computeNewPosition(Vector2 velocity, Vector2 acceleration, Node node)
    {
        var timeSq = (float)Math.Pow(DeltaTime, 2);
        var dx = velocity.X * DeltaTime + acceleration.X * timeSq / 2; 
        var dy = velocity.Y * DeltaTime + acceleration.Y * timeSq / 2;
        
        var newPosition = new Vector2(node.Position.X + dx, node.Position.Y + dy);
        
        return newPosition;
    }

    private static void resetVelocities()
    {
        foreach (var node in _nodes)
        {
            node.Velocity = Vector2.Zero;
        }
    }

    private static void copyList(List<Node> originalNodes, List<Node> newNodes)
    {
        newNodes.Clear();
        foreach (var node in originalNodes)
        {
            newNodes.Add((Node)node.Clone());
        }
    }
}