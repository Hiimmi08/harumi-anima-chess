using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public int Id { get; private set; }

    public void Init(int id)
    {
        this.Id = id;
    }

    public void EnablePlay()
    {
        Debug.Log($"Enabled {gameObject.name}");
    }

    public void DisablePlay()
    {
        Debug.Log($"Disabled {gameObject.name}");
    }
}