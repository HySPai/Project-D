using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTriggerPlay : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;
    public void PlayParticle()
    {
        particle?.Play();
    }
}
