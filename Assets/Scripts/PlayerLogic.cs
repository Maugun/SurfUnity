using UnityEngine;
using UnityEngine.UI;

namespace P90brush
{
    public class PlayerLogic : MonoBehaviour, ISurfControllable
    {
        public float _walkSpeed = 80f;
        public float _jumpForce = 40f;

        [Header("Physics Settings")]
        public int _tickRate = 128;
        public Camera _fpsCamera;


        [Header("UI")]
        public Text _debugText;

        [Header("Movement Config")]
        [SerializeField]
        public MovementConfig moveConfig = new MovementConfig();

        [Header("Hookshot")]
        [SerializeField]
        public Hookshot _hookshot;

        [Header("Hookshot")]
        public SlowMotion _slowMotion;


        #region =============================== Private Properties ===============================#

        //Var set in Start() callback
        private Rigidbody _rb;
        private Collider _collider;
        //private Quaternion originalRotation;
        private Vector3 _startPosition;
        private Quaternion _originalRotation;

        //Surf Oject
        private SurfController _controller = new SurfController();

        #endregion ===============================================================================#


        #region =============================== ISurfControllable Impl ===============================#

        public MovementConfig MoveConfig {
            get { return moveConfig; }
        }

        public PlayerData PlayerData { get; } = new PlayerData();

        public InputData InputData { get; } = new InputData();

        public Collider Collider {
            get { return _collider; }
        }

        public Vector3 BaseVelocity { get; }

        public Camera FpsCamera {
            get { return _fpsCamera; }
        }

        #endregion ===================================================================================#

        private void Awake() {
            // Setup V-Sync
            Application.targetFrameRate = 144;
            QualitySettings.vSyncCount = 1;

            // Setup TickRate
            Time.fixedDeltaTime = 1f / _tickRate;
        }

        void Start() {
            // Setup RB
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.freezeRotation = true;//Encore utile ??

            // Setup Collider
            _collider = gameObject.GetComponent<Collider>();
            _collider.isTrigger = true;

            // Setup Spawn Point & Rotation
            PlayerData.Origin = transform.position;
            _startPosition = transform.position;
            _originalRotation = transform.localRotation;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update() {
            // UI
            UpdateUI();
            if (PauseMenu.GameIsPaused) {
                return;
            }

            // Updates
            InputData.Update(moveConfig);
            UpdateHook();
            UpdateSlowMotion();
            UpdateViewAngle();
        }

        void FixedUpdate() {
            if (InputData.ResetPressed) {
                PlayerData.Velocity = Vector3.zero;
                PlayerData.Origin = _startPosition;
                _slowMotion.IsSlowMotion = false;
            }

            float fixedDeltaTime = Time.fixedDeltaTime;
            _hookshot.CatchMovement(this, fixedDeltaTime);//Todo: Improve
            _controller.ProcessMovement(this, moveConfig, fixedDeltaTime);

            ApplyPlayerMovement();
        }

        private void LateUpdate() {
            ApplyMouseMovement();
        }

        private void UpdateUI() {
            // TODO add this to graphy
            float magnitude = PlayerData.Velocity.x * PlayerData.Velocity.x + PlayerData.Velocity.z * PlayerData.Velocity.z;
            _debugText.text = string.Format(
                "Velocity: {0}\n" +
                "  x {1}\n" +
                "  y {2}\n" +
                "  z {3}\n" +
                "FlyingStatus: {4}\n" +
                "Hooked: {5} {6}",
                magnitude,
                PlayerData.Velocity.x.ToString("F2"), PlayerData.Velocity.y.ToString("F2"), PlayerData.Velocity.z.ToString("F2"),
                PlayerData.IsGrounded() ? "OnGround" : "Fly/Surfing",
                PlayerData.IsHooked() ? Vector3.Distance(PlayerData.Origin, PlayerData.HookedPosition).ToString("F2") : "-1",
                _hookshot.GetStatus() //Todo: Improve
                );
        }

        private void UpdateHook()
        {
            if (!InputData.HookPressedLastUpdate && InputData.HookPressed) //Player has Started to Press to the Hook Button
                _hookshot.TriggerHook(this);
            _hookshot.CheckForRelease(this);
        }

        private void UpdateSlowMotion()
        {
            if (!_slowMotion.IsSlowMotion && InputData.SlowMotionPressed)
                _slowMotion.IsSlowMotion = true;
        }

        private void UpdateViewAngle()
        {
            var rot = PlayerData.ViewAngles + new Vector3(-InputData.MouseY, InputData.MouseX, 0f);
            rot.x = GameUtils.ClampAngle(rot.x, -85f, 85f);
            PlayerData.ViewAngles = rot;
        }

        private void ApplyPlayerMovement() {
            transform.position = PlayerData.Origin;
        }

        private void ApplyMouseMovement() {
            // Get the rotation you will be at next as a Quaternion
            Quaternion yQuaternion = Quaternion.AngleAxis(PlayerData.ViewAngles.x, Vector3.right);
            Quaternion xQuaternion = Quaternion.AngleAxis(PlayerData.ViewAngles.y, Vector3.up);

            // Rotate the rigidbody for horizontal move
            transform.localRotation = xQuaternion;

            // Rotate the attached camera for vertival move
            _fpsCamera.transform.localRotation = yQuaternion;
        }
    }
}