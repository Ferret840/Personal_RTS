using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathing;
using UnityEngine.UI;

namespace Selectable
{

    public class Owner : MonoBehaviour
    {
        [SerializeField]
        public int PlayerNumber
        {
            get;
            protected set;
        }
    
        public int m_EditorSetPlayerNum;
    
        [SerializeField]
        protected UniversalStats m_UnivStats;

        public GameObject m_SelectionIconPrefab;
        [SerializeField]
        protected Sprite m_Image;
        public Sprite GetImage
        {
            get
            {
                return m_Image;
            }
        }
    
        [System.NonSerialized]
        public bool m_IsSelected = false;
    
        public GameObject m_SelectedEffect;
        public GameObject m_HighlightedEffect;
        
        public GameObject m_MinimapObject;
    
        public Goal TargetGoal
        {
            get;
            private set;
        }

        public delegate void DeathDelegate(Owner _owner);
        event DeathDelegate OnDeathDelegateEvent = delegate { };

        public void AddOnDeathCall(DeathDelegate _method)
        {
            OnDeathDelegateEvent += _method;
        }
        public void RemoveOnDeathCall(DeathDelegate _method)
        {
            OnDeathDelegateEvent -= _method;
        }

        List<KeyValuePair<int, GameObject>> m_UISelectionIcons = new List<KeyValuePair<int, GameObject>>();
    
        // Use this for initialization
        void Awake()
        {
            PlayerNumber = m_EditorSetPlayerNum;
        }
    
        // Use this for initialization
        void Start()
        {
            UpdateMinimapLayer();
        }
    
        // Update is called once per frame
        void Update()
        {
    
        }

        virtual protected void SetSelectionSlicedMeshSizes(SlicedMesh _sliced)
        {
            _sliced.BorderVertical = 0.25f;
            _sliced.BorderHorizontal = 0.25f;

            _sliced.Height = transform.lossyScale.y + _sliced.BorderVertical * 2f;
            _sliced.Width = transform.lossyScale.x + _sliced.BorderHorizontal * 2f;

            _sliced.MarginVertical = 0.25f;
            _sliced.MarginHorizontal = 0.25f;
        }

        virtual protected void SetChildGlobalScale(Transform _transform, Vector3 _globalScale)
        {
            _transform.localScale = Vector3.one;
            _transform.localScale = new Vector3(_globalScale.x / transform.lossyScale.x, _globalScale.y / transform.lossyScale.y, _globalScale.z / transform.lossyScale.z);
        }

        virtual protected void UpdateMinimapLayer()
        {
            m_MinimapObject.layer = gameObject.layer + 3;
        }
    
        virtual public void OnRightMouse()
        {
    
        }
    
        virtual public void SetTargetGoal(Goal _targetGoal)
        {
            if (TargetGoal != null)
                TargetGoal.RemoveOwner(this);

            if (_targetGoal == null)
            {
                TargetGoal = _targetGoal;
            }
            else if (_targetGoal.Target != this.transform)
            {
                TargetGoal = _targetGoal;
                TargetGoal.AddOwner(this);
            }
        }

        virtual public Pathing.Goal.TargetDeathDelegate TargetDeathFunction()
        {
            return SetTargetGoal;
        }

        /// <summary>
        /// Overrideable function for handling when a key is pressed
        /// </summary>
        /// <param name="_k">An int within Keycode corresponding to the key that was pressed</param>
        virtual public void OnKeyDown(KeyCode _k)
        {
    
        }
    
        /// <summary>
        /// Overrideable function for handling when a key is released
        /// </summary>
        /// <param name="_k">An int within Keycode corresponding to the key that was released</param>
        virtual public void OnKeyUp(KeyCode _k)
        {
    
        }
    
        virtual public void Deselect()
        {
            m_SelectedEffect.SetActive(false);
            m_IsSelected = false;
        }
    
        virtual public void Select()
        {
            m_SelectedEffect.SetActive(true);
            m_IsSelected = true;
        }

        virtual public void AddSelectionIcon(int _playerNum, GameObject _icon)
        {
            m_UISelectionIcons.Add(new KeyValuePair<int, GameObject>(_playerNum, _icon));
            _icon.GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, (float)m_UnivStats.m_CurrentHealth / (float)m_UnivStats.m_MaxHealth);
            _icon.GetComponent<SelectionClickHandler>().AddDoubleClick(SelectFromIcon);
            _icon.GetComponent<SelectionClickHandler>().AddRightClick(DeselectFromIcon);
        }

        virtual public void RemoveSelectionIcon(int _playerNum)
        {
            foreach (var pair in m_UISelectionIcons)
            {
                if (pair.Key == _playerNum)
                {
                    GameObject icon = pair.Value;
                    m_UISelectionIcons.Remove(pair);
                    icon.GetComponent<SelectionClickHandler>().RemoveDoubleClick(SelectFromIcon);
                    icon.GetComponent<SelectionClickHandler>().RemoveRightClick(DeselectFromIcon);
                    Destroy(icon);
                    return;
                }
            }
        }

        virtual public void SetHighlighted(bool _isHighlighted)
        {
            m_HighlightedEffect.SetActive(_isHighlighted);
        }
    
        virtual public void TakeDamage(int _incomingDamage)
        {
            if (_incomingDamage >= m_UnivStats.m_CurrentHealth)
            {
                HandleDeath();
            }
            m_UnivStats.m_CurrentHealth = Mathf.Clamp(m_UnivStats.m_CurrentHealth - _incomingDamage, 0, m_UnivStats.m_MaxHealth);
            foreach (var pair in m_UISelectionIcons)
            {
                pair.Value.GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, (float)m_UnivStats.m_CurrentHealth / (float)m_UnivStats.m_MaxHealth);
            }

        }

        void SelectFromIcon(GameObject _obj)
        {
            foreach(var pair in m_UISelectionIcons)
            {
                if (pair.Value == _obj)
                {
                    Players.PlayerManager.Instance.PlayerList[pair.Key].Selector.DeselectAllOthers(this);
                    break;
                }
            }
        }

        void DeselectFromIcon(GameObject _obj)
        {
            foreach (var pair in m_UISelectionIcons)
            {
                if (pair.Value == _obj)
                {
                    Players.PlayerManager.Instance.PlayerList[pair.Key].Selector.DeselectSingle(this);
                    break;
                }
            }
        }

        virtual protected void OnDestroy()
        {
        }
    
        virtual protected void HandleDeath()
        {
            OnDeathDelegateEvent(this);

            foreach (var pair in m_UISelectionIcons)
            {
                pair.Value.GetComponent<SelectionClickHandler>().RemoveDoubleClick(SelectFromIcon);
                Destroy(pair.Value);
            }
            m_UISelectionIcons.Clear();
            
            if(this.TargetGoal != null)
                TargetGoal.RemoveOwner(this);
            Destroy(gameObject);
        }
    }
    
    [System.Serializable]
    public struct UniversalStats
    {
        public int m_Cost;
        public float m_BuildTime;

        public int m_MaxHealth;
        public int m_CurrentHealth;
    }

}