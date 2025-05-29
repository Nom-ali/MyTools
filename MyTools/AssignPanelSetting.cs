using MyTools;
using System.Collections;
using UnityEngine;

public class AssignPanelSetting : MonoBehaviour
{
    [SerializeField] private PanelType panelType = PanelType.None;
    
    public void OnEnable()
    {
        StartCoroutine(On_Start());
    }

    IEnumerator On_Start()
    {
        var uiManager = FindObjectOfType<UIManagerBase>();

        yield return new WaitUntil(() => uiManager != null);

        uiManager.AssignPanelSetting(panelType);
    }

   
}
