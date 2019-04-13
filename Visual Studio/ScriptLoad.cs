using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using On;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Rewired;

namespace OutwardQuickSlotMod
{
	public class ScriptLoad : MonoBehaviour
	{
		public static QuickSlotMod qsm;

		private ChatPanel cp;

		public Item[] defaultSlotItems;
		public Item[] secondarySlotItems;
		public string[] defaultIDs = new string[8];


		private bool isInited = false;
		private int slotAmount = 8;
		private string charID;

		private bool dev = true;

		public enum BarMode
		{
			DEFAULT,
			SECONDARY
		}


		public BarMode barMode = BarMode.DEFAULT;


		public void Initialise()
		{
			// Debug.Log("::QUICK SLOT MOD PATCH INIT::");
			Patch();
		}

		public void Patch()
		{
			On.Character.Update += new On.Character.hook_Update(characterUpdateHook);
			On.PlayerSystem.Start += new On.PlayerSystem.hook_Start(playerSystemStartHook);

			On.CharacterQuickSlotManager.ctor += new On.CharacterQuickSlotManager.hook_ctor(characterQuickSlotCtor);
			On.CharacterQuickSlotManager.OnAssigningQuickSlot_1 += new On.CharacterQuickSlotManager.hook_OnAssigningQuickSlot_1(onAsignSlotHook);
			//On.Global.OnApplicationQuit += new On.Global.hook_OnApplicationQuit(onAppQuitHook);
		}

		private void playerSystemStartHook(On.PlayerSystem.orig_Start orig, PlayerSystem self)
		{
			orig(self);
			qsm.LoadConfig();
            defaultSlotItems = new Item[8];
            secondarySlotItems = new Item[8];
            qsm.SetCurrentCharacter(self.CharUID, self.Name);

		}

		private void characterQuickSlotCtor(On.CharacterQuickSlotManager.orig_ctor orig, CharacterQuickSlotManager self)
		{
			orig(self);
			

			//LoadQuickSlotsFromJSON(self);

		}

		private void characterUpdateHook(On.Character.orig_Update orig, Character self)
		{
			orig(self);
			var QuickSlotManager = self.GetComponent<CharacterQuickSlotManager>();

			if (QuickSlotManager != null)
			{
                if (barMode == BarMode.SECONDARY)
                {
                    if (Input.GetKeyDown(qsm.currentCharacter["keybind"]))
                    {

                        if (dev)
                        {
                            Debug.Log(":::::");
                            Debug.Log("TAB PRESSED USE DEFAULT BAR");
                        }


                        SetBarMode(BarMode.DEFAULT);
                        SwitchQuickSlot(QuickSlotManager);
                    }
                }
                else if (barMode == BarMode.DEFAULT)
                {
                    if (Input.GetKeyDown(qsm.currentCharacter["keybind"]))
                    {

                        if (dev)
                        {
                            Debug.Log(":::::");
                            Debug.Log("TAB PRESSED USING SECONDARY BAR");
                        }

                        SetBarMode(BarMode.SECONDARY);
                        SwitchQuickSlot(QuickSlotManager);
                    }
                }
            }
			

		}

		private void onAppQuitHook(On.Global.orig_OnApplicationQuit orig, Global self)
		{
			orig(self);
			//Debug.Log("GAme quitting setting bar 1 as default");
			//SetBarMode(BarMode.DEFAULT);
			//SwitchQuitSlot(charQuickSlot);
		}

		private void onAsignSlotHook(On.CharacterQuickSlotManager.orig_OnAssigningQuickSlot_1 orig, CharacterQuickSlotManager self, Item _itemToQuickSlot)
		{
			orig(self, _itemToQuickSlot);
			if (dev)
			{
				Debug.Log("ASSIGNING Quickslot Index" + _itemToQuickSlot.QuickSlotIndex + " to " + _itemToQuickSlot + " on bar mode " + barMode.ToString());
			}

            if (_itemToQuickSlot.QuickSlotIndex == -1)
            {
                //Debug.Log("On Assign Quick slot index is -1");
            }
            else
            {
                SaveQuickSlotByIndex(_itemToQuickSlot.QuickSlotIndex, _itemToQuickSlot);
            }
			
			//qsm.LoadConfig();
		}

		private void SaveQuickSlotByIndex(int index, Item item)
		{
			switch (barMode)
			{
				case BarMode.DEFAULT:				
					if (dev)
					{
						Debug.Log("Setting default temp array " + index + " to " + item);
						Debug.Log(defaultSlotItems[index]);
					}

                    defaultSlotItems[index] = item;
                    qsm.currentCharacter["DefaultBarIDS"][index] = item.ItemID;
					break;
				case BarMode.SECONDARY:				
					if (dev)
					{
						Debug.Log("Setting secondary temp array " + index + " to " + item);
						Debug.Log(secondarySlotItems[index]);
					}
                    secondarySlotItems[index] = item;
                    qsm.currentCharacter["SecondaryBarIDS"][index] = item.ItemID;
					break;
			}

			qsm.SaveConfig();
            //qsm.LoadConfig();
        }


		private void LoadQuickSlotsFromJSON(CharacterQuickSlotManager self)
		{        
			CharacterInventory charInvent = self.gameObject.GetComponent<CharacterInventory>();
			CharacterSkillKnowledge charSkills = charInvent.SkillKnowledge;
			ItemManager itemMan = ItemManager.Instance;


            //Load Quickslots for Bar 1
			for (int i = 0; i < slotAmount; i++)
			{
                if (dev)
                {
                    Debug.Log("LOADING QUICK SLOT " + i + " FOR BAR 1");
                }

                var itemID = qsm.currentCharacter["DefaultBarIDS"][i];
				if (itemID != 0)
				{
                    if (dev)
                    {
                        Debug.Log("ID of slot " + i + " from JSON is : " + itemID);
                    }
                    var item = charSkills.GetItemFromItemID(itemID.AsInt);

                    //this isnt a skill
                    if (item == null)
                    {
                        item = charInvent.GetOwnedItems(itemID).First();
                    }

                    defaultSlotItems[i] = item;			
				}
				else
				{
					if (dev)
					{
						Debug.Log("QUICK SLOT " + i + " Has No ID");
					}
				}
			}


			for (int i = 0; i < slotAmount; i++)
			{
				if (dev)
				{
					Debug.Log("LOADING QUICK SLOT " + i + " FOR BAR 2");
				}
				var itemID = qsm.currentCharacter["SecondaryBarIDS"][i];
				if (itemID != 0)
				{
					if (dev)
					{
						Debug.Log("ID of slot " + i + " from JSON is : " + itemID);
					}
                    var item = charSkills.GetItemFromItemID(itemID.AsInt);

                    //this isnt a skill
                    if (item == null)
                    {
                        item = charInvent.GetOwnedItems(itemID).First();
                    }

                    secondarySlotItems[i] = item;
				}
				else
				{
					if (dev)
					{
						Debug.Log("QUICK SLOT " + i + " Has No ID");
					}
				}
			}
		}


		private void SwitchQuickSlot(CharacterQuickSlotManager self)
		{
            LoadQuickSlotsFromJSON(self);

			if (self.isActiveAndEnabled)
			{
				if (dev)
				{
					Debug.Log("Switching Quick Slots");
				}

				switch (barMode)
				{
					//Tab has been pressed while in secondary mode
					//switch it to default mode
				case BarMode.DEFAULT:
				if (dev)
				{
					Debug.Log("In Default Mode");
				}
				for (int i = 0; i < slotAmount; i++)
				{
					//get a reference to the items current in the bar in secondary mode
					 Item itemFromLastBar = self.GetQuickSlot(i).ActiveItem;

					//check the replacement item isnt null
					 if (defaultSlotItems[i] != null)
					{
						if (dev)
						{
							Debug.Log("Switching QuickSlot Index " + i + " from " + itemFromLastBar + " TO " + defaultSlotItems[i]);
							Debug.Log(itemFromLastBar);
                         }

						//add these references to temp array
                        SetSecondarySlotArrayAtIndex(i, itemFromLastBar);
						self.SetQuickSlot(i, defaultSlotItems[i], false);
					}
					//if it is null
					 else
					{
						//clear the slot
					    if (dev)
					    {
						    Debug.Log("There is no replacement skill for this quick slot, clearing ");
					    }
						self.ClearQuickSlot(i);
					}
				}

				break;

				case BarMode.SECONDARY:
				if (dev)
				{
					Debug.Log("In Secondary Mode");
				}

                //Iterate current slots and add them to DefaultSlotArray
				for (int i = 0; i < slotAmount; i++)
				{
					Item itemFromLastBar = self.GetQuickSlot(i).ActiveItem;
					if (secondarySlotItems[i] != null)
					{
						if (dev)
						{
                            Debug.Log("Switching QuickSlot Index " + i + " from " + itemFromLastBar + " TO " + secondarySlotItems[i]);
						}

                        SetDefaultSlotArrayAtIndex(i, itemFromLastBar);
						self.SetQuickSlot(i, secondarySlotItems[i], false);
					}
					else
					{
						if (dev)
						{
							Debug.Log("There is no replacement skill for this quick slot, clearing ");
						}
						self.ClearQuickSlot(i);
					}

				}

				break;
			}


		}
	}

    private void SetDefaultSlotArrayAtIndex(int index, Item item)
    {
            defaultSlotItems[index] = item;
    }

    private void SetSecondarySlotArrayAtIndex(int index, Item item)
    {
        secondarySlotItems[index] = item;
    }


    private void SetBarMode(BarMode bm)
	{
		barMode = bm;
	}
 
	private void setChatPanelReference(On.ChatPanel.orig_StartInit orig, ChatPanel self)
	{
		cp = self;
	}

	}
}
