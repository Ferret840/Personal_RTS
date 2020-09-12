using UnityEngine;

namespace Tools
{

    public static class Utils
    {
        static Texture2D m_s_WhiteTexture;
        public static Texture2D WhiteTexture
        {
            get
            {
                if (m_s_WhiteTexture == null)
                {
                    m_s_WhiteTexture = new Texture2D(1, 1);
                    m_s_WhiteTexture.SetPixel(0, 0, Color.white);
                    m_s_WhiteTexture.Apply();
                }

                return m_s_WhiteTexture;
            }
        }

        public static void DrawScreenRect_s(Rect _rect, Color _color)
        {
            GUI.color = _color;
            GUI.DrawTexture(_rect, WhiteTexture);
            GUI.color = Color.white;
        }

        public static void DrawScreenRectBorder_s(Rect _rect, float _thickness, Color _color)
        {
            // Top
            Utils.DrawScreenRect_s(new Rect(_rect.xMin, _rect.yMin, _rect.width, _thickness), _color);
            // Left
            Utils.DrawScreenRect_s(new Rect(_rect.xMin, _rect.yMin, _thickness, _rect.height), _color);
            // Right
            Utils.DrawScreenRect_s(new Rect(_rect.xMax - _thickness, _rect.yMin, _thickness, _rect.height), _color);
            // Bottom
            Utils.DrawScreenRect_s(new Rect(_rect.xMin, _rect.yMax - _thickness, _rect.width, _thickness), _color);
        }

        public static Rect GetScreenRect_s(Vector3 _screenPosition1, Vector3 _screenPosition2)
        {
            // Move origin from bottom left to top left
            _screenPosition1.y = Screen.height - _screenPosition1.y;
            _screenPosition2.y = Screen.height - _screenPosition2.y;
            // Calculate corners
            var topLeft = Vector3.Min(_screenPosition1, _screenPosition2);
            var bottomRight = Vector3.Max(_screenPosition1, _screenPosition2);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        public static Bounds GetViewportBounds_s(Camera _camera, Vector3 _screenPosition1, Vector3 _screenPosition2)
        {
            var v1 = Camera.main.ScreenToViewportPoint(_screenPosition1);
            var v2 = Camera.main.ScreenToViewportPoint(_screenPosition2);
            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);
            min.z = _camera.nearClipPlane;
            max.z = _camera.farClipPlane;

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        /// <summary>
        /// Takes a LayerMask and converts it to an int containing which dimensions are exposed to that LayerMask.
        /// (i.e. It can see Dimension 1 and/or Dimension 2)
        /// </summary>
        /// <param name="_original">The original LayerMask value</param>
        /// <returns>Returns an int containing the exposed dimensions</returns>
        public static int LayerMaskToInt_s(LayerMask _original)
        {
            int dim = 0;

            if ((_original & (1 << 8)) != 0)
                dim |= 1;
            if ((_original & (1 << 9)) != 0)
                dim |= 2;

            return dim;
        }

        /// <summary>
        /// Converts the given Layer of an object to which dimension the object exists in.
        /// (i.e. It's in Dimension 1, 2, or Both (3))
        /// </summary>
        /// <param name="_original">The original Layer value of the object</param>
        /// <returns>Returns the game logic value for dimensions</returns>
        public static int ObjectLayerToInt_s(int _original)
        {
            int dim = 0;

            if (_original == 8)
                dim |= 1;
            if (_original == 9)
                dim |= 2;
            if (_original == 10)
                dim |= 3;

            return dim;
        }

        public static int IntToLayer_s(int _original)
        {
            return _original + 7;
        }
    }

}