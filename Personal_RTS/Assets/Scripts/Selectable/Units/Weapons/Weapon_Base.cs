using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Selectable.Stats;

#if DEBUG
using System.Reflection;
using System.Linq;
using System;
using Tools;
#endif

namespace Selectable
{
    namespace Weapons
    {
        [System.Serializable]
        public class Weapon_Base : MonoBehaviour
        {
            private WeaponAttackVariables m_WeaponAttackVariables;

            // Start is called before the first frame update
            void Start()
            {
                m_WeaponAttackVariables = new WeaponAttackVariables(5f, 0.1f, 10f, 1, 5, 20.0f, WeaponAttackVariables.eDamageType.AntiInfantry);

                // Basic Functionality Test
                Debug.Log(m_WeaponAttackVariables.GetDPS());
                m_WeaponAttackVariables.AddFlatModifier(WeaponAttackVariables.eStatType.AttackCooldown, -0.2f, GetInstanceID());
                m_WeaponAttackVariables.AddPercentModifier(WeaponAttackVariables.eStatType.AttackCooldown, 50f, GetInstanceID());
                m_WeaponAttackVariables.AddFlatModifier(WeaponAttackVariables.eStatType.NumAttacks, 0.5f, GetInstanceID());
                Debug.Log(m_WeaponAttackVariables.DebugGetStat(WeaponAttackVariables.eStatType.AttackCooldown).ToString());
                Debug.Log(m_WeaponAttackVariables.DebugGetStat(WeaponAttackVariables.eStatType.NumAttacks).ToString());
                Debug.Log(m_WeaponAttackVariables.GetDPS());
            }

            // Update is called once per frame
            void Update()
            {
                m_WeaponAttackVariables.Pump(Time.deltaTime);
            }

            [System.Serializable]
            public class WeaponAttackVariables
            {
                public enum eStatType
                {
                    Invalid = -1,

                    AttackCooldown, // Seconds to reload between bursts
                    BurstDelay,     // Seconds delay between attacks in a burst
                    AttackRange,    // Attack Radius
                    ProjectileSpeed,// Projectile speed of an attack

                    NumAttacks,     // Number of attacks in a burst, decimals are % chance for extra attacks
                    Damage,         // Damage per attack in a burst

                    EnumCount
                }

                public enum eDamageType
                {
                    Invalid = -1,

                    AntiInfantry,
                    AntiTank,

                    EnumCount
                };

                private Dictionary<eStatType, FloatStat> m_FloatStats = new Dictionary<eStatType, FloatStat>();

                public readonly eDamageType m_BaseDamageType = eDamageType.Invalid;
                public readonly eDamageType m_DamageType = eDamageType.Invalid;

                private short m_Dimension;

                public WeaponAttackVariables(float _baseCooldown, float _attackDelay, float _attackRange, float _numAttacks, float _damage, float _projectileSpeed, eDamageType _damageType)
                {
                    m_FloatStats[eStatType.AttackCooldown] = new FloatStat(_baseCooldown);
                    m_FloatStats[eStatType.BurstDelay] = new FloatStat(_attackDelay);
                    m_FloatStats[eStatType.AttackRange] = new FloatStat(_attackRange);
                    m_FloatStats[eStatType.ProjectileSpeed] = new FloatStat(_projectileSpeed);
                    m_FloatStats[eStatType.NumAttacks] = new FloatStat(_numAttacks);
                    m_FloatStats[eStatType.Damage] = new FloatStat(_damage);
                    
                    m_DamageType = m_BaseDamageType = _damageType;
                }

                public float GetStatValue(eStatType _stat)
                {
                    return m_FloatStats[_stat].Value;
                }
#if DEBUG
                public FloatStat DebugGetStat(eStatType _stat)
                {
                    return m_FloatStats[_stat];
                }
#endif

                public void AddPercentModifier(eStatType _stat, float _percent, int _sourceID)
                {
                    m_FloatStats[_stat].AddPrecentModifier(_percent, _sourceID);
                }

                public void RemovePercentModifier(eStatType _stat, int _sourceID)
                {
                    m_FloatStats[_stat].RemovePrecentModifier(_sourceID);
                }

                public void AddFlatModifier(eStatType _stat, float _value, int _sourceID)
                {
                    m_FloatStats[_stat].AddFlatModifier(_value, _sourceID);
                }

                public void RemoveFlatModifier(eStatType _stat, int _sourceID)
                {
                    m_FloatStats[_stat].RemoveFlatModifier(_sourceID);
                }

                public float GetDPS()
                {
                    float totalDelay = m_FloatStats[eStatType.NumAttacks].Value * m_FloatStats[eStatType.BurstDelay].Value;
                    float attackCycleTime = m_FloatStats[eStatType.AttackCooldown].Value + totalDelay;
                    float totalDamage = m_FloatStats[eStatType.NumAttacks].Value * m_FloatStats[eStatType.Damage].Value;

                    return totalDamage / attackCycleTime;
                }

            public void Pump(float _dt)
                {
#if DEBUG
                    CustomLogger.LogAllProperties_s(this);
#endif
                }
            };
        }
    }
}
