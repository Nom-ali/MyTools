using DG.Tweening;
using RNA;
using RNA.SaveManager;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Selection : AddListeners
{
    [SerializeField] private AssetLabelReference AssetLabel;

    [SerializeField] private AssetReference PrefabRef;
    [SerializeField] private Transform Container;
    [SerializeField] private List<Button> Btns;

    UIManagerBase manager = null;
    //[SerializeField] private List<Sprite> BtnSprites;

    private void Awake()
    {
        manager = FindObjectOfType<UIManagerBase>();
    }

    private void OnEnable()
    {
        GetComponent<AddListeners>().GetButton(0).interactable = false;
        Init(manager);
        _ = AddListener();
    }

    public override void Init(UIManagerBase manager)
    {
        base.Init(manager);
    }

    async Task AddListener()
    {
        for (int i = 0; i < Btns.Count; i++)
        {
            int index = i;
            Button btn = Btns[index];

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                SaveManager.Prefs.SetInt(SharedVariables.SelectedCaseTypeID, index, true);
                GetComponent<AddListeners>().GetButton(0).interactable = true;
            });
            btn.gameObject.SetActive(false);
        }

        StartCoroutine(EnableBtns());
        await Task.CompletedTask;
    }

    IEnumerator EnableBtns()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < Btns.Count; i++)
        {
            Btns[i].gameObject.SetActive(true);
            Btns[i].transform.DOScale(Vector3.one, 0.3f).From(0f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void RemoveListener()
    {
        for (int i = 0; i < Btns.Count; i++)
        {
            int index = i;
            Button btn = Btns[index];

            btn.onClick.RemoveAllListeners();
            btn.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        RemoveListener();
    }
}
