using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Stationary gun: scans for a target a few times per second, turns its head toward it and fires bursts of pooled
/// bullets while the target stays in range.
/// </summary>
public class Turret : MonoBehaviour
{
    private enum State { Idle, Aiming, Firing }

    private const float MinSqrAimDistance = 0.001f;

    [Header("Detection")]
    [FormerlySerializedAs("_range")]
    [SerializeField] private float range = 10f;
    [FormerlySerializedAs("_targetLayer")]
    [SerializeField] private LayerMask targetLayer;
    [Min(0f)]
    [SerializeField] private float scanInterval = 0.1f;

    [Header("Aiming")]
    [FormerlySerializedAs("_rotatePart")]
    [SerializeField] private Transform rotatePart;
    [FormerlySerializedAs("_firePoint")]
    [SerializeField] private Transform firePoint;
    [FormerlySerializedAs("_rotateSpeed")]
    [SerializeField] private float rotateSpeed = 360f;
    [FormerlySerializedAs("_aimThreshold")]
    [SerializeField] private float aimThreshold = 5f;

    [Header("Firing")]
    [SerializeField] private Bullet bulletPrefab;
    [FormerlySerializedAs("_damage")]
    [SerializeField] private float damage = 10f;
    [FormerlySerializedAs("_bulletsPerBurst")]
    [SerializeField] private int bulletsPerBurst = 3;
    [FormerlySerializedAs("_shotCooldown")]
    [SerializeField] private float shotCooldown = 0.3f;
    [FormerlySerializedAs("_burstCooldown")]
    [SerializeField] private float burstCooldown = 3f;

    private readonly Collider[] scanBuffer = new Collider[4];

    private State state = State.Idle;
    private Transform target;
    private bool isBursting;
    private float nextScanTime;
    private CancellationTokenSource fireCts;

    private void Update()
    {
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;
            DetectTarget();
        }

        switch (state)
        {
            case State.Aiming:
                if (RotateTowardsTarget()) state = State.Firing;
                break;
            case State.Firing:
                HandleFiring();
                break;
        }
    }

    private void DetectTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, range, scanBuffer, targetLayer, QueryTriggerInteraction.Ignore);
        if (count > 0)
        {
            target = scanBuffer[0].transform;
            if (state == State.Idle) state = State.Aiming;
            return;
        }

        target = null;
        if (state == State.Idle) return;

        state = State.Idle;
        CancelFiring();
    }

    private void HandleFiring()
    {
        if (target == null) return;

        RotateTowardsTarget();
        if (!isBursting)
            FireLoop(NewFireToken()).Forget();
    }

    private bool RotateTowardsTarget()
    {
        if (target == null) return false;

        Vector3 dir = target.position - rotatePart.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < MinSqrAimDistance) return true;

        Quaternion look = Quaternion.LookRotation(dir);
        rotatePart.rotation = Quaternion.RotateTowards(rotatePart.rotation, look, rotateSpeed * Time.deltaTime);
        return Quaternion.Angle(rotatePart.rotation, look) <= aimThreshold;
    }

    private async UniTaskVoid FireLoop(CancellationToken token)
    {
        isBursting = true;
        try
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(burstCooldown), cancellationToken: token);

                for (int i = 0; i < bulletsPerBurst; i++)
                {
                    token.ThrowIfCancellationRequested();
                    SpawnBullet();

                    if (i < bulletsPerBurst - 1)
                        await UniTask.Delay(TimeSpan.FromSeconds(shotCooldown), cancellationToken: token);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            isBursting = false;
        }
    }

    private void SpawnBullet()
    {
        if (bulletPrefab == null) return;

        Bullet bullet = ObjectPool.Get(bulletPrefab);
        bullet.Fire(firePoint.position, firePoint.rotation, damage);
    }

    private CancellationToken NewFireToken()
    {
        CancelFiring();
        fireCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        return fireCts.Token;
    }

    private void CancelFiring()
    {
        fireCts?.Cancel();
        fireCts?.Dispose();
        fireCts = null;
        isBursting = false;
    }

    /// <summary>Stops firing and forgets the target, e.g. on a world reset.</summary>
    public void ResetTurret()
    {
        CancelFiring();
        state = State.Idle;
        target = null;
    }

    private void OnDestroy() => CancelFiring();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
