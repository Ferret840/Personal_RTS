using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachines
{
    namespace UIStates
    {
        using UIScenes;
        public interface I_SkirmishController
        {

        }

        class SkirmishState : UIBaseState, I_SkirmishController
        {
            //SkirmishUI m_SkirmishUI;

            public override void OnEnter()
            {
                base.OnEnter();

                if (LoadScene())
                {

                }
                else
                {
                    //m_SkirmishUI = new SkirmishUI(this);
                }
            }

            public override void OnUpdate()
            {
                //base.OnUpdate();
            }

            public override void OnExit()
            {
                //m_MainMenu = null;
                base.OnExit();
            }
        }
    }
}
