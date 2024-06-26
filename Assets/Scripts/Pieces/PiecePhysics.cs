using System;
using UnityEngine;

public class PiecePhysics : MonoBehaviour
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