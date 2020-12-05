using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TTG_Tools
{
    [Serializable()]
    public class Settings
    {
        public static void SaveConfig(Settings settings)
        {
            string xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "config.xml";
            XmlSerializer xmlS = new XmlSerializer(typeof(Settings));
            System.IO.TextWriter xmlW = new System.IO.StreamWriter(xmlPath);
            xmlS.Serialize(xmlW, settings);

            xmlW.Flush();
            xmlW.Close();
        }


        private string _pathForInputFolder;
        private string _pathForOutputFolder;
        private int _ASCII_N;
        private bool _deleteD3DTXafterImport;
        private bool _deleteDDSafterImport;
        private bool _importingOfName;
        private bool _sortSameString;
        private string _AdditionalChar = "";
        private bool _exportRealID;
        private int _unicodeSettings;

        private bool _encLangdb;
        private bool _encDDSonly;
        private bool _encNewLua;
        private bool _iOSsupport; //for PVR textures
        private bool _customKey;
        private bool _tsvFormat;
        private int _encKeyIndex;
        private int _versionEnc;
        private string _encCustomKey;

        [XmlAttribute("pathForInputFolder")]
        public string pathForInputFolder
        {
            get
            {
                return _pathForInputFolder;
            }
            set
            {
                _pathForInputFolder = value;
            }
        }
        [XmlAttribute("pathForOutputFolder")]
        public string pathForOutputFolder
        {
            get
            {
                return _pathForOutputFolder;
            }
            set
            {
                _pathForOutputFolder = value;
            }
        }
        
        [XmlAttribute("ASCII_N")]
        public int ASCII_N
        {
            get
            {
                return _ASCII_N;
            }
            set
            {
                _ASCII_N = value;
            }
        }
        [XmlAttribute("AdditionalChar")]
        public string additionalChar
        {
            get
            {
                return _AdditionalChar;
            }
            set
            {
                _AdditionalChar = value;
            }
        }
        [XmlAttribute("deleteD3DTXafterImport")]
        public bool deleteD3DTXafterImport
        {
            get
            {
                return _deleteD3DTXafterImport;
            }
            set
            {
                _deleteD3DTXafterImport = value;
            }
        }
        [XmlAttribute("deleteDDSafterImport")]
        public bool deleteDDSafterImport
        {
            get
            {
                return _deleteDDSafterImport;
            }
            set
            {
                _deleteDDSafterImport = value;
            }
        }

        [XmlAttribute("importingOfName")]
        public bool importingOfName
        {
            get
            {
                return _importingOfName;
            }
            set
            {
                _importingOfName = value;
            }
        }

        [XmlAttribute("sortSameString")]
        public bool sortSameString
        {
            get
            {
                return _sortSameString;
            }
            set
            {
                _sortSameString = value;
            }
        }

        [XmlAttribute("exportRealID")]
        public bool exportRealID
        {
            get
            {
                return _exportRealID;
            }
            set
            {
                _exportRealID = value;
            }
        }

        [XmlAttribute("unicodeMode")]

        public int unicodeSettings
        {
            get
            {
                return _unicodeSettings;
            }
            set
            {
                _unicodeSettings = value;
            }
        }

        [XmlAttribute("encLangdb")]
        public bool encLangdb
        {
            get
            {
                return _encLangdb;
            }
            set
            {
                _encLangdb = value;
            }
        }

        [XmlAttribute("encDDSonly")]
        public bool encDDSonly
        {
            get
            {
                return _encDDSonly;
            }
            set
            {
                _encDDSonly = value;
            }
        }

        [XmlAttribute("encNewLua")]
        public bool encNewLua
        {
            get
            {
                return _encNewLua;
            }
            set
            {
                _encNewLua = value;
            }
        }

        [XmlAttribute("iOSsupport")]
        public bool iOSsupport
        {
            get
            {
                return _iOSsupport;
            }
            set
            {
                _iOSsupport = value;
            }
        }

        [XmlAttribute("customKey")]
        public bool customKey
        {
            get
            {
                return _customKey;
            }
            set
            {
                _customKey = value;
            }
        }
        
        [XmlAttribute("tsvFormat")]
        public bool tsvFormat
        {
            get
            {
                return _tsvFormat;
            }
            set
            {
                _tsvFormat = value;
            }
        }

        [XmlAttribute("encKeyIndex")]
        public int encKeyIndex
        {
            get
            {
                return _encKeyIndex;
            }
            set
            {
                _encKeyIndex = value;
            }
        }

        [XmlAttribute("versionEnc")]
        public int versionEnc
        {
            get
            {
                return _versionEnc;
            }
            set
            {
                _versionEnc = value;
            }
        }

        [XmlAttribute("encCustomKey")]
        public string encCustomKey
        {
            get
            {
                return _encCustomKey;
            }
            set
            {
                _encCustomKey = value;
            }
        }

        public Settings(
            string _pathForInputFolder,
            string _pathForOutputFolder,
            string _additionalChar,
            int _ASCII_N,
            bool _deleteD3DTXafterImport,
            bool _deleteDDSafterImport,
            bool _importingOfName,
            bool _sortSameString,
            bool _exportRealID,
            int _unicodeSettings,
            bool _encLangdb,
            bool _encDDSonly,
            bool _encNewLua,
            bool _iOSsupport,
            bool _customKey,
            bool _tsvFormat,
            int _encKeyIndex,
            int _versionEnc,
            string _encCustomKey)
        {
            this.ASCII_N = _ASCII_N;
            this.pathForInputFolder = _pathForInputFolder;
            this.pathForOutputFolder = _pathForOutputFolder;
            this.additionalChar = _AdditionalChar;
            this.deleteD3DTXafterImport = _deleteD3DTXafterImport;
            this.deleteDDSafterImport = _deleteDDSafterImport;
            this.importingOfName = _importingOfName;
            this.sortSameString = _sortSameString;
            this.exportRealID = _exportRealID;
            this.unicodeSettings = _unicodeSettings;
            this.encLangdb = _encLangdb;
            this.encDDSonly = _encDDSonly;
            this.encNewLua = _encNewLua;
            this.iOSsupport = _iOSsupport;
            this.customKey = _customKey;
            this.tsvFormat = _tsvFormat;
            this.encKeyIndex = _encKeyIndex;
            this.versionEnc = _versionEnc;
            this.encCustomKey = _encCustomKey;
        }

        public Settings()
        { }
    }
}
