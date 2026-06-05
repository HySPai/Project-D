
using System;
using System.Collections;
using UnityEngine;

public class ParabolicMovement : MonoBehaviour
{
    public float speed;
    public Vector3 target;
    public Vector3 _target;
    public float arcHeight;
    private Vector3 _startPosition;
    private float _stepScale;
    private float _progress;
    public Transform secondaryTarget;

    private void Start()
    {
        _startPosition = transform.position;

        float distance = Vector3.Distance(_startPosition, target);

        // This is one divided by the total flight duration, to help convert it to 0-1 progress.
        _stepScale = speed / distance;
    }

    public void DoMoveObj()
    {
        StartCoroutine(MoveObj());
    }

    private IEnumerator MoveObj()
    {
        float t = 0;
        _progress = 0;
        transform.position = _startPosition;
        _target = target;
        while (t < 10)
        {
            t += Time.deltaTime;
            _progress = Mathf.Min(_progress + Time.deltaTime * _stepScale, 1.0f);

            // Turn this 0-1 value into a parabola that goes from 0 to 1, then back to 0.
            float parabola = 1.0f - 4.0f * (_progress - 0.5f) * (_progress - 0.5f);

            // Travel in a straight line from our start position to the target.        
            Vector3 nextPos = Vector3.Lerp(_startPosition, _target, _progress);
            float step = 3f * Time.deltaTime;
            //_target = Vector3.MoveTowards(_target, secondaryTarget.position, step);
            // Then add a vertical arc in excess of this.
            nextPos.y += parabola * arcHeight;

            // Continue as before.
            transform.LookAt(nextPos, transform.forward);
            transform.position = nextPos;
            if (transform.position.y == 0)
            {
                yield break;
            }
            yield return null;
        }
    }
}