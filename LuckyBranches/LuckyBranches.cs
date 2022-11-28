using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using Random = System.Random;

namespace LuckyBranchesNS
{
    [BepInPlugin(LuckyBranches.PluginId, "LuckyBranches", "1.0.1")]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class LuckyBranches : BaseUnityPlugin
    {   public const string PluginId = "mennowar.mods.LuckyBranches";
        public const string SharedName = "LuckyBranches";
        
        public static ConfigEntry<bool> WriteDebugOutput;
        public static ConfigEntry<bool> IsEnabled;
        public static ConfigEntry<bool> IsMeadowsEnabled;
        public static ConfigEntry<bool> IsBlackforestEnabled;
        public static ConfigEntry<bool> IsSwampEnabled;

        public static ConfigEntry<bool> ShowInfoText;
        public static ConfigEntry<int> Chance;
        public static ConfigEntry<int> Amount;

        private static Random m_random = new Random(); // randomizer for the rolling of pickup chance
        private static ManualLogSource m_logSource = null;  // just a logger instance
        private Harmony m_Harmony = new Harmony(PluginId);

        /// <summary>
        /// When WriteDebugOutput is enabled, writes to the console as message
        /// </summary>
        /// <param name="value"></param>
        public static void Debug(string value)
        {
            if (WriteDebugOutput.Value)
            {
                if (m_logSource == null)
                {
                    m_logSource = BepInEx.Logging.Logger.CreateLogSource(LuckyBranches.SharedName);
                }

                if (m_logSource != null)
                {
                    m_logSource.LogMessage(value);
                }
            }
        }

        /// <summary>
        /// Just gives some Debug Information about the current settings
        /// </summary>
        private static void WriteStatus()
        {
            Debug($"{SharedName} is {(IsEnabled.Value ? "Enabled" : "Disabled")} with a chance of {Chance.Value} and amount of {Amount.Value}");
            Debug($"I will {(ShowInfoText.Value ? "" : "<b>NOT</b>")} show Notification");
            Debug($"Add wood in Meadows: {IsMeadowsEnabled.Value}");
            Debug($"Add wood in BF: {IsBlackforestEnabled.Value}");
            Debug($"Add wood in Swamp: {IsSwampEnabled.Value}");
        }

        /// <summary>
        /// Creates the configuration values / initial config file
        /// </summary>
        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;

            IsEnabled = Config.Bind<bool>("General", "isEnabled", true,
                new ConfigDescription("Is this mod enabled?", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            Chance = Config.Bind<int>("General", "dropChance", 30,
                new ConfigDescription("The Dropchance of special wood (% Value)", new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            Amount = Config.Bind("General", "dropAmount", 2,
                new ConfigDescription("The amount of special wood that you will find",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ShowInfoText = Config.Bind<bool>("General", "showMessage", true,
                new ConfigDescription("Display Infotext when special wood has been found?", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            WriteDebugOutput = Config.Bind<bool>("Debug", "writeDebug", true,
                new ConfigDescription("Write Debug Informations to the console?", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            IsMeadowsEnabled = Config.Bind<bool>("Biomes", "meadowsEnabled", true,
                new ConfigDescription("Enable special wood finding in the Meadows?", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            IsBlackforestEnabled = Config.Bind<bool>("Biomes", "blackforestEnabled", true,
                new ConfigDescription("Enable special wood finding in the Black Forest?", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            IsSwampEnabled = Config.Bind<bool>("Biomes", "swampEnabled", true,
                new ConfigDescription("Enable special wood finding in the Swamp??", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            SynchronizationManager.OnConfigurationSynchronized += SynchronizationManager_OnConfigurationSynchronized;
        }

        /// <summary>
        /// .. don't awake without any coffe or it's a bad day.
        /// </summary>
        private void Awake()
        {
            CreateConfigValues();

            m_Harmony.PatchAll();
            WriteStatus();
        }

        /// <summary>
        /// Just log to the console what kind of config change has been detected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SynchronizationManager_OnConfigurationSynchronized(object sender, Jotunn.Utils.ConfigurationSynchronizationEventArgs e)
        {
            if (e.InitialSynchronization)
            {
                Jotunn.Logger.LogMessage("Initial Config sync event received");
            }
            else
            {
                Jotunn.Logger.LogMessage("Config sync event received");
            }

            WriteStatus();
        }

        /// <summary>
        /// Is called each time a branch is picked up. Works with mass-farming like a charm
        /// </summary>
        /// <param name="inventory"></param>
        public static void ProcessPickup(Inventory inventory)
        {
            if (!IsEnabled.Value || inventory == null) return;
            string fineWoodName = string.Empty;

            var rolled = m_random.Next(1, 100);

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

        /// <summary>
        /// Patcherclass for picking up objects
        /// </summary>
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
