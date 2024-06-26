using System;
using TMPro;
using UnityEngine;

namespace Cells
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private CellType cellType;
        [SerializeField] private CellPhysics physics;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private TMP_Text idLabel;

        public CellType Type => cellType;

        private Color _baseColor;
        
        public static event Action<Cell> OnClickHandler;

        public int Id { get; private set; }
        public bool IsClickable { get; private set; }
        public Vector3 Position { get; private set; }

        private void OnDestroy()
        {
            RemoveEvent();
        }

        public void Init(int id, int x, int z)
        {
            this.Id = id;
            idLabel.text = $"{id}";
            this.Position = new Vector3(x, 0f, z);
            this.IsClickable = false;

            _baseColor = meshRenderer.material.color;

            AddEvent();
        }

        public void Highlight()
        {
            Debug.Log($"Highlighted {Id} ");
            meshRenderer.material.color = Color.yellow;
            IsClickable = true;
        }

        public void ResetHighlight()
        {
            meshRenderer.material.color = _baseColor;
            IsClickable = false;
        }

        public bool IsRiver() => cellType == CellType.River;

        public bool IsTrap() => cellType == CellType.Trap;

        public bool IsCave() => cellType == CellType.Cave;

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
            Debug.Log($"OnClick {Id} {IsClickable}");
            if (IsClickable)
            {
                OnClickHandler?.Invoke(this);
            }
        }
    }
}