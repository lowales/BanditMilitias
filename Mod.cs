﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using static Bandit_Militias.Helpers.Helper;
using static Bandit_Militias.Globals;

// ReSharper disable ConditionIsAlwaysTrueOrFalse  
// ReSharper disable ClassNeverInstantiated.Global  
// ReSharper disable UnusedMember.Local  
// ReSharper disable InconsistentNaming

namespace Bandit_Militias
{
    // ReSharper disable once UnusedMember.Global
    public enum LogLevel
    {
        Disabled,
        Info,
        Error,
        Debug
    }

    public class Mod : MBSubModuleBase
    {
        internal const LogLevel logging = LogLevel.Disabled;
        internal static readonly Harmony harmony = new Harmony("ca.gnivler.bannerlord.BanditMilitias");
        private static readonly string modDirectory = new FileInfo(@"..\..\Modules\Bandit Militias\").DirectoryName;

        internal static void Log(object input, LogLevel logLevel = LogLevel.Debug)
        {
            if (Globals.Settings == null || !Globals.Settings.Debug)
            {
                return;
            }

            FileLog.Log($"[Bandit Militias] {input ?? "null"}");
            using (var sw = new StreamWriter(Path.Combine(modDirectory, "mod.log"), true))
            {
                sw.WriteLine($"[{DateTime.Now:G}] {input ?? "null"}");
            }
        }

        protected override void OnSubModuleLoad()
        {
            try
            {
                ReadConfig();
                Log($"Startup {DateTime.Now.ToShortTimeString()}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log(ex, LogLevel.Error);
                Globals.Settings = new Settings();
            }

            CacheBanners();
            RunManualPatches();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void CacheBanners()
        {
            //T.Restart();
            for (var i = 0; i < 10_000; i++)
            {
                // need to cache the banners before CEK adds background colours which
                // causes custom banners to crash for reasons unknown
                // have to use a seed because the game hasn't initialized the game's
                // inefficient but does it really matter for 38ms once at startup
                Banners.Add(Banner.CreateRandomBanner((int) DateTime.Now.Ticks));
            }

            //Log(T.ElapsedMilliseconds);
        }

        private static void ReadConfig()
        {
            try
            {
                var fileName = Path.Combine(modDirectory, "mod_settings.json");
                if (File.Exists(fileName))
                {
                    Globals.Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(fileName));
                    ClampSettingsValues(ref Globals.Settings);
                    PrintValidatedSettings(Globals.Settings);
                }
                else
                {
                    Log($"Configuration file expected at {fileName} but not found, using default settings", LogLevel.Error);
                    Globals.Settings = new Settings();
                }
            }
            catch (Exception ex)
            {
                FileLog.Log(ex.ToString());
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            if ((Input.IsKeyDown(InputKey.LeftControl) || Input.IsKeyDown(InputKey.RightControl)) &&
                (Input.IsKeyDown(InputKey.LeftAlt) || Input.IsKeyDown(InputKey.RightAlt)) &&
                (Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift)) &&
                Input.IsKeyPressed(InputKey.F11))
            {
                TestingMode = !TestingMode;
                InformationManager.AddQuickInformation(new TextObject("Testing mode: " + TestingMode));
            }

            if ((Input.IsKeyDown(InputKey.LeftControl) || Input.IsKeyDown(InputKey.RightControl)) &&
                (Input.IsKeyDown(InputKey.LeftAlt) || Input.IsKeyDown(InputKey.RightAlt)) &&
                (Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift)) &&
                Input.IsKeyPressed(InputKey.C))
            {
                ReadConfig();
                InformationManager.AddQuickInformation(new TextObject("Reloaded config"));
            }

            if ((Input.IsKeyDown(InputKey.LeftControl) || Input.IsKeyDown(InputKey.RightControl)) &&
                (Input.IsKeyDown(InputKey.LeftAlt) || Input.IsKeyDown(InputKey.RightAlt)) &&
                (Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift)) &&
                Input.IsKeyPressed(InputKey.F12))
            {
                foreach (var militia in PartyMilitiaMap.Values.OrderByDescending(x => x.MobileParty.MemberRoster.TotalManCount))
                {
                    Log($">> {militia.Hero.Name,-30}: {militia.MobileParty.MemberRoster.TotalManCount}/{militia.MobileParty.Party.TotalStrength:0}");
                    for (int tier = 0; tier <= 6; tier++)
                    {
                        Log($"  Tier {tier}: {militia.MobileParty.MemberRoster.GetTroopRoster().Where(x => x.Character.Tier == tier).Sum(x => x.Number)}.");
                    }
                }

                Log($">> Total {PartyMilitiaMap.Values.Count} = {PartyMilitiaMap.Values.Select(x => x.MobileParty.MemberRoster.TotalManCount).Sum()}");
            }

            if ((Input.IsKeyDown(InputKey.LeftControl) || Input.IsKeyDown(InputKey.RightControl)) &&
                (Input.IsKeyDown(InputKey.LeftAlt) || Input.IsKeyDown(InputKey.RightAlt)) &&
                Input.IsKeyPressed(InputKey.N))
            {
                try
                {
                    Nuke();
                    Nuke();
                    InformationManager.AddQuickInformation(new TextObject("BANDIT MILITIAS CLEARED"));
                }
                catch (Exception ex)
                {
                    Log(ex, LogLevel.Error);
                }
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (gameStarterObject is CampaignGameStarter gameStarter)
            {
                gameStarter.AddBehavior(new MilitiaBehavior());
            }
        }

        private static void RunManualPatches()
        {
            //try
            //{
            //}
            //catch (Exception ex)
            //{
            //    Log(ex, LogLevel.Error);
            //}
        }
    }
}
