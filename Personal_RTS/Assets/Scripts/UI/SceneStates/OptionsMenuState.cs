using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachines
{
    namespace UIStates
    {
        class OptionsMenuState : UIBaseState
        {
            public override void OnEnter()
            {
                base.OnEnter();
            }

            public override void OnUpdate()
            {
                GetMachine().ChangeState<MainMenuState>();
                base.OnUpdate();
            }

            public override void OnExit()
            {
                base.OnExit();
            }
        }
    }
}