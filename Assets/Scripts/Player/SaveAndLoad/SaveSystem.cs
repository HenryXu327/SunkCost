using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections.Generic;

namespace SaveAndLoad
{
    [Serializable]
    internal class PlayersStore
    {
        public Dictionary<string, PlayerData> PlayerIdToData = new Dictionary<string, PlayerData>();
    }

    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "players.dat");

        private static PlayersStore LoadStore()
        {
            if (!File.Exists(SavePath))
            {
                return new PlayersStore();
            }

            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(SavePath, FileMode.Open))
            {
                return (PlayersStore)formatter.Deserialize(stream);
            }
        }

        private static void SaveStore(PlayersStore store)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(SavePath, FileMode.Create))
            {
                formatter.Serialize(stream, store);
            }
        }

        public static void SavePlayer(string playerId, PlayerData data)
        {
            if (string.IsNullOrEmpty(playerId) || data == null) return;
            var store = LoadStore();
            store.PlayerIdToData[playerId] = data;
            SaveStore(store);
        }

        public static bool TryLoadPlayer(string playerId, out PlayerData data)
        {
            data = null;
            if (string.IsNullOrEmpty(playerId)) return false;
            var store = LoadStore();
            if (store.PlayerIdToData != null && store.PlayerIdToData.TryGetValue(playerId, out var found))
            {
                data = found;
                return true;
            }
            return false;
        }

        public static bool PlayerExists(string playerId)
        {
            if (string.IsNullOrEmpty(playerId)) return false;
            var store = LoadStore();
            return store.PlayerIdToData != null && store.PlayerIdToData.ContainsKey(playerId);
        }

        public static Dictionary<string, PlayerData> LoadAllPlayers()
        {
            var store = LoadStore();
            return new Dictionary<string, PlayerData>(store.PlayerIdToData);
        }
    }
}