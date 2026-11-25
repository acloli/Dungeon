using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TFramework.UI;

namespace Dungeon.Runtime.Common.Character
{
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] private CharacterMotion _motion;
        [SerializeField] private Image _characterImage;
        [SerializeField] private GameObject _daiza;
        [SerializeField] private GameObject _commentObject;
        [SerializeField] private TFTextUGUI _commentText;

        public void Setup(Sprite characterSprite, bool canClick)
        {
            if (characterSprite is null)
            {
                gameObject.SetActive(false);
                return;
            }

            _characterImage.sprite = characterSprite;
            _motion.SetClickable(canClick);
        }

        public void SetDaiza(bool isActive) => _daiza.SetActive(isActive);

        public void SetComment(string comment)
        {
            if (comment is null || string.IsNullOrEmpty(comment))
            {
                _commentObject.SetActive(false);
                return;
            }

            _commentObject.SetActive(true);
            _commentText.SetTextContent(comment);
        }
    }
}
