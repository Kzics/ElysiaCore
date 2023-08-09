using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElysiaInteractMenu
{
    public class PlayerAlcoholManager
    {

        public Dictionary<int, float> AlcoholData;
        public Dictionary<int, long> LastAlcoholTimestamps;
        public PlayerAlcoholManager()
        {
        }

        public float GetAlcoholValue(int characterId) => !AlcoholData.ContainsKey(characterId) ? 0f : AlcoholData[characterId];
        public void AddAlcoholValue(int characterId, float amount)
        {
            if (AlcoholData.TryGetValue(characterId, out var currentValue))
            {
                AlcoholData[characterId] = currentValue + amount;
            }
            else
            {
                AlcoholData[characterId] = amount;
            }

            LastAlcoholTimestamps[characterId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        

        public long GetLastAlcoholTimestamp(int characterId) =>
            LastAlcoholTimestamps.TryGetValue(characterId, out var timestamp) ? timestamp : 0L;
    }
}