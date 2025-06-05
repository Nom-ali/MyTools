using MyTools.SaveManager;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour
{
    [SerializeField] private GameObject LoadingPanel;
    [SerializeField] private bool DisableOnPurchase = true;

    private CodelessIAPButton codelessIAPButton;

    private string ProductID => codelessIAPButton ? codelessIAPButton.productId : "";


    private void OnEnable()
    {
        if (DisableOnPurchase && gameObject.name.ToLower().Contains("removeads") && SaveManager.Prefs.GetBool(SharedVariables.RemoveAds, false) == true)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void Start()
    {
        if (DisableOnPurchase && gameObject.name.ToLower().Contains("removeads") && SaveManager.Prefs.GetBool(SharedVariables.RemoveAds, false) == true)
        {
            gameObject.SetActive(false);
        }

        if (TryGetComponent(out codelessIAPButton))
        {
            if (codelessIAPButton.buttonType == CodelessButtonType.Purchase)
            {
                codelessIAPButton.onPurchaseComplete.RemoveAllListeners();
                codelessIAPButton.onPurchaseComplete.AddListener(OnPurchaseComplete);

                codelessIAPButton.onPurchaseFailed.RemoveAllListeners();
                codelessIAPButton.onPurchaseFailed.AddListener(OnPurchaseFailed);
            }
            else if (codelessIAPButton.buttonType == CodelessButtonType.Restore)
            {
                codelessIAPButton.onTransactionsRestored.RemoveAllListeners();
                codelessIAPButton.onTransactionsRestored.AddListener(OnTransactionRestored);
                
                codelessIAPButton.onProductFetched.RemoveAllListeners();
                codelessIAPButton.onProductFetched.AddListener(OnProductFetched);
            }

            if (codelessIAPButton.button)
                codelessIAPButton.button.onClick.AddListener(() => LoadingPanel?.GetComponent<AnimationBase>().Show());
        }
        else
        {
            Debug.LogError("No Codeless IAP button found");
        }
    }

    #region Purchase
    public void OnPurchaseComplete(Product product)
    {
        Debug.Log($"New purchase completed: {product.definition.id}");
        GrantProduct(product);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
    {
        LoadingPanel?.GetComponent<AnimationBase>().Hide();
        Debug.Log($"Product: {product.definition.id} failed,\n Reason: {description}");
    }
    #endregion Purchase

    #region Restore
    void OnTransactionRestored(bool success, string error)
    {
        if (success)
        {
            Debug.Log("Restore process finished successfully.");
        }
        else
        {
            Debug.LogError($"Restore failed: {error}");
            // Optionally inform user
        }

        LoadingPanel?.GetComponent<AnimationBase>().Hide();
    }

    void OnProductFetched(Product product)
    {
        Debug.Log($"Restored product: {product.definition.id}");
        GrantProduct(product);
    }
    #endregion Restore

    /// <summary>
    /// Grants the purchased or restored product entitlements.
    /// </summary>
    /// <param name="product">The product to grant</param>
    private void GrantProduct(Product product)
    {
        if (product.definition.id.Equals(ProductID))
        {
            SaveManager.Prefs.SetBool(SharedVariables.RemoveAds, true);
            if (AdsManager.Instance)
                AdsManager.Instance.DestroyBanner();

            if(DisableOnPurchase && gameObject.name.ToLower().Contains("removeads"))
                gameObject.SetActive(false);
        }
        LoadingPanel?.GetComponent<AnimationBase>().Hide();
        Debug.Log($"Product: {product.definition.id} purchased successfully.");
    }

    private void OnDisable()
    {
        if (codelessIAPButton)
        {
            codelessIAPButton.onPurchaseComplete.RemoveAllListeners();
            codelessIAPButton.onPurchaseFailed.RemoveAllListeners();
            codelessIAPButton.onTransactionsRestored.RemoveAllListeners();
            codelessIAPButton.onProductFetched.RemoveAllListeners();
        }
    }
}
