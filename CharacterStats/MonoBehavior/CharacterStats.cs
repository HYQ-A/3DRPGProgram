using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    //Ѫ��UI�¼�
    public event Action<int, int> UpdateHealthBarOnAttack; 
    //ģ������
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;
    public AttackData_SO attackData;

    [HideInInspector]
    //�ж��Ƿ񱩻�
    public bool isCritical;

    private void Awake()
    {
        if (templateData != null)
            characterData = Instantiate(templateData);
    }

    #region Read from Data_SO
    public int MaxHealth
    {
        get { if (characterData != null) return (int)characterData.maxHealth; else return 0; }
        set { characterData.maxHealth = value; }
    }

    public int CurrentHealth
    {
        get { if (characterData != null) return (int)characterData.currentHealth; else return 0; }
        set { characterData.currentHealth = value; }
    }

    public int BaseDefence
    {
        get { if (characterData != null) return (int)characterData.baseDefence; else return 0; }
        set { characterData.baseDefence = value; }
    }

    public int CurrentDefence
    {
        get { if (characterData != null) return (int)characterData.currentDefence; else return 0; }
        set { characterData.currentDefence = value; }
    }
    #endregion

    #region Character Combat
    public void TakeDamage(CharacterStats attacker,CharacterStats defender)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defender.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        if (attacker.isCritical)
        {
            //���˵Ķ���Ч��
            defender.GetComponent<Animator>().SetTrigger("Hit");
        }
        //TODO:����UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        //TODO:���¾���
        if (CurrentHealth <= 0)
            attacker.characterData.UpdateExp(this.characterData.killPoint);
    }

    public void TakeDamage(int damage,CharacterStats defender)
    {
        int currentDamage = (int)MathF.Max(damage - defender.CurrentDefence, 0);
        CurrentHealth = (int)MathF.Max(CurrentHealth - currentDamage, 0);

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamge, attackData.maxDamge);
        //�ж��Ƿ񱩻�
        if(isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
        }
        return (int)coreDamage;
    }

    #endregion

}
