using System;
using UnityEngine;

namespace Cells
{
    public class CellPhysics : MonoBehaviour
    {
        [SerializeField] private Collider collider;
    
        public event Action OnClickHandler;

        public void SetClickable(bool isClickable)
        {
            collider.enabled = isClickable;
        }
    
        private void OnMouseDown()
        {
            OnClickHandler?.Invoke();
        }
    }
}