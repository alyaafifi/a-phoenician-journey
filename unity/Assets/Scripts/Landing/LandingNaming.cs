namespace PhoenicianJourney.Landing
{
    public enum MeshRole
    {
        /// <summary>Visible and collides.</summary>
        Solid,

        /// <summary>Visible, no collision.</summary>
        Visual,

        /// <summary>Invisible collider, such as a wall or barrier.</summary>
        Collider,
    }

    public enum StationId
    {
        Cargo,
        Dye,
        Scribe,
    }

    /// <summary>
    /// Naming convention for the Blender export, unchanged from the Babylon.js prototype:
    /// - <c>COL_</c> prefix: invisible collider.
    /// - <c>NOCOL_</c> prefix or a name containing <c>water</c>: visible, no collision.
    /// - A node named <c>SPAWN</c>: player feet position.
    /// - A name containing <c>cargo</c>, <c>dye</c>, or <c>scribe</c>: that station's footprint.
    /// - Every other mesh is visible and collides.
    /// Names are matched case-insensitively.
    /// </summary>
    public static class LandingNaming
    {
        public const string SpawnNode = "spawn";

        public static MeshRole RoleOf(string name)
        {
            var lower = name.ToLowerInvariant();
            if (lower.StartsWith("col_"))
            {
                return MeshRole.Collider;
            }

            if (lower.StartsWith("nocol_") || lower.Contains("water"))
            {
                return MeshRole.Visual;
            }

            return MeshRole.Solid;
        }

        public static bool IsSpawn(string name) => name.ToLowerInvariant() == SpawnNode;

        public static StationId? StationOf(string name)
        {
            var lower = name.ToLowerInvariant();
            if (lower.Contains("cargo"))
            {
                return StationId.Cargo;
            }

            if (lower.Contains("dye"))
            {
                return StationId.Dye;
            }

            if (lower.Contains("scribe"))
            {
                return StationId.Scribe;
            }

            return null;
        }
    }
}
