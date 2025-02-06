using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable;

namespace Players
{

    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour
    {
        public GameObject m_MinimapCanvasPrefab;
        public GameObject m_ResourcesCanvasPrefab;
        public GameObject m_MinimapCameraPrefab;

        public int m_PlayerNumber;
        Camera m_Cam;
        Camera m_MinimapCam;

        LayerMask m_View;
        LayerMask m_MinimapView;

        int m_ViewIndex = 1;

        public LayerMask[] m_Dimensions = new LayerMask[3];
        public LayerMask[] m_MinimapDimensions = new LayerMask[3];

        Vector3 m_MouseMoved;

        public Material m_Material;

        public float m_EffectMaxDuration = 1f;
        float m_VisualEffectDuration = 0f;

        // Use this for initialization
        void Start()
        {
            Instantiate(m_MinimapCanvasPrefab);
            Instantiate(m_ResourcesCanvasPrefab);
            m_MinimapCam = Instantiate(m_MinimapCameraPrefab, new Vector3(50, 100, 50), Quaternion.Euler(90, 0, 0)).GetComponent<Camera>();

            m_Cam = GetComponent<Camera>();
            //m_Cam.targetDisplay = m_PlayerNumber;

            m_View = m_Dimensions[m_ViewIndex];
            m_MinimapView = m_MinimapDimensions[m_ViewIndex];
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
                m_ViewIndex = 0;
            }
            else if (Input.GetButtonDown("ViewDimension2"))
            {
                m_ViewIndex = 2;
            }
            else if (Input.GetButtonDown("ViewDimension3"))
            {
                m_ViewIndex = 1;
            }
            else if (Input.GetButtonDown("StepDimensionView"))
            {
                if (Input.GetAxisRaw("StepDimensionView") > 0)
                    ++m_ViewIndex;
                else
                    --m_ViewIndex;

                if (m_ViewIndex > 2)
                    m_ViewIndex = 0;
                else if (m_ViewIndex < 0)
                    m_ViewIndex = 2;
            }
            else if (Input.GetButtonDown("ChangeViewToSelection"))
            {
                int selectedLayer = 1;

                foreach (Owner o in PlayerManager.Instance.PlayerList[m_PlayerNumber].Selector.SelectedObjects)
                {
                    selectedLayer |= (1 << o.gameObject.layer);
                }

                if (selectedLayer == 1)
                    selectedLayer = m_View;
                else if ((selectedLayer & (1 << 8)) != 0 || (selectedLayer & (1 << 9)) != 0)
                {
                    selectedLayer |= 1 << 10;
                }
                else if ((selectedLayer & (1 << 10)) != 0)
                {
                    selectedLayer = m_Dimensions[1];
                }

                if (selectedLayer == m_Dimensions[0])
                    m_ViewIndex = 0;
                else if (selectedLayer == m_Dimensions[1])
                    m_ViewIndex = 1;
                else if (selectedLayer == m_Dimensions[2])
                    m_ViewIndex = 2;
            }
            else if (Input.GetButtonDown("HoldDragMoveCamera"))
            {
                m_MouseMoved = Vector3.zero;
            }
            else if (Input.GetButton("HoldDragMoveCamera"))
            {
                m_MouseMoved.x += Input.GetAxis("MouseMoveX");
                m_MouseMoved.z += Input.GetAxis("MouseMoveY");

                transform.position += m_MouseMoved;
            }
            else if (Input.GetAxis("Zoom") != 0 && !UIManager.Instance.UIIsMouseover())
            {
                transform.position += transform.forward * Input.GetAxis("Zoom");
            }


            if (m_View != m_Dimensions[m_ViewIndex])
            {
                m_View = m_Dimensions[m_ViewIndex];
                m_MinimapView = m_MinimapDimensions[m_ViewIndex];

                m_VisualEffectDuration = m_EffectMaxDuration;

                StopCoroutine("ChangeViewEffect");
                StartCoroutine("ChangeViewEffect");
            }
        }

        IEnumerator ChangeViewEffect()
        {
            float i = 0;
            m_MinimapCam.cullingMask = m_MinimapView;

            for (i = 0; i < m_EffectMaxDuration / 2; i += Time.deltaTime, m_VisualEffectDuration -= Time.deltaTime)
            {
                m_Material.SetFloat("_Oscillater", Mathf.Lerp(0, 1, i / m_EffectMaxDuration * 2));

                yield return null;
            }

            m_Cam.cullingMask = m_View;

            for (i = 0; i < m_EffectMaxDuration / 2; i += Time.deltaTime, m_VisualEffectDuration -= Time.deltaTime)
            {
                m_Material.SetFloat("_Oscillater", Mathf.Lerp(1, 0, i / m_EffectMaxDuration * 2));

                yield return null;
            }
        }

        private void OnRenderImage(RenderTexture _source, RenderTexture _destination)
        {
            if (m_VisualEffectDuration > 0f)
            {
                Graphics.Blit(_source, _destination, m_Material);
            }
            else
            {
                Graphics.Blit(_source, _destination);
            }
        }
    }

}