using UnityEngine;

namespace Tools
{

    public static class Utils
    {
        static Texture2D _whiteTexture;
        public static Texture2D WhiteTexture
        {
            get
            {
                if (_whiteTexture == null)
                {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, Color.white);
                    _whiteTexture.Apply();
                }

                return _whiteTexture;
            }
        }

        public static void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = Color.white;
        }

        public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Top
            Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Left
            Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Right
            Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Bottom
            Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }

        public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
        {
            // Move origin from bottom left to top left
            screenPosition1.y = Screen.height - screenPosition1.y;
            screenPosition2.y = Screen.height - screenPosition2.y;
            // Calculate corners
            var topLeft = Vector3.Min(screenPosition1, screenPosition2);
            var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
        {
            var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
            var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);
            min.z = camera.nearClipPlane;
            max.z = camera.farClipPlane;

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        /// <summary>
        /// Takes a LayerMask and converts it to an int containing which dimensions are exposed to that LayerMask.
        /// (i.e. It can see Dimension 1 and/or Dimension 2)
        /// </summary>
        /// <param name="original">The original LayerMask value</param>
        /// <returns>Returns an int containing the exposed dimensions</returns>
        public static int LayerMaskToInt(LayerMask original)
        {
            int dim = 0;

            if ((original & (1 << 8)) != 0)
                dim |= 1;
            if ((original & (1 << 9)) != 0)
                dim |= 2;

            return dim;
        }

        /// <summary>
        /// Converts the given Layer of an object to which dimension the object exists in.
        /// (i.e. It's in Dimension 1, 2, or Both (3))
        /// </summary>
        /// <param name="original">The original Layer value of the object</param>
        /// <returns>Returns the game logic value for dimensions</returns>
        public static int ObjectLayerToInt(int original)
        {
            int dim = 0;

            if (original == 8)
                dim |= 1;
            if (original == 9)
                dim |= 2;
            if (original == 10)
                dim |= 3;

            return dim;
        }

        public static int IntToLayer(int original)
        {
            return original + 7;
        }
    }

}