using UnityEngine;

namespace Entities
{
    public class Player : Unit
    {

        [Header("Editor References")]
        [SerializeField] private GameObject playerPrefab;

        private GameObject playerRoot;
        private GameObject playerModel;
        private InputController inputController;
        private CameraController cameraController;

        protected override void Awake()
        {
            base.Awake();

            inputController = GetComponent<InputController>();
            cameraController = GetComponent<CameraController>();
        }

        private void Start()
        {
            playerRoot = new GameObject("PlayerRoot");

            playerModel = Instantiate(playerPrefab, playerRoot.transform);
            playerModel.name = "PlayerModel";

            if (cameraController != null)
            {
                cameraController.AttachToPlayer(playerRoot.transform);
            }
        }

        public void SelectUnit(ISelectable selectable)
        {
            if (GetCurrentTarget() == selectable as Unit)
                return;

            DeselectCurrentUnit();

            SetCurrentTarget(selectable as Unit);
            selectable?.OnSelected();
        }

        public void DeselectCurrentUnit()
        {
            if (GetCurrentTarget() == null)
                return;

            (GetCurrentTarget() as ISelectable)?.OnDeselected();
            SetCurrentTarget(null);
        }

        public Transform GetPlayerTransform()
        {
            return playerRoot != null ? playerRoot.transform : null;
        }

        public Transform GetPlayerModelTransform()
        {
            return playerModel != null ? playerModel.transform : null;
        }

        public Vector3 GetPlayerPosition()
        {
            return playerRoot != null ? playerRoot.transform.position : Vector3.zero;
        }
    }
}
