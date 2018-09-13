using UnityEngine;
using System.Collections;

public class ClientMove : MonoBehaviour
{
    private MovementBase ClientBase = new MovementBase();

    void Start()
    {
        ClientBase.SetPlayer(gameObject);
        ClientBase.Spawn();
    }

    void Update()
    {
        if (!ClientBase.IsAlive)
            return;

        ClientBase.UpdateInput();
    }

    void FixedUpdate()
    {
        if (!ClientBase.IsAlive)
            return;

        ClientBase.UpdateView();
        ClientBase.UpdateMove();
    }
}