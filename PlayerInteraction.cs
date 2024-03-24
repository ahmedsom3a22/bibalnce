using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    PlayerController playerController;

    //The land the player is currently selecting
    Land selectedLand = null;

    //The interactable object the player is currently selecting
    InteractableObject selectedInteractable = null;

    // Start is called before the first frame update
    void Start()
    {
        //Get access to our PlayerController component
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 1))
        {
            OnInteractableHit(hit);
        }
    }

    //Handles what happens when the interaction raycast hits something interactable
    void OnInteractableHit(RaycastHit hit)
    {
        Collider other = hit.collider;

        //Check if the player is going to interact with land
        if (other.tag == "Land")
        {
            //Get the land component
            Land land = other.GetComponent<Land>();
            SelectLand(land);
            return;
        }

        //Check if the player is going to interact with an Item
        if (other.tag == "Item")
        {
            //Set the interactable to the currently selected interactable
            selectedInteractable = other.GetComponent<InteractableObject>();
            return;
        }

        //Deselect the interactable if the player is not standing on anything at the moment
        if (selectedInteractable != null)
        {
            selectedInteractable = null;
        }

        //Deselect the land if the player is not standing on any land at the moment
        if (selectedLand != null)
        {
            selectedLand.Select(false);
            selectedLand = null;
        }
    }


    //Handles the selection process of the land
    void SelectLand(Land land)
    {
        //Set the previously selected land to false (If any)
        if(selectedLand != null)
        {
            selectedLand.Select(false);
        }

        //Set the new selected land to the land we're selecting now
        selectedLand = land;
        land.Select(true);
    }

    //Triggered when the player presses the tool button
    public void Interact()
    {
        //The player shouldn't be able to use his tool when he has his hands full with an item
        if (InventoryManager.Instance.SlotEquipped(InventorySlot.InventoryType.Item))
        {
            return;
        }

        //Check if the player is selecting any land
        if (selectedLand != null)
        {
            selectedLand.Interact();
            return;
        }

        Debug.Log("Not on any land!");
    }


    //Triggered when the player presses the item interact button
    public void ItemInteract()
    {
        //Check if there is an interactable selected
        if (selectedInteractable != null)
        {
            //If the player isn't holding anything, pick up an item
            if (!selectedInteractable.canInteractWhileEquippingItem)
            {
                if (InventoryManager.Instance.SlotEquipped(InventorySlot.InventoryType.Item))
                {
                    return;
                }
            }

            //Pick it up
            selectedInteractable.Pickup();
        }

    }

    public void ItemKeep()
    {
        //If the player is holding something, keep it in his inventory
        if (InventoryManager.Instance.SlotEquipped(InventorySlot.InventoryType.Item))
        {
            InventoryManager.Instance.HandToInventory(InventorySlot.InventoryType.Item);
            return;
        }
    }

}
