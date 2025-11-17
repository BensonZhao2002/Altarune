using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SavageHealthBar : MonoBehaviour {

    public event System.Action OnBarInit;

    [SerializeField] private Image topLayerMask, topLayer, interpolationLayerMask, interpolationLayer;
    [SerializeField] private Color normalColor, lockedColor;
    [SerializeField] private CanvasGroupFader lockIconCGFader;
    [SerializeField] private HealthCellIconAnimator lockIconAnimator;
    [SerializeField] private float initialFillTime, lockTime;
    private Coroutine lockCoroutine, updateCoroutine;

    private float Health => savageGolem ? savageGolem.Health : 1;
    private float MaxHealth => savageGolem ? savageGolem.MaxHealth : 1;
    private float FillAmount => MaxHealth > 0 ? Health / MaxHealth : 0;

    private GolemSavage savageGolem;

    public void DoSpawn() => StartCoroutine(ISpawnHealthbar());

    public void Init(GolemSavage savageGolem) {
        this.savageGolem = savageGolem;
        savageGolem.OnHealReceived += AttachedEntity_OnHealReceived;
        savageGolem.OnDamageReceived += AttachedEntity_OnDamageTaken;
        savageGolem.OnPhaseTransition += SavageGolem_OnPhaseTransition;
        savageGolem.OnPerish += AttachedEntity_OnPerish;
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
        updateCoroutine = StartCoroutine(IDoHealthbar());
    }

    public void ToggleLock(bool on) {
        lockIconCGFader.DoFade(on);
        lockIconAnimator.Toggle(on);
        if (lockCoroutine != null) StopCoroutine(lockCoroutine); 
        lockCoroutine = StartCoroutine(IDoLock(on));
    }

    private IEnumerator ISpawnHealthbar() {
        while (interpolationLayerMask.fillAmount < FillAmount) {
            topLayerMask.fillAmount = Mathf.MoveTowards(topLayerMask.fillAmount,
                                                        FillAmount, Time.unscaledDeltaTime.SafeDivide(initialFillTime));
            interpolationLayerMask.fillAmount = Mathf.MoveTowards(interpolationLayerMask.fillAmount,
                                                                  FillAmount, Time.unscaledDeltaTime.SafeDivide(initialFillTime));
            yield return null;
        }
        OnBarInit?.Invoke();
    }

    private IEnumerator IDoHealthbar() {
        while (true) {
            topLayerMask.fillAmount = Mathf.MoveTowards(topLayerMask.fillAmount,
                                                        FillAmount, Time.unscaledDeltaTime);
            interpolationLayerMask.fillAmount = Mathf.MoveTowards(interpolationLayerMask.fillAmount,
                                                                  FillAmount, Time.unscaledDeltaTime);
            yield return null;
        }
    }

    private IEnumerator IDoLock(bool on) {
        Color targetColor = on ? lockedColor : normalColor;
        while (Vector4.Distance(topLayer.color, targetColor) > 0) {
            topLayer.color = Vector4.MoveTowards(topLayer.color, targetColor, Time.unscaledDeltaTime.SafeDivide(lockTime));
            yield return null;
        }
        lockCoroutine = null;
    }

    private void AttachedEntity_OnDamageTaken(int amount) {
        topLayerMask.fillAmount = Health / MaxHealth;
    }

    private void AttachedEntity_OnHealReceived(int _) {
        interpolationLayerMask.fillAmount = Health / MaxHealth;
    }

    private void SavageGolem_OnPhaseTransition(SavagePhase _, int __) => Disconnect();

    private void AttachedEntity_OnPerish(BaseObject _) => Disconnect();

    private void Disconnect() {
        savageGolem.OnHealReceived -= AttachedEntity_OnHealReceived;
        savageGolem.OnDamageReceived -= AttachedEntity_OnDamageTaken;
        savageGolem.OnPhaseTransition -= SavageGolem_OnPhaseTransition;
        savageGolem.OnPerish -= AttachedEntity_OnPerish;
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
    }
}
