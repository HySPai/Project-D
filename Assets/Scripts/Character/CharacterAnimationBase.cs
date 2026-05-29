using UnityEngine;

public abstract class CharacterAnimationBase : MonoBehaviour
{
    public abstract void UpdateAnimation(Vector2 moveInput);
}