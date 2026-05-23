using System;
using System.Windows;

namespace IPchanger
{
    public partial class MemoWindow : Window
    {
        public event EventHandler WindowClosedByUI;

        public MemoWindow()
        {
            InitializeComponent();
            Closing += MemoWindow_Closing;
        }

        private void MemoWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 保存処理はデータバインディング(TwoWay)で行われているが、念のためSave
            Properties.Settings.Default.MemoWidth = Width;
            Properties.Settings.Default.MemoHeight = Height;
            Properties.Settings.Default.Save();
            
            WindowClosedByUI?.Invoke(this, EventArgs.Empty);
        }
    }
}