using System;
using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Generic grid system for 2D games - reusable for pathfinding, tilemaps, etc.
    /// </summary>
    public class Grid2D<TGridObject>
    {
        public event EventHandler<Vector2Int> OnGridObjectChanged;

        private readonly int width;
        private readonly int height;
        private readonly float cellSize;
        private readonly Vector2 origin;
        private readonly TGridObject[,] gridArray;

        public Grid2D(int width, int height, float cellSize, Vector2 origin,
                     Func<Grid2D<TGridObject>, Vector2Int, TGridObject> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.origin = origin;

            gridArray = new TGridObject[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int gridPosition = new Vector2Int(x, y);
                    gridArray[x, y] = createGridObject(this, gridPosition);
                }
            }
        }

        public Vector2 GetWorldPosition(Vector2Int gridPosition)
        {
            return new Vector2(gridPosition.x, gridPosition.y) * cellSize + origin;
        }

        public Vector2Int GetGridPosition(Vector2 worldPosition)
        {
            Vector2 localPosition = worldPosition - origin;
            return new Vector2Int(
                Mathf.RoundToInt(localPosition.x / cellSize),
                Mathf.RoundToInt(localPosition.y / cellSize)
            );
        }

        public void SetGridObject(Vector2Int gridPosition, TGridObject value)
        {
            if (IsValidGridPosition(gridPosition))
            {
                gridArray[gridPosition.x, gridPosition.y] = value;
                OnGridObjectChanged?.Invoke(this, gridPosition);
            }
        }

        public TGridObject GetGridObject(Vector2Int gridPosition)
        {
            if (IsValidGridPosition(gridPosition))
            {
                return gridArray[gridPosition.x, gridPosition.y];
            }
            return default;
        }

        public TGridObject GetGridObject(Vector2 worldPosition)
        {
            return GetGridObject(GetGridPosition(worldPosition));
        }

        public bool IsValidGridPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < width &&
                   gridPosition.y >= 0 && gridPosition.y < height;
        }

        public bool IsValidWorldPosition(Vector2 worldPosition)
        {
            return IsValidGridPosition(GetGridPosition(worldPosition));
        }

        public void TriggerGridObjectChanged(Vector2Int gridPosition)
        {
            OnGridObjectChanged?.Invoke(this, gridPosition);
        }

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public Vector2 Origin => origin;

        // Utility methods for common grid operations
        public void ForEach(Action<Vector2Int, TGridObject> action)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    action(position, gridArray[x, y]);
                }
            }
        }

        public Vector2Int[] GetNeighborPositions(Vector2Int position, bool includeDiagonals = false)
        {
            var neighbors = new System.Collections.Generic.List<Vector2Int>();

            // Cardinal directions
            neighbors.Add(new Vector2Int(position.x + 1, position.y));
            neighbors.Add(new Vector2Int(position.x - 1, position.y));
            neighbors.Add(new Vector2Int(position.x, position.y + 1));
            neighbors.Add(new Vector2Int(position.x, position.y - 1));

            if (includeDiagonals)
            {
                // Diagonal directions
                neighbors.Add(new Vector2Int(position.x + 1, position.y + 1));
                neighbors.Add(new Vector2Int(position.x + 1, position.y - 1));
                neighbors.Add(new Vector2Int(position.x - 1, position.y + 1));
                neighbors.Add(new Vector2Int(position.x - 1, position.y - 1));
            }

            // Filter valid positions
            return System.Array.FindAll(neighbors.ToArray(), IsValidGridPosition);
        }
    }

    /// <summary>
    /// Simple grid position struct for 2D grids
    /// </summary>
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int x;
        public int y;

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition position && Equals(position);
        }

        public bool Equals(GridPosition other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public static bool operator ==(GridPosition left, GridPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridPosition left, GridPosition right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
