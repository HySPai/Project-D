using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerInputHandler input;
    private PlayerMovement movement;
    private PlayerAnimation animationController;
    private PlayerCombat combat;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();
        animationController = GetComponent<PlayerAnimation>();
        combat = GetComponent<PlayerCombat>();
    }

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
