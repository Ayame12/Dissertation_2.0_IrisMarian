using UnityEngine;

public class DisableOutline : MonoBehaviour
{
    float delay = 0.01f;
    Outline outline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outline = GetComponent<Outline>();
        Invoke("disableOutline", delay);
    }

    private void disableOutline()
    {
        outline.enabled = false;
    }
}
