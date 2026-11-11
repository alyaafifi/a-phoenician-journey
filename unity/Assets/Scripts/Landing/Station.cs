using System.Collections.Generic;
using UnityEngine;

namespace PhoenicianJourney.Landing
{
    /// <summary>
    /// A station footprint, such as the cargo area or dye stall. The scene builder adds one
    /// to each mesh whose name contains <c>cargo</c>, <c>dye</c>, or <c>scribe</c>.
    /// </summary>
    public sealed class Station : MonoBehaviour
    {
        /// <summary>How close, in metres, the player must stand to a footprint's edge to use it.</summary>
        public const float Reach = 1.5f;

        private static readonly List<Station> active = new List<Station>();

        [SerializeField] private StationId id;

        public StationId Id => id;

        public static IReadOnlyList<Station> Active => active;

        public void Configure(StationId stationId) => id = stationId;

        private void OnEnable() => active.Add(this);

        private void OnDisable() => active.Remove(this);

        private Bounds? footprint;

        /// <summary>
        /// World-space footprint, merged across this object's renderers and colliders.
        /// Stations do not move, so it is measured once.
        /// </summary>
        public Bounds Footprint => footprint ??= MeasureFootprint();

        private Bounds MeasureFootprint()
        {
            var hasBounds = false;
            var bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                bounds = Encapsulate(bounds, renderer.bounds, ref hasBounds);
            }

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                bounds = Encapsulate(bounds, collider.bounds, ref hasBounds);
            }

            return bounds;
        }

        private static Bounds Encapsulate(Bounds current, Bounds next, ref bool hasBounds)
        {
            if (!hasBounds)
            {
                hasBounds = true;
                return next;
            }

            current.Encapsulate(next);
            return current;
        }

        /// <summary>Distance in X and Z from a point to the footprint; zero when inside it.</summary>
        public float DistanceTo(Vector3 point)
        {
            var bounds = Footprint;
            var dx = Mathf.Max(bounds.min.x - point.x, 0f, point.x - bounds.max.x);
            var dz = Mathf.Max(bounds.min.z - point.z, 0f, point.z - bounds.max.z);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
