using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace SAwareness
{
    public static class SUpdater
    {
        private const int localmajorversion = 0;
        private const int localversion = 6;

        public static void UpdateCheck()
        {
            var bgw = new BackgroundWorker();
            bgw.DoWork += bgw_DoWork;
            bgw.RunWorkerAsync();
        }

        private static void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            var myUpdater = new Updater("https://raw.githubusercontent.com/Screeder/SAwareness/master/Properties/Version", "https://github.com/Screeder/SAwareness/releases/download/", "SAwareness.exe", localmajorversion, localversion);
            if (myUpdater.NeedUpdate)
            {
                Game.PrintChat("SAwareness Updating ...");
                if (myUpdater.Update())
                {
                    Game.PrintChat("SAwareness updated, reload please");
                }
            }
        }
    }

    class Updater
    {
        private readonly string _updatelink;

        private readonly System.Net.WebClient _wc = new System.Net.WebClient { Proxy = null };
        public bool NeedUpdate = false;

        public Updater(string versionlink, string updatelink, String assemblyName, int localmajorversion, int localversion)
        {
            _updatelink = updatelink;

            String str = _wc.DownloadString(versionlink);
            _updatelink = updatelink + "v" + str + "/" + assemblyName;
            if(Convert.ToInt32(str.Remove(str.IndexOf("."))) > localmajorversion)
                NeedUpdate = true;
            if (Convert.ToInt32(str.Remove(0, str.IndexOf(".") + 1)) > localversion)
                NeedUpdate = true;
        }

        public bool Update()
        {
            try
            {
                if (
                    System.IO.File.Exists(
                        System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".bak"))
                {
                    System.IO.File.Delete(
                        System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".bak");
                }
                System.IO.File.Move(System.Reflection.Assembly.GetExecutingAssembly().Location,
                    System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".bak");
                _wc.DownloadFile(_updatelink,
                    System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
