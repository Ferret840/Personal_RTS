using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SlicedMesh : MonoBehaviour
{
    [SerializeField]
    private float m_BorderVertical = 0.1f;
    public float BorderVertical
    {
        get
        {
            return m_BorderVertical;
        }
        set
        {
            m_BorderVertical = value;
            m_SettingChanged = true;
        }
    }

    [SerializeField]
    private float m_BorderHorizontal = 0.1f;
    public float BorderHorizontal
    {
        get
        {
            return m_BorderHorizontal;
        }
        set
        {
            m_BorderHorizontal = value;
            m_SettingChanged = true;
        }
    }

    [SerializeField]
    private float m_Width = 1.0f;
    public float Width
    {
        get
        {
            return m_Width;
        }
        set
        {
            m_Width = value;
            m_SettingChanged = true;
        }
    }

    [SerializeField]
    private float m_Height = 1.0f;
    public float Height
    {
        get
        {
            return m_Height;
        }
        set
        {
            m_Height = value;
            m_SettingChanged = true;
        }
    }

    [SerializeField]
    [Range(0, 0.5f)]
    private float m_MarginVertical = 0.4f;
    public float MarginVertical
    {
        get
        {
            return m_MarginVertical;
        }
        set
        {
            m_MarginVertical = value;
            m_SettingChanged = true;
        }
    }

    [SerializeField]
    [Range(0, 0.5f)]
    private float m_MarginHorizontal = 0.4f;
    public float MarginHorizontal
    {
        get
        {
            return m_MarginHorizontal;
        }
        set
        {
            m_MarginHorizontal = value;
            m_SettingChanged = true;
        }
    }

    [SerializeField]
    bool m_SettingChanged = false;

    void Start()
    {
        UpdateSlicedMesh();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (m_SettingChanged)
        {
            UpdateSlicedMesh();
            m_SettingChanged = false;
        }
    }
#endif

    void UpdateSlicedMesh()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3 midPoint = new Vector3(-m_Width / 2.0f, -m_Height / 2.0f, 0);

        mesh.vertices = new Vector3[] {
            midPoint + new Vector3(0, 0, 0),                         midPoint + new Vector3(m_BorderHorizontal, 0, 0),                         midPoint + new Vector3(m_Width-m_BorderHorizontal, 0, 0),                         midPoint + new Vector3(m_Width, 0, 0),
            midPoint + new Vector3(0, m_BorderVertical, 0),          midPoint + new Vector3(m_BorderHorizontal, m_BorderVertical, 0),          midPoint + new Vector3(m_Width-m_BorderHorizontal, m_BorderVertical, 0),          midPoint + new Vector3(m_Width, m_BorderVertical, 0),
            midPoint + new Vector3(0, m_Height-m_BorderVertical, 0), midPoint + new Vector3(m_BorderHorizontal, m_Height-m_BorderVertical, 0), midPoint + new Vector3(m_Width-m_BorderHorizontal, m_Height-m_BorderVertical, 0), midPoint + new Vector3(m_Width, m_Height-m_BorderVertical, 0),
            midPoint + new Vector3(0, m_Height, 0),                  midPoint + new Vector3(m_BorderHorizontal, m_Height, 0),                  midPoint + new Vector3(m_Width-m_BorderHorizontal, m_Height, 0),                  midPoint + new Vector3(m_Width, m_Height, 0)
        };

        mesh.uv = new Vector2[] {
            new Vector2(0, 0),                  new Vector2(m_MarginHorizontal, 0),                  new Vector2(1-m_MarginHorizontal, 0),                  new Vector2(1, 0),
            new Vector2(0, m_MarginVertical),   new Vector2(m_MarginHorizontal, m_MarginVertical),   new Vector2(1-m_MarginHorizontal, m_MarginVertical),   new Vector2(1, m_MarginVertical),
            new Vector2(0, 1-m_MarginVertical), new Vector2(m_MarginHorizontal, 1-m_MarginVertical), new Vector2(1-m_MarginHorizontal, 1-m_MarginVertical), new Vector2(1, 1-m_MarginVertical),
            new Vector2(0, 1),                  new Vector2(m_MarginHorizontal, 1),                  new Vector2(1-m_MarginHorizontal, 1),                  new Vector2(1, 1)
        };

        mesh.triangles = new int[] {
            0, 4, 5,
            0, 5, 1,
            1, 5, 6,
            1, 6, 2,
            2, 6, 7,
            2, 7, 3,
            4, 8, 9,
            4, 9, 5,
            5, 9, 10,
            5, 10, 6,
            6, 10, 11,
            6, 11, 7,
            8, 12, 13,
            8, 13, 9,
            9, 13, 14,
            9, 14, 10,
            10, 14, 15,
            10, 15, 11
        };

        mesh.RecalculateBounds();
    }
}