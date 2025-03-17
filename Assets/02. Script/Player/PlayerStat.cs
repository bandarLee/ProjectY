using UnityEngine;
using System;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance { get; private set; }

    [Header("Player Stat")]
    [SerializeField] private float MaxHealth = 100f;
    private float CurrentHealth;
    [SerializeField] private float AttackPower = 10f;
    [SerializeField] private float MagicPower = 15f;
    [SerializeField] private float BaseSpeed = 7f;
    private float CurrentSpeed;

    public event Action OnStatChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        CurrentHealth = MaxHealth;
        CurrentSpeed = BaseSpeed;
    }
    public float GetHealth() => CurrentHealth;
    public float GetMaxHealth() => MaxHealth;

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Debug.Log("플레이어 사망!");
            // 죽었을 때 처리 (예: 리스폰, 게임 오버 등)
        }
        OnStatChanged?.Invoke(); // UI 업데이트 트리거
    }

    public void Heal(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        OnStatChanged?.Invoke();
    }

    // 🔹 공격력 & 주문력 관련 메서드
    public float GetAttackPower() => AttackPower;
    public void IncreaseAttackPower(float amount)
    {
        AttackPower += amount;
        OnStatChanged?.Invoke();
    }

    public float GetMagicPower() => MagicPower;
    public void IncreaseMagicPower(float amount)
    {
        MagicPower += amount;
        OnStatChanged?.Invoke();
    }


    public float GetSpeed() => CurrentSpeed;
    public void SetSpeed(float newSpeed)
    {
        CurrentSpeed = newSpeed;
        OnStatChanged?.Invoke();
    }

    public void ResetSpeed()
    {
        CurrentSpeed = BaseSpeed;
        OnStatChanged?.Invoke();
    }
}
