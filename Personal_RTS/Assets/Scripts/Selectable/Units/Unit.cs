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
            //const float minPathUpdateTime = .2f;
            //const float pathUpdateMoveThreshold = .5f;
            //
            //public short dimension = 0;
            //
            //public Transform target;
            //public float speed = 5;
            //public float turnDist = 5;
            //public float turnSpeed = 3;
            //public float stoppingDist = 10;
            //

            PathRequest pathingRequest;

            [SerializeField]
            UnitVariables stats;

            public UnitVariables GetUnitStats
            {
                get
                {
                    return stats;
                }
            }
            public UnitVariables SetUnitStats
            {
                set
                {
                    stats = value;
                }
            }

            private void Start()
            {
                PlayerManager.instance.PlayerList[PlayerNumber].Selector.AddUnit(this);
                //StartCoroutine(UpdatePath());
                //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
            }

            private void Update()
            {
                //if (IsSelected && Input.GetMouseButtonDown(1))
                //{
                //    SetMoveLocation();
                //}

                if (IsSelected && Input.GetKeyDown(KeyCode.Delete))
                {
                    TakeDamage(1);
                }
            }

            override public void OnRightMouse()
            {
                base.OnRightMouse();
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }

            IEnumerator FollowPath()
            {
                float moveDirection = TargetGoal.GetDirFromPosition(transform.position);

                while (moveDirection != 1000f)
                {
                    if (moveDirection == ushort.MaxValue)
                        break;

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, moveDirection, 0), stats.turnSpeed);
                    //transform.rotation = Quaternion.Euler(0, moveDirection, 0);

                    transform.Translate(Vector3.forward * Time.deltaTime * stats.speed, Space.Self);

                    moveDirection = TargetGoal.GetDirFromPosition(transform.position);

                    yield return null;
                }
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

            override public void SetHighlighted(bool IsHighlighted)
            {
                base.SetHighlighted(IsHighlighted);
                //owner.HighlightedEffect.SetActive(IsHighlighted);
            }

            protected override void HandleDeath()
            {
                PlayerManager.instance.PlayerList[PlayerNumber].Selector.RemoveUnit(this);
                base.HandleDeath();
            }

            public void OnDrawGizmos()
            {

            }
        }

        [System.Serializable]
        public struct UnitVariables
        {
            const float minPathUpdateTime = .2f;
            const float pathUpdateMoveThreshold = .5f;

            public short dimension;

            public float speed;
            public float turnDist;
            public float turnSpeed;
            public float stoppingDist;
        }

    }
}