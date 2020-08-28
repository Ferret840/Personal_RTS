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
    
        public int EditorSetPlayerNum;
    
        [SerializeField]
        protected UniversalStats univStats;

        public GameObject SelectionIconPrefab;
        [SerializeField]
        protected Sprite image;
        public Sprite getImage
        {
            get
            {
                return image;
            }
        }
    
        [System.NonSerialized]
        public bool IsSelected = false;
    
        public GameObject SelectedEffect;
        public GameObject HighlightedEffect;
        
        public GameObject MinimapObject;
    
        public delegate void OnDied(Owner owner);
        public event OnDied onDied;
    
        public LayerMask ValidMovementLayers;
        public Goal TargetGoal
        {
            get;
            private set;
        }

        List<KeyValuePair<int, GameObject>> UISelectionIcons = new List<KeyValuePair<int, GameObject>>();
    
        // Use this for initialization
        void Awake()
        {
            PlayerNumber = EditorSetPlayerNum;
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

        virtual protected void UpdateMinimapLayer()
        {
            MinimapObject.layer = gameObject.layer + 3;
        }
    
        virtual public void OnRightMouse()
        {
    
        }
    
        virtual public void SetTargetGoal(Goal _targetGoal)
        {
            if (TargetGoal != null)
                TargetGoal.RemoveOwner(gameObject.GetInstanceID());
    
            TargetGoal = _targetGoal;
            TargetGoal.AddOwner(gameObject.GetInstanceID());
        }
    
        /// <summary>
        /// Overrideable function for handling when a key is pressed
        /// </summary>
        /// <param name="k">An int within Keycode corresponding to the key that was pressed</param>
        virtual public void OnKeyDown(KeyCode k)
        {
    
        }
    
        /// <summary>
        /// Overrideable function for handling when a key is released
        /// </summary>
        /// <param name="k">An int within Keycode corresponding to the key that was released</param>
        virtual public void OnKeyUp(KeyCode k)
        {
    
        }
    
        virtual public void Deselect()
        {
            SelectedEffect.SetActive(false);
            IsSelected = false;
        }
    
        virtual public void Select()
        {
            SelectedEffect.SetActive(true);
            IsSelected = true;
        }

        virtual public void AddSelectionIcon(int playerNum, GameObject icon)
        {
            UISelectionIcons.Add(new KeyValuePair<int, GameObject>(playerNum, icon));
            icon.GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, (float)univStats.CurrentHealth / (float)univStats.MaxHealth);
            icon.GetComponent<SelectionClickHandler>().AddDoubleClick(SelectFromIcon);
            icon.GetComponent<SelectionClickHandler>().AddRightClick(DeselectFromIcon);
        }

        virtual public void RemoveSelectionIcon(int playerNum)
        {
            foreach (var pair in UISelectionIcons)
            {
                if (pair.Key == playerNum)
                {
                    GameObject icon = pair.Value;
                    UISelectionIcons.Remove(pair);
                    icon.GetComponent<SelectionClickHandler>().RemoveDoubleClick(SelectFromIcon);
                    icon.GetComponent<SelectionClickHandler>().RemoveRightClick(DeselectFromIcon);
                    Destroy(icon);
                    return;
                }
            }
        }

        virtual public void SetHighlighted(bool IsHighlighted)
        {
            HighlightedEffect.SetActive(IsHighlighted);
        }
    
        virtual public void TakeDamage(int incomingDamage)
        {
            if (incomingDamage >= univStats.CurrentHealth)
            {
                HandleDeath();
            }
            univStats.CurrentHealth = Mathf.Clamp(univStats.CurrentHealth - incomingDamage, 0, univStats.MaxHealth);
            foreach (var pair in UISelectionIcons)
            {
                pair.Value.GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, (float)univStats.CurrentHealth / (float)univStats.MaxHealth);
            }

        }

        void SelectFromIcon(GameObject obj)
        {
            foreach(var pair in UISelectionIcons)
            {
                if (pair.Value == obj)
                {
                    Players.PlayerManager.Instance.PlayerList[pair.Key].Selector.DeselectAllOthers(this);
                    break;
                }
            }
        }

        void DeselectFromIcon(GameObject obj)
        {
            foreach (var pair in UISelectionIcons)
            {
                if (pair.Value == obj)
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
            onDied(this);

            foreach (var pair in UISelectionIcons)
            {
                pair.Value.GetComponent<SelectionClickHandler>().RemoveDoubleClick(SelectFromIcon);
                Destroy(pair.Value);
            }
            UISelectionIcons.Clear();
            
            if(this.TargetGoal != null)
                TargetGoal.RemoveOwner(gameObject.GetInstanceID());
            Destroy(gameObject);
        }
    }
    
    [System.Serializable]
    public struct UniversalStats
    {
        public int Cost;
        public float BuildTime;

        public int MaxHealth;
        public int CurrentHealth;
    }

}