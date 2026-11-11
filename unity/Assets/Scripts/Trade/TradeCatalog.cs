using System.Collections.Generic;

namespace PhoenicianJourney.Trade
{
    // Goods, destinations, and hold positions for the Sprint 2 shipment.
    // Sources and reconstruction notes for each entry are in CARGO_RESEARCH.md.

    public enum ProductId
    {
        PurpleCloth,
        Wine,
    }

    public enum DestinationId
    {
        Kition,
        Egypt,
    }

    public enum HoldPositionId
    {
        DryLocker,
        JarBay,
    }

    public sealed class Product
    {
        public Product(ProductId id, string name, string unitSingular, string unitPlural)
        {
            Id = id;
            Name = name;
            UnitSingular = unitSingular;
            UnitPlural = unitPlural;
        }

        public ProductId Id { get; }
        public string Name { get; }
        public string UnitSingular { get; }
        public string UnitPlural { get; }

        public string UnitLabel(int quantity) => quantity == 1 ? UnitSingular : UnitPlural;
    }

    public sealed class Destination
    {
        public Destination(DestinationId id, string name, string region, string route)
        {
            Id = id;
            Name = name;
            Region = region;
            Route = route;
        }

        public DestinationId Id { get; }
        public string Name { get; }
        public string Region { get; }

        /// <summary>Proposed route from Byblos, in plain language.</summary>
        public string Route { get; }
    }

    public sealed class HoldPosition
    {
        public HoldPosition(HoldPositionId id, string name, string location, string description)
        {
            Id = id;
            Name = name;
            Location = location;
            Description = description;
        }

        public HoldPositionId Id { get; }
        public string Name { get; }
        public string Location { get; }
        public string Description { get; }
    }

    public static class TradeCatalog
    {
        public static readonly IReadOnlyDictionary<ProductId, Product> Products = new Dictionary<ProductId, Product>
        {
            [ProductId.PurpleCloth] = new Product(ProductId.PurpleCloth, "Purple cloth", "bundle", "bundles"),
            [ProductId.Wine] = new Product(ProductId.Wine, "Wine", "jar", "jars"),
        };

        public static readonly IReadOnlyDictionary<DestinationId, Destination> Destinations = new Dictionary<DestinationId, Destination>
        {
            [DestinationId.Kition] = new Destination(
                DestinationId.Kition,
                "Kition",
                "Cyprus",
                "North-west across open water to the Phoenician town on Cyprus's south-east coast, about 200 km."),
            [DestinationId.Egypt] = new Destination(
                DestinationId.Egypt,
                "Egypt",
                "Nile Delta",
                "South along the Levantine coast or straight across open water to the Nile Delta, about 450 km."),
        };

        public static readonly IReadOnlyDictionary<HoldPositionId, HoldPosition> HoldPositions = new Dictionary<HoldPositionId, HoldPosition>
        {
            [HoldPositionId.DryLocker] = new HoldPosition(
                HoldPositionId.DryLocker,
                "Dry locker",
                "stern",
                "A covered space raised on brushwood, away from bilge water."),
            [HoldPositionId.JarBay] = new HoldPosition(
                HoldPositionId.JarBay,
                "Jar bay",
                "amidships",
                "Jars stand in rows, packed with brushwood and tied with rope."),
        };

        public static readonly IReadOnlyList<DestinationId> DestinationOrder = new[] { DestinationId.Kition, DestinationId.Egypt };

        /// <summary>Stern to bow, the order the hold is drawn in.</summary>
        public static readonly IReadOnlyList<HoldPositionId> HoldOrder = new[] { HoldPositionId.DryLocker, HoldPositionId.JarBay };
    }
}
