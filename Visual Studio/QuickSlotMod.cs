using UnityEngine;
using Partiality.Modloader;
using System.IO;
using System;
using SimpleJSON;
namespace OutwardQuickSlotMod
{
    public class QuickSlotMod : PartialityMod
    {

        public static ScriptLoad qsmPatch;
        public static string modConfigFile = "Mods/QuickSlot_Config.json";
        public SimpleJSON.JSONNode modConfig;
        public JSONNode currentCharacter;


        public QuickSlotMod()
        {
            this.ModID = "Quick Slot Switcher";
            this.Version = "0.4";
            this.author = "Emo#7953";

        }

        public override void OnEnable()
        {
            base.OnEnable();
     
            ScriptLoad.qsm = this;

            LoadConfig();

            GameObject obj = new GameObject();
            qsmPatch = obj.AddComponent<ScriptLoad>();
            qsmPatch.Initialise();


            Debug.Log("::QUICK SLOT MOD ENABLED::");
        }


        public void LoadConfig()
        {
            //Debug.Log(":: Loading Config ::");
            string json = File.ReadAllText(modConfigFile);
           
            modConfig = JSON.Parse(json);
        }

        public void SetCurrentCharacter(string playerUID, string playerName)
        {

            //Debug.Log("Setting Current Character UID " + playerUID + " name " + playerName);
            if (modConfig["characters"] != null)
            {
                //Debug.Log("Found Characters Array in Config File");

                if (DoesCharacterExist(playerUID))
                {
                   // Debug.Log("Character Does Exist in Config File");
                    var charSaveIndex = GetCharacterSaveIndex(playerUID);
                    var charSaveObject = modConfig["characters"][charSaveIndex];


                    //Debug.Log("Setting as currentCharacter");
                    currentCharacter = charSaveObject;
                }
                else
                {
                    //Debug.Log("Character Does Not Exist in Config File");


                    //Debug.Log("Setting as currentCharacter");
                    currentCharacter = CreateNewCharacterSave(playerUID, playerName);
                }


            }

            SaveConfig();
        }


        public bool DoesCharacterExist(string playerUID)
        {
            for (int i = 0; i < modConfig["characters"].AsArray.Count; i++)
            {

                var eachCharID = modConfig["characters"][i]["characterUID"];
                //if the current ID exists modify instead of adding
                if (eachCharID == playerUID)
                {
                    return true;
                }

            }

            return false;
        }

        public int GetCharacterSaveIndex(string playerUID)
        {
            for (int i = 0; i < modConfig["characters"].AsArray.Count; i++)
            {

                var eachCharID = modConfig["characters"][i]["characterUID"];
                //if the current ID exists modify instead of adding
                if (eachCharID == playerUID)
                {
                    return i;
                }

            }

            return 0;
        }

        public JSONObject CreateNewCharacterSave(string characterUID, string characterName)
        {

            //Debug.Log("Adding a New Character");
            var newplayerObject = new JSONObject();
            modConfig["characters"][-1] = newplayerObject;


            //values to add the new player object
            var newPlayerUID = new JSONString(characterUID);
            var newPlayerName = new JSONString(characterName);
            var newPlayerDefaultBind = new JSONString("tab");
            var newPlayerDefaultBarIDS = new JSONArray();

            newPlayerDefaultBarIDS[0] = 0;
            newPlayerDefaultBarIDS[1] = 0;
            newPlayerDefaultBarIDS[2] = 0;
            newPlayerDefaultBarIDS[3] = 0;
            newPlayerDefaultBarIDS[4] = 0;
            newPlayerDefaultBarIDS[5] = 0;
            newPlayerDefaultBarIDS[6] = 0;
            newPlayerDefaultBarIDS[7] = 0;

            var newPlayerSecondBarIDS = new JSONArray();

            newPlayerSecondBarIDS[0] = 0;
            newPlayerSecondBarIDS[1] = 0;
            newPlayerSecondBarIDS[2] = 0;
            newPlayerSecondBarIDS[3] = 0;
            newPlayerSecondBarIDS[4] = 0;
            newPlayerSecondBarIDS[5] = 0;
            newPlayerSecondBarIDS[6] = 0;
            newPlayerSecondBarIDS[7] = 0;

            //now actually add them to the object
            newplayerObject.Add("characterUID", newPlayerUID);
            newplayerObject.Add("characterName", newPlayerName);
            newplayerObject.Add("keybind", newPlayerDefaultBind);
            newplayerObject.Add("DefaultBarIDS", newPlayerDefaultBarIDS);
            newplayerObject.Add("SecondaryBarIDS", newPlayerSecondBarIDS);

            return newplayerObject;
        }

        public void SaveConfig()
        {
            //Debug.Log(":: Saving Config ::");
            //var newJson = JSON.ToJson(modConfig.ToString());
            File.WriteAllText(modConfigFile, modConfig.ToString());
        }
    }

    [Serializable]
    public class ModConfig
    {
        public string SwitchBarBind;
        public int[] DefaultBarItemIDs;
        public int[] SecondaryBarItemIDs;
    }
}
