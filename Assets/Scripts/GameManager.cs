using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    CancellationToken cancellationToken;
    private void Start()
    {
        cancellationToken = this.GetCancellationTokenOnDestroy();
        Application.targetFrameRate = 60;
        GameStateManager.Instance.Install(cancellationToken).Forget();
    }
   
}
