using UnityEngine;

public class UIPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;

    [Header("Settings")]
    [SerializeField] private bool hideOnStart = true;

    private void Awake()
    {
        if (panelRoot == null)
        {
            Debug.LogError("Panel Root is missing.", this);
            return;
        }

        if (hideOnStart)
            panelRoot.SetActive(false);
    }

    public void Show()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(false);
    }

    public void Toggle()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(!panelRoot.activeSelf);
    }
}