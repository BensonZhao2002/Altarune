using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarCelled : MonoBehaviour
{
    public event System.Action OnInit;

    [SerializeField] private HealthCell cellPrefab;
    [SerializeField] private float cellSpawnInterval;
    [SerializeField] private bool readEntityHealth = true;

    private Entity entity;
    private HealthCell[] cellArray;
    private int lastActiveIndex = -1;
    private bool isInitializing;

    public void Init(Entity entity, int cellAmount, int activeAmount) {
        isInitializing = true;

        this.entity = entity;
        UpdateCellInstances(cellAmount);

        StopAllCoroutines();
        StartCoroutine(IActivateCellsSequential(activeAmount));

        if (readEntityHealth) {
            entity.OnDamageReceived += Entity_OnDamageReceived;
            entity.OnHealReceived += Entity_OnHealReceived;
        }

        if (entity is GolemSavage) {
            (entity as GolemSavage).OnPhaseTransition += GolemSavage_OnPhaseTransition;
        }
    }

    private void UpdateCellInstances(int cellAmount) {
        int prevLength = cellArray == null ? 0 : cellArray.Length;
        HealthCell[] newArray = new HealthCell[cellAmount];

        float cellSpacing = 1f / cellAmount;

        if (cellArray != null) {
            for (int i = 0; i < cellArray.Length; i++) {
                cellArray[i].SetAnchors(i * cellSpacing, (i + 1) * cellSpacing);
                if (i < cellAmount) {
                    newArray[i] = cellArray[i];
                } else {
                    break;
                }
            }
        }

        for (int i = prevLength; i < cellAmount; i++) {
            HealthCell cell = Instantiate(cellPrefab, transform);
            cell.SetAnchors(i * cellSpacing, (i + 1) * cellSpacing);
            newArray[i] = cell;
        }

        cellArray = newArray;
    }

    private IEnumerator IActivateCellsSequential(int cellAmount) {
        for (int i = 0; i < cellAmount; i++) {
            ActivateCell();
            yield return new WaitForSecondsRealtime(cellSpawnInterval);
        }

        isInitializing = false;
        if (readEntityHealth) Entity_OnDamageReceived(0);
        OnInit?.Invoke();
    }

    private void GolemSavage_OnPhaseTransition(SavagePhase _, int __) => Disconnect();

    public void ActivateCells(int cellAmount) {
        for (int i = 0; i < cellAmount; i++) ActivateCell();
    }

    public void DeactivateCells(int cellAmount) {
        for (int i = 0; i < cellAmount; i++) DeactivateCell();
    }

    private void ActivateCell() {
        lastActiveIndex = Mathf.Min(cellArray.Length - 1, lastActiveIndex + 1);
        cellArray[lastActiveIndex].DoCharge();
    }

    private void DeactivateCell() {
        if (lastActiveIndex < 0) return;
        cellArray[lastActiveIndex].DoDischarge();
        lastActiveIndex = Mathf.Max(-1, lastActiveIndex - 1);
    }

    private void Entity_OnDamageReceived(int _) {
        if (!isInitializing) {
            DeactivateCells(Mathf.Max(0, lastActiveIndex - entity.Health + 1));
        }
    }

    private void Entity_OnHealReceived(int _) {
        if (!isInitializing) {
            ActivateCells(Mathf.Max(0, entity.Health - lastActiveIndex + 1));
        }
    }

    public void Disconnect() {
        if (entity) {
            for (int i = 0; i < cellArray.Length; i++) cellArray[i].DoDespawn();
            lastActiveIndex = -1;

            if (readEntityHealth) {
                entity.OnDamageReceived -= Entity_OnDamageReceived;
                entity.OnHealReceived -= Entity_OnHealReceived;
            }

            if (entity is GolemSavage) {
                (entity as GolemSavage).OnPhaseTransition -= GolemSavage_OnPhaseTransition;
            }
        }
    }
}
