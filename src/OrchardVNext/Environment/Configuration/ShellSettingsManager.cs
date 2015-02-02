using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Framework.ConfigurationModel;
using OrchardVNext.Environment.Configuration.Sources;
using OrchardVNext.FileSystems.AppData;

namespace OrchardVNext.Environment.Configuration {
    public interface IShellSettingsManager {
        /// <summary>
        /// Retrieves all shell settings stored.
        /// </summary>
        /// <returns>All shell settings.</returns>
        IEnumerable<ShellSettings> LoadSettings();

        /// <summary>
        /// Persists shell settings to the storage.
        /// </summary>
        /// <param name="settings">The shell settings to store.</param>
        void SaveSettings(ShellSettings settings);
    }

    public class ShellSettingsManager : IShellSettingsManager {
        private readonly IAppDataFolder _appDataFolder;
        private const string _settingsFileNameFormat = "Settings.{0}";
        private readonly string[] _settingFileNameExtensions = new string[] { "txt", "json" };

        public ShellSettingsManager(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            var filePaths = _appDataFolder
                .ListDirectories("Sites")
                .SelectMany(path => _appDataFolder.ListFiles(path))
                .Where(path => {
                    var filePathName = Path.GetFileName(path);

                    return _settingFileNameExtensions.Any(p =>
                        string.Equals(filePathName, string.Format(_settingsFileNameFormat, p), StringComparison.OrdinalIgnoreCase)
                    );
                });

            List<ShellSettings> shellSettings = new List<ShellSettings>();

            foreach (var filePath in filePaths) {
                IConfigurationSourceContainer configurationContainer = null;

                var extension = Path.GetExtension(filePath);

                switch (extension) {
                    case ".json":
                        configurationContainer = new Microsoft.Framework.ConfigurationModel.Configuration()
                            .AddJsonFile(filePath);
                        break;
                    case ".xml":
                        configurationContainer = new Microsoft.Framework.ConfigurationModel.Configuration()
                            .AddXmlFile(filePath);
                        break;
                    case ".ini":
                        configurationContainer = new Microsoft.Framework.ConfigurationModel.Configuration()
                            .AddIniFile(filePath);
                        break;
                    case ".txt":
                        configurationContainer = new Microsoft.Framework.ConfigurationModel.Configuration()
                            .Add(new DefaultFileConfigurationSource(_appDataFolder, filePath));
                        break;
                }

                if (configurationContainer != null) {
                    var shellSetting = new ShellSettings {
                        Name = configurationContainer.Get<string>("Name"),
                        RequestUrlPrefix = configurationContainer.Get<string>("RequestUrlPrefix")
                    };

                    TenantState state;
                    shellSetting.State = Enum.TryParse(configurationContainer.Get<string>("State"), true, out state) ? state : TenantState.Uninitialized;

                    shellSettings.Add(shellSetting);
                }
            }

            return shellSettings;
        }

        public void SaveSettings(ShellSettings settings) {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (String.IsNullOrEmpty(settings.Name))
                throw new ArgumentException("The Name property of the supplied ShellSettings object is null or empty; the settings cannot be saved.", "settings");

            Logger.Information("Saving ShellSettings for tenant '{0}'...", settings.Name);
            var filePath = Path.Combine(Path.Combine("Sites", settings.Name), string.Format(_settingsFileNameFormat, "txc"));
            var source = new DefaultFileConfigurationSource(_appDataFolder, filePath);

            source.Set("Name", settings.Name);

            source.Commit();
        }
    }
}