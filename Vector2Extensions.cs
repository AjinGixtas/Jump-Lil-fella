using Godot;
public static class Vector2Extensions
{
    static readonly Vector2[] directions = { new(1, 0), new Vector2(1, 1).Normalized(), new(0, 1), new Vector2(-1, 1).Normalized(), new(-1, 0), new Vector2(-1, -1).Normalized(), new(0, -1), new Vector2(1, -1).Normalized() };

    public static Vector2 SnapTo8Directions(Vector2 vector)
    {
        Vector2 snappedDirection = Vector2.Zero;
        float maxDot = float.NegativeInfinity;
        foreach (var direction in directions)
        {
            float dot = vector.Dot(direction);
            if (dot > maxDot)
            {
                maxDot = dot;
                snappedDirection = direction;
            }
        }
        return snappedDirection;
    }
    public static Vector2 GetPerpendicularVector(Vector2 vector) {
        return new(vector.Y, -vector.X);
    }
}