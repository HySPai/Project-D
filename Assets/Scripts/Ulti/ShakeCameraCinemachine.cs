using System;
using UnityEngine;
using Unity.Cinemachine;   // đổi namespace
using UniRx;
using Observer;

public class ShakeCameraCinemachine : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeAmplitude = 1.2f;
    [SerializeField] private float shakeFrequency = 2.0f;

    private CinemachineCamera virtualCamera;                 // đổi type
    private CinemachineBasicMultiChannelPerlin noise;
    private IDisposable shakeTimerDisposable;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        if (virtualCamera != null)
        {
            // noise giờ là component thường
            noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null)
            {
                noise = virtualCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }
    }

    private void OnEnable()
    {
        EventDispatcher.Instance?.RegisterListener(EventID.ShakeCamera, ShakeCamera);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener(EventID.ShakeCamera, ShakeCamera);
    }

    private void OnDestroy()
    {
        shakeTimerDisposable?.Dispose();
    }

    public void ShakeCamera(object data)
    {
        if (noise == null) return;

        float amplitude, frequency, duration;

        if (data == null)
        {
            amplitude = shakeAmplitude;
            frequency = shakeFrequency;
            duration = shakeDuration;
        }
        else
        {
            ShakeCameraData d = (ShakeCameraData)data;
            amplitude = d.amplitude;
            frequency = d.frequency;
            duration = d.duration;
        }

        noise.AmplitudeGain = amplitude;   // bỏ m_
        noise.FrequencyGain = frequency;

        shakeTimerDisposable?.Dispose();
        shakeTimerDisposable = Observable.Timer(TimeSpan.FromSeconds(duration))
            .Subscribe(_ => StopShake());
    }

    private void StopShake()
    {
        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 0f;
        }
    }
}

public struct ShakeCameraData
{
    public float duration;
    public float amplitude;
    public float frequency;
}