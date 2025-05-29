//using MyTools.SaveManager;
//using UnityEngine;
//using UnityEngine.Purchasing;
//using UnityEngine.Purchasing.Extension;


//public class IAPManager : MonoBehaviour
//{
//    //private string RemoveAds = "ios.flag.painting.removeads";
//    private CodelessIAPButton codelessIAPButton;

//    private string RemoveAds => codelessIAPButton.productId;

//    [SerializeField] private GameObject LoadingPanel;

//    private void OnEnable()
//    {
//        if (gameObject.name.ToLower().Contains("removeads") && PlayerPrefs.GetInt(SharedVariables.RemoveAds, 0) == 1)
//        {
//            gameObject.SetActive(false);
//            return;
//        }
//    }

//    private void Start()
//    {
//        if (SaveManager.Prefs.GetBool(SharedVariables.RemoveAds, false) == true)
//        {
//            gameObject.SetActive(false);
//        }

//        if (TryGetComponent(out codelessIAPButton))
//        {
//            codelessIAPButton.onPurchaseComplete.RemoveAllListeners();
//            codelessIAPButton.onPurchaseComplete.AddListener(OnPurchaseComplete);

//            codelessIAPButton.onPurchaseFailed.RemoveAllListeners();
//            codelessIAPButton.onPurchaseFailed.AddListener(OnPurchaseFailed);

//            if (codelessIAPButton.button)
//                codelessIAPButton.button.onClick.AddListener(() => LoadingPanel.GetComponent<AnimationBase>().Show());
//        }
//        else
//        {
//            Debug.LogError("No Codeless IAP button found");
//        }
//    }

//    public void OnPurchaseComplete(Product product)
//    {
//        if (product.definition.id.Equals(RemoveAds))
//        {
//            SaveManager.Prefs.SetBool(SharedVariables.RemoveAds, true);
//            //if (AdsManager.Instance)
//            //    AdsManager.Instance.DestroyBanner();
//            gameObject.SetActive(false);
//        }
//        LoadingPanel.GetComponent<AnimationBase>().Hide();
//        Debug.Log($"Product: {product.definition.id} purchased successfully.");
//    }

//    public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
//    {
//        LoadingPanel.GetComponent<AnimationBase>().Hide();
//        Debug.Log($"Product: {product.definition.id} failed,\n Reason: {description}");
//    }

//    private void OnDisable()
//    {
//        codelessIAPButton.onPurchaseComplete.RemoveAllListeners();
//        codelessIAPButton.onPurchaseFailed.RemoveAllListeners();
//    }
//}
