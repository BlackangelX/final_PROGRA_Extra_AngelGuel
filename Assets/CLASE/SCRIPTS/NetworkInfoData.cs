using Fusion;
using UnityEngine;

public struct NetworkInfoData : INetworkInput
{
    public Vector2 movementAxis;
    public bool runInputPressed;
    public bool movingBackwards;
    public bool movingOnXAxis;
    public bool moveInputPressed;
    public Vector2 mousePos;       
    public Vector2 lookRotation;
    public bool isShooting;
    public bool isMeleeAttacking;
}
