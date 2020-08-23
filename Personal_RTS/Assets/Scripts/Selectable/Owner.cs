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
    
    
        public delegate void OnDied(Owner owner);
        public event OnDied onDied;
    
        public LayerMask ValidMovementLayers;
        public Goal TargetGoal
        {
            get;
            private set;
        }

        Dictionary<int, GameObject> UISelectionIcons = new Dictionary<int, GameObject>();
    
        // Use this for initialization
        void Awake()
        {
            PlayerNumber = EditorSetPlayerNum;
        }
    
        // Use this for initialization
        void Start()
        {
    
        }
    
        // Update is called once per frame
        void Update()
        {
    
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
            UISelectionIcons.Add(playerNum, icon);
            icon.GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, (float)univStats.CurrentHealth / (float)univStats.MaxHealth);
        }

        virtual public void RemoveSelectionIcon(int playerNum)
        {
            GameObject icon = UISelectionIcons[playerNum];
            UISelectionIcons.Remove(playerNum);
            Destroy(icon);
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
            foreach (var icon in UISelectionIcons)
            {
                icon.Value.GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, (float)univStats.CurrentHealth / (float)univStats.MaxHealth);
            }
        }
    
        virtual protected void OnDestroy()
        {
        }
    
        virtual protected void HandleDeath()
        {
            onDied(this);
            foreach (var icon in UISelectionIcons)
            {
                RemoveSelectionIcon(icon.Key);
            }
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