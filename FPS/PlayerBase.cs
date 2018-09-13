using UnityEngine;

public class PlayerBase
{
    public float WalkSpeed, RushSpeed, CrouchSpeed, JumpSpeed;
    public float Health, BaseWeight, AdditionWeight;
    public bool IsAlive;

    virtual public void Spawn()
    {
        IsAlive = true;
        Health = 100.0f;
        BaseWeight = 80.0f;
        AdditionWeight = 0.0f;
        WalkSpeed = 3.0f;
        RushSpeed = 5.0f;
        CrouchSpeed = 1.0f;
        JumpSpeed = 4.0f;
    }

    virtual public void Die()
    {
        IsAlive = false;
        Health = 0.0f;
    }
}