using UnityEngine;
using UnityEngine.EventSystems;

public class HighlightManager : MonoBehaviour
{
    private int enemyLayer;
    private bool setupDone = false;

    private GameObject highlightedObject;
    private GameObject selectedObject;
    public LayerMask blueLayer;
    public LayerMask redLayer;

    private Outline highlightOutline;
    private RaycastHit hit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!setupDone)
        {
            return;
        }

            hoverHighlight();
    }

    private void hoverHighlight()
    {
        if (!setupDone)
        {
            return;
        }

        if (highlightedObject != null && highlightOutline != null)
        {
            highlightOutline.enabled = false;
            highlightedObject = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!EventSystem.current.IsPointerOverGameObject() && (Physics.Raycast(ray, out hit, blueLayer) || Physics.Raycast(ray, out hit, redLayer)))
        {
            if (hit.transform != null)
            {
                highlightedObject = hit.transform.root.gameObject;


                if (highlightedObject.layer == enemyLayer && highlightedObject != selectedObject)
                {
                    if (highlightedObject.GetComponent<Outline>())
                    {
                        highlightOutline = highlightedObject.GetComponent<Outline>();
                        highlightOutline.enabled = true;
                    }
                }
                else
                {
                    highlightedObject = null;
                }
            }
        }
    }

    public void selectedHighlight(GameObject newSelection)
    {
        if (!setupDone)
        {
            return;
        }

        if (highlightedObject.layer == enemyLayer)
        {
            if (selectedObject != null)
            {
                if (selectedObject.GetComponent<Outline>())
                {
                    selectedObject.GetComponent<Outline>().enabled = false;
                }
            }

            selectedObject = newSelection;

            if (selectedObject.GetComponent<Outline>())
            {
                selectedObject.GetComponent<Outline>().enabled = true;
            }

            highlightOutline.enabled = true;
            highlightedObject = null;
        }
    }

    public void deselectHighlight()
    {
        if (!setupDone)
        {
            return;
        }

        selectedObject.GetComponent<Outline>().enabled = false;
        selectedObject = null;
    }

    public void setup(int playerEnemyLayer)
    {
        enemyLayer = playerEnemyLayer;
        setupDone = true;
    }
}
