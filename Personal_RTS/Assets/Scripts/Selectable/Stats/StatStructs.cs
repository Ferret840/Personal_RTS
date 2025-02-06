using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selectable
{
    namespace Stats
    {
//        public class IntStat
//        {
//            private readonly int m_BaseStat;
//            private int m_CalculatedStat;
//
//            private Dictionary<int, float> m_PercentModifiers = new Dictionary<int, float>();
//            private float m_TotalPercent = 100f;
//
//            private Dictionary<int, int> m_FlatModifiers = new Dictionary<int, int>();
//            private int m_TotalFlat = 0;
//
//#if DEBUG
//            bool m_Modified = true;
//#endif
//
//            public IntStat(int _initVal)
//            {
//                m_CalculatedStat = m_BaseStat = _initVal;
//            }
//
//            public override bool Equals(object _obj)
//            {
//#if DEBUG
//                if (!m_Modified)
//                    return false;
//#endif
//                var newObj = _obj as int?;
//                if (newObj != null)
//#if DEBUG
//                    m_Modified = false;
//                if (newObj != null)
//#endif
//                    if (newObj == Value)
//                    {
//                        return true;
//                    }
//                return false;
//            }
//
//            public int Value
//            {
//                get { return m_CalculatedStat; }
//            }
//
//            public override string ToString()
//            {
//                return $"{Value}    ({m_BaseStat} * {m_TotalPercent}% + {m_TotalFlat})";
//            }
//
//            //Percent
//            public void AddPrecentModifier(float _percent, int _modID)
//            {
//                m_TotalPercent += _percent;
//                m_PercentModifiers.Add(_modID, _percent);
//                UpdateCalculatedStat();
//            }
//            public void RemovePrecentModifier(int _modID)
//            {
//                m_TotalPercent -= m_PercentModifiers[_modID];
//                m_PercentModifiers.Remove(_modID);
//                UpdateCalculatedStat();
//            }
//            //Flat
//            public void AddFlatModifier(int _value, int _modID)
//            {
//                m_TotalFlat += _value;
//                m_FlatModifiers.Add(_modID, _value);
//                UpdateCalculatedStat();
//            }
//            public void RemoveFlatModifier(int _modID)
//            {
//                m_TotalFlat -= m_FlatModifiers[_modID];
//                m_FlatModifiers.Remove(_modID);
//                UpdateCalculatedStat();
//            }
//
//            private void UpdateCalculatedStat()
//            {
//#if DEBUG
//                m_Modified = true;
//#endif
//                m_CalculatedStat = System.Math.Max((int)(m_BaseStat * m_TotalPercent / 100f + m_TotalFlat), 0);
//            }
//        }

        public class FloatStat
        {
            private readonly float m_BaseStat;
            private float m_CalculatedStat;

            private Dictionary<int, float> m_PercentModifiers = new Dictionary<int, float>();
            private float m_TotalPercent = 100f;

            private Dictionary<int, float> m_FlatModifiers = new Dictionary<int, float>();
            private float m_TotalFlat = 0f;

#if DEBUG
            bool m_Modified = true;
#endif

            public FloatStat(float _initVal)
            {
                m_CalculatedStat = m_BaseStat = _initVal;
            }

            public override bool Equals(object _obj)
            {
#if DEBUG
                if (!m_Modified)
                    return false;
#endif
                var newObj = _obj as float?;
                if (newObj != null)
#if DEBUG
                    m_Modified = false;
                if (newObj != null)
#endif
                    if (Mathf.Approximately(newObj.Value, Value))
                    {
                        return true;
                    }
                return false;
            }

            public float Value
            {
                get { return m_CalculatedStat; }
            }

            public override string ToString()
            {
                return $"{Value}    ({m_BaseStat} * {m_TotalPercent}% + {m_TotalFlat})";
            }

            //Percent
            public void AddPrecentModifier(float _percent, int _modID)
            {
                m_TotalPercent += _percent;
                m_PercentModifiers.Add(_modID, _percent);
                UpdateCalculatedStat();
            }
            public void RemovePrecentModifier(int _modID)
            {
                m_TotalPercent -= m_PercentModifiers[_modID];
                m_PercentModifiers.Remove(_modID);
                UpdateCalculatedStat();
            }
            //Flat
            public void AddFlatModifier(float _value, int _modID)
            {
                m_TotalFlat += _value;
                m_FlatModifiers.Add(_modID, _value);
                UpdateCalculatedStat();
            }
            public void RemoveFlatModifier(int _modID)
            {
                m_TotalFlat -= m_FlatModifiers[_modID];
                m_FlatModifiers.Remove(_modID);
                UpdateCalculatedStat();
            }

            private void UpdateCalculatedStat()
            {
#if DEBUG
                m_Modified = true;
#endif
                m_CalculatedStat = System.Math.Max(m_BaseStat * m_TotalPercent / 100f + m_TotalFlat, 0);
            }
        }

//        public class DoubleStat
//        {
//            private readonly double m_BaseStat;
//            private double m_CalculatedStat;
//
//            private Dictionary<int, float> m_PercentModifiers = new Dictionary<int, float>();
//            private float m_TotalPercent = 100f;
//
//            private Dictionary<int, double> m_FlatModifiers = new Dictionary<int, double>();
//            private double m_TotalFlat = 0;
//
//#if DEBUG
//            bool m_Modified = true;
//#endif
//
//            public DoubleStat(double _initVal)
//            {
//                m_CalculatedStat = m_BaseStat = _initVal;
//            }
//
//            public override bool Equals(object _obj)
//            {
//#if DEBUG
//                if (!m_Modified)
//                    return false;
//#endif
//                var newObj = _obj as double?;
//                if (newObj != null)
//#if DEBUG
//                    m_Modified = false;
//                if (newObj != null)
//#endif
//                    if (newObj.Value >= Value - Mathf.Epsilon && newObj.Value <= Value + Mathf.Epsilon)
//                    {
//                        return true;
//                    }
//                return false;
//            }
//
//            public double Value
//            {
//                get { return m_CalculatedStat; }
//            }
//
//            public override string ToString()
//            {
//                return $"{Value}    ({m_BaseStat} * {m_TotalPercent}% + {m_TotalFlat})";
//            }
//
//            //Percent
//            public void AddPrecentModifier(float _percent, int _modID)
//            {
//                m_TotalPercent += _percent;
//                m_PercentModifiers.Add(_modID, _percent);
//                UpdateCalculatedStat();
//            }
//            public void RemovePrecentModifier(int _modID)
//            {
//                m_TotalPercent -= m_PercentModifiers[_modID];
//                m_PercentModifiers.Remove(_modID);
//                UpdateCalculatedStat();
//            }
//            //Flat
//            public void AddFlatModifier(double _value, int _modID)
//            {
//                m_TotalFlat += _value;
//                m_FlatModifiers.Add(_modID, _value);
//                UpdateCalculatedStat();
//            }
//            public void RemoveFlatModifier(int _modID)
//            {
//                m_TotalFlat -= m_FlatModifiers[_modID];
//                m_FlatModifiers.Remove(_modID);
//                UpdateCalculatedStat();
//            }
//
//            private void UpdateCalculatedStat()
//            {
//#if DEBUG
//                m_Modified = true;
//#endif
//                m_CalculatedStat = System.Math.Max(m_BaseStat * m_TotalPercent / 100f + m_TotalFlat, 0);
//            }
//        }
    }
}