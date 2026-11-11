using System.Collections.Generic;

namespace PhoenicianJourney.Trade
{
    /// <summary>One line of the shipping letter. Each line carries its own quantity, route, and stowage.</summary>
    public sealed class OrderLine
    {
        public OrderLine(int line, ProductId product, int quantity, DestinationId destination, HoldPositionId stowage, string text)
        {
            Line = line;
            Product = product;
            Quantity = quantity;
            Destination = destination;
            Stowage = stowage;
            Text = text;
        }

        public int Line { get; }
        public ProductId Product { get; }
        public int Quantity { get; }
        public DestinationId Destination { get; }
        public HoldPositionId Stowage { get; }

        /// <summary>Plain-language wording shown in the letter.</summary>
        public string Text { get; }
    }

    /// <summary>What the player has chosen for one product on the cargo screen.</summary>
    public sealed class LinePlan
    {
        public int Quantity;
        public DestinationId? Destination;
        public HoldPositionId? Stowage;
    }

    public enum ProblemField
    {
        Availability,
        Quantity,
        Destination,
        Stowage,
    }

    /// <summary>The first thing wrong with a line, so the dockworker can point the player back to it.</summary>
    public readonly struct ShipmentProblem
    {
        public ShipmentProblem(int line, ProductId product, ProblemField field, bool missing)
        {
            Line = line;
            Product = product;
            Field = field;
            Missing = missing;
        }

        public int Line { get; }
        public ProductId Product { get; }
        public ProblemField Field { get; }

        /// <summary>True when the player has not chosen a value yet, as opposed to choosing a wrong one.</summary>
        public bool Missing { get; }

        public override string ToString() => $"line {Line} {Product} {Field} missing={Missing}";
    }

    public static class Shipment
    {
        /// <summary>
        /// The sample order for Sprint 2: one letter, two goods, two destinations.
        /// The routes are reconstructed; see CARGO_RESEARCH.md.
        /// </summary>
        public static readonly IReadOnlyList<OrderLine> SampleOrder = new[]
        {
            new OrderLine(
                1,
                ProductId.PurpleCloth,
                2,
                DestinationId.Kition,
                HoldPositionId.DryLocker,
                "Send two bundles of purple cloth to Kition on Cyprus. Keep them in the dry locker at the stern."),
            new OrderLine(
                2,
                ProductId.Wine,
                3,
                DestinationId.Egypt,
                HoldPositionId.JarBay,
                "Send three jars of wine to Egypt. Stand them in the jar bay amidships."),
        };

        public static Dictionary<ProductId, LinePlan> EmptyPlan() => new Dictionary<ProductId, LinePlan>
        {
            [ProductId.PurpleCloth] = new LinePlan(),
            [ProductId.Wine] = new LinePlan(),
        };

        /// <summary>
        /// Checks the plan line by line, in letter order. Within a line the checks run from
        /// availability to quantity, destination, and stowage, so the dockworker raises one
        /// clear problem at a time.
        /// </summary>
        public static List<ShipmentProblem> Check(
            IReadOnlyList<OrderLine> order,
            IReadOnlyDictionary<ProductId, LinePlan> plan,
            IReadOnlyDictionary<ProductId, int> available)
        {
            var problems = new List<ShipmentProblem>();
            foreach (var line in order)
            {
                var chosen = plan[line.Product];
                var stock = available[line.Product];
                if (stock < line.Quantity)
                {
                    problems.Add(new ShipmentProblem(line.Line, line.Product, ProblemField.Availability, stock == 0));
                }
                else if (chosen.Quantity != line.Quantity)
                {
                    problems.Add(new ShipmentProblem(line.Line, line.Product, ProblemField.Quantity, chosen.Quantity == 0));
                }
                else if (chosen.Destination != line.Destination)
                {
                    problems.Add(new ShipmentProblem(line.Line, line.Product, ProblemField.Destination, chosen.Destination == null));
                }
                else if (chosen.Stowage != line.Stowage)
                {
                    problems.Add(new ShipmentProblem(line.Line, line.Product, ProblemField.Stowage, chosen.Stowage == null));
                }
            }

            return problems;
        }

        /// <summary>What the dockworker says about a problem, pointing back to the letter line.</summary>
        public static string DockworkerMessage(ShipmentProblem problem)
        {
            var product = TradeCatalog.Products[problem.Product];
            var item = product.Name.ToLowerInvariant();
            var line = $"Line {problem.Line}";
            var reread = $"Read line {problem.Line} again.";
            switch (problem.Field)
            {
                case ProblemField.Availability:
                    return problem.Product == ProductId.PurpleCloth
                        ? $"{line} asks for purple cloth, but none is ready. Finish the swatch at the dye stall, then come back."
                        : $"{line} asks for more {item} than we have on the landing.";
                case ProblemField.Quantity:
                    return problem.Missing
                        ? $"{line} needs {item}. Set how many {product.UnitPlural} to load."
                        : $"{reread} That's not the right number of {product.UnitPlural} of {item}.";
                case ProblemField.Destination:
                    return problem.Missing
                        ? $"{line}: where is the {item} going? Pick its destination."
                        : $"{reread} The {item} is going somewhere else.";
                default:
                    return problem.Missing
                        ? $"{line}: the {item} isn't stowed yet. Select it, then click its place in the hold."
                        : $"{reread} The {item} belongs in a different place in the hold.";
            }
        }
    }
}
