using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace BuildPipeline.Core.Services.Implements
{
    [Export]
    internal class LocalizeService : AbstractService, ILocalizeService
    {
        #region Properties
        public string CultureName { get; private set; }
        public string Language => CultureName;

        private Dictionary<string, string> LocalTexts = null;
        #endregion

        #region Interfaces
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when [on culture changed].
        /// </summary>
        public event EventHandler OnCultureChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Localization
        public string this[string key]
        {
            get
            {
                if (LocalTexts != null && LocalTexts.TryGetValue(key, out var text))
                {
                    return text;
                }

                return key;
            }
        }

        /// <summary>
        /// Gets the available cultures.
        /// </summary>
        /// <value>The available cultures.</value>
        public ISelectableList<CultureInfoData> AvailableCultures { get; private set; } = new SelectableList<CultureInfoData>();


        public LocalizeService()
        {
            var locDirectory = AppFramework.GetPublishApplicationDirectory().JoinPath("assets/localization");
            CultureInfoData currentInfo = null;

            if (locDirectory.IsDirectoryExists())
            {
                var files = Directory.GetFiles(locDirectory, "*.json", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var name = file.GetFileNameWithoutExtension();

                    var cultureInfo = CultureInfo.GetCultureInfo(name);

                    if (cultureInfo != null)
                    {
                        var info = new CultureInfoData(cultureInfo, file);

                        if (cultureInfo.Name == CultureInfo.CurrentCulture.Name)
                        {
                            currentInfo = info;
                        }

                        AvailableCultures.Add(info);
                    }
                }
            }

            AvailableCultures.SelectionChanged += OnSelectionChanged;

            if (currentInfo != null)
            {
                AvailableCultures.SelectedValue = currentInfo;
            }
            else
            {
                currentInfo = AvailableCultures.ToList().Find(x => x.CultureInfo.Name == "en-US");
                AvailableCultures.SelectedValue = currentInfo;
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            var selectedData = (sender as ISelectableList<CultureInfoData>).SelectedValue;

            if (selectedData == null)
            {
                LocalTexts = null;
                PostReload();
                return;
            }

            try
            {
                LoadAllSources(selectedData.Path);

                PostReload();
            }
            catch (Exception ee)
            {
                Logger.LogError("Failed load localization file :{0}\n{1}\n{2}", selectedData.Path, ee.Message, ee.StackTrace);
            }
        }

        private void LoadAllSources(string appLanguageFile)
        {
            LocalTexts = new Dictionary<string, string>();

            List<string> pathes = new List<string>()
                {
                    appLanguageFile
                };

            foreach (var plugin in PluginLoader.PluginDict.Values)
            {
                var p = plugin.Path.JoinPath("assets/localization", appLanguageFile.GetFileName());

                if (p.IsFileExists())
                {
                    pathes.Add(p);
                }
            }

            foreach (var path in pathes)
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        var tempDict = ReadJsonStringDictionary(sr.ReadToEnd());

                        if (tempDict != null)
                        {
                            foreach (var pair in tempDict)
                            {
                                if (!LocalTexts.ContainsKey(pair.Key))
                                {
                                    LocalTexts.Add(pair.Key, pair.Value);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PostReload()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            OnCultureChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Reads the json string dictionary.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        public static Dictionary<string, string> ReadJsonStringDictionary(string json)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string[] configs = json.Split('\r', '\n');

            foreach (var line in configs)
            {
                var configLine = line.Trim();

                if (!configLine.Contains(":") || configLine.StartsWith("\\"))
                {
                    continue;
                }

                int endPos = 0;
                string key = PickStringToken(configLine, 0, out endPos);

                if (key == null)
                {
                    continue;
                }

                string value = PickStringToken(configLine, endPos + 1, out _);

                if (value == null)
                {
                    continue;
                }

                dict.Add(key, value);
            }

            return dict;
        }

        private static string PickStringToken(string line, int startPos, out int endPos)
        {
            int Begin = -1;
            int Escape = -1;
            endPos = -1;

            for (int i = startPos; i < line.Length; ++i)
            {
                char ch = line[i];

                if (ch == '"')
                {
                    if (Escape == i - 1 && Escape != -1)
                    {
                        Escape = -1;
                        continue;
                    }
                    else if (Begin == -1)
                    {
                        Begin = i;
                    }
                    else if (Begin != -1)
                    {
                        endPos = i;

                        return line.Substring(Begin + 1, endPos - Begin - 1);
                    }
                }
                else if (ch == '\\')
                {
                    Escape = i;
                }
            }

            return null;
        }
        #endregion
    }

}