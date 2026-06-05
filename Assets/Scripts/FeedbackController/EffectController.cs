using DG.Tweening.Core.Easing;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;


public class EffectController : SingletonMonoBehaviour<EffectController>
{
    [SerializeField] private SO_Vfx so_Vfx;
    protected override void OnDestroy()
    {
        SCR_Pool.Flush();
        base.OnDestroy();
    }
    /// <summary>
    /// spawn free fx from pool
    /// </summary>
    /// <param name="idFx"></param>
    /// <param name="posSpawn"></param>
    /// <param name="timeOffFx"></param>
    public void SpawnFx(VFXName vFXName, Vector3 posSpawn, float timeOffFx)
    {
        var obj = SCR_Pool.GetFreeObject(so_Vfx.GetVfxPrefab(vFXName));
        obj.transform.position = posSpawn;
        obj.transform.parent = this.transform;
        StartCoroutine(TurnOffFx(obj, timeOffFx));
    }
    public GameObject GetFx(VFXName vFXName, Vector3 posSpawn)
    {
        var obj = SCR_Pool.GetFreeObject(so_Vfx.GetVfxPrefab(vFXName));
        obj.transform.position = posSpawn;
        return obj;
    }
    /// <summary>
    /// spawn fx from pool with parent
    /// </summary>
    /// <param name="idFx"></param>
    /// <param name="posSpawn"></param>
    /// <param name="parentFx"></param>
    /// <param name="timeOffFx"></param>
    public void SpawnFx(VFXName vFXName, Vector3 posSpawn, Transform parentFx, float timeOffFx)
    {
        var obj = SCR_Pool.GetFreeObject(so_Vfx.GetVfxPrefab(vFXName));
        obj.transform.position = posSpawn;
        obj.transform.parent = parentFx;
        StartCoroutine(TurnOffFx(obj, timeOffFx));
    }
    /// <summary>
    /// spawn fx not from pool, use instantiate
    /// </summary>
    /// <param name="idFx"></param>
    /// <param name="posSpawn"></param>
    /// <param name="parentFx"></param>
    /// <param name="timeOffFx"></param>
    public void SpawnFxNotPool(VFXName vFXName, Vector3 posSpawn, Transform parentFx, float timeOffFx)
    {
        var obj = Instantiate(so_Vfx.GetVfxPrefab(vFXName));
        obj.transform.position = posSpawn;
        obj.transform.parent = parentFx;
        StartCoroutine(TurnOffFx(obj, timeOffFx));
    }
    
    IEnumerator TurnOffFx(GameObject target, float timeOff)
    {
        yield return new WaitForSeconds(timeOff);
        target.SetActive(false);
    }
}

