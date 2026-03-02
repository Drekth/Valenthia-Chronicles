using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Entities
{
    public interface ISelectable
    {
        void OnSelected();
        void OnDeselected();
        bool IsSelectable();
    }
    
    public abstract class Unit : MonoBehaviour, ISelectable
    {
        [Header("Selection")]
        [SerializeField] private DecalProjector selectionDecal;

        private Unit currentTarget;
        protected UnitStats stats;

        public int CurrentHealth => stats != null ? stats.CurrentHealth : 0;
        public int CurrentMana => stats != null ? stats.CurrentMana : 0;
        public int MaxHealth => stats != null ? stats.MaxHealth : 0;
        public int MaxMana => stats != null ? stats.MaxMana : 0;

        public UnitStats Stats => stats;

        public Unit GetCurrentTarget() => currentTarget;
        public void SetCurrentTarget(Unit target) => currentTarget = target;

        protected virtual void Awake()
        {
            stats = GetComponent<UnitStats>();

            if (selectionDecal != null)
            {
                selectionDecal.enabled = false;
            }
        }

        private void Start()
        {
            if (stats is null)
            {
                Debug.LogError($"Unit '{gameObject.name}' is missing UnitStats component!", this);
                Destroy(this.gameObject);
            }
        }

        public virtual void OnSelected()
        {
            if (selectionDecal != null)
            {
                selectionDecal.enabled = true;
            }
        }

        public virtual void OnDeselected()
        {
            if (selectionDecal != null)
            {
                selectionDecal.enabled = false;
            }
        }

        public virtual bool IsSelectable()
        {
            return true;
        }
    }
}