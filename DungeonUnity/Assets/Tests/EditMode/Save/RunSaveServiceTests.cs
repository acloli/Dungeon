using System.Collections.Generic;
using Dungeon.Runtime.InGame.Save.Model;
using NUnit.Framework;
using UnityEngine;

namespace Dungeon.Tests.EditMode.Save
{
    /// <summary>
    /// RunSaveDataおよびRunSaveServiceのEditModeテスト
    /// </summary>
    [TestFixture]
    public sealed class RunSaveServiceTests
    {
        [Test]
        public void RunSaveData_CanBeSerializedToJson()
        {
            RunSaveData data = new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 80,
                PlayerHp = 45,
                PlayerEnergy = 3,
                Gold = 150,
                CurrentNodeIndex = 5,
                CurrentPage = 1, // Map
                DeckCardIds = new List<int> { 101, 102, 103 }
            };

            string json = JsonUtility.ToJson(data);

            Assert.That(json, Does.Contain("\"RunProfileId\":5501"));
            Assert.That(json, Does.Contain("\"PlayerMaxHp\":80"));
            Assert.That(json, Does.Contain("\"PlayerHp\":45"));
            Assert.That(json, Does.Contain("\"PlayerEnergy\":3"));
            Assert.That(json, Does.Contain("\"Gold\":150"));
            Assert.That(json, Does.Contain("\"CurrentNodeIndex\":5"));
            Assert.That(json, Does.Contain("\"CurrentPage\":1"));
            Assert.That(json, Does.Contain("\"DeckCardIds\":[101,102,103]"));
        }

        [Test]
        public void RunSaveData_CanBeDeserializedFromJson()
        {
            string json = "{\"RunProfileId\":5501,\"PlayerMaxHp\":80,\"PlayerHp\":45,\"PlayerEnergy\":3,\"Gold\":150,\"CurrentNodeIndex\":5,\"CurrentPage\":1,\"DeckCardIds\":[101,102,103]}";

            RunSaveData data = JsonUtility.FromJson<RunSaveData>(json);

            Assert.That(data.RunProfileId, Is.EqualTo(5501));
            Assert.That(data.PlayerMaxHp, Is.EqualTo(80));
            Assert.That(data.PlayerHp, Is.EqualTo(45));
            Assert.That(data.PlayerEnergy, Is.EqualTo(3));
            Assert.That(data.Gold, Is.EqualTo(150));
            Assert.That(data.CurrentNodeIndex, Is.EqualTo(5));
            Assert.That(data.CurrentPage, Is.EqualTo(1));
            Assert.That(data.DeckCardIds.Count, Is.EqualTo(3));
            Assert.That(data.DeckCardIds[0], Is.EqualTo(101));
            Assert.That(data.DeckCardIds[1], Is.EqualTo(102));
            Assert.That(data.DeckCardIds[2], Is.EqualTo(103));
            Assert.That(data.IsValid, Is.True);
        }

        [Test]
        public void RunSaveData_IsValid_WhenRunProfileIdIsGreaterThanZero()
        {
            RunSaveData data1 = new RunSaveData { RunProfileId = 0 };
            Assert.That(data1.IsValid, Is.False);

            RunSaveData data2 = new RunSaveData { RunProfileId = 1 };
            Assert.That(data2.IsValid, Is.True);
        }
    }
}
