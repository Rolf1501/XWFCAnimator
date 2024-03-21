namespace XWFC
{
    public record Relation
    {
        public int Other { get; }
        public float Weight { get; }
        public int[] Rotations { get; } = { 0 };

        #nullable enable
        public Relation(int other, int[]? rotations, float weight = 1)
        {
            Other = other;
            Rotations = rotations ?? new[] { 0 };
            Weight = weight;
        }
    }
}