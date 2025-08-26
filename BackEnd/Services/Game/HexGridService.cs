namespace LoDCompanion.BackEnd.Services.Game
{
    public readonly struct Hex
    {
        public int Q { get; }
        public int R { get; }
        public int S { get; }

        public Hex(int q, int r, int s)
        {
            if (q + r + s != 0)
            {
                throw new ArgumentException("q + r + s must equal 0");
            }
            Q = q;
            R = r;
            S = s;
        }

        // --- Core Hex Grid Logic ---

        public static Hex Add(Hex a, Hex b) => new Hex(a.Q + b.Q, a.R + b.R, a.S + b.S);
        public static Hex Subtract(Hex a, Hex b) => new Hex(a.Q - b.Q, a.R - b.R, a.S - b.S);
        public static Hex Scale(Hex a, int k) => new Hex(a.Q * k, a.R * k, a.S * k);

        public static int Distance(Hex a, Hex b)
        {
            var vec = Subtract(a, b);
            return (Math.Abs(vec.Q) + Math.Abs(vec.R) + Math.Abs(vec.S)) / 2;
        }

        // --- Equality and Hashing for Dictionary Keys ---

        public override bool Equals(object? obj) => obj is Hex other && this.Equals(other);

        public bool Equals(Hex other) => Q == other.Q && R == other.R && S == other.S;

        public override int GetHashCode() => HashCode.Combine(Q, R, S);

        public static bool operator ==(Hex left, Hex right) => left.Equals(right);
        public static bool operator !=(Hex left, Hex right) => !(left == right);
    }

    public enum TerrainType
    {
        Road,
        OffRoad,
        Desert,
        QuestSite,
        Town
    }

    public class HexTile
    {
        public Hex Position { get; }
        public TerrainType Terrain { get; set; } = TerrainType.OffRoad;
        public double MovementCost => Terrain == TerrainType.OffRoad ? 1.5 : Terrain == TerrainType.Desert ? 2 : 1;

        // Add other properties like "HasRoad", "SettlementName", etc.

        public HexTile(Hex position)
        {
            Position = position;
        }
    }

    public class HexGridService
    {
        // Store your world map in a dictionary for easy lookups
        public Dictionary<Hex, HexTile> WorldGrid { get; } = new Dictionary<Hex, HexTile>();

        // Pre-defined direction vectors for finding neighbors easily
        private static readonly List<Hex> HexDirections = new List<Hex>
        {
            new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1),
            new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1)
        };

        // --- Grid Generation ---
        public void GenerateMap(int radius)
        {
            WorldGrid.Clear();
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Math.Max(-radius, -q - radius);
                int r2 = Math.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    var hex = new Hex(q, r, -q - r);
                    WorldGrid[hex] = new HexTile(hex);
                    // TODO: Add logic to assign terrain types
                }
            }
        }

        // --- Core Service Methods ---

        public HexTile? GetTileAt(Hex hex)
        {
            WorldGrid.TryGetValue(hex, out var tile);
            return tile;
        }

        public List<HexTile> GetNeighbors(Hex hex)
        {
            var neighbors = new List<HexTile>();
            foreach (var direction in HexDirections)
            {
                var neighborHex = Hex.Add(hex, direction);
                var tile = GetTileAt(neighborHex);
                if (tile != null)
                {
                    neighbors.Add(tile);
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Finds all reachable tiles and the shortest path to each, within a given movement budget.
        /// </summary>
        /// <param name="startHex">The starting hexagon.</param>
        /// <param name="movementBudget">The maximum total movement cost allowed.</param>
        /// <returns>A dictionary where the key is the reachable Hex and the value is the list of Hexes representing the path.</returns>
        public Dictionary<Hex, List<Hex>> GetTilesInRange(Hex startHex, double movementBudget)
        {
            var visited = new Dictionary<Hex, Hex> { { startHex, startHex } }; // Stores <Hex, ParentHex>
            var costs = new Dictionary<Hex, double> { { startHex, 0 } };
            var frontier = new Queue<Hex>();
            frontier.Enqueue(startHex);

            while (frontier.Count > 0)
            {
                var currentHex = frontier.Dequeue();

                foreach (var direction in HexDirections)
                {
                    var neighborHex = Hex.Add(currentHex, direction);

                    if (WorldGrid.TryGetValue(neighborHex, out var neighborTile))
                    {
                        double newCost = costs[currentHex] + neighborTile.MovementCost;

                        if (newCost <= movementBudget)
                        {
                            if (!visited.ContainsKey(neighborHex) || newCost < costs[neighborHex])
                            {
                                costs[neighborHex] = newCost;
                                visited[neighborHex] = currentHex; // Set the parent for path reconstruction
                                frontier.Enqueue(neighborHex);
                            }
                        }
                    }
                }
            }

            // Now, reconstruct the path for each visited hex
            var paths = new Dictionary<Hex, List<Hex>>();
            foreach (var hex in visited.Keys)
            {
                paths[hex] = ReconstructPath(startHex, hex, visited);
            }

            return paths;
        }

        /// <summary>
        /// Helper method to reconstruct the path from the 'visited' dictionary.
        /// </summary>
        private List<Hex> ReconstructPath(Hex start, Hex end, Dictionary<Hex, Hex> cameFrom)
        {
            var path = new List<Hex>();
            var current = end;
            while (!current.Equals(start))
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }
    }

        /// <summary>
        /// Helper method to get all tiles within a simple hex radius, ignoring movement cost.
        /// </summary>
        private List<HexTile> GetTilesInRadius(Hex center, int range)
        {
            var results = new List<HexTile>();
            for (int q = -range; q <= range; q++)
            {
                int r1 = Math.Max(-range, -q - range);
                int r2 = Math.Min(range, -q + range);
                for (int r = r1; r <= r2; r++)
                {
                    var hex = Hex.Add(center, new Hex(q, r, -q - r));
                    var tile = GetTileAt(hex);
                    if (tile != null)
                    {
                        results.Add(tile);
                    }
                }
            }
            return results;
        }
    }
}
