using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCellIconAnimator : MonoBehaviour
{
    [SerializeField] private ParticleSystem dropParticles;
    [SerializeField] private AnimationCurve xCurve, yCurve;
    [SerializeField] private float spawnTime, despawnTime;
    private Vector3 anchorScale;

    private Vector3 CurrentAnimationTarget => new Vector3(xCurve.Evaluate(animationLerp) * anchorScale.x,
                                                          yCurve.Evaluate(animationLerp) * anchorScale.y,
                                                          anchorScale.z);
    private float animationLerp;

    void Awake() {
        anchorScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    public void Toggle(bool on) {
        StopAllCoroutines();
        StartCoroutine(on ? IDoToggleOn() : IDoToggleOff());
    }

    private IEnumerator IDoToggleOn() {
        while (animationLerp < 1
                || Vector3.Distance(transform.localScale, CurrentAnimationTarget) > 0) {
            animationLerp = Mathf.MoveTowards(animationLerp, 1, Time.unscaledDeltaTime.SafeDivide(spawnTime));
            transform.localScale = CurrentAnimationTarget;
            yield return null;
        }
        dropParticles.Play();
    }

    private IEnumerator IDoToggleOff() {
        while (Vector3.Distance(transform.localScale, Vector3.zero) > 0) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.unscaledDeltaTime.SafeDivide(despawnTime));
            yield return null;
        }
        animationLerp = 0;
    }
}
