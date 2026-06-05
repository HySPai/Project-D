using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IView
{
    /// <summary>
    /// init view
    /// </summary>
    /// <param name="propagateCancellationToken"></param>
    public void Initialize(CancellationToken propagateCancellationToken);

    /// <summary>
    /// create cancellation token
    /// </summary>
    /// <returns></returns>
    public CancellationTokenSource CreateLinkedTokenSource();


    public void PropagateCancellationToken(CancellationToken cancellationToken);

    /// <summary>
    /// show view
    /// </summary>
    public void Show();

    /// <summary>
    /// hide view
    /// </summary>
    public void Hide();
}
