using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;
    public float updateInterval = 0.2f;
    public float stoppingDistance = 1.2f;
    public bool lookAtPlayerWhenClose = true;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        InvokeRepeating(nameof(UpdateDestination), 0f, updateInterval);
    }

    void UpdateDestination()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null) return;
        agent.SetDestination(player.position);

        if (lookAtPlayerWhenClose && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, 10f * Time.deltaTime);
            }
        }
    }

    void OnDisable()
    {
        CancelInvoke(nameof(UpdateDestination));
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
            Gizmos.DrawWireSphere(player.position, stoppingDistance);
        }
    }
}
