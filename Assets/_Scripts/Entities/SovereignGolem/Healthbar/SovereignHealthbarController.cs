using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SovereignHealthBarController : MonoBehaviour {

    [SerializeField] private CanvasGroupFader cgFader;
    [SerializeField] private HealthBarCelled phaseCounterBar;
    [SerializeField] private HealthBarCelled crystalCounterBar;
    [SerializeField] private SovereignGolem sovereign;
    [SerializeField] private float phaseDeactivationTime;
    private SovereignPhase activePhase;
    private int activePhaseCells, activeMaxCrystalCells;
    private bool isInitialized;

    public void Toggle(bool on) => cgFader.DoFade(on);

    void Awake() {
        sovereign.OnPhaseTransition += Sovereign_OnPhaseTransition;
        sovereign.OnPerish += Sovereign_OnPerish;
    }

    private void Sovereign_OnPhaseTransition(SovereignPhase activePhase, int maxCrystalHealth) {
        activeMaxCrystalCells = maxCrystalHealth;

        if (activePhase == SovereignPhase.Macro1
                && !isInitialized) {
            isInitialized = true;
            Init();
        } else if (this.activePhase != activePhase) {
            this.activePhase = activePhase;

            int phaseCellsLeft = 1 + sovereign.ConfigurationAmount - (int) activePhase;
            int diff = activePhaseCells - phaseCellsLeft;

            if (diff > 0) {
                activePhaseCells -= diff;
                phaseCounterBar.DeactivateCells(diff);
                StartCoroutine(IReinitBar(maxCrystalHealth));
            }
        }
    }

    private void Init() {
        Toggle(true);
        activePhaseCells = sovereign.ConfigurationAmount;
        phaseCounterBar.OnInit += PhaseCounterBar_OnInit;
        phaseCounterBar.Init(sovereign, activePhaseCells, activePhaseCells);
    }

    private void PhaseCounterBar_OnInit() {
        phaseCounterBar.OnInit -= PhaseCounterBar_OnInit;
        crystalCounterBar.Init(sovereign, activeMaxCrystalCells, sovereign.Health == 0 ? activeMaxCrystalCells : sovereign.Health);
    }

    private IEnumerator IReinitBar(int maxCrystalHealth) {
        crystalCounterBar.Disconnect();
        yield return new WaitForSecondsRealtime(phaseDeactivationTime);
        crystalCounterBar.Init(sovereign, maxCrystalHealth, sovereign.Health == 0 ? maxCrystalHealth : sovereign.Health);
    }

    private void Sovereign_OnPerish(BaseObject _) {
        sovereign.OnPerish -= Sovereign_OnPerish;
        phaseCounterBar.Disconnect();
        crystalCounterBar.Disconnect();
        Toggle(false);
    }
}
