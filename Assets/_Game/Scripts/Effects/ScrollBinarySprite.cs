using UnityEngine;

public class ScrollBinaryWall : MonoBehaviour
{
    public float scrollSpeed = 0.1f;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        // Ensure the material uses UV-based scrolling (not static)
        rend.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    void Update()
    {
        // Scroll vertically downward (Y axis)
        float offsetY = Time.time * scrollSpeed;
        rend.material.mainTextureOffset = new Vector2(0, offsetY);
    }
}
