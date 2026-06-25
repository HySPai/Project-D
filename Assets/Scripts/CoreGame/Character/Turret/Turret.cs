using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private enum State { Idle, Aiming, Firing }

    [Header("Detection")]
    [SerializeField] private float _range = 10f;
    [SerializeField] private LayerMask _targetLayer;

    [Header("Aiming")]
    [SerializeField] private Transform _rotatePart;   // phần xoay của trụ
    [SerializeField] private Transform _firePoint;    // điểm bắn ra
    [SerializeField] private float _rotateSpeed = 360f;
    [SerializeField] private float _aimThreshold = 5f;

    [Header("Firing")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private int _bulletsPerBurst = 3;
    [SerializeField] private float _shotCooldown = 0.3f;   // giữa từng viên
    [SerializeField] private float _burstCooldown = 3f;    // chờ trước loạt đầu & giữa các loạt

    private State _state = State.Idle;
    private Transform _target;
    private bool _isBursting;
    private CancellationTokenSource _fireCts;

    private void Update()
    {
        DetectTarget();

        switch (_state)
        {
            case State.Idle: break;
            case State.Aiming: HandleAiming(); break;
            case State.Firing: HandleFiring(); break;
        }
    }

    private void DetectTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _range, _targetLayer);
        if (hits.Length > 0)
        {
            _target = hits[0].transform;
            if (_state == State.Idle) _state = State.Aiming;
        }
        else
        {
            _target = null;
            if (_state != State.Idle)
            {
                _state = State.Idle;
                CancelFiring();
            }
        }
    }

    private void HandleAiming()
    {
        if (RotateTowardsTarget()) // đã ngắm đủ gần
            _state = State.Firing;
    }

    private void HandleFiring()
    {
        if (_target == null) return;

        RotateTowardsTarget(); // vẫn bám theo trong lúc bắn

        if (!_isBursting)
            FireLoop(GetFireToken()).Forget();
    }

    private bool RotateTowardsTarget()
    {
        if (_target == null) return false;

        Vector3 dir = _target.position - _rotatePart.position;
        dir.y = 0f; // chỉ xoay quanh trục Y, bỏ nếu muốn xoay tự do
        if (dir.sqrMagnitude < 0.001f) return true;

        Quaternion look = Quaternion.LookRotation(dir);
        _rotatePart.rotation = Quaternion.RotateTowards(
            _rotatePart.rotation, look, _rotateSpeed * Time.deltaTime);

        return Quaternion.Angle(_rotatePart.rotation, look) <= _aimThreshold;
    }

    private async UniTaskVoid FireLoop(CancellationToken token)
    {
        _isBursting = true;
        try
        {
            // Chờ 3s trước loạt đầu, sau đó lặp lại mỗi loạt
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(_burstCooldown),
                    cancellationToken: token);

                for (int i = 0; i < _bulletsPerBurst; i++)
                {
                    token.ThrowIfCancellationRequested();
                    SpawnBullet();

                    if (i < _bulletsPerBurst - 1)
                        await UniTask.Delay(
                            System.TimeSpan.FromSeconds(_shotCooldown),
                            cancellationToken: token);
                }
            }
        }
        catch (System.OperationCanceledException)
        {
            // Bị hủy khi player rời tầm — bỏ qua
        }
        finally
        {
            _isBursting = false;
        }
    }

    private void SpawnBullet()
    {
        GameObject obj = SCR_Pool.GetFreeObject(_bulletPrefab);
        Bullet bullet = obj.GetComponent<Bullet>();
        bullet.Fire(_firePoint.position, _firePoint.rotation, _damage);
    }

    private CancellationToken GetFireToken()
    {
        _fireCts?.Cancel();
        _fireCts?.Dispose();
        _fireCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy());
        return _fireCts.Token;
    }

    private void CancelFiring()
    {
        _fireCts?.Cancel();
        _fireCts?.Dispose();
        _fireCts = null;
        _isBursting = false;
    }

    private void OnDestroy()
    {
        CancelFiring();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _range);
    }
}