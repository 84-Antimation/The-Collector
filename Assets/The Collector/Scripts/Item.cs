using UnityEngine;

namespace FoxX.TheCollector
{
    [RequireComponent(typeof(ItemSelector))]
    public class Item : MonoBehaviour
    {
        [Header(" Components ")]
        private ItemSelector _selector;

        [Header(" Settings ")]
        private EItemState State;

        [Header(" Data ")]
        private ItemSpot _spot;
        public ItemSpot Spot => _spot;

        [SerializeField] private Sprite _icon;
        public Sprite Icon => _icon;

        private void Awake()
        {
            _selector = GetComponent<ItemSelector>();
            State = EItemState.Active;
        }

        public void AssignSpot(ItemSpot spot)
            => this._spot = spot;

        public void UnassignSpot() 
            => _spot = null;

        public void SetState(EItemState state)
            => this.State = state;

        public void DisablePhysics()
        {
            // Disable the collider
            Collider collider = GetComponentInChildren<Collider>();
            collider.enabled = false;

            // Turn it as Kinematic
            Rigidbody rig = GetComponent<Rigidbody>();
            rig.isKinematic = true;
        }

        public void EnablePhysics()
        {
            // Disable the collider
            Collider collider = GetComponentInChildren<Collider>();
            collider.enabled = transform;

            // Turn it as Kinematic
            Rigidbody rig = GetComponent<Rigidbody>();
            rig.isKinematic = false;
        }

        public void Select(Material outlineMaterial)
        {
            _selector.Select(outlineMaterial);
        }

        public void Deselect()
        {
            _selector.Deselect();
        }

        public void DisableShadows()
        {
            _selector.DisableShadows();
        }

        public void EnableShadows()
        {
            _selector.EnableShadows();
        }

        public bool CanBeSelected()
        {
            return State == EItemState.Active;
        }

        public bool IsOnSpot()
            => State == EItemState.OnSpot;

        public void ApplyRandomForce(float magnitude)
        {
            Rigidbody rig = GetComponent<Rigidbody>();
            rig.AddForce(Random.onUnitSphere * magnitude, ForceMode.VelocityChange);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;

            float height = .1f;
            Gizmos.DrawCube(transform.position - Vector3.up * height / 2f, new Vector3(.5f, height, .5f));
        }
    }
}