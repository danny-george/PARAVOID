﻿using System;
using UnityEngine;

namespace Fragsurf.Movement
{
    //test
    
    /// <summary>
    /// Easily add a surfable character to the scene
    /// </summary>
    [AddComponentMenu("Fragsurf/Surf Character")]
    public class SurfCharacter : MonoBehaviour, ISurfControllable
    {

        public enum ColliderType
        {
            Capsule,
            Box
        }

        ///// Fields /////

        [Header("Physics Settings")]
        public int TickRate = 128;
        public Vector3 ColliderSize = new Vector3(1, 2, 1);
        public ColliderType CollisionType;

        [Header("View Settings")]
        public Transform viewTransform;
        public Vector3 ViewOffset = new Vector3(0, 0.61f, 0);

        [Header("Input Settings")]
        public float XSens = 50;
        public float YSens = 50;
        public PlayerControls playerControls;

        [Header("Movement Config")]
        [SerializeField]
        public MovementConfig moveConfig;

        private GameObject _groundObject;
        private Vector3 _baseVelocity;
        private Collider _collider;
        private Vector3 _angles;
        private Vector3 _startPosition;

        private MoveData _moveData = new MoveData();
        private SurfController _controller = new SurfController();

        ///// Properties /////

        public MoveType MoveType
        {
            get { return MoveType.Walk; }
        }

        public MovementConfig MoveConfig
        {
            get { return moveConfig; }
        }

        public MoveData MoveData
        {
            get { return _moveData; }
        }

        public Collider Collider
        {
            get { return _collider; }
        }

        public GameObject GroundObject
        {
            get { return _groundObject; }
            set { _groundObject = value; }
        }

        public Vector3 BaseVelocity
        {
            get { return _baseVelocity; }
        }

        public Vector3 Forward
        {
            get { return viewTransform.forward; }
        }

        public Vector3 Right
        {
            get { return viewTransform.right; }
        }

        public Vector3 Up
        {
            get { return viewTransform.up; }
        }

        ///// Methods /////

        private void Awake()
        {
            playerControls = new PlayerControls();
            Application.targetFrameRate = 144;
            QualitySettings.vSyncCount = 1;

            Time.fixedDeltaTime = 1f / TickRate;
        }

        void OnEnable()
        {
            playerControls.Enable();
        }    

        void OnDisable()
        {
            playerControls.Disable();
        }

        public Vector2 getPlayerMoveVector()
        {
            return playerControls.Player.Move.ReadValue<Vector2>();
        }

        public Vector2 getMouseDeltaVector()
        {
            return playerControls.Player.Look.ReadValue<Vector2>();
        }

        public bool PlayerJumped()
        {
            return playerControls.Player.Jump.triggered;
        }

        private void Start()
        {
            if(viewTransform == null)
                viewTransform = Camera.main.transform;
            viewTransform.SetParent(transform, false);
            viewTransform.localPosition = ViewOffset;
            viewTransform.localRotation = transform.rotation;

            _collider = gameObject.GetComponent<Collider>();

            if (_collider != null)
                GameObject.Destroy(_collider);

            // rigidbody is required to collide with triggers
            var rbody = gameObject.GetComponent<Rigidbody>();
            if (rbody == null)
                rbody = gameObject.AddComponent<Rigidbody>();
            rbody.isKinematic = true;

            switch(CollisionType)
            {
                case ColliderType.Box:
                    _collider = gameObject.AddComponent<BoxCollider>();
                    var boxc = (BoxCollider)_collider;
                    boxc.size = ColliderSize;
                    break;

                case ColliderType.Capsule:
                    _collider = gameObject.AddComponent<CapsuleCollider>();
                    var capc = (CapsuleCollider)_collider;
                    capc.height = ColliderSize.y;
                    capc.radius = ColliderSize.x / 2f;
                    break;
            }

            _collider.isTrigger = true;
            _moveData.Origin = transform.position;
            _startPosition = transform.position;
        }

        private void Update()
        {
            UpdateTestBinds();
            UpdateRotation();
            UpdateMoveData();

        }
        public float GetTheVelocity()
        {
            return Mathf.Round(_moveData.Velocity.magnitude);
        }
       public bool accessController()
        {
            return _controller.bruh;
        }
            

        private void UpdateTestBinds()
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                MoveData.Velocity = Vector3.zero;
                MoveData.Origin = _startPosition;
            }
        }

        

        private void FixedUpdate()
        {
            _controller.ProcessMovement(this, moveConfig, Time.fixedDeltaTime);
            transform.position = MoveData.Origin;
        }

        private void UpdateMoveData()
        {

            if (getPlayerMoveVector().x == 0)
                _moveData.SideMove = 0;
            else if (getPlayerMoveVector().x < 0)
                _moveData.SideMove = -MoveConfig.Accel;
            else if (getPlayerMoveVector().x > 0)
                _moveData.SideMove = MoveConfig.Accel;

            if (getPlayerMoveVector().y == 0)
                _moveData.ForwardMove = 0;
            else if (getPlayerMoveVector().y > 0)
                _moveData.ForwardMove = MoveConfig.Accel;
            else if (getPlayerMoveVector().y < 0)
                _moveData.ForwardMove = -MoveConfig.Accel;

            if (PlayerJumped())
                _moveData.Buttons = _moveData.Buttons.AddFlag((int)InputButtons.Jump);
            else
                _moveData.Buttons = _moveData.Buttons.RemoveFlag((int)InputButtons.Jump);

            _moveData.OldButtons = _moveData.Buttons;
            _moveData.ViewAngles = _angles;
        }
        private void LateUpdate()
        {
            viewTransform.rotation = Quaternion.Euler(_angles);
            
        }
        private void UpdateRotation()
        {
            float mx = (Input.GetAxis("Mouse X") * XSens * .02f);
            float my = Input.GetAxis("Mouse Y") * YSens * .02f;
            var rot = _angles + new Vector3(-my, mx, 0f);
            rot.x = Mathf.Clamp(rot.x, -85f, 85f);
            _angles = rot;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float ClampAngle(float angle, float from, float to)
        {
            if (angle < 0f) angle = 360 + angle;
            if (angle > 180f) return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);
        }

    }
}

