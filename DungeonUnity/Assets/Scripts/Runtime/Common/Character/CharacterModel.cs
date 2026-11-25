using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Runtime.Common.Character
{
    public class CharacterModel
    {
        private List<string> _characterComments = new();
        private Sprite _characterImage;

        public IReadOnlyList<string> CharacterComments => _characterComments;
        public Sprite CharacterImage => _characterImage;

        public CharacterModel(Sprite sprite, List<string> comments)
        {
            _characterImage = sprite;
            _characterComments = comments ?? new();
        }
    }
}