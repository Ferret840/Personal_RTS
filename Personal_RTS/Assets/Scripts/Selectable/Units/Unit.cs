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
            UnitVariables m_Stats;

            public UnitVariables GetUnitStats()
            {
                return m_Stats;
            }
            public void SetUnitStats(UnitVariables _value)
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
                if(TargetGoal != null)
                    StartCoroutine("FollowPath");
            }

            IEnumerator FollowPath()
            {
                float moveDirection = TargetGoal.GetDirFromPosition(transform.position);

                Rigidbody rigid = GetComponent<Rigidbody>();
                CapsuleCollider collider = GetComponent<CapsuleCollider>();
                rigid.velocity = Vector3.zero;
                //rigid.mass *= 10;
                rigid.drag = 10;

                while (moveDirection == m_s_STUCKDIRECTION)
                {
                    yield return null;

                    moveDirection = TargetGoal.GetDirFromPosition(transform.position);
                }
                //rigid.mass /= 10;
                rigid.drag = 0;

                while (moveDirection != m_s_ENDDIRECTION)
                {
                    if (moveDirection == m_s_STUCKDIRECTION)
                    {
                        yield return null;
                        moveDirection = TargetGoal.GetDirFromPosition(transform.position);
                        continue;
                    }

                    if (moveDirection == int.MaxValue)
                        break;

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, moveDirection, 0), m_Stats.m_TurnSpeed);
                    //transform.rotation = Quaternion.Euler(0, moveDirection, 0);

                    rigid.AddForce(transform.forward * m_Stats.m_Speed, ForceMode.VelocityChange);
                    rigid.velocity = rigid.velocity.normalized * m_Stats.m_Speed * Time.deltaTime;
                    //transform.Translate(Vector3.forward * Time.deltaTime * stats.speed, Space.Self);

                    moveDirection = TargetGoal.GetDirFromPosition(transform.position);

                    yield return null;

                    if (collider.bounds.SqrDistance(TargetGoal.Position) < collider.radius)
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
        }

        [System.Serializable]
        public struct UnitVariables
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