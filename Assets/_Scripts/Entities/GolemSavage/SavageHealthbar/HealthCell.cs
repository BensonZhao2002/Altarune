using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCell : MonoBehaviour
{
    [SerializeField] private CanvasGroupFader cgFader;
    [SerializeField] private HealthCellActivationOverlay activationOverlay;
    [SerializeField] private float spawnHeight, spawnTime;
    private bool isOn, isActive;

    private float SpawnHeight => anchorPosition.y + spawnHeight;
    private Vector2 anchorPosition;

    void Awake() {
        anchorPosition = RectTransform.anchoredPosition;
    }

    private RectTransform RectTransform => transform as RectTransform;

    public void SetAnchors(float xMin, float xMax) {
        SetXMin(xMin);
        SetXMax(xMax);
    }

    public void DoCharge() {
        isActive = true;
        if (!isOn) {
            RectTransform.anchoredPosition = anchorPosition + new Vector2(0, spawnHeight);

            isOn = true;
            cgFader.DoFade(true);

            StopAllCoroutines();
            StartCoroutine(IDoSpawn());
        } else {
            activationOverlay.Toggle(true);
        }
    }

    public void DoDischarge() {
        isActive = false;
        activationOverlay.Toggle(false);
    }

    public void DoDespawn() {
        isOn = false;
        if (isActive) DoDischarge();
        cgFader.DoFade(false);
    }

    private IEnumerator IDoSpawn() {
        float lerpVal = (RectTransform.anchoredPosition.y - SpawnHeight) / (anchorPosition.y - SpawnHeight);
        while (lerpVal < 1) {
            lerpVal = Mathf.MoveTowards(lerpVal, 1, Time.deltaTime.SafeDivide(spawnTime));
            RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x,
                                                         Mathf.Lerp(SpawnHeight, anchorPosition.y, lerpVal));
            yield return null;
        }
        if (isActive) activationOverlay.Toggle(true);
    }

    private void SetXMin(float xMin) {
        RectTransform.anchorMin = new(xMin, RectTransform.anchorMin.y);
    }

    private void SetXMax(float xMax) {
        RectTransform.anchorMax = new(xMax, RectTransform.anchorMax.y);
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawSolidDisc(transform.position + Vector3.up * spawnHeight, transform.forward, 1f);
        UnityEditor.Handles.color = Color.white;
    }
    #endif
}
