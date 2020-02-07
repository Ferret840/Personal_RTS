using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable;

namespace Players
{

    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour
    {
        public int PlayerNumber;
        Camera cam;

        LayerMask view;

        int viewIndex = 1;

        public LayerMask[] dimensions = new LayerMask[3];

        Vector3 mouseMoved;

        public Material mat;

        public float EffectMaxDuration = 1f;
        float visualEffectDuration = 0f;

        // Use this for initialization
        void Start()
        {
            cam = GetComponent<Camera>();
            cam.targetDisplay = PlayerNumber;

            view = dimensions[viewIndex];
        }

        // Update is called once per frame
        void Update()
        {
            HandleInput();
        }

        void HandleInput()
        {
            if (Input.GetButtonDown("ViewDimension1"))
            {
                viewIndex = 0;
            }
            else if (Input.GetButtonDown("ViewDimension2"))
            {
                viewIndex = 2;
            }
            else if (Input.GetButtonDown("ViewDimension3"))
            {
                viewIndex = 1;
            }
            else if (Input.GetButtonDown("StepDimensionView"))
            {
                if (Input.GetAxisRaw("StepDimensionView") > 0)
                    ++viewIndex;
                else
                    --viewIndex;

                if (viewIndex > 2)
                    viewIndex = 0;
                else if (viewIndex < 0)
                    viewIndex = 2;
            }
            else if (Input.GetButtonDown("ChangeViewToSelection"))
            {
                int selectedLayer = 1;

                foreach (Owner o in PlayerManager.instance.PlayerList[PlayerNumber].Selector.selectedObjects)
                {
                    selectedLayer |= (1 << o.gameObject.layer);
                }

                if (selectedLayer == 1)
                    selectedLayer = view;
                else if ((selectedLayer & (1 << 8)) != 0 || (selectedLayer & (1 << 9)) != 0)
                {
                    selectedLayer |= 1 << 10;
                }
                else if ((selectedLayer & (1 << 10)) != 0)
                {
                    selectedLayer = dimensions[1];
                }

                if (selectedLayer == dimensions[0])
                    viewIndex = 0;
                else if (selectedLayer == dimensions[1])
                    viewIndex = 1;
                else if (selectedLayer == dimensions[2])
                    viewIndex = 2;
            }
            else if (Input.GetButtonDown("HoldDragMoveCamera"))
            {
                mouseMoved = Vector3.zero;
            }
            else if (Input.GetButton("HoldDragMoveCamera"))
            {
                mouseMoved.x += Input.GetAxis("MouseMoveX");
                mouseMoved.z += Input.GetAxis("MouseMoveY");

                transform.position += mouseMoved;
            }
            else if (Input.GetAxis("Zoom") != 0)
            {
                transform.position += transform.forward * Input.GetAxis("Zoom");
            }


            if (view != dimensions[viewIndex])
            {
                view = dimensions[viewIndex];

                visualEffectDuration = EffectMaxDuration;

                StopCoroutine("ChangeViewEffect");
                StartCoroutine("ChangeViewEffect");
            }
        }

        IEnumerator ChangeViewEffect()
        {
            float i = 0;

            for (i = 0; i < EffectMaxDuration / 2; i += Time.deltaTime, visualEffectDuration -= Time.deltaTime)
            {
                mat.SetFloat("_Oscillater", Mathf.Lerp(0, 1, i / EffectMaxDuration * 2));

                yield return null;
            }

            cam.cullingMask = view;

            for (i = 0; i < EffectMaxDuration / 2; i += Time.deltaTime, visualEffectDuration -= Time.deltaTime)
            {
                mat.SetFloat("_Oscillater", Mathf.Lerp(1, 0, i / EffectMaxDuration * 2));

                yield return null;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (visualEffectDuration > 0f)
            {
                Graphics.Blit(source, destination, mat);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
    }

}