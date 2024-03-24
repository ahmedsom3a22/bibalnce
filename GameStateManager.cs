using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour ,ITimeTracker
{
    public static GameStateManager Instance { get; private set; }

    //Check if the screen has finished fading out
    bool screenFadedOut;

    private void Awake()
    {
        //If there is more than one instance, destroy the extra
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            //Set the static instance to this instance
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Add this to TimeManager's Listener list
        TimeManager.Instance.RegisterTracker(this);
    }

    public void ClockUpdate(GameTimestamp timestamp)
    {
        UpdateShippingState(timestamp);
        UpdateFarmState(timestamp);
        IncubationManager.UpdateEggs();

        if(timestamp.hour == 0 && timestamp.minute == 0)
        {
            OnDayReset();
        }
    }

    //Called when the day has been reset
    void OnDayReset()
    {
        Debug.Log("Day has been reset");
        foreach(NPCRelationshipState npc in RelationshipStats.relationships)
        {
            npc.hasTalkedToday = false;
            npc.giftGivenToday = false;
        }
    }

    void UpdateShippingState(GameTimestamp timestamp)
    {
        //Check if the hour is here (Exactly 1800 hours)
        if(timestamp.hour == ShippingBin.hourToShip && timestamp.minute == 0)
        {
            ShippingBin.ShipItems();
        }
    }

    public void UpdateFarmState(GameTimestamp timestamp)
    {
        //Update the Land and Crop Save states as long as the player is outside of the Farm scene
        if (SceneTransitionManager.Instance.currentLocation != SceneTransitionManager.Location.Farm)
        {
            //If there is nothing to update to begin with, stop
            if (LandManager.farmData == null) return;

            //Retrieve the Land and Farm data from the static variable
            List<LandSaveState> landData = LandManager.farmData.Item1;
            List<CropSaveState> cropData = LandManager.farmData.Item2;

            //If there are no crops planted, we don't need to worry about updating anything
            if (cropData.Count == 0) return;

            for (int i = 0; i < cropData.Count; i++)
            {
                //Get the crop and corresponding land data
                CropSaveState crop = cropData[i];
                LandSaveState land = landData[crop.landID];

                if (crop.cropState == CropBehaviour.CropState.Wilted) continue;

                //Update the Land's state
                land.ClockUpdate(timestamp);
                //Update the crop's state based on the land state
                if(land.landStatus == Land.LandStatus.Watered)
                {
                    crop.Grow();
                } else if(crop.cropState != CropBehaviour.CropState.Seed)
                {
                    crop.Wither();
                }

                //Update the element in the array
                cropData[i] = crop;
                landData[crop.landID] = land;

            }

            LandManager.farmData.Item2.ForEach((CropSaveState crop) =>{
                Debug.Log(crop.seedToGrow +"\n Health: " + crop.health + "\n Growth: " + crop.growth + "\n State: " + crop.cropState.ToString());
            });
        }
    }

    public void Sleep()
    {
        //Call a fadeout
        UIManager.Instance.FadeOutScreen();
        screenFadedOut = false;
        StartCoroutine(TransitionTime());
    }

    IEnumerator TransitionTime()
    {
        //Calculate how many ticks we need to advance the time to 6am

        //Get the time stamp of 6am the next day
        GameTimestamp timestampOfNextDay = TimeManager.Instance.GetGameTimestamp();
        timestampOfNextDay.day += 1;
        timestampOfNextDay.hour = 6;
        timestampOfNextDay.minute = 0;
        Debug.Log(timestampOfNextDay.day + " " + timestampOfNextDay.hour + ":" + timestampOfNextDay.minute);

        //Wait for the scene to finish fading out before loading the next scene
        while (!screenFadedOut)
        {
            yield return new WaitForSeconds(1f);
        }
        TimeManager.Instance.SkipTime(timestampOfNextDay);
        //Save
        SaveManager.Save(ExportSaveState());
        //Reset the boolean
        screenFadedOut = false;
        UIManager.Instance.ResetFadeDefaults();
    }

    //Called when the screen has faded out
    public void OnFadeOutComplete()
    {
        screenFadedOut = true;

    }

    public GameSaveState ExportSaveState()
    {
        //Retrieve From Data
        List<LandSaveState> landData;
        List<CropSaveState> cropData;

        if (LandManager.farmData != null)
        {
            landData = LandManager.farmData.Item1;
            cropData = LandManager.farmData.Item2;
        }
        else
        {
            landData = new List<LandSaveState>();
            cropData = new List<CropSaveState>();
        }

        //Retrieve Inventory Data
        ItemSlotData[] toolSlots = InventoryManager.Instance.GetInventorySlots(InventorySlot.InventoryType.Tool);
        ItemSlotData[] itemSlots = InventoryManager.Instance.GetInventorySlots(InventorySlot.InventoryType.Item);

        ItemSlotData equippedToolSlot = InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Tool);
        ItemSlotData equippedItemSlot = InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Item);

        //Time
        GameTimestamp timestamp = TimeManager.Instance.GetGameTimestamp();
        return new GameSaveState(landData, cropData, toolSlots, itemSlots, equippedItemSlot, equippedToolSlot, timestamp, PlayerStats.Money, RelationshipStats.relationships, IncubationManager.eggIncubating);
    }

    public void LoadSave()
    {
        //Retrieve the loaded save
        GameSaveState save = SaveManager.Load();
        //Load up the parts

        //Time
        TimeManager.Instance.LoadTime(save.timestamp);

        //Inventory
        ItemSlotData[] toolSlots = ItemSlotData.DeserializeArray(save.toolSlots);
        ItemSlotData equippedToolSlot = ItemSlotData.DeserializeData(save.equippedToolSlot);
        ItemSlotData[] itemSlots = ItemSlotData.DeserializeArray(save.itemSlots);
        ItemSlotData equippedItemSlot = ItemSlotData.DeserializeData(save.equippedItemSlot);
        InventoryManager.Instance.LoadInventory(toolSlots, equippedToolSlot, itemSlots, equippedItemSlot);

        //Farming data
        LandManager.farmData = new System.Tuple<List<LandSaveState>, List<CropSaveState>>(save.landData, save.cropData);

        //Player Stats
        PlayerStats.LoadStats(save.money);

        //Relationship stats
        RelationshipStats.LoadStats(save.relationships);

        //Animal Farming Stats
        IncubationManager.eggIncubating = save.eggIncubating;
    }
}
