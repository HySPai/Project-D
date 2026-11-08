using UnityEngine;
using UnityEngine.Serialization;

/// <summary>Pooled kinematic turret bullet: flies straight, deals damage through its DamageCollider and hides on a wall or timeout.</summary>
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [FormerlySerializedAs("_speed")]
    [SerializeField] private float speed = 20f;
    [FormerlySerializedAs("_lifeTime")]
    [SerializeField] private float lifeTime = 5f;
    [FormerlySerializedAs("_damageCollider")]
    [SerializeField] private DamageCollider damageCollider;
    [SerializeField] private Rigidbody body;

    [Header("Wall Detection")]
    [FormerlySerializedAs("_wallLayer")]
    [SerializeField] private LayerMask wallLayer;

    private float despawnTimer;

    /// <summary>Places the bullet and arms it with damage; called right after it leaves the pool.</summary>
    public void Fire(Vector3 position, Quaternion rotation, float damage)
    {
        transform.SetPositionAndRotation(position, rotation);
        if (damageCollider != null)
            damageCollider.SetDamage(damage, DamageSource.Machine);
        despawnTimer = lifeTime;
    }

    private void Update()
    {
        despawnTimer -= Time.deltaTime;
        if (despawnTimer <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        body.MovePosition(body.position + transform.forward * (speed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & wallLayer) != 0)
            gameObject.SetActive(false);
    }
}
