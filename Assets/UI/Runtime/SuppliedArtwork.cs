using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    // Local art preview: draw supplied sprites without changing UI behavior.
    public static class SuppliedArtwork
    {
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public static Sprite Get(string path)
        {
            if (!sprites.TryGetValue(path, out var sprite))
            {
                sprite = Resources.Load<Sprite>("Redone/" + path);
                sprites[path] = sprite;
            }
            return sprite;
        }
        public static void Draw(VertexHelper mesh, Rect rect, Sprite sprite, float alpha, bool fit = false)
        {
            mesh.Clear();
            if (sprite == null) return;
            if (fit)
            {
                float scale = Mathf.Min(rect.width / sprite.rect.width, rect.height / sprite.rect.height);
                Vector2 size = sprite.rect.size * scale;
                rect = new Rect(rect.center - size * .5f, size);
            }
            var uv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            Color tint = new Color(1,1,1,alpha);
            mesh.AddVert(new Vector2(rect.xMin,rect.yMin),tint,new Vector2(uv.x,uv.y));
            mesh.AddVert(new Vector2(rect.xMin,rect.yMax),tint,new Vector2(uv.x,uv.w));
            mesh.AddVert(new Vector2(rect.xMax,rect.yMax),tint,new Vector2(uv.z,uv.w));
            mesh.AddVert(new Vector2(rect.xMax,rect.yMin),tint,new Vector2(uv.z,uv.y));
            mesh.AddTriangle(0,1,2); mesh.AddTriangle(0,2,3);
        }
    }
}
