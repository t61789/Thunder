using System;
using System.Linq;
using Framework;
using Thunder;
using UnityEngine;

namespace Thunder
{
    [RequireComponent(typeof(FastKineticEnergyLauncher))]
    public class ProtoEnemy : BaseEntity
    {
        public float AlertRange = 5;
        public float EscapeRange = 10;
        public float PlayerVisibleCheckInterval = 0.5f;
        public float AimTime = 1;
        public event Action OnAimmingStart;

        private SemiAutoCounter _AimCounter;
        private Status _Status;
        private Collider _SearchCollider;
        private Player _Player;
        private float _AlertRangeSq;
        private float _EscapeRangeSq;

        protected override void Awake()
        {
            base.Awake();
            _SearchCollider = GetComponents<Collider>().First(x => x.isTrigger);

            _AlertRangeSq = AlertRange * AlertRange;
            _EscapeRangeSq = EscapeRange * EscapeRange;

            _AimCounter = new SemiAutoCounter(AimTime);

            InstructionBalancing.AddAction(this, PlayerShootableRayCast, PlayerVisibleCheckInterval);
        }

        private void FixedUpdate()
        {
            _AimCounter.FixedUpdate();

            PlayerStatusUpdate();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (_Status != Status.Idle) return;
            if ((_Player = collider.GetComponent<Player>()) == null)
                return;
            _SearchCollider.enabled = false;
        }

        private void OnDisable()
        {
            InstructionBalancing.RemoveAction(this);
        }

        private void PlayerStatusUpdate()
        {
            if (_Status != Status.Idle)
            {
                if ((Trans.position - _Player.Trans.position).sqrMagnitude > EscapeRange)
                {
                    _Player = null;
                    _SearchCollider.enabled = true;
                    _Status = Status.Idle;
                }
            }
        }

        private void PlayerShootableRayCast()
        {
            var ray = new Ray(
                Trans.position,
                _Player.Trans.position - Trans.position);
            if (Physics.Raycast(ray, out var hitInfo))
            {

            }
        }

        private enum Status
        {
            Idle,
            Searching,
            Aimming,
            Shooting
        }
    }
}