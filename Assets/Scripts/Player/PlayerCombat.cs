using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private bool isAttacking;

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
    }

    // gọi từ Animation Event
    public void EndAttack()
    {
        isAttacking = false;
    }
}