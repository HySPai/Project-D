using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIGame
{
    public class GamePlayView : BaseUIView
    {
        [Header("Hearts")]
        [SerializeField] private Transform heartContainer;
        [SerializeField] private Heart heartPrefab;

        [Header("Stamina")]
        [SerializeField] private Slider staminaSlider;

        private readonly List<Heart> hearts = new List<Heart>();

        public void BuildHearts(int maxHearts)
        {
            foreach (var h in hearts)
                if (h != null) Destroy(h.gameObject);
            hearts.Clear();

            for (int i = 0; i < maxHearts; i++)
            {
                Heart heart = Instantiate(heartPrefab, heartContainer);
                heart.Init(true);
                hearts.Add(heart);
            }
        }

        public void UpdateHearts(int current, int max)
        {
            if (hearts.Count != max)
                BuildHearts(max);

            for (int i = 0; i < hearts.Count; i++)
                if (hearts[i] != null)
                    hearts[i].SetFilled(i < current);   // đủ thì đầy, thiếu thì rỗng → tự animate
        }

        public void UpdateStamina(float current, float max)
        {
            if (staminaSlider != null) staminaSlider.value = max > 0f ? current / max : 0f;
        }

        public override void OpenView() => base.OpenView();
    }
}