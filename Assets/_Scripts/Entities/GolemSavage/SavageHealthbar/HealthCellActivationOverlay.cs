using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCellActivationOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroupFader cgFader;
    [SerializeField] private HealthCellIconAnimator iconAnimator;
    [SerializeField] private ParticleSystem activationParticles, deactivationParticles;

    public void Toggle(bool on) {
        cgFader.DoFade(on);
        if (iconAnimator) iconAnimator.Toggle(on);
        if (on) activationParticles.Play();
        else deactivationParticles.Play();
    }
}
