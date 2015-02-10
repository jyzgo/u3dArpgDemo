using UnityEngine;
using System.Collections;

public class AttackInfo
{
	public float[] _attackPoints;
	//public float[] _attackPercents;
	public float _damageScale = 1;
	public float _criticalChance;
	public float _criticalDamage;
    public float _skillTriggerChance;
    public float _skillAttackDamage;
	public AttackHitType _hitType;
	public float _effectTime;
	public bool _isFromSkill = false;
	public int _pushStrength = 0;
	public float _pushTime = 0;
	public float _pushAngle = 0;
	public bool _pushByPoint = false;
	
	public FC_DAMAGE_TYPE _damageType = FC_DAMAGE_TYPE.NONE;
}

public class DefenseInfo
{
	public float[] _defensePoints;
	public float[] _defensePercents;
    public float _criticalDamageResist;
	public DefenseInfo()
	{
		_defensePoints = new float[FCConst.MAX_DAMAGE_TYPE];
		_defensePercents = new float[FCConst.MAX_DAMAGE_TYPE];
	}
}

public interface AttackUnit
{	
	ActionController GetOwner();
	bool IsFrom2P();
	void GetHurtDirection(Transform targetTransform,ref Vector3 direction);
	AttackInfo GetAttackInfo();
	Transform GetAttackerTransform();
	int GetFinalDamage(DefenseInfo di, out bool isCriticalHit, ActionController target);
    Eot[] GetEots(DefenseInfo di);
	int GetSharpness();
	bool CanHit(ActionController ac);
	FC_OBJECT_TYPE GetObjType();
	FCWeapon.WEAPON_HIT_TYPE GetAttackerType();
}




public class HandleHitTarget
{
	
	public static void HandleHit(ActionController owner, AttackUnit attackUnit, FC_DAMAGE_TYPE damageType, ActionController target)
	{
		if(owner.IsAlived)
		{
			if(owner.ACGetCurrentAttack() != null)
			{
				owner.ACIsHitTarget(target, attackUnit.GetSharpness(), owner.ACGetCurrentAttack()._hitGainEnergy, attackUnit.GetAttackInfo()._isFromSkill);
			}
			else
			{
				owner.ACIsHitTarget(target, 0, false, attackUnit.GetAttackInfo()._isFromSkill);
			}
		}
		if(attackUnit.GetAttackInfo()._damageType != FC_DAMAGE_TYPE.NONE)
		{
			damageType = attackUnit.GetAttackInfo()._damageType;
		}
		if(damageType != FC_DAMAGE_TYPE.NONE 
		&& !(target.SuperArmorSelf.ActiveArmor() == FCConst.SUPER_ARMOR_LVL2 
		&& target.SuperArmorSelf.DamageAbsorb(FCConst.SUPER_ARMOR_LVL2) > 0.99f))
		{
		
			//play blood effect
			Vector3 bloodPos = target.ACGetTransformByName(EnumEquipSlot.belt).position;
			
					
			//play blood on ground with random rotation
			int rotY = Random.Range(0, 360);
			Quaternion rot = Quaternion.Euler(0, rotY, 0);
			Vector3 bloodGroundPos = target.ACGetTransformByName(EnumEquipSlot.foot_point).position;
			bloodGroundPos.y -= target.SelfMoveAgent.GetFlyHeight();
			GlobalEffectManager.Instance.PlayEffect(FC_GLOBAL_EFFECT.BLOOD_GROUND, bloodGroundPos, rot);
		
			//play effect by damage type
			switch(damageType)
			{
				case FC_DAMAGE_TYPE.PHYSICAL:
				{
					if(target.HasBloodEffect)
					{
						//play physical attack effect
						GlobalEffectManager.Instance.PlayEffect(FC_GLOBAL_EFFECT.ATTACK_PHYSICAL
							, bloodPos);
					
						GlobalEffectManager.Instance.PlayEffect(FC_GLOBAL_EFFECT.BLOOD
							, bloodPos);
					}

				}
				break;
					
				case FC_DAMAGE_TYPE.FIRE:
				{					
					CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.FIRE_ATTACK
						,target._avatarController	,-1);
				}
				break;
			
				case FC_DAMAGE_TYPE.POISON:
				{					
					CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DAMAGE_POISON
						,target._avatarController	,-1);
				}
				break;
			
				case FC_DAMAGE_TYPE.ICE:
				{					
					CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DAMAGE_ICE
						,target._avatarController	,-1);
				}
				break;
			
				case FC_DAMAGE_TYPE.THUNDER:
				{					
					CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DAMAGE_THUNDER
						,target._avatarController	,-1);
				}
				break;
					
				default:
				{
					Assertion.Check(false, "now we do not support this type damage");
				}
				break;
			}
		}		
	}
}



public class DamageCounter
{
	public static string attacklog;

	private const float c1 = 85;

	private const float c2 = 400;

	public static int Result(AttackInfo ai, DefenseInfo di, out bool isCriticalHit, ActionController attacker, ActionController target)
	{
		float ret = 0;
		isCriticalHit = false;

		float physicalDamageReduction = 0;

		if (GameManager.Instance.IsPVPMode)
		{
			ret = Mathf.Max(ai._attackPoints[0] - di._defensePoints[0], ((float)ai._attackPoints[0]) * 0.3f); //pvp damage reduction
			if (attacker.SelfCountAgent != null)
			{
				attacker.SelfCountAgent.OnTryToHit((int)ai._attackPoints[0]);
			}
		}
		else
		{
			ret = Mathf.Max(ai._attackPoints[0] - di._defensePoints[0], ((float)ai._attackPoints[0]) * 0.1f); //pve damage reduction
		}

        //ret = ret * (1 - di._defensePercents[0]);


        //if (ret < 1)
        //{
        //    ret = 1;
        //}

		for (int i = 1; i < ai._attackPoints.Length; i++)
		{
			float ret1 = ((float)(ai._attackPoints[i] - di._defensePoints[i])) * (1 - di._defensePercents[i]);
			if (ret1 < 0.5)
			{
				ret1 = 0;
			}
			ret += ret1;
		}

		/*if(attacker.IsPlayer)
		{
			float r1 = Mathf.Pow((ai._criticalChance*10/(75* Mathf.Exp((attacker.Data._level-1)*(0.065f-attacker.Data._level*0.0002f)))),1.7f);
			chance = (int)(Mathf.Sqrt(r1)*0.7f +0.5f);
			//chance = (int)(Mathf.Log(ai._criticalChance*0.25f)*1.25f);
			//Debug.Log(chance);
		}*/

        int chance = Mathf.RoundToInt(ai._criticalChance * 100);
        chance = Mathf.Clamp(chance, 1, 50);

        if (chance >= Random.Range(1, 100))
        {
            ret = (1 + ai._criticalDamage - di._criticalDamageResist) * ret;
            isCriticalHit = true;
        }

		float addScale = 0;

		if (attacker.ACGetCurrentAttack() != null
			&& attacker.AIUse.AddDamageScale > 0)
		{
			addScale = attacker.AIUse.AddDamageScale;
		}
        
		ret = ret * (ai._damageScale + addScale);

        if (ai._skillAttackDamage > 0)
        {
            chance = Mathf.RoundToInt(ai._skillTriggerChance * 100);
            chance = Mathf.Clamp(chance, 1, 50);

            if (chance >= Random.Range(1, 100))
            {
                ret = (1 + ai._skillAttackDamage) * ret;
            }
        }

		if (CheatManager.showAttackInfo)
		{
			if (isCriticalHit)
			{
				attacklog = attacklog + "This is a critical \n";
			}

			attacklog = attacklog + "Final damage = " + ret + "\n";

		}

		if (ret < 1 && ret >= 0)
		{
			ret = 1;
		}

        if (attacker.IsPlayerSelf && isCriticalHit)
        {
            if (isCriticalHit)
            {
                if (attacker.ACGetCurrentAttack() != null)
                {
                    attacker.ACHitTargetIsCrit(target);
                }

                if (attacker.SkillGodDown)
                {
                    int deltaHp = Mathf.RoundToInt(ret * attacker.Data.PassiveSkillGodDownCriticalToHp);
                    attacker.ACIncreaseHP(deltaHp);
                }

            }
        }

		if (CheatManager.showAttackInfo)
		{
			attacklog = "------------------------------------------------------\n";
			attacklog = attacklog + "Attacker is " + attacker.Data.id + "\n";
			attacklog = attacklog + "Target is " + target.Data.id + "\t Level " + target.Data.Level + "\tArmor = " + target.Data.TotalDefense + "\n";
			attacklog = attacklog + target.Data.id + " hitpoint before hurt = " + target.HitPoint + "\n";
			attacklog = attacklog + "Normal Attack point = " + ai._attackPoints[0] + "\n";
			attacklog = attacklog + "Normal Defense point = " + di._defensePoints[0] + "\n";
			attacklog = attacklog + "Damage reduction percent = " + physicalDamageReduction + "\n";
			attacklog = attacklog + "Damage = " + ret + "\n";
		}

		return (int)ret;
	}
}