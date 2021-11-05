using System;

namespace Utils.WinForm.Expander
{
    /// <summary>
    /// Информация о развёртывании/свёртывании контрола
    /// </summary>
    public class ExpandCollapseEventArgs : EventArgs
    {
        /// <summary>
        /// true - контрол развёрнут. false - контрол свёрнут
        /// </summary>
        public bool IsExpanded { get; }

        /// <summary>
        /// true - контрол развёрнут. false - контрол свёрнут
        /// </summary>
        public bool IsChecked { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="isExpanded">true - контрол развёрнут. false - контрол свёрнут</param>
        /// <param name="isChecked"></param>
        public ExpandCollapseEventArgs(bool isExpanded, bool isChecked)
        {
            IsExpanded = isExpanded;
            IsChecked = isChecked;
        }
    }
}
