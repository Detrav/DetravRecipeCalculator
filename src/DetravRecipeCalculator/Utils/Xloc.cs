using DetravRecipeCalculator.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DetravRecipeCalculator.Utils
{
    public class Xloc
    {
        private static readonly Dictionary<string, Xloc> _locales = new Dictionary<string, Xloc>();
        private static readonly List<WeakReference<XLocItem>> items = new List<WeakReference<XLocItem>>();
        private Dictionary<string, string> _localization = new Dictionary<string, string>();

        static Xloc()
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            foreach (var name in typeof(Xloc).Assembly.GetManifestResourceNames())
            {
                if (name.StartsWith("DetravRecipeCalculator.Localization.", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".lang", StringComparison.OrdinalIgnoreCase))
                {
                    var stream = typeof(Xloc).Assembly.GetManifestResourceStream(name);
                    if (stream != null)
                    {
                        var items = name.Split('.');

                        LoadLocalization(items[items.Length - 2], stream);
                    }
                }
            }
            FallbackLocale = GetLocale("en");
            CurrentLocale = GetLocale(Config.Instance.CurrentLocale ?? "en");
        }

        public static IEnumerable<string> AvailableLocales => _locales.Keys;
        public static Xloc CurrentLocale { get; private set; } = new Xloc();
        public static Xloc FallbackLocale { get; private set; } = new Xloc();

        public static string Get(string id)
        {
            return CurrentLocale.Get(id, true);
        }

        public static Xloc GetLocale(string locale)
        {
            if (_locales.TryGetValue(locale, out var xloc))
                return xloc;
            return FallbackLocale;
        }

        public static void Register(XLocItem item)
        {
            items.Add(new WeakReference<XLocItem>(item));

            CurrentLocale.LoadLocale(item);
        }

        public static void SwitchLocalization(string locale)
        {
            CurrentLocale = GetLocale(locale);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].TryGetTarget(out var subItem))
                {
                    CurrentLocale.LoadLocale(subItem);
                }
                else
                {
                    items.RemoveAt(i);
                    i--;
                }
            }

            var targetCulture = Get("__LanguageCulture");

            //try
            //{
            //    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(targetCulture);
            //}
            //catch
            //{
            //    CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
            //}
        }

        public string Get(string id, bool fallBack2Default)
        {
            if (_localization.TryGetValue(id, out var value) || fallBack2Default && FallbackLocale._localization.TryGetValue(id, out value))
            {
                return value;
            }
            return id;
        }

        private static void LoadLocalization(string name, Stream stream)
        {
            Xloc xloc = new Xloc();

            using var tr = new StreamReader(stream, Encoding.UTF8);

            var lines = tr.ReadToEnd().Split("\n");

            foreach (var line in lines)
            {
                var s = line.IndexOf("=");
                if (s > 0)
                {
                    string id = line.Substring(0, s).Trim();
                    string value = line.Substring(s + 1).Trim();
                    xloc._localization[id] = Unescape(value);
                }
            }
            _locales[name] = xloc;
        }

        private static string Unescape(string value)
        {
            StringBuilder sb = new StringBuilder(value.Length);

            bool escape = false;

            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (escape)
                {
                    if (c == 'n')
                    {
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    escape = false;
                }
                else
                {
                    if (c == '\\')
                    {
                        escape = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        private void LoadLocale(XLocItem item)
        {
            item.Text = Get(item.Id, true);
            //if (_localization.TryGetValue(item.Id, out var value) || FallbackLocale._localization.TryGetValue(item.Id, out value))
            //{
            //    item.Text = value;
            //}
            //else
            //{
            //    item.Text = item.Id;
            //}
        }
    }
}