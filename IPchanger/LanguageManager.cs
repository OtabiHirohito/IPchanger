using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Data;

namespace IPchanger
{
    public class LanguageManager : INotifyPropertyChanged
    {
        private static readonly Lazy<LanguageManager> _instance = new Lazy<LanguageManager>(() => new LanguageManager());
        public static LanguageManager Instance => _instance.Value;

        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public event PropertyChangedEventHandler? PropertyChanged;

        private LanguageManager()
        {
            _resourceManager = new ResourceManager("IPchanger.Resources.Resources", typeof(LanguageManager).Assembly);
            _currentCulture = CultureInfo.CurrentUICulture;
        }

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (_currentCulture.Name != value.Name)
                {
                    _currentCulture = value;
                    CultureInfo.CurrentCulture = value;
                    CultureInfo.CurrentUICulture = value;
                    Thread.CurrentThread.CurrentCulture = value;
                    Thread.CurrentThread.CurrentUICulture = value;
                    OnPropertyChanged(string.Empty); // Notify all property changes (including indexer)
                }
            }
        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;
                var value = _resourceManager.GetString(key, _currentCulture);
                return value ?? key;
            }
        }

        public string GetString(string key) => this[key];

        public string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            return string.Format(format, args);
        }

        public void ChangeLanguage(string cultureCode)
        {
            CurrentCulture = new CultureInfo(cultureCode);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
