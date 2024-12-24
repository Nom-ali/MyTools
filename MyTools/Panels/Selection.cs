using RNA.SaveManager;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
    [SerializeField] private AssetLabelReference AssetLabel;

    [SerializeField] private AssetReference PrefabRef;
    [SerializeField] private Transform Container;
    [SerializeField] private List<Sprite> BtnSprites;
    [SerializeField] private List<Button> Btns;

    //Called from Inspector
    public async void LoadData()
    {
        GameObject obj = await AddressableManager.LoadAsset<GameObject>(PrefabRef);
        Button button = obj.GetComponent<Button>();
        List<Sprite> data = await AddressableManager.LoadAssets<Sprite>(AssetLabel);

        Debug.Log("Return Data: " + data.Count);
        while (data == null)
        {
            await Task.Yield();
        }
        BtnSprites.AddRange(data);
        await CreatBtns(button);
        await Task.CompletedTask;
    }

    async Task CreatBtns(Button button)
    {
        if (BtnSprites.Count <= 0)
        {
            Debug.LogError("Btn array Lenght is less than 0");
            return;
        }

        for (int i = 0; i < BtnSprites.Count; i++)
        {
            Button btn = Instantiate(button, Container);
            Btns.Add(btn);

            int index = i;
            Sprite sprite = BtnSprites[index];

            btn.transform.GetChild(0).GetComponent<Image>().sprite = sprite;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                SaveManager.Prefs.SetInt(SharedVariables.SelectedCaseTypeID, index, true);
            });
        }

        await Task.CompletedTask;
    }

    void ReleaseData()
    {
        AddressableManager.ReleaseAsset<Sprite>(AssetLabel.labelString);
    }

    private void OnDisable()
    {
        ReleaseData();
    }
}
