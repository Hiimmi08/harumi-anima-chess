using Cells;
using Pieces;
using System.Collections.Generic;
using UnityEngine;

namespace Boards
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private Cell cellNormal;
        [SerializeField] private Cell cellTrap;
        [SerializeField] private Cell cellCave;
        [SerializeField] private Cell cellRiver;

        //[SerializeField] private Piece piece;

        private static readonly int[] CaveIds = { 27, 35 };
        private static readonly int[] RiverIds = { 12, 13, 14, 21, 22, 23, 39, 40, 41, 48, 49, 50 };
        private static readonly int[] TrapIds = { 18, 28, 36, 26, 34, 44 };

        private static readonly Dictionary<int, Neighbors> CrossableCells = new()
        {
            {3, new Neighbors(south: 2, north: 4, west: -1, east: 30)},
            {4, new Neighbors(south: 3, north: 5, west: -1, east: 31)},
            {5, new Neighbors(south: 4, north: 6, west: -1, east: 32)},
            {11, new Neighbors(south: 10, north: 15, west: 2, east: 20)},
            {15, new Neighbors(south: 11, north: 16, west: 6, east: 24)},
            {20, new Neighbors(south: 19, north: 24, west: 11, east: 29)},
            {24, new Neighbors(south: 23, north: 20, west: 15, east: 33)},
            {30, new Neighbors(south: 29, north: 31, west: 3, east: 57)},
            {31, new Neighbors(south: 30, north: 32, west: 4, east: 58)},
            {32, new Neighbors(south: 31, north: 33, west: 5, east: 59)},
            {38, new Neighbors(south: 37, north: 42, west: 29, east: 47)},
            {42, new Neighbors(south: 38, north: 43, west: 33, east: 51)},
            {47, new Neighbors(south: 46, north: 51, west: 38, east: 56)},
            {51, new Neighbors(south: 47, north: 52, west: 42, east: 60)},
            {57, new Neighbors(south: 56, north: 58, west: 30, east: -1)},
            {58, new Neighbors(south: 57, north: 59, west: 31, east: -1)},
            {59, new Neighbors(south: 58, north: 60, west: 32, east: -1)},
        };

        private static readonly Dictionary<int, Neighbors> CornerCells = new()
        {
            {0, new Neighbors(south: -1, north: 1, west: -1, east: 9)},
            {8, new Neighbors(south: 7, north: -1, west: -1, east: 17)},
            {54, new Neighbors(south: -1, north: 55, west: 45, east: -1)},
            {62, new Neighbors(south: 61, north: -1, west: 53, east: -1)},
        };

        private static readonly List<int> NorthTrapCells = new()
        {
            26, 34, 44
        };
        
        private static readonly List<int> SouthTrapCells = new()
        {
            18, 28, 36
        };

        private const int NorthCaveCell = 35;
        private const int SouthCaveCell = 27;

        private List<Cell> cellList = new();
        private List<Cell> highlightedCells = new();

        private const int BoardWidth = 7;
        private const int BoardLength = 9;

        public static Board Instance;

        public void Init()
        {
            var id = 0;
            for (var i = 0; i < BoardWidth; i++)
            {
                for (var j = 0; j < BoardLength; j++)
                {
                    var cellPrefab = GetCellPrefab(id);
                    var cell = Instantiate(cellPrefab, this.transform);
                    cell.transform.position = new Vector3(i, 0f, j);
                    cell.Init(id, i, j);
                    cellList.Add(cell);
                    id++;
                }
            }

            AddEvent();
        }

        public bool IsNorthTrap(Cell cell) => NorthTrapCells.Contains(cell.Id);
        public bool IsSouthTrap(Cell cell) => SouthTrapCells.Contains(cell.Id);
        public bool IsNorthCave(Cell cell) => cell.Id == NorthCaveCell;
        public bool IsSouthCave(Cell cell) => cell.Id == SouthCaveCell;

        public void OnClickCell(Cell cell)
        {
            // TODO: Implement to reset highlight cells after piece finishes moving
        }
        
        public void ResetHighlightedCells()
        {
            if (highlightedCells == null || highlightedCells.Count <= 0)
            {
                return;
            }

            foreach (var cell in highlightedCells)
            {
                cell.ResetHighlight();
            }
            
            highlightedCells.Clear();
        }
        
        public Cell GetCell(int id)
        {
            if (id < 0)
            {
                return null;
            }
            
            foreach(var item in cellList)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        private void AddEvent()
        {
            PieceManager.Instance.PieceClickedHandler += OnClickPiece;
        }

        private void RemoveEvent()
        {
            PieceManager.Instance.PieceClickedHandler -= OnClickPiece;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            RemoveEvent();
        }

        private void OnClickPiece(int pieceCellId, bool isSwimmable, bool isCrossable)
        {
            if (pieceCellId < 0 || pieceCellId >= cellList.Count)
            {
                Debug.LogError($"Piece's cell ID is invalid.");
                return;
            }

            // Reset old highlight cells
            foreach (var cell in highlightedCells)
            {
                cell.ResetHighlight();
            }
            
            if (CornerCells.ContainsKey(pieceCellId))
            {
                var neighbors = CornerCells[pieceCellId];
                HighlightCell(GetCell(neighbors.South));
                HighlightCell(GetCell(neighbors.North));
                HighlightCell(GetCell(neighbors.West));
                HighlightCell(GetCell(neighbors.East));

                return;
            }

            var cellSouth = GetCell(pieceCellId - 1);
            var cellWest = GetCell(pieceCellId - 9);
            var cellNorth = GetCell(pieceCellId + 1);
            var cellEast = GetCell(pieceCellId + 9);

            if (isSwimmable)
            {
                HighlightCell(cellSouth);
                HighlightCell(cellNorth);
                HighlightCell(cellWest);
                HighlightCell(cellEast);

                return;
            }

            if (isCrossable)
            {
                // Check if cell is special case
                if (CrossableCells.ContainsKey(pieceCellId))
                {
                    var neighbors = CrossableCells[pieceCellId];
                    HighlightCell(GetCell(neighbors.South));
                    HighlightCell(GetCell(neighbors.North));
                    HighlightCell(GetCell(neighbors.West));
                    HighlightCell(GetCell(neighbors.East));

                    return;
                }
            }

            if (cellSouth != null && !cellSouth.IsRiver())
            {
                HighlightCell(cellSouth);
            }

            if (cellNorth != null && !cellNorth.IsRiver())
            {
                HighlightCell(cellNorth);
            }

            if (cellWest != null && !cellWest.IsRiver())
            {
                HighlightCell(cellWest);
            }

            if (cellEast != null && !cellEast.IsRiver())
            {
                HighlightCell(cellEast);
            }
        }

        private Cell GetCellPrefab(int id)
        {
            // Check if cell is Cave
            if (CaveIds.Contains(id))
            {
                return cellCave;
            }

            // Check if cell is River
            if (RiverIds.Contains(id))
            {
                return cellRiver;
            }

            // Check if cell is Trap
            if (TrapIds.Contains(id))
            {
                return cellTrap;
            }

            // return Normal
            return cellNormal;
        }

        private void HighlightCell(Cell cell)
        {
            if (cell == null)
            {
                return;
            }

            cell.Highlight();
            highlightedCells.Add(cell);
        }
    }
}