using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavageHealthBarController : MonoBehaviour
{
    [SerializeField] private CanvasGroupFader cgFader;
    [SerializeField] private SavageHealthBar phase1Bar;
    [SerializeField] private HealthBarCelled phase2Bar;
    [SerializeField] private SavageHealthBar phase3Bar;
    [SerializeField] private GolemSavage savage;

    public void Toggle(bool on) => cgFader.DoFade(on);

    void Awake() {
        savage.OnPhaseTransition += Savage_OnPhaseTransition;
        savage.OnPerish += Savage_OnPerish;
    }

    private void Savage_OnPhaseTransition(SavagePhase activePhase, int maxHealth) {
        switch (activePhase) {
            case SavagePhase.Phase1:
                Init();
                break;
            case SavagePhase.Phase2:
                phase2Bar.Init(savage, maxHealth, maxHealth);
                phase3Bar.ToggleLock(true);
                break;
            case SavagePhase.Phase3:
                phase3Bar.Init(savage);
                phase3Bar.ToggleLock(false);
                break;
        }
    }

    private void Init() {
        Toggle(true);
        phase3Bar.OnBarInit += Phase3Bar_OnBarInit;
        phase3Bar.DoSpawn();
    }

    private void Phase3Bar_OnBarInit() {
        phase3Bar.OnBarInit -= Phase3Bar_OnBarInit;
        phase1Bar.DoSpawn();
        phase1Bar.Init(savage);
    }

    private void Savage_OnPerish(BaseObject _) {
        savage.OnPerish -= Savage_OnPerish;
        Toggle(false);
    }
}