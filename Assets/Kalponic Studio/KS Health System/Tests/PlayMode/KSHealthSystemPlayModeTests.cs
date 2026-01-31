using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace KalponicStudio.Health.Tests
{
    public class KSHealthSystemPlayModeTests
    {
        [Test]
        public void ShieldAbsorbsBeforeHealth()
        {
            var go = new GameObject("ShieldHealthTest");
            var shield = go.AddComponent<ShieldSystem>();
            var health = go.GetComponent<HealthSystem>();

            health.SetMaxHealth(100);
            health.SetHealth(100);
            shield.SetMaxShield(50);
            shield.SetShield(50);

            health.TakeDamage(30);
            Assert.AreEqual(100, health.CurrentHealth);
            Assert.AreEqual(20, shield.CurrentShield);

            health.TakeDamage(40);
            Assert.AreEqual(80, health.CurrentHealth);
            Assert.AreEqual(0, shield.CurrentShield);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LowHealthOverlayTogglesOnHealthChanged()
        {
            var go = new GameObject("LowHealthTest");
            var health = go.AddComponent<HealthSystem>();
            var visuals = go.AddComponent<HealthVisualSystem>();

            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            var overlayGo = new GameObject("LowHealthOverlay", typeof(Image));
            overlayGo.transform.SetParent(canvasGo.transform, false);
            var overlay = overlayGo.GetComponent<Image>();

            SetPrivateField(visuals, "lowHealthOverlay", overlay);
            SetPrivateField(visuals, "lowHealthThreshold", 0.25f);

            health.SetMaxHealth(100);
            health.SetHealth(100);
            InvokePrivateMethod(visuals, "UpdateLowHealthEffect");

            health.SetHealth(20);
            InvokePrivateMethod(visuals, "UpdateLowHealthEffect");
            Assert.IsTrue(overlay.gameObject.activeSelf);

            health.SetHealth(80);
            InvokePrivateMethod(visuals, "UpdateLowHealthEffect");
            Assert.IsFalse(overlay.gameObject.activeSelf);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(canvasGo);
        }

        [Test]
        public void HealthSystemEmitsUnityAndChannelEvents()
        {
            var go = new GameObject("EventTest");
            var health = go.AddComponent<HealthSystem>();

            var channel = ScriptableObject.CreateInstance<HealthEventChannelSO>();
            SetPrivateField(health, "healthEvents", channel);

            int unityCount = 0;
            int channelCount = 0;
            health.onDamageTaken.AddListener(_ => unityCount++);
            channel.onDamageTaken.AddListener(_ => channelCount++);

            health.SetMaxHealth(100);
            health.SetHealth(100);
            health.TakeDamage(10);

            Assert.AreEqual(1, unityCount);
            Assert.AreEqual(1, channelCount);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(channel);
        }

        private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            var field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {typeof(TTarget).Name}");
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod<TTarget>(TTarget target, string methodName)
        {
            var method = typeof(TTarget).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {typeof(TTarget).Name}");
            method.Invoke(target, null);
        }
    }
}
