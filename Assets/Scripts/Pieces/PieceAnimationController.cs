using System.Collections.Generic;
using UnityEngine;

public class PieceAnimationController : MonoBehaviour
{
    [SerializeField] private List<Animator> animators = new();

    private const string IdleState = "Idle_A";
    private const string ClickedState = "Clicked";

    public void Init()
    {
        foreach(var animator in animators)
        {
            animator.Play(IdleState);
        }
    }

    public void OnClick()
    {
        foreach (var animator in animators)
        {
            animator.Play(ClickedState);
        }
    }

    public void Idle()
    {
        foreach (var animator in animators)
        {
            animator.Play(IdleState);
        }
    }
}