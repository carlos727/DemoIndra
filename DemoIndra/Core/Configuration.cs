using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DemoIndra.Core
{
    class Configuration
    {
        public string URL { get; set; }
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public string DATASRC { get; set; }
        public string FILEPATH { get; set; }
        public string IMAGEDIR { get; set; }
        public char SEPARATOR { get; set; }
        public string DOCTYPE { get; set; }
        public string[] ORDER { get; set; }
        public string DATEFORMAT { get; set; }
        public bool MOVEFILE { get; set; }
        public string HISTORYPATH { get; set; }
        public bool DELETEIMG { get; set; }

        public Configuration()
        {
            URL = "http://192.168.30.192/appserver/service.asmx";
            USERNAME = "MANAGER";
            PASSWORD = "PASSWORD";
            DATASRC = "OnBase";
            FILEPATH = @"C:\UploadToOnbase\upload.txt";
            IMAGEDIR = @"C:\UploadToOnbase\";
            SEPARATOR = ';';
            DOCTYPE = "FacturaTest";
            ORDER = "archivo;Invoice #;Invoice Amount;Invoice Date".Split(SEPARATOR);
            DATEFORMAT = "dd/MM/yyyy";
            MOVEFILE = true;
            HISTORYPATH = @"C:\Historicos\";
            DELETEIMG = false;
        }

        public Configuration(string configFile)
        {
            XDocument xmlFile = XDocument.Load(configFile);

            var onbase = (from c in xmlFile.Descendants("onbase").Elements() select c).ToList();
            URL = onbase.Where(c => c.Name == "url").FirstOrDefault().Value;
            USERNAME = onbase.Where(c => c.Name == "username").FirstOrDefault().Value;
            PASSWORD = onbase.Where(c => c.Name == "password").FirstOrDefault().Value;
            DATASRC = onbase.Where(c => c.Name == "datasource").FirstOrDefault().Value;

            var importacion = (from c in xmlFile.Descendants("importacion").Elements() select c).ToList();
            FILEPATH = importacion.Where(c => c.Name == "documentofuente").FirstOrDefault().Value;
            IMAGEDIR = importacion.Where(c => c.Name == "folderimagenes").FirstOrDefault().Value;
            DOCTYPE = importacion.Where(c => c.Name == "tipodocumental").FirstOrDefault().Value;
            string parametros = importacion.Where(c => c.Name == "parametros").FirstOrDefault().Value;
            SEPARATOR = char.Parse(importacion.Where(c => c.Name == "separador").FirstOrDefault().Value);
            DATEFORMAT = importacion.Where(c => c.Name == "formatofecha").FirstOrDefault().Value;
            ORDER = parametros.Split(SEPARATOR);

            var control = (from c in xmlFile.Descendants("control").Elements() select c).ToList();
            MOVEFILE = bool.Parse(control.Where(c => c.Name == "moverdocumento").FirstOrDefault().Value);
            HISTORYPATH = control.Where(c => c.Name == "folderhistorico").FirstOrDefault().Value;
            DELETEIMG = bool.Parse(control.Where(c => c.Name == "borrarimagenes").FirstOrDefault().Value);
        }
    }
}
