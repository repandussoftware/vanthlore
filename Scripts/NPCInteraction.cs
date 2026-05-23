using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class NPCInteraction : MonoBehaviour
{
    [Header("Veri Kaynağı")]
    public NPCData data;

    [Header("UI Ayarları (Hiyerarşi İsimleri)")]
    public string mainPanelName = "DialogPanel";
    public string itemsPartName = "ItemsPart";
    public string goldCountName = "GoldCount";
    public GameObject interactionUI;

    [Header("TradeConfirmation Referansları")]
    private GameObject confirmationPanel;
    private TextMeshProUGUI confirmItemName;
    private TextMeshProUGUI confirmItemDesc;
    private Image confirmItemIcon;
    private TextMeshProUGUI counterText; // CalculaterPanel içindeki sayı
    private TextMeshProUGUI statePartText; // "BUY" veya "SELL" yazısı
    private TextMeshProUGUI totalPriceText; // Fiyat kısmı
    private TextMeshProUGUI stateTitleText; // En üstteki "STATE" başlığı

    [Header("Trade Ayarları")]
    public GameObject traderSlotPrefab;

    // Private Referanslar
    private GameObject mainDialogPanel;
    private TextMeshProUGUI nameTMP;
    private TextMeshProUGUI dialogueTMP;
    private Image portraitImage;
    private GameObject dialogPart;
    private GameObject tradePart;
    private TextMeshProUGUI goldCountText;
    private Transform itemsParent;

    private bool _isSaving = false;
    private bool isSellMode = false;

    // İşlem Mantığı Değişkenleri
    private ItemData _pendingItem;
    private bool _isTradeSelling;
    private int _selectedQuantity = 1;

    private void Awake()
    {
        if (interactionUI != null)
        {
            Button btn = interactionUI.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(StartInteraction);
            }
            interactionUI.SetActive(false);
        }
    }

    private void UpdateSliderVisuals(Transform sliderTrans, float value)
    {
        if (sliderTrans == null) return;

        // Hiyerarşideki isimlere göre textleri buluyoruz
        Transform onText = sliderTrans.Find("OnText");
        Transform offText = sliderTrans.Find("OffText");

        bool isOn = (value >= 0.5f);

        if (onText != null) onText.gameObject.SetActive(!isOn);
        if (offText != null) offText.gameObject.SetActive(isOn);
    }

    private void FindUIReferences()
    {
        if (mainDialogPanel != null && itemsParent != null) return;

        Debug.Log("<color=cyan>Aritheon:</color> UI referansları taranıyor...");

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        mainDialogPanel = allObjects.FirstOrDefault(obj => obj.name == mainPanelName && obj.hideFlags == HideFlags.None);

        if (mainDialogPanel != null)
        {
            portraitImage = mainDialogPanel.GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.name == "Portrait");
            nameTMP = mainDialogPanel.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(tmp => tmp.name == "PortraitHeader");

            dialogPart = mainDialogPanel.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "DialogPart")?.gameObject;
            tradePart = mainDialogPanel.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "TradePart")?.gameObject;

            if (tradePart != null)
            {
                itemsParent = tradePart.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == itemsPartName);
                goldCountText = tradePart.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(tmp => tmp.name == goldCountName);
            }

            if (dialogPart != null)
                dialogueTMP = dialogPart.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(tmp => tmp.name == "Dialogue");

            SetupButton(mainDialogPanel.transform.Find("ClosePopup"), EndInteraction);


            Transform confTrans = mainDialogPanel.transform.Find("TradeConfirmation");
            if (confTrans != null)
            {
                confirmationPanel = confTrans.gameObject;
                stateTitleText = confTrans.Find("stateTitle")?.GetComponent<TextMeshProUGUI>();

                Transform iPanel = confTrans.Find("itemPanel");
                confirmItemName = iPanel?.Find("itemName")?.GetComponent<TextMeshProUGUI>();
                confirmItemDesc = iPanel?.Find("itemDescription")?.GetComponent<TextMeshProUGUI>();
                confirmItemIcon = iPanel?.Find("itemPrev")?.GetComponent<Image>();

                counterText = confTrans.Find("CalculaterPanel/Counter")?.GetComponent<TextMeshProUGUI>();
                statePartText = confTrans.Find("stateDescription/statePart")?.GetComponent<TextMeshProUGUI>();
                totalPriceText = confTrans.Find("stateDescription/pricePart")?.GetComponent<TextMeshProUGUI>();

                confirmationPanel.SetActive(false);
            }

            Debug.Log("<color=green>Aritheon:</color> UI Tarama başarıyla tamamlandı.");
        }
    }

    private void SetupAnyClickable(Transform trans, UnityEngine.Events.UnityAction action)
    {
        if (trans == null) return;

        Button btn = trans.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
            return;
        }

        Slider slider = trans.GetComponent<Slider>();
        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((val) =>
            {
                if (mainDialogPanel != null && mainDialogPanel.activeInHierarchy)
                {
                    action.Invoke();
                }
            });
        }
    }

    private void SetupButton(Transform btnTrans, UnityEngine.Events.UnityAction action)
    {
        if (btnTrans != null)
        {
            Button btn = btnTrans.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
            }
        }
    }

    public void SwitchTradeAndDialog()
    {
        if (tradePart == null || dialogPart == null) return;

        bool isOpeningTrade = !tradePart.activeSelf;
        tradePart.SetActive(isOpeningTrade);
        dialogPart.SetActive(!isOpeningTrade);

        // Ana mühür görselini güncelle canım
        Transform mainToggleTrans = mainDialogPanel.transform.Find("TradeButton/Toggle");
        if (mainToggleTrans != null)
        {
            Slider s = mainToggleTrans.GetComponent<Slider>();
            if (s != null)
            {
                float targetVal = isOpeningTrade ? 1f : 0f;
                s.SetValueWithoutNotify(targetVal);
                UpdateSliderVisuals(mainToggleTrans, targetVal); // BURADA DA GÜNCELLEME YAPTIK
            }
        }

        if (isOpeningTrade)
        {
            UpdateGoldUI();
            RefreshTradeWindow();
            if (portraitImage != null && data.tradeWindowBanner != null)
                portraitImage.sprite = data.tradeWindowBanner;
        }
        else
        {
            if (portraitImage != null) portraitImage.sprite = data.GetPortrait(NPCOffset.Default);
        }
    }

    public void StartInteraction()
    {
        FindUIReferences();
        if (data == null || mainDialogPanel == null) return;

        // --- 1. MOD VE GENEL SIFIRLAMA ---
        isSellMode = false; // Her zaman AL moduyla başlasın canım

        // --- 2. İÇERİDEKİ AL/SAT SLIDER'I (Market Modu Değiştirici) ---
        Transform marketSliderTrans = mainDialogPanel.GetComponentsInChildren<Slider>(true)
                                     .FirstOrDefault(s => s.name == "Toggle")?.transform;
        if (marketSliderTrans != null)
        {
            Slider s = marketSliderTrans.GetComponent<Slider>();
            if (s != null)
            {
                s.SetValueWithoutNotify(0f); // Çökme koruması
                UpdateSliderVisuals(marketSliderTrans, 0f); // Metinleri AL moduna zorla

                SetupAnyClickable(marketSliderTrans, () =>
                {
                    isSellMode = (s.value >= 0.5f);
                    UpdateSliderVisuals(marketSliderTrans, s.value); // Metinleri anlık güncelle canım
                    ToggleTradeMode();
                });
            }
        }

        // --- 3. ANA MÜHÜR BUTONU (Diyalog/Ticaret Geçişi) ---
        Transform tradeToggleBtn = mainDialogPanel.transform.Find("TradeButton/Toggle");
        if (tradeToggleBtn != null)
        {
            Slider s = tradeToggleBtn.GetComponent<Slider>();
            if (s != null)
            {
                s.SetValueWithoutNotify(0f); // Sessizce Diyalog tarafına çek
                UpdateSliderVisuals(tradeToggleBtn, 0f); // Metinleri SOHBET moduna zorla

                SetupAnyClickable(tradeToggleBtn, () =>
                {
                    UpdateSliderVisuals(tradeToggleBtn, s.value); // Metinleri anlık güncelle
                    SwitchTradeAndDialog();
                });
            }
        }

        // --- 4. ONAY PANELİ BUTONLARI ---
        if (confirmationPanel != null)
        {
            SetupButton(confirmationPanel.transform.Find("buttons/Positive"), ConfirmTrade);
            SetupButton(confirmationPanel.transform.Find("buttons/Negative"), CloseConfirmation);
            SetupButton(confirmationPanel.transform.Find("CalculaterPanel/UpperButton"), () => AdjustQuantity(1));
            SetupButton(confirmationPanel.transform.Find("CalculaterPanel/DownerButton"), () => AdjustQuantity(-1));
        }

        // --- 5. VERİLERİ DOLDUR VE PANELİ AÇ ---
        if (nameTMP != null) nameTMP.text = data.npcName;
        if (portraitImage != null) portraitImage.sprite = data.GetPortrait(NPCOffset.Default);

        mainDialogPanel.SetActive(true);

        // Başlangıç Ayarı: Oyuncu önce NPC ile konuşsun (Diyalog Aktif)
        if (dialogPart != null) dialogPart.SetActive(true);
        if (tradePart != null) tradePart.SetActive(false);

        UpdateGoldUI();
        ApplyAIState(true);
        saver();
    }
    public void ToggleTradeMode()
    {
        if (data == null || mainDialogPanel == null || tradePart == null) return;

        // Sadece ticaret paneli aktifken (açıkken) listeyi tazele
        if (tradePart.activeSelf)
        {
            UpdateGoldUI();
            RefreshTradeWindow();

            if (portraitImage != null)
            {
                if (isSellMode && data.tradeWindowBanner != null)
                    portraitImage.sprite = data.tradeWindowBanner;
                else
                    portraitImage.sprite = data.GetPortrait(NPCOffset.Default);
            }
        }
    }
    public void RefreshTradeWindow()
    {
        if (itemsParent == null) return;

        foreach (Transform child in itemsParent) Destroy(child.gameObject);

        if (isSellMode)
        {
            if (StatsManager.Instance != null && StatsManager.Instance.startingItems != null)
            {
                // --- GRUPLAMA MANTIĞI ---
                Dictionary<string, int> itemStacks = new Dictionary<string, int>();
                List<ItemData> uniqueItems = new List<ItemData>();

                foreach (ItemData item in StatsManager.Instance.startingItems)
                {
                    if (item == null) continue;
                    if (item.isStackable)
                    {
                        if (itemStacks.ContainsKey(item.itemID)) itemStacks[item.itemID]++;
                        else { itemStacks.Add(item.itemID, 1); uniqueItems.Add(item); }
                    }
                    else { uniqueItems.Add(item); }
                }

                // Slotları oluştur ve adet bilgisini gönder
                foreach (ItemData item in uniqueItems)
                {
                    int count = item.isStackable ? itemStacks[item.itemID] : 1;
                    CreateSlot(item, true, count); // Yeni parametre: count
                }
            }
        }
        else
        {
            // NPC Satış Modu (Burada genelde adet 1 olur veya sınırsızdır)
            if (data != null && data.shopItems != null)
            {
                foreach (ItemData item in data.shopItems)
                {
                    CreateSlot(item, false, 1);
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemsParent.GetComponent<RectTransform>());
    }

    // CreateSlot metodunu da adet alacak şekilde güncelliyoruz:
    private void CreateSlot(ItemData item, bool selling, int quantity)
    {
        if (item == null || traderSlotPrefab == null) return;
        GameObject newSlot = Instantiate(traderSlotPrefab, itemsParent);
        TraderSlot slotScript = newSlot.GetComponent<TraderSlot>();
        if (slotScript != null)
        {
            // TraderSlot scriptine 'quantity' parametresi eklemelisin
            slotScript.Setup(item, this, selling, quantity);
        }
    }

    public void BuyItem(ItemData item) => OpenTradeConfirmation(item, false);
    public void SellItem(ItemData item) => OpenTradeConfirmation(item, true);

    private void OpenTradeConfirmation(ItemData item, bool selling)
    {
        _pendingItem = item;
        _isTradeSelling = selling;
        _selectedQuantity = 1;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            if (stateTitleText != null) stateTitleText.text = selling ? "SELL ITEM" : "BUY ITEM";
            if (statePartText != null) statePartText.text = selling ? "SELL" : "BUY";
            if (confirmItemName != null) confirmItemName.text = item.itemName;
            if (confirmItemDesc != null) confirmItemDesc.text = item.description;
            if (confirmItemIcon != null) confirmItemIcon.sprite = item.icon;

            UpdateConfirmationUI();
        }
    }

    public void AdjustQuantity(int amount)
    {
        _selectedQuantity = Mathf.Max(1, _selectedQuantity + amount);

        if (_isTradeSelling && StatsManager.Instance != null)
        {
            // Elimizde bu itemdan toplam kaç tane var?
            int maxAvailable = StatsManager.Instance.startingItems.Count(x => x.itemID == _pendingItem.itemID);
            _selectedQuantity = Mathf.Clamp(_selectedQuantity, 1, maxAvailable);
        }

        UpdateConfirmationUI();
    }

    private void UpdateConfirmationUI()
    {
        if (counterText != null) counterText.text = _selectedQuantity.ToString();
        int unitPrice = _isTradeSelling ? _pendingItem.sellPrice : _pendingItem.buyPrice;
        if (totalPriceText != null) totalPriceText.text = (unitPrice * _selectedQuantity).ToString();
    }

    public void ConfirmTrade()
    {
        if (_pendingItem == null) return;
        int totalCost = (_isTradeSelling ? _pendingItem.sellPrice : _pendingItem.buyPrice) * _selectedQuantity;

        if (_isTradeSelling)
        {
            StatsManager.Instance.totalCoins += totalCost;
            for (int i = 0; i < _selectedQuantity; i++) StatsManager.Instance.startingItems.Remove(_pendingItem);
            saver();
            PotionsBarManager.Instance.RefreshAllSlots(); // İksir barını da güncelle
        }
        else
        {
            // ConfirmTrade içindeki satın alma bloğuna ekle:
            if (StatsManager.Instance.totalCoins >= totalCost)
            {
                StatsManager.Instance.totalCoins -= totalCost;
                for (int i = 0; i < _selectedQuantity; i++) StatsManager.Instance.startingItems.Add(_pendingItem);
                saver(); // Satın almayı da kaydet canım
                PotionsBarManager.Instance.RefreshAllSlots(); // İksir barını da güncelle
            }
            else
            {
                Debug.Log("<color=red>Yetersiz Altın!</color>");
                UIManager.Instance.ShowWarning("Not enough gold!");
                return;
            }
        }

        if (InventoryManager.Instance != null) InventoryManager.Instance.RefreshInventoryUI();
        UpdateGoldUI();
        RefreshTradeWindow();
        CloseConfirmation();
    }

    public void CloseConfirmation() => confirmationPanel?.SetActive(false);
    public void UpdateGoldUI()
    {
        if (goldCountText != null && StatsManager.Instance != null)
            goldCountText.text = StatsManager.Instance.totalCoins.ToString();
    }

    public void EndInteraction()
    {
        if (mainDialogPanel != null)
        {
            // Sliderları sessizce sıfırla ve dinleyicileri sil
            Slider[] allSliders = mainDialogPanel.GetComponentsInChildren<Slider>(true);
            foreach (Slider s in allSliders)
            {
                s.onValueChanged.RemoveAllListeners();
                s.SetValueWithoutNotify(0f);
            }

            // Onay Butonları temizliği
            if (confirmationPanel != null)
            {
                RemoveButtonListener(confirmationPanel.transform.Find("buttons/Positive"));
                RemoveButtonListener(confirmationPanel.transform.Find("buttons/Negative"));
                RemoveButtonListener(confirmationPanel.transform.Find("CalculaterPanel/UpperButton"));
                RemoveButtonListener(confirmationPanel.transform.Find("CalculaterPanel/DownerButton"));
            }

            mainDialogPanel.SetActive(false);
        }
        ApplyAIState(false);
    }

    // Yardımcı metod: Buton dinleyicilerini siler
    private void RemoveButtonListener(Transform t)
    {
        if (t != null)
        {
            Button b = t.GetComponent<Button>();
            if (b != null) b.onClick.RemoveAllListeners();
        }
    }

    private void ApplyAIState(bool isInteracting)
    {
        TerinAI terinAI = GetComponent<TerinAI>();
        if (terinAI != null)
        {
            terinAI.isInteracting = isInteracting;
            if (isInteracting)
            {
                Animator anim = GetComponentInChildren<Animator>();
                if (anim != null) anim.SetInteger("state", 0);
            }
        }
    }

    public async void saver()
    {
        if (_isSaving) return;
        _isSaving = true;
        try
        {
            if (StatsManager.Instance != null && SaveManager.instance != null)
            {
                SaveData currentData = new SaveData();
                StatsManager.Instance.ExportToSaveData(currentData);
                currentData.lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                currentData.saveName = "NPC_Save_" + (data != null ? data.npcName : "Unknown");
                await SaveManager.instance.SaveGame(currentData, "Aritheon_QuickSave");
            }
        }
        catch (System.Exception e) { Debug.LogError("Save Hatası: " + e.Message); }
        finally { _isSaving = false; }
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) interactionUI.SetActive(true); }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Yerdeki "Konuş/Etkileşim" butonunu her zaman gizle canım, 
            // çünkü oyuncu artık etkileşim başlatacak kadar yakın değil.
            if (interactionUI != null) interactionUI.SetActive(false);

            // KRİTİK NOKTA: Eğer ana diyalog paneli şu an AÇIK DEĞİLSE etkileşimi bitir.
            // Eğer panel açıksa, oyuncu uzaklaşsa bile EndInteraction() çağrılmayacak.
            if (mainDialogPanel != null && !mainDialogPanel.activeInHierarchy)
            {
                EndInteraction();
            }

            // Not: Eğer panel açıkken oyuncu çok uzağa giderse kapansın istersen, 
            // buraya bir mesafe kontrolü (Vector2.Distance) ekleyebiliriz ama 
            // istediğin mantıkta "X" butonuna basılana kadar kapanmayacaktır.
        }
    }
}