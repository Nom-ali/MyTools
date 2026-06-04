using GoogleAds;
using MyTools.SaveManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    [Serializable]
    public class ObjectsToDisable
    {
        public GameObject[] ObjectToDisable = null;
    }

    [SerializeField] private GameObject Loadingpanel;
    [SerializeField] private bool ShowPriceFromStore = false;
  
    [SerializeField] private bool DisableOnPurchase = false;
    [MyBox.ConditionalField(nameof(DisableOnPurchase), false)]
    public bool self = true;
    
    [MyBox.ConditionalField(new[] { nameof(DisableOnPurchase), nameof(self) }, new[] { false, false }, true, false)]
    public ObjectsToDisable objectsToDisable;

    private AnimationBase loadingpanel;
    private CodelessIAPButton codelessIAPButton;
    private Dictionary<string, Action> productRewards;
    private Dictionary<string, Func<bool>> IAPProductPurchased = new()
    {
        { "removeads", () => PlayerPrefs.GetInt("RemoveAds", 0) == 1},
        { "megabundle", () => PlayerPrefs.GetInt("MegaBundle", 0) == 1},
        { "specialbundle", () => PlayerPrefs.GetInt("Tubes", 0) == 1},
    };

    private void OnEnable()
    {
        StartCoroutine(DisableObject());
    }

    private void Awake()
    {
        if (!loadingpanel) loadingpanel = Loadingpanel.GetComponent<AnimationBase>();

        if (TryGetComponent(out codelessIAPButton))
        {
            if (codelessIAPButton.buttonType == CodelessButtonType.Purchase)
            {
                //if (ShowPriceFromStore) GetComponentInChildren<TextMeshProUGUI>().text = "Buy";

                codelessIAPButton.onProductFetched.RemoveAllListeners();
                codelessIAPButton.onProductFetched.AddListener(OnProductDataFetched);

                codelessIAPButton.onProductFetchFailed.RemoveAllListeners();
                codelessIAPButton.onProductFetchFailed.AddListener(OnProductDataFetchedFailed);

                codelessIAPButton.onPurchaseFetched.RemoveAllListeners();
                codelessIAPButton.onPurchaseFetched.AddListener(OnPurchaseFetched);

                codelessIAPButton.onOrderPending.RemoveAllListeners();
                codelessIAPButton.onOrderPending.AddListener(OnOrderPending);

                codelessIAPButton.onOrderConfirmed.RemoveAllListeners();
                codelessIAPButton.onOrderConfirmed.AddListener(OnOrderConfirmed);

                codelessIAPButton.onPurchaseFailed.RemoveAllListeners();
                codelessIAPButton.onPurchaseFailed.AddListener(OnPurchaseFailed);

                codelessIAPButton.onOrderDeferred.RemoveAllListeners();
                codelessIAPButton.onOrderDeferred.AddListener(OnOrderDeferred);
            }
            else if (codelessIAPButton.buttonType == CodelessButtonType.Restore)
            {
                codelessIAPButton.onTransactionsRestored.RemoveAllListeners();
                codelessIAPButton.onTransactionsRestored.AddListener(OnTransactionRestored);
            }
        }
        else
        {
            Debug.LogError("No Codeless IAP m_Button found");
        }
    }

    private IEnumerator Start()
    {
        InitializeProductRewards();

        yield return new WaitUntil(() => codelessIAPButton != null && codelessIAPButton.button != null);

        if (codelessIAPButton.button)
            codelessIAPButton.button.onClick.AddListener(() =>
            {
                if (loadingpanel) loadingpanel.Show();
            });

        yield return null;
    }

    private IEnumerator DisableObject()
    {
        string lowName = gameObject.name.ToLower();
        if (DisableOnPurchase && IAPProductPurchased.TryGetValue(lowName, out var action))
        {
            bool check = (bool)action?.Invoke();
            Debug.Log($"[IAP] Disabling object based on purchase Product '{lowName}' purchased: {check}", gameObject);
            if (DisableOnPurchase && check)
            {
                if(self)
                    gameObject.SetActive(false);
                else
                {
                    for (int i = 0; i < objectsToDisable.ObjectToDisable.Length; i++)
                    {
                        objectsToDisable.ObjectToDisable[i].SetActive(false);
                    }
                }
            }
        }
        yield return null;
    }

    void OnProductDataFetched(Product product)
    {
        if (product == null) return;

        Debug.Log("Product data fetched: " + product);

        if (ShowPriceFromStore)
            GetComponentInChildren<TextMeshProUGUI>().text = product.metadata.localizedPriceString;
    }

    void OnProductDataFetchedFailed(ProductDefinition definition, string message)
    {
        Debug.Log("Product data failed to fetched: " + definition);

        if (ShowPriceFromStore)
            GetComponentInChildren<TextMeshProUGUI>().text = "Buy";

        codelessIAPButton.button.interactable = false;
    }

    void OnPurchaseFetched(Order order)
    {
        if (loadingpanel) loadingpanel.Hide();
        if (order == null)
        {
            Debug.LogWarning("[IAP] OnPurchaseFetched called with null Order.");
            return;
        }

        foreach (var item in order.CartOrdered.Items())
        {
            GrantProduct(item.Product);
        }
    }

    private void OnOrderPending(PendingOrder pendingOrder)
    {
        if (loadingpanel) loadingpanel.Hide();
        if (pendingOrder == null)
        {
            Debug.LogWarning("[IAP] OnOrderPending called with null PendingOrder.");
            return;
        }

        foreach (var item in pendingOrder.CartOrdered.Items())
        {
            Debug.Log($"[IAP] cart item: {item.Product}");
            GrantProduct(item.Product);
        }
    }

    private void OnOrderConfirmed(ConfirmedOrder confirmedOrder)
    {
        if (confirmedOrder == null)
        {
            Debug.LogWarning("[IAP] OnOrderConfirmed called with null ConfirmedOrder.");
            return;
        }

        Debug.Log("[IAP] Order confirmed.");
        StartCoroutine(DisableObject());
        // Don’t grant here (granting should be done in OnOrderPending).
        // Use for: analytics, UI success popup, hide spinner, etc.
    }

    private void OnOrderDeferred(DeferredOrder deferredOrder)
    {
        if (loadingpanel) loadingpanel.Hide();
        if (deferredOrder == null)
        {
            Debug.LogWarning("[IAP] OnOrderDeferred called with null DeferredOrder.");
            return;
        }

        Debug.Log("[IAP] Order deferred (e.g., Ask to Buy). Show waiting UI.");
    }


    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        if (loadingpanel) loadingpanel.Hide();
        if (failedOrder == null)
        {
            Debug.LogWarning("[IAP] OnPurchaseFailed called with null FailedOrder.");
            return;
        }

        // FailedOrder usually contains details; depending on exact IAP build,
        // you may have accessors for reason/message.
        Debug.LogWarning("[IAP] Purchase failed (FailedOrder).");
        // Example: show "Cancelled" / "Failed" UI and re-enable button
    }

    void OnTransactionRestored(bool success, string error)
    {
        Debug.Log("Transactions restored: " + success);
        if (loadingpanel) loadingpanel.Hide();
    }

    /// <summary>
    /// Grants the purchased or restored product entitlements.
    /// </summary>
    /// <param name="product">The product to grant</param>
    private void GrantProduct(Product product)
    {
        string id = product.definition.id.ToLower();

        if (PlayerPrefs.GetInt($"IAP_GRANTED_{id}", 0) == 1)
            return;

        if (productRewards.TryGetValue(id, out var rewardAction))
        {
            rewardAction.Invoke();
            PlayerPrefs.SetInt($"IAP_GRANTED_{id}", 1);
            Debug.Log($"[IAP] Granted product: {id}");
        }
        else
        {
            Debug.LogWarning($"[IAP] Unhandled product ID: {id}");
        }
    }


    private void InitializeProductRewards()
    {
        productRewards = new Dictionary<string, System.Action>
        {
            {
                IAPProductIDs.RemoveAds, () =>
                {
                    SaveManager.Prefs.SetBool(SharedVariables.RemoveAds, true, true);
                    AdsManager.Instance?.DestroyBanner();
                    AdsManager.Instance?.DestroyBigBanner();
                }
            },
            {
                IAPProductIDs.Coin500, () =>
                {
                    SaveManager.Currency.Value += 500;
                }
            },
            {
                IAPProductIDs.Coin1000, () =>
                {
                    SaveManager.Currency.Value += 1000;
                }
            },
            {
                IAPProductIDs.MegaBundle, () =>
                {
                    MegaBundle();
                }
            },
            {
                IAPProductIDs.SpecialBundle, () =>
                {
                    SpecialBundle();
                }
            }

            // Add more products here as needed
        };
    }



    void MegaBundle()
    {
        SaveManager.Prefs.SetBool(GetUnlockKey(SelectionCategory.Ball, 18), true, true);
        SaveManager.Prefs.SetBool(SharedVariables.RemoveAds, true, true);
        AdsManager.Instance?.DestroyBanner();
        AdsManager.Instance?.DestroyBigBanner();
        SaveManager.Currency.Value += 1000;

    }

    void SpecialBundle()
    {
        SaveManager.Prefs.SetBool(GetUnlockKey(SelectionCategory.Ball, 20), true, true);
        SaveManager.Prefs.SetBool(GetUnlockKey(SelectionCategory.Ball, 21), true, true);
        SaveManager.Prefs.SetBool(SharedVariables.RemoveAds, true, true);
        AdsManager.Instance?.DestroyBanner();
        AdsManager.Instance?.DestroyBigBanner();
        SaveManager.Currency.Value += 2000;
    }
    private string GetUnlockKey(SelectionCategory category, int index)
    {
        return $"UNLOCK_{category}_{index}";
    }

}

public static class IAPProductIDs
{
    public static readonly string Coin500 = "com.polaris.ballgame.coins500";
    public static readonly string Coin1000 = "com.polaris.ballgame.coins1000";
    public static readonly string RemoveAds = "com.polaris.ballgame.removeads";
    public static readonly string MegaBundle = "com.polaris.ballgame.megabundle";
    public static readonly string SpecialBundle = "com.polaris.ballgame.specialbundle";
}