namespace IdleDefense.Core.Math
{
    /// <summary>
    /// Plain 3D position so Core events do not depend on UnityEngine.Vector3.
    /// </summary>
    public readonly struct Position3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Position3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
