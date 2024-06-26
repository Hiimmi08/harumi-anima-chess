using Boards;
using Cells;
using Players;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public class PieceManager : MonoBehaviour
    {
        public static PieceManager Instance;

        public event Action<int, bool, bool> PieceClickedHandler;
        public event Action<Piece> PieceActionCompleteHandler;
        
        [SerializeField] private float spawnY = 0.15f;

        [Header("Piece Prefabs")]
        [SerializeField] private Piece elephantPrefab;
        [SerializeField] private Piece lionPrefab;
        [SerializeField] private Piece tigerPrefab;
        [SerializeField] private Piece leopardPrefab;
        [SerializeField] private Piece dogPrefab;
        [SerializeField] private Piece wolfPrefab;
        [SerializeField] private Piece catPrefab;
        [SerializeField] private Piece ratPrefab;

        #region Variables

        // North player
        private static readonly List<(Rank rank, int cell)> NorthStartPositions = new()
        {
            (Rank.Elephant, 60),
            (Rank.Lion, 8),
            (Rank.Tiger, 62),
            (Rank.Leopard, 24),
            (Rank.Dog, 16),
            (Rank.Wolf, 42),
            (Rank.Cat, 52),
            (Rank.Rat, 6),
        };

        // South player
        private static readonly List<(Rank rank, int cell)> SouthStartPositions = new()
        {
            (Rank.Elephant, 2),
            (Rank.Lion, 54),
            (Rank.Tiger, 0),
            (Rank.Leopard, 38),
            (Rank.Dog, 46),
            (Rank.Wolf, 20),
            (Rank.Cat, 10),
            (Rank.Rat, 56),
        };

        private PlayerPosition _currentPlayerPosition;
        private List<Piece> _northPieces;
        private List<Piece> _southPieces;
        private List<Piece> _allPieces;
        private List<Piece> _deadPieces;
        public Piece _selectedPiece;
        
        #endregion

        #region Spawn Pieces

        public void SpawnPieces()
        {
            SpawnPieceForPlayer(PlayerPosition.North);
            SpawnPieceForPlayer(PlayerPosition.South);
            
            _allPieces.Clear();
            _allPieces.AddRange(_northPieces);
            _allPieces.AddRange(_southPieces);
        }

        private void SpawnPieceForPlayer(PlayerPosition playerPosition)
        {
            List<(Rank rank, int cell)> startPositions;
            PieceDirection direction;
            List<Piece> pieces;
            var playerId = 0;

            switch (playerPosition)
            {
                case PlayerPosition.North:
                    {
                        startPositions = NorthStartPositions;
                        direction = PieceDirection.North;
                        pieces = _northPieces;
                        playerId = (int)playerPosition;
                    }
                    break;

                case PlayerPosition.South:
                    {
                        startPositions = SouthStartPositions;
                        direction = PieceDirection.South;
                        pieces = _southPieces;
                        playerId = (int)playerPosition;
                    }
                    break;

                default:
                    return;
            }

            if (startPositions == null)
            {
                return;
            }

            foreach (var (rank, cellId) in startPositions)
            {
                SpawnPieceToCell(rank, cellId, direction, playerId, pieces);
            }
        }

        private void SpawnPieceToCell(Rank rank, int cellId, PieceDirection direction,
            int playerId, List<Piece> pieces)
        {
            Piece piece = rank switch
            {
                Rank.Rat => SpawnPieceToCell(ratPrefab, cellId, direction, playerId),
                Rank.Cat => SpawnPieceToCell(catPrefab, cellId, direction, playerId),
                Rank.Dog => SpawnPieceToCell(dogPrefab, cellId, direction, playerId),
                Rank.Wolf => SpawnPieceToCell(wolfPrefab, cellId, direction, playerId),
                Rank.Leopard => SpawnPieceToCell(leopardPrefab, cellId, direction, playerId),
                Rank.Tiger => SpawnPieceToCell(tigerPrefab, cellId, direction, playerId),
                Rank.Lion => SpawnPieceToCell(lionPrefab, cellId, direction, playerId),
                Rank.Elephant => SpawnPieceToCell(elephantPrefab, cellId, direction, playerId),
                _ => null,

            };

            if (piece != null && pieces != null)
            {
                pieces.Add(piece);
            }
        }

        private Piece SpawnPieceToCell(Piece piece, int cellId, PieceDirection direction, int playerId)
        {
            var cell = Board.Instance.GetCell(cellId);
            if (cell == null)
            {
                return null;
            }

            var position = cell.Position + new Vector3(0f, spawnY, 0f);
            var p = Instantiate(piece, position, Quaternion.identity);
            p.transform.SetParent(transform);
            p.Init(cell, direction, playerId);

            return p;
        }

        #endregion

        public void Init()
        {
            _southPieces = new();
            _northPieces = new();
            _allPieces = new();
            _deadPieces = new();

            AddEvents();
        }

        private void AddEvents()
        {
        }

        private void RemoveEvents()
        {
        }

        public void OnClickPieceHandler(Piece piece)
        {
            if (piece == null)
            {
                return;
            }

            if (_selectedPiece != null && _selectedPiece != piece)
            {
                _selectedPiece.ResetOnClick();
            }

            _selectedPiece = piece;
            piece.SetOnClick();
            var cellId = piece.CurrentCell.Id;
            var rank = piece.CurrentRank;
            PieceClickedHandler?.Invoke(cellId, rank.IsSwimmable(), rank.IsCrossable());
            
            if (_allPieces is not {Count: > 0})
            {
                return;
            }
            
            // Make other pieces unclickable
            MakeOpponentPiecesUnClickable(piece);
        }

        public void OnClickCell(Cell cell)
        {
            if (_selectedPiece == null)
            {
                return;
            }

            // Check if existed opponent's piece on targeted cell
            // If yes, make the piece defeated (dead)
            var targetedPiece = GetPieceOnCell(cell);
            if (targetedPiece != null)
            {
                Debug.Log($"Existed piece:{targetedPiece.gameObject.name}");
                if (!_selectedPiece.IsDefeatable(targetedPiece))
                {
                    Debug.LogError($"Cant defeat this piece!");
                    return;
                }
                
                targetedPiece.Hide();
                _deadPieces.Add(targetedPiece);
                _allPieces.Remove(targetedPiece);
            }
            
            // Move
            _selectedPiece.MoveToCell(cell, spawnY, IsMovingToTrapCell(cell), IsMovingToCaveCell(cell));
            
            // Reset
            _selectedPiece.ResetOnClick(); // need change if move has animation
            PieceActionCompleteHandler?.Invoke(_selectedPiece);
            _selectedPiece = null;
        }

        public void OnPieceMovingComplete()
        {
            if (_allPieces is not {Count: > 0})
            {
                return;
            }
            
            foreach (var piece in _allPieces)
            {
                piece.SetClickable(true);
            }
        }

        public void OnSetPlayerTurn(PlayerPosition position)
        {
            _currentPlayerPosition = position;
        }

        public void OnReset()
        {
            _allPieces.AddRange(_deadPieces);
            _deadPieces.Clear();
        }

        private bool IsMovingToTrapCell(Cell destinationCell)
        {
            if (_currentPlayerPosition == PlayerPosition.North)
            {
                return Board.Instance.IsSouthTrap(destinationCell);
            }
            else
            {
                return Board.Instance.IsNorthTrap(destinationCell);
            }
        }

        private bool IsMovingToCaveCell(Cell destinationCell)
        {
            if (_currentPlayerPosition == PlayerPosition.North)
            {
                return Board.Instance.IsSouthCave(destinationCell);
            }
            else
            {
                return Board.Instance.IsNorthCave(destinationCell);
            }
        }

        private Piece GetPieceOnCell(Cell cell)
        {
            if (_allPieces is not {Count: > 0})
            {
                return null;
            }
                
            foreach (var piece in _allPieces)
            {
                if (piece.CurrentCell == cell)
                {
                    return piece;
                }
            }

            return null;
        }

        private void MakeOpponentPiecesUnClickable(Piece selectedPiece)
        {
            var pieces = _currentPlayerPosition == PlayerPosition.North ? _southPieces : _northPieces;
            foreach (var p in pieces)
            {
                if (p != selectedPiece)
                {
                    p.SetClickable(false);
                }
            }
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
            RemoveEvents();
        }
    }
}