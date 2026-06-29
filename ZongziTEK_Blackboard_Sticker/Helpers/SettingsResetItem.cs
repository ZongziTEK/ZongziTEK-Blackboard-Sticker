using System;
using System.Collections.Generic;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    public sealed class SettingsResetItem
    {
        private readonly Action<bool> setEnabled;
        private readonly Func<bool> hasChanged;
        private readonly Action resetValue;
        private readonly Action applyValue;

        public SettingsResetItem(Action<bool> setEnabled, Func<bool> hasChanged, Action resetValue, Action applyValue = null)
        {
            this.setEnabled = setEnabled ?? throw new ArgumentNullException(nameof(setEnabled));
            this.hasChanged = hasChanged ?? throw new ArgumentNullException(nameof(hasChanged));
            this.resetValue = resetValue ?? throw new ArgumentNullException(nameof(resetValue));
            this.applyValue = applyValue;
        }

        public void UpdateEnabled()
        {
            setEnabled(hasChanged());
        }

        public void Reset()
        {
            resetValue();
            applyValue?.Invoke();
        }

        public static SettingsResetItem Register<T>(
            ICollection<SettingsResetItem> resetItems,
            Action<bool> setEnabled,
            Func<T> getCurrent,
            Func<T> getDefault,
            Action<T> setCurrent,
            Action applyValue = null,
            Func<T, T, bool> equals = null)
        {
            if (resetItems == null) throw new ArgumentNullException(nameof(resetItems));
            if (getCurrent == null) throw new ArgumentNullException(nameof(getCurrent));
            if (getDefault == null) throw new ArgumentNullException(nameof(getDefault));
            if (setCurrent == null) throw new ArgumentNullException(nameof(setCurrent));

            if (equals == null)
            {
                equals = EqualityComparer<T>.Default.Equals;
            }

            var item = new SettingsResetItem(
                setEnabled,
                () => !equals(getCurrent(), getDefault()),
                () => setCurrent(getDefault()),
                applyValue);

            resetItems.Add(item);
            return item;
        }

        public static SettingsResetItem Register(
            ICollection<SettingsResetItem> resetItems,
            Action<bool> setEnabled,
            Func<bool> hasChanged,
            Action resetValue,
            Action applyValue = null)
        {
            if (resetItems == null) throw new ArgumentNullException(nameof(resetItems));

            var item = new SettingsResetItem(setEnabled, hasChanged, resetValue, applyValue);
            resetItems.Add(item);
            return item;
        }

        public static void UpdateAll(IEnumerable<SettingsResetItem> resetItems)
        {
            if (resetItems == null) return;

            foreach (SettingsResetItem item in resetItems)
            {
                item.UpdateEnabled();
            }
        }

        public static bool AreClose(double a, double b)
        {
            return Math.Abs(a - b) < 0.0000001;
        }
    }
}
