using BuildPipeline.Core;
using PropertyModels.ComponentModel;
using PropertyModels.Localilzation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildPipeline.GUI.Utils
{
    class PropertyGridLocalizationAdapter : MiniReactiveObject, ILocalizationService
    {
        ILocalizeService Service;
        List<ServiceData> DataList = new List<ServiceData>();

        public PropertyGridLocalizationAdapter(ILocalizeService service)
        {
            Service = service;

            foreach(var culture in Service.AvailableCultures)
            {
                ServiceData Data = new ServiceData(Service, culture);

                DataList.Add(Data);
            }
        }

        class ServiceData : ICultureData
        {
            ILocalizeService Service;
            public readonly CultureInfoData Data;

            public ServiceData(ILocalizeService service, CultureInfoData data)
            {
                Service = service;
                Data = data;
            }

            public string this[string key] => Service[key];

            public CultureInfo Culture => Data.CultureInfo;

            public Uri Path => new Uri(Data.Path);

            public bool IsLoaded => true;

            public bool Reload()
            {
                return true;
            }
        }

        public string this[string key]
        {
            get
            {
                return Service[key];
            }
        }

        public ICultureData CultureData => throw new NotImplementedException();

        public event EventHandler OnCultureChanged;

        public void AddExtraService(ILocalizationService service)
        {
        }

        public ICultureData[] GetCultures()
        {
            return DataList.ToArray();
        }

        public ILocalizationService[] GetExtraServices()
        {
            return null;
        }

        public void RemoveExtraService(ILocalizationService service)
        {
            
        }

        public void SelectCulture(string cultureName)
        {
            OnCultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
