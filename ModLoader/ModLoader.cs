﻿using System;
using System.IO;
using System.Reflection;
using UnityEngine;

/* 
 * Todo:
 * 
 * Implement a window in the main menu that shows which mods are loaded
 */

namespace Modding {
    public static class ModLoader {
//        private static readonly List<IMod> _loadedMods = new List<IMod>();

        public static void Initialize(RainWorld rainworld) {
            ModLogger.EnableLogging();
            Debug.Log("Yay, we're in the mod loader! Rainworld version: " + rainworld.gameVersion);
            
            // Iterate over mods, load them in order

            string modsPath = Path.Combine(GetGameRootPath(), "Mods");
            var modDirs = Directory.GetDirectories(modsPath);

            for (int i = 0; i < modDirs.Length; i++) {
                var assembly = LoadModAssemblyFromDirectory(modDirs[i]);
                if (assembly != null) {
                    LoadModFromAssembly(assembly);
                }
                else {
                    Debug.LogError("Failed to load mod assembly, skipping...");
                }
            }

//            Debug.Log("Loaded mods: ");
//            for (int i = 0; i < _loadedMods.Count; i++) {
//                Debug.Log("" + i + ": " + _loadedMods[i]);
//            }
        }

        private static string GetGameRootPath() {
            // GetExecutingAssembly().location gives managed assembly dir, so root is two dirs up
            var directoryInfo = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent;
            string path = directoryInfo?.ToString();

            return path;
        }

        private static Assembly LoadModAssemblyFromDirectory(string path) {
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++) {
                if (files[i].Contains("Mod.dll")) {
                    Debug.Log("Found Mod Assembly: " + files[i]);
                    var assembly = Assembly.LoadFrom(files[i]);
                    return assembly;
                }
            }
            
            return null;
        }

        private static void LoadModFromAssembly(Assembly assembly) {
            foreach (Module module in assembly.GetModules()) {
                foreach (Type type in module.GetTypes()) {
                    if (type.Name.EndsWith("Mod")) { // Todo: lol, make more rigorous
                        Console.WriteLine("Found Mod Entrypoint! " + type.FullName);

                        try {
                            var initMethod = type.GetMethod("Initialize");
                            if (initMethod == null) {
                                Debug.LogError("Couldn't find a public static Intialize() method on mod");
                                return;
                            }
                            initMethod.Invoke(null, null);
                        }
                        catch (Exception e) {
                            Debug.LogError($"Something went wrong loading {type.FullName}, {e.Message}");
                        }
                    }
                }
            }
        }
    }

//    public interface IMod {
//        string Name { get; }
//        string Version { get; }
//
//        void Init(RainWorld rainworld);
//    }

    public static class ModLogger {
        public static void EnableLogging() {
            if (File.Exists("exceptionLog.txt"))
                File.Delete("exceptionLog.txt");
            if (File.Exists("consoleLog.txt"))
                File.Delete("consoleLog.txt");
            Application.RegisterLogCallback(new Application.LogCallback(HandleLog));
        }

        public static void HandleLog(string logString, string stackTrace, LogType type) {
            if (type == LogType.Error || type == LogType.Exception) {
                File.AppendAllText("exceptionLog.txt", logString + Environment.NewLine);
                File.AppendAllText("exceptionLog.txt", stackTrace + Environment.NewLine);
                return;
            }
            File.AppendAllText("consoleLog.txt", logString + Environment.NewLine);
        }
    }
}

