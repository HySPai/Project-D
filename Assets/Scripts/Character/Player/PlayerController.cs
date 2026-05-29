using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerInputHandler input;
    [SerializeField] PlayerMovement movement;
    [SerializeField] PlayerAnimation animationController;
    [SerializeField] PlayerCombat combat;

    private void Update()
    {
        Vector2 moveInput = input.GetMoveInput();

        movement.SetInput(moveInput);
        animationController.UpdateAnimation(moveInput);

        if (input.IsFire())
        {
            combat.Attack();
        }
    }

    private void FixedUpdate()
    {
        movement.Move();
    }
}
