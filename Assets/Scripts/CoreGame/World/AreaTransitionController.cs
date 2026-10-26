using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class AreaTransitionController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Volume fadeVolume;
    [SerializeField] private AreaRoom startRoom;
    [SerializeField] private AreaRoom[] rooms;
    [SerializeField] private float fadeOutDuration = 0.1f;
    [SerializeField] private float fadeInDuration = 0.1f;
    [SerializeField] private Ease fadeOutEase = Ease.InQuad;
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;

    private Tween fadeTween;
    private AreaRoom currentRoom;

    public bool IsTransitioning { get; private set; }
    public PlayerController Player => player;
    public AreaRoom CurrentRoom => currentRoom;
    public AreaRoom StartRoom => startRoom;

    private void Awake()
    {
        fadeVolume.weight = 0f;
        currentRoom = startRoom;

        for (int i = 0; i < rooms.Length; i++)
            rooms[i].SetVisible(rooms[i] == startRoom);

        ApplyBackdrop(startRoom);
    }

    private void OnDestroy() => fadeTween?.Kill();

    /// <summary>
    /// Carries the player through a doorway into the linked one behind a short fade, keeping their
    /// offset, heading and control, so the two rooms read as one connected space.
    /// </summary>
    public void RequestTransition(AreaPortal source)
    {
        if (IsTransitioning) return;
        if (source.LinkedPortal == null) return;
        if (player.GetState.IsDead) return;

        TransitionAsync(source).Forget();
    }

    private async UniTaskVoid TransitionAsync(AreaPortal source)
    {
        var token = this.GetCancellationTokenOnDestroy();
        IsTransitioning = true;

        await FadeTo(1f, fadeOutDuration, fadeOutEase);
        if (token.IsCancellationRequested) return;

        AreaPortal target = source.LinkedPortal;
        source.MapThrough(player.transform.position, player.transform.rotation, out Vector3 position, out Quaternion rotation);

        ShowRoom(target.Room);

        Vector3 delta = position - player.transform.position;
        player.Teleport(position, rotation);
        if (virtualCamera != null) virtualCamera.OnTargetObjectWarped(player.transform, delta);

        await FadeTo(0f, fadeInDuration, fadeInEase);
        IsTransitioning = false;
    }

    /// <summary>Shows a room at once and tells the camera the player was already moved there from previousPosition.</summary>
    public void SnapToRoom(AreaRoom room, Vector3 previousPosition)
    {
        ShowRoom(room);
        if (virtualCamera != null)
            virtualCamera.OnTargetObjectWarped(player.transform, player.transform.position - previousPosition);
    }

    private void ShowRoom(AreaRoom room)
    {
        if (room == null || room == currentRoom) return;

        room.SetVisible(true);
        if (currentRoom != null) currentRoom.SetVisible(false);
        currentRoom = room;
        ApplyBackdrop(room);
    }

    private void ApplyBackdrop(AreaRoom room)
    {
        if (sceneCamera == null || room == null) return;

        sceneCamera.clearFlags = room.DarkOutside ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
        sceneCamera.backgroundColor = Color.black;
    }

    private async UniTask FadeTo(float weight, float duration, Ease ease)
    {
        fadeTween?.Kill();
        fadeTween = DOTween.To(() => fadeVolume.weight, v => fadeVolume.weight = v, weight, duration)
            .SetEase(ease)
            .SetUpdate(true);

        await fadeTween.AsyncWaitForCompletion();
    }
}
