using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIGame
{
    public class GamePlayView : BaseUIView
    {
        [Serializable]
        private struct ElementDisplay
        {
            public ElementType element;
            public string label;
            public Color color;
        }

        [Header("Hearts")]
        [SerializeField] private Transform heartContainer;
        [SerializeField] private Heart heartPrefab;

        [Header("Stamina")]
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private SliderChangeEffect staminaEffect;

        [Header("Element")]
        [SerializeField] private Slider manaSlider;
        [SerializeField] private SliderChangeEffect manaEffect;
        [SerializeField] private Image manaFill;
        [SerializeField] private Image elementIcon;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TMP_Text elementLabel;
        [SerializeField] private ElementDisplay[] elementDisplays;
        [SerializeField] private float elementPunchScale = 0.25f;
        [SerializeField] private float elementPunchDuration = 0.25f;

        private readonly List<Heart> hearts = new List<Heart>();
        private Tween elementTween;

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
            float value = max > 0f ? current / max : 0f;

            if (staminaEffect != null) staminaEffect.SetValue(value);
            else if (staminaSlider != null) staminaSlider.value = value;
        }

        public void UpdateMana(float current, float max)
        {
            float value = max > 0f ? current / max : 0f;

            if (manaEffect != null) manaEffect.SetValue(value);
            else if (manaSlider != null) manaSlider.value = value;
        }

        public void UpdateElement(ElementType element)
        {
            if (!TryGetDisplay(element, out ElementDisplay display)) return;

            if (manaFill != null) manaFill.color = display.color;
            if (elementIcon != null) elementIcon.color = display.color;
            if (elementLabel != null) elementLabel.text = display.label;

            if (elementIcon == null) return;

            elementTween?.Kill(true);
            elementIcon.transform.localScale = Vector3.one;
            elementTween = elementIcon.transform
                .DOPunchScale(Vector3.one * elementPunchScale, elementPunchDuration, 1, 0f)
                .SetLink(gameObject);
        }

        public void UpdateSpellCooldown(float remaining01)
        {
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = remaining01;
        }

        private bool TryGetDisplay(ElementType element, out ElementDisplay display)
        {
            if (elementDisplays != null)
            {
                foreach (ElementDisplay candidate in elementDisplays)
                {
                    if (candidate.element != element) continue;

                    display = candidate;
                    return true;
                }
            }

            display = default;
            return false;
        }

        public override void OpenView() => base.OpenView();
    }
}