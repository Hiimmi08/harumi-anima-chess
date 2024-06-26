using Cells;
using System;
using UnityEngine;

namespace Pieces
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private Rank baseRank;
        [SerializeField] private PieceAnimationController animationController;
        [SerializeField] private PiecePhysics physics;

        public int Id { get; private set; }
        public Rank BaseRank => baseRank;
        public Rank CurrentRank { get; private set; }
        public Cell CurrentCell { get; private set; }
        public PieceStatus Status { get; private set; }
        public PieceDirection Direction { get; private set; }
        public int PlayerId { get; private set; }

        public static event Action<Piece> OnClickHandler;
        public static event Action<bool> OnMovingComplete;

        public void Init(Cell cell, PieceDirection direction, int playerId)
        {
            CurrentCell = cell;
            Direction = direction;
            PlayerId = playerId;
            OnSetDirection();

            CurrentRank = BaseRank;
            Status = PieceStatus.Alive;

            animationController?.Init();
            SetClickable(true);

            AddEvent();
        }

        private void OnDestroy()
        {
            RemoveEvent();
        }

        private void AddEvent()
        {
            physics.OnClickHandler += OnClick;
        }

        private void RemoveEvent()
        {
            physics.OnClickHandler -= OnClick;
        }

        private void OnClick()
        {
            Debug.Log($"Click: rank={CurrentRank}");
            OnClickHandler?.Invoke(this);
        }

        // Start showing animation for being clicked
        public void SetOnClick()
        {
            animationController?.OnClick();
        }

        public void ResetOnClick()
        {
            animationController?.Idle();
        }

        public void SetClickable(bool isClickable)
        {
            physics.SetClickable(isClickable);
        }

        public void OnMoveToTrapCell()
        {
            CurrentRank = Rank.Default;
        }

        public void OnMoveOutOfTrapCell()
        {
            CurrentRank = BaseRank;
        }

        public void MoveToCell(Cell destination, float spawnY, bool isMovingToTrapCell, bool isMovingToCaveCell)
        {
            if (destination == null || !IsMovable() || CurrentCell == destination)
            {
                return;
            }

            // Move out of trap cell
            if (CurrentCell.IsTrap())
            {
                OnMoveOutOfTrapCell();
            }
            
            // Move to trap cell
            if (isMovingToTrapCell)
            {
                OnMoveToTrapCell();
            }
            
            Debug.Log($"Moving to cell: {destination.Id}");
            transform.position = destination.Position + new Vector3(0f, spawnY, 0f);
            CurrentCell = destination;
            
            OnMovingComplete?.Invoke(isMovingToCaveCell);
        }

        public bool IsMovable()
        {
            return Status == PieceStatus.Alive;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnSetDirection()
        {
            Vector3 rotation = Direction switch
            {
                PieceDirection.North => new Vector3(0f, 180f, 0f),
                PieceDirection.South => Vector3.zero,
                _ => Vector3.zero,
            };

            transform.eulerAngles = rotation;
        }
    }
}