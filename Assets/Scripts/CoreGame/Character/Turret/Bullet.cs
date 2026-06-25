using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] private DamageCollider _damageCollider;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask _wallLayer;

    private Rigidbody _rb;
    private float _despawnTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Gọi từ Turret khi lấy ra khỏi pool
    public void Fire(Vector3 position, Quaternion rotation, float damage)
    {
        transform.SetPositionAndRotation(position, rotation);
        _damageCollider?.SetDamage(damage);
        _despawnTimer = _lifeTime;
    }

    private void Update()
    {
        _despawnTimer -= Time.deltaTime;
        if (_despawnTimer <= 0f) { Despawn(); return; }

        // Di chuyển kinematic
        _rb.MovePosition(_rb.position + transform.forward * (_speed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chạm tường → mất
        if (((1 << other.gameObject.layer) & _wallLayer) != 0)
            Despawn();
    }

    private void Despawn()
    {
        gameObject.SetActive(false); // trả về pool
    }
}