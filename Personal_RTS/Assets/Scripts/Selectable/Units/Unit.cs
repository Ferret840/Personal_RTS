using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Players;
using Pathing;

namespace Selectable
{
    namespace Units
    {

        [RequireComponent(typeof(Collider))]
        public class Unit : Owner
        {
            const float m_s_ENDDIRECTION = 69.0f;
            const float m_s_STUCKDIRECTION = -69.0f;

            [SerializeField]
            UnitMovementVariables m_Stats;

            public UnitMovementVariables GetUnitStats()
            {
                return m_Stats;
            }
            public void SetUnitStats(UnitMovementVariables _value)
            {
                m_Stats = _value;
            }

            private void Start()
            {
                UpdateMinimapLayer();
                PlayerManager.Instance.PlayerList[PlayerNumber].Selector.AddUnit(this);

                SetSelectionSlicedMeshSizes(m_SelectedEffect.GetComponent<SlicedMesh>());
                SetSelectionSlicedMeshSizes(m_HighlightedEffect.GetComponent<SlicedMesh>());
                SetChildGlobalScale(m_HighlightedEffect.transform, Vector3.one);
                SetChildGlobalScale(m_SelectedEffect.transform, Vector3.one);
                //StartCoroutine(UpdatePath());
                //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
            }

            private void Update()
            {
                //if (IsSelected && Input.GetMouseButtonDown(1))
                //{
                //    SetMoveLocation();
                //}

                if (m_IsSelected && Input.GetKeyDown(KeyCode.Delete))
                {
                    TakeDamage(1);
                }
            }

            override protected void SetSelectionSlicedMeshSizes(SlicedMesh _sliced)
            {
                _sliced.BorderVertical = 0f;
                _sliced.BorderHorizontal = 0f;

                _sliced.Height = transform.lossyScale.y + 1.0f;
                _sliced.Width = transform.lossyScale.x + 1.0f;

                _sliced.MarginVertical = 0f;
                _sliced.MarginHorizontal = 0f;
            }

            override protected void SetChildGlobalScale(Transform _transform, Vector3 _globalScale)
            {
                _transform.localScale = Vector3.one;
                _transform.localScale = new Vector3(_globalScale.x / transform.lossyScale.x, _globalScale.y / transform.lossyScale.y, _globalScale.z / transform.lossyScale.z);
            }

            override public void OnRightMouse()
            {
                base.OnRightMouse();
                StopCoroutine("FollowPath");
                if(CurrentGoal != null)
                    StartCoroutine("FollowPath");
            }

            IEnumerator FollowPath()
            {
                float moveDirection = CurrentGoal.GetDirFromPosition(transform.position);

                Rigidbody rigid = GetComponent<Rigidbody>();
                CapsuleCollider collider = GetComponent<CapsuleCollider>();
                rigid.velocity = Vector3.zero;
                //rigid.mass *= 10;
                rigid.drag = 10;

                while (moveDirection == m_s_STUCKDIRECTION)
                {
                    yield return null;

                    moveDirection = CurrentGoal.GetDirFromPosition(transform.position);
                }
                //rigid.mass /= 10;
                rigid.drag = 0;

                while (moveDirection != m_s_ENDDIRECTION || CurrentGoal.IsTargetingUnit())
                {
                    if (moveDirection == m_s_STUCKDIRECTION)
                    {
                        yield return null;
                        moveDirection = CurrentGoal.GetDirFromPosition(transform.position);
                        continue;
                    }

                    if (moveDirection == int.MaxValue)
                        break;

                    if (m_Stats.m_MovementType == 0)
                    {
                        OriginalMovement(moveDirection, rigid); // Needs lower Speed 
                    }
                    else if (m_Stats.m_MovementType == 2)
                    {
                        OriginalMovementRotationFix(moveDirection, rigid); // Needs lower speed
                    }
                    else if (m_Stats.m_MovementType == 2)
                    {
                        NewMovement(moveDirection, rigid); // Needs higher speed
                    }
                    else if (m_Stats.m_MovementType == 3)
                    {
                        DeltaTimeMovement(moveDirection, rigid); // Needs higher speed and VERY high TurnSpeed
                    }

                    moveDirection = CurrentGoal.GetDirFromPosition(transform.position);

                    yield return null;

                    if (CurrentGoal.IsTargetingUnit())
                    {
                        continue;
                    }

                    if (collider.bounds.SqrDistance(CurrentGoal.Position) < collider.radius)
                        break;
                }
                rigid.velocity = Vector3.zero;
                //rigid.mass *= 10;
                rigid.drag = 10;
            }

            override public void Deselect()
            {
                base.Deselect();
                //owner.SelectedEffect.SetActive(false);
                //owner.IsSelected = false;
            }

            override public void Select()
            {
                base.Select();
                //owner.SelectedEffect.SetActive(true);
                //owner.IsSelected = true;
            }

            override public void SetHighlighted(bool _isHighlighted)
            {
                base.SetHighlighted(_isHighlighted);
                //owner.HighlightedEffect.SetActive(IsHighlighted);
            }

            protected override void HandleDeath()
            {
                PlayerManager.Instance.PlayerList[PlayerNumber].Selector.RemoveUnit(this);
                base.HandleDeath();
            }

            public void OnDrawGizmos()
            {

            }

            private void NewMovement(float _moveDirection, Rigidbody _rigid)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, _moveDirection, 0), m_Stats.m_TurnSpeed);

                _rigid.AddForce(transform.forward * m_Stats.m_Acceleration, ForceMode.Acceleration);
                _rigid.velocity = Vector3.ClampMagnitude(_rigid.velocity, m_Stats.m_Speed);
            }

            private void DeltaTimeMovement(float _moveDirection, Rigidbody _rigid)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, _moveDirection, 0), m_Stats.m_TurnSpeed * Time.deltaTime);
                float angleDifference = Quaternion.Angle(transform.rotation, Quaternion.Euler(0, _moveDirection, 0));
                if (angleDifference > m_Stats.m_TurnSpeed)
                {
                    _rigid.velocity = Vector3.zero;
                }
                else
                {
                    _rigid.AddForce(transform.forward * m_Stats.m_Acceleration, ForceMode.Acceleration);
                    _rigid.velocity = Vector3.ClampMagnitude(_rigid.velocity, m_Stats.m_Speed);
                }
            }

            private void OriginalMovement(float _moveDirection, Rigidbody _rigid)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, _moveDirection, 0), m_Stats.m_TurnSpeed);

                _rigid.AddForce(transform.forward * m_Stats.m_Speed, ForceMode.VelocityChange);
                _rigid.velocity = _rigid.velocity.normalized * m_Stats.m_Speed * Time.deltaTime;
            }

            private void OriginalMovementRotationFix(float _moveDirection, Rigidbody _rigid)
            {
                Vector3 direction = new Vector3(Mathf.Sin(_moveDirection * Mathf.Deg2Rad), 0, Mathf.Cos(_moveDirection * Mathf.Deg2Rad));
                Vector3 targetPosition = transform.position + direction;
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_Stats.m_TurnSpeed);

                _rigid.AddForce(transform.forward * m_Stats.m_Speed, ForceMode.VelocityChange);
                _rigid.velocity = _rigid.velocity.normalized * m_Stats.m_Speed * Time.deltaTime;
            }
        }

        [System.Serializable]
        public struct UnitMovementVariables
        {
            const float m_s_MinPathUpdateTime = .2f;
            const float m_s_PathUpdateMoveThreshold = .5f;

            public short m_Dimension;

            public float m_Speed;
            public float m_Acceleration;
            public float m_TurnDist;
            public float m_TurnSpeed;
            public float m_StoppingDist;

            public int m_MovementType;
        }

        [System.Serializable]
        public struct UnitWeapons
        {
            
        }

        [System.Serializable]
        public struct UnitDurabilityVariables
        {
            const float m_s_MinPathUpdateTime = .2f;
            const float m_s_PathUpdateMoveThreshold = .5f;

            public short m_Dimension;

            public float m_Speed;
            public float m_TurnDist;
            public float m_TurnSpeed;
            public float m_StoppingDist;
        }
    }
}