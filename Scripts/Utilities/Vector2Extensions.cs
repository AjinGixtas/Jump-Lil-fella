using Godot;
public static class Vector2Extensions
{
    static readonly Vector2[] directions = { new(1, 0), new Vector2(1, 1).Normalized(), new(0, 1), new Vector2(-1, 1).Normalized(), new(-1, 0), new Vector2(-1, -1).Normalized(), new(0, -1), new Vector2(1, -1).Normalized() };

    ///<summary>Return a normalized vector closest to the input</summary>
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
    ///<summary>return new Vector2(vector.Y, -vector.X)</summary>
    public static Vector2 GetPerpendicularVector(Vector2 vector) {
        return new(vector.Y, -vector.X);
    }
    ///<summary>return new Vector2(-vector.Y, vector.X)</summary>
    public static Vector2 GetOppositePerpendicularVector(Vector2 vector) {
        return new(-vector.Y, vector.X);
    }
    ///<summary>Return the Euclidean distance between 2 point</summary>
    public static float GetEuclideanDistance(Vector2 A, Vector2 B) {
        return Mathf.Sqrt(Mathf.Pow(A.X - B.X, 2) + Mathf.Pow(A.X - B.X, 2));
    }
    ///<summary>Return the Euclidean squared distance between 2 point. (Skips the slow square root calculation. Useful in specific application) </summary>
    public static float GetSquaredEuclideanDistance(Vector2 A, Vector2 B) {
        return Mathf.Pow(A.X - B.X, 2) + Mathf.Pow(A.Y - B.Y, 2);
    }
    ///<summary>Return the Manhattan distance between 2 point</summary>
    public static float GetManhattanDistance(Vector2 A, Vector2 B) {
        return Mathf.Abs(A.X - B.X) + Mathf.Abs(A.Y - B.Y);
    }
    ///<summary>Return the Chebyshev distance between 2 point</summary>
    public static float GetChebyshevDistance(Vector2 A, Vector2 B) {
        return Mathf.Max(Mathf.Abs(A.X - B.X), Mathf.Abs(A.Y - B.Y));
    }
    ///<summary>Return a random normalized vector using GD.Randf(), change it to your own randomize function if needed.</summary>
    public static Vector2 GetRandomVector() {
        float angle = GD.Randf() * Mathf.Pi * 2f;
        return new(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}