using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWoodBranch
{
    [BepInPlugin(FineWoodBranch.PluginId, "Finewood Branches", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class FineWoodBranch : BaseUnityPlugin
    {
        public const string PluginId = "mennowar.mods.FineWoodBranches";
        public const string SharedName = "FineWoodBranches";
        static Random rnd = new Random();
        public static ConfigEntry<bool> writeDebugOutput;
        private static ManualLogSource log = null;
        public static ConfigEntry<bool> IsEnabled;
        public static ConfigEntry<bool> IsMeadowsEnabled;
        public static ConfigEntry<bool> IsBlackforestEnabled;
        public static ConfigEntry<bool> IsSwampEnabled;

        public static ConfigEntry<bool> ShowInfoText;
        public static ConfigEntry<int> Chance;
        public static ConfigEntry<int> Amount;
        Harmony harmony = new Harmony(PluginId);

        public static void Debug(string value)
        {
            if (writeDebugOutput.Value)
            {
                if (log == null)
                {
                    log = BepInEx.Logging.Logger.CreateLogSource(FineWoodBranch.SharedName);
                }

                if (log != null)
                {
                    log.LogMessage(value);
                }
            }
        }

        private void Awake()
        {
            IsEnabled = Config.Bind<bool>("General", "isEnabled", true, "Is this Mod enabled?");
            Chance = Config.Bind<int>("General", "dropChance", 30, "The Dropchance of Finewood (% Value)");
            Amount = Config.Bind<int>("General", "dropAmount", 2, "The number of Finewood that you will find");
            ShowInfoText = Config.Bind<bool>("General", "showMessage", true, "Display Infotext when special wood has been found?");
            writeDebugOutput = Config.Bind<bool>("Debug", "writeDebug", true, "Write Debug Informations to the console?");
            IsMeadowsEnabled = Config.Bind<bool>("Biomes", "meadowsEnabled", true, "Enable special wood finding in the Meadows?");
            IsBlackforestEnabled = Config.Bind<bool>("Biomes", "blackforestEnabled", true, "Enable special wood finding in the Black Forest?");
            IsSwampEnabled = Config.Bind<bool>("Biomes", "swampEnabled", true, "Enable special wood finding in the Swamp?");

            harmony.PatchAll();

            Debug($"{SharedName} is {(IsEnabled.Value ? "Enabled" : "Disabled")} with a change of {Chance.Value} and amount of {Amount.Value}");
            Debug($"I will {(ShowInfoText.Value ? "" : "<b>NOT</b>")} show Notification");
            Debug($"Add wood in Meadows: {IsMeadowsEnabled.Value}");
            Debug($"Add wood in BF: {IsBlackforestEnabled.Value}");
            Debug($"Add wood in Swamp: {IsSwampEnabled.Value}");
        }

        public static void ProcessPickup(Inventory inventory)
        {
            if (!IsEnabled.Value || inventory == null) return;
            string fineWoodName = string.Empty;

            var rolled = rnd.Next(1, 100);
            // Debug($"Pickup Rolled a: {rolled} with a chance of {Chance.Value}%");
            Debug($"Pickup Rolled a: {rolled}");

            if (rolled <= Chance.Value && Player.m_localPlayer != null)
            {
                var currentBiome = Player.m_localPlayer.GetCurrentBiome();
                if (IsMeadowsEnabled.Value && currentBiome == Heightmap.Biome.Meadows || currentBiome == Heightmap.Biome.Mountain)
                {
                    fineWoodName = "FineWood";
                } else if (IsBlackforestEnabled.Value && currentBiome == Heightmap.Biome.BlackForest)
                {
                    fineWoodName = "RoundLog";
                }
                else if (IsSwampEnabled.Value && currentBiome == Heightmap.Biome.Swamp)
                {
                    fineWoodName = "ElderBark";
                }

                inventory.AddItem(fineWoodName, Amount.Value, 1, 0, 0, string.Empty);
                if (Player.m_localPlayer != null && ShowInfoText != null && ShowInfoText.Value)
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"You found {Amount.Value} {fineWoodName} under the branches", 0, null);
            }
        }

        [HarmonyPatch(typeof(Pickable), "Interact")]
        static class PickPatch0
        {
            static void Prefix(Humanoid character, bool repeat, bool alt)
            {
                if (repeat || character == null) return;

                var hoverObject = character.GetHoverObject();
                if (hoverObject == null) return;

                var componentInParent = hoverObject.GetComponentInParent<Pickable>();
                if (componentInParent != null && !string.IsNullOrEmpty(componentInParent.name))
                {
                    if (componentInParent.name.ToUpper().StartsWith("PICKABLE_BRANCH"))
                    {
                        var inv = character.GetInventory();
                        ProcessPickup(inv);
                    }
                }
            }
        }
    }
}
