using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace TTG_Tools
{
    public delegate void ProgressHandler(int progress);
    public delegate void ReportHandler(string report);
    public delegate void GlossaryAdd(string orText, string trText);
    public delegate void RefAllText(List<TextEditor.AllText> allText);
    public delegate void RefAllText2(List<TextEditor.AllText> allText2);

    public class ForThreads
    {
        public event ProgressHandler Progress;
        public event ReportHandler ReportForWork;
        public event RefAllText BackAllText;
        public event RefAllText2 BackAllText2;

        public void CreateExportingTXTfromAllTextN(ref List<TextEditor.AllText> allText)
        {
            for (int i = 0; i < allText.Count; i++)
            {
                if (allText[i].exported == false && allText[i].isChecked == false)
                {
                    allText[i].exported = true;
                    for (int j = 0; j < allText.Count; j++)
                    {
                        if (!MainMenu.settings.exportRealID)
                        {
                            if (TextCollector.IsStringsSame(allText[i].orText, allText[j].orText, false) && allText[j].exported == false)
                            {
                                allText[j].exported = true;
                                allText.Insert(i + 1, allText[j]);
                                allText[i + 1].exported = true;
                                allText.RemoveAt(j + 1);
                            }
                        }
                        else
                        {
                            if ((allText[i].realID == allText[j].realID) && allText[j].exported == false)
                            {
                                allText[j].exported = true;
                                allText.Insert(i + 1, allText[j]);
                                allText[i + 1].exported = true;
                                allText.RemoveAt(j + 1);
                            }
                        }
                    }
                }
                allText[i].isChecked = true;
                Progress(i);
            }
        }

        public void CreateExportingTXTfromAllText(object inputList)
        {
            List<TextEditor.AllText> allText = inputList as List<TextEditor.AllText>;
            CreateExportingTXTfromAllTextN(ref allText);
            BackAllText(allText);
        }

        public void CreateExportingTXTfromAllText2(object inputList)
        {

            List<TextEditor.AllText> allText2 = inputList as List<TextEditor.AllText>;
            CreateExportingTXTfromAllTextN(ref allText2);
            BackAllText2(allText2);
        }

        public void CreateGlossaryFromFirstAndSecondAllText(object TwoList)
        {
            //потом передавать мега-класс, содержащий всё это 
            List<List<TextEditor.AllText>> twoAllText = TwoList as List<List<TextEditor.AllText>>;
            List<TextEditor.AllText> firstText = twoAllText[0];
            List<TextEditor.AllText> secondText = twoAllText[1];
            List<TextEditor.Glossary> glossary = new List<TextEditor.Glossary>();

            for (int i = 0; i < firstText.Count; i++)
            {
                for (int j = 0; j < secondText.Count; j++)
                {
                    if ((firstText[i].orText != "" || firstText[i].orText != string.Empty) && firstText[i].orText == secondText[j].orText)
                    {
                        if (IsStringExist(secondText[j].orText, glossary) == false)
                        {
                            glossary.Add(new TextEditor.Glossary(firstText[i].orText, firstText[i].trText, false, false));
                        }
                    }
                }
                Progress(i);
            }
            FileStream ExportStream = new FileStream("txt.txt", FileMode.OpenOrCreate);
            for (int i = 0; i < glossary.Count; i++)
            {
                TextCollector.SaveString(ExportStream, glossary[i].orText + "\r\n", MainMenu.settings.ASCII_N);
                TextCollector.SaveString(ExportStream, glossary[i].trText + "\r\n\r\n", MainMenu.settings.ASCII_N);
            }
            ExportStream.Close();


            for (int i = 0; i < secondText.Count; i++)
            {
                int q = IsStringinGlossary(glossary, secondText[i].orText, secondText[i].trText);
                if (q >= 0)
                {
                    secondText[i].trText = glossary[q].trText;
                }
            }
            BackAllText2(secondText);
        }

        public int IsStringinGlossary(List<TextEditor.Glossary> glossary, string orText, string trText)
        {

            for (int e = 0; e < glossary.Count; e++)
            {
                if (glossary[e].orText == orText && glossary[e].trText != trText)
                {
                    //System.Windows.Forms.MessageBox.Show(glossary[e].trText+"\r\n"+trText);
                    return e;
                }
            }
            return -1;
        }

        public bool IsStringExist(string str, List<TextEditor.Glossary> glossary)
        {
            for (int e = 0; e < glossary.Count; e++)
            {
                if (glossary[e].orText == str)
                {
                    return true;
                }
            }
            return false;
        }

        //Импорт
        public void DoImportEncoding(object parametres)
        {

            List<string> param = parametres as List<string>;
            string versionOfGame = param[0];
            string encrypt = param[1];
            string destinationForExport;
            string whatImport;
            string pathInput = param[2];
            string pathOutput = param[3];
            //string pathTemp = param[4];
            bool deleteFromInputSource = false;
            bool deleteFromInputImported = false;
            if (param[5] == "True")
            {
                deleteFromInputSource = true;
            }
            if (param[4] == "True")
            {
                deleteFromInputImported = true;
            }

            List<string> destination = new List<string>();
            destination.Add(".d3dtx");
            destination.Add(".d3dtx");
            destination.Add(".langdb");
            destination.Add(".langdb");
            destination.Add(".font");
            destination.Add(".landb");
            destination.Add(".landb");
            destination.Add(".prop");
            List<string> extention = new List<string>();
            extention.Add(".dds");
            extention.Add(".pvr");
            extention.Add(".txt");
            extention.Add(".tsv");
            extention.Add("NOTHING");
            extention.Add(".txt");
            extention.Add(".tsv");
            extention.Add(".txt");

            bool show = false;

            for (int d = 0; d < destination.Count; d++)
            {
                destinationForExport = destination[d];
                whatImport = extention[d];

                show = false;

                if (Directory.Exists(pathInput) && Directory.Exists(pathOutput))
                {
                    DirectoryInfo dir = new DirectoryInfo(pathInput);
                    FileInfo[] inputFiles = dir.GetFiles('*' + destinationForExport);
                    
                        for (int i = 0; i < inputFiles.Length; i++)
                        {
                            int countCorrectWork = 0;//переменная для подсчёта корректного импорта текстур
                            int countOfAllFiles = 0;//всего файлов для импорта
                            string onlyNameImporting = inputFiles[i].Name.Split('(')[0].Split('.')[0];
                            for (int q = 0; q < 2; q++)
                            {
                                bool correct_work = true;

                                FileInfo[] fileDestination;
                                if (q == 0)
                                {
                                    fileDestination = dir.GetFiles(onlyNameImporting + whatImport);
                                }
                                else
                                {
                                    fileDestination = dir.GetFiles(onlyNameImporting + "(*)" + whatImport);
                                }
                                countOfAllFiles += fileDestination.Count();

                                for (int j = 0; j < fileDestination.Length; j++)
                                {
                                    switch (destinationForExport)
                                    {
                                        case ".d3dtx":
                                            {
                                                ImportDDSinD3DTX(inputFiles, fileDestination, i, j, pathOutput, ref correct_work, versionOfGame);
                                                show = true;
                                                break;
                                            }
                                        case ".landb":
                                            {
                                                ImportTXTinLANDB(inputFiles, fileDestination, i, j, pathOutput, ref correct_work, versionOfGame);
                                                show = true;
                                                break;
                                            }
                                        case ".langdb":
                                            {
                                                ImportTXTinLANGDB(inputFiles, fileDestination, i, j, pathOutput, ref correct_work, versionOfGame);
                                                show = true;
                                                break;
                                            }
                                        case ".prop":
                                            {
                                                ImportTXTinPROP(inputFiles, fileDestination, i, j, pathOutput, ref correct_work);
                                                show = true;
                                                break;
                                            }
                                        default:
                                            {
                                                MessageBox.Show("Error in Switch!");
                                                break;
                                            }
                                    }
                                    if (correct_work)//если файл импортирован был хорошо, то удаляем
                                    {
                                        countCorrectWork++;
                                        if (deleteFromInputImported)
                                        {
                                            Methods.DeleteCurrentFile(fileDestination[j].FullName);
                                        }
                                    }
                                }
                            }
                            if (deleteFromInputSource && countCorrectWork == countOfAllFiles)//если все файлы были импортированы правильно, то удаляем при необходимости файл
                            {
                                Methods.DeleteCurrentFile(inputFiles[i].FullName);
                            }
                        }
                }
                else
                {
                    ReportForWork("Check for existing Input and Output folders and check pathes in config.xml!");
                }
                if(show) ReportForWork("IMPORT OF ALL *" + destinationForExport.ToUpper() + " IS COMPLETE!");
            }
        }

        public void ImportTXTinPROP(FileInfo[] inputFiles, FileInfo[] fileDestination, int i, int j, string pathOutput, ref bool correctWork)
        {
            FileStream fs = new FileStream(inputFiles[i].FullName, FileMode.Open);
            byte[] binContent = Methods.ReadFull(fs);
            fs.Close();

            List<AutoPacker.Prop> proplist = new List<AutoPacker.Prop>();
            List<string> strs = new List<string>();

            byte[] header = null, countOfBlock = null, lengthAllText = null;
            int type = -1;

            AutoPacker.ReadProp(binContent, proplist, ref header, ref countOfBlock, ref lengthAllText, ref type);

            string[] texts = File.ReadAllLines(fileDestination[j].FullName);

            bool number_find = false;
            int n_str = -1;
            //int number_of_next_list = 0;

            for (int c = 0; c < texts.Length; c++)
            {
                if (texts[c].IndexOf(")") > -1 && number_find == false)
                {
                    if ((Methods.IsNumeric(texts[c].Substring(0, texts[c].IndexOf(")")))))
                    {
                        number_find = true;
                        n_str++;
                        strs.Add("");
                    }
                    else
                    {
                        strs[n_str] += "\r\n" + texts[c];
                        number_find = false;
                    }
                }
                else
                {
                    strs[n_str] += texts[c];
                    number_find = false;
                }
            }

            if (proplist.Count == strs.Count)
            {
                try
                {
                    for (int c = 0; c < proplist.Count; c++)
                    {
                        proplist[c].text = strs[c];
                        if (proplist[c].text.Contains("\r\n")) proplist[c].text = proplist[c].text.Replace("\r\n", "\n");
                        byte[] tmp = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(proplist[c].text);
                        proplist[c].lenght_of_text = BitConverter.GetBytes((int)tmp.Length);
                    }

                    AutoPacker.CreateProp(header, countOfBlock, proplist, (pathOutput + "\\" + inputFiles[i].Name), type);
                    ReportForWork("File " + Methods.GetNameOfFileOnly(inputFiles[i].Name, ".prop") + ".txt imported in " + inputFiles[i].Name);
                }
                catch
                {
                    ReportForWork("Something wrong with import file " + inputFiles[i].Name);
                }
            }
            else ReportForWork("Count of strings doesn't equal in files " + inputFiles[i].Name + " & " + fileDestination[j].Name);
        }

        public void findStringByID(List<TextCollector.TXT_collection> all_text, int c, ref int id)
        {
            for (int i = 0; i < all_text.Count; i++)
            {
                if (all_text[i].number == c)
                {
                    id = i;
                    break;
                }
            }
        }

        public void ImportTXTinLANDB(FileInfo[] inputFiles, FileInfo[] fileDestination, int i, int j, string pathOutput, ref bool correctWork, string versionOfGame)
        {
            int index = -1;
            try
            {
                List<AutoPacker.Langdb> landb = new List<AutoPacker.Langdb>();
                FileStream fs = new FileStream(inputFiles[i].FullName, FileMode.Open);
                byte[] binContent = Methods.ReadFull(fs);

                List<byte[]> header = new List<byte[]>();
                List<byte[]> end_of_file = new List<byte[]>();
                byte[] lenght_of_all_text = new byte[4];
                AutoPacker.ReadLandb(binContent, landb, ref header, ref lenght_of_all_text, ref end_of_file);
                fs.Close();

                byte[] check_header = new byte[4];
                Array.Copy(binContent, 16, check_header, 0, 4);

                if (BitConverter.ToInt32(check_header, 0) < 9) versionOfGame = " ";
                if (BitConverter.ToInt32(check_header, 0) == 9) versionOfGame = "WAU";
                else if (BitConverter.ToInt32(check_header, 0) == 10) versionOfGame = "TFTB";

                if (landb.Count != 0)
                {
                    List<TextCollector.TXT_collection> all_text = new List<TextCollector.TXT_collection>();
                    string error = string.Empty;
                    if (fileDestination[j].Extension == ".tsv" && MainMenu.settings.tsvFormat == true) AutoPacker.ImportTSV(fileDestination[j].FullName, ref all_text, "\\n", ref error);
                    else AutoPacker.ImportTXT(fileDestination[j].FullName, ref all_text, false, MainMenu.settings.ASCII_N, "\r\n", ref error);

                    if (error == string.Empty)
                    {
                        for (int q = 0; q < all_text.Count; q++)
                        {
                            if (MainMenu.settings.importingOfName == true)
                            {
                                landb[all_text[q].number - 1].name = all_text[q].name;
                                landb[all_text[q].number - 1].lenght_of_name = BitConverter.GetBytes(landb[all_text[q].number - 1].name.Length);
                            }

                            if (versionOfGame == "TFTB")
                            {
                                if (MainMenu.settings.unicodeSettings == 1)
                                {
                                    if (all_text[q].text.IndexOf("\\0") > 0)
                                    {
                                        all_text[q].text = all_text[q].text.Replace("\\0", "\0");
                                        return;
                                    }


                                    string alphabet = MainMenu.settings.additionalChar;

                                    for (int a = 0; a < alphabet.Length; a++)
                                    {
                                        all_text[q].text = all_text[q].text.Replace(alphabet[a].ToString(), ("Г" + alphabet[a]));
                                    }
                                }
                            }

                            //index = all_text[q].number;

                            if (fileDestination[j].Extension == ".txt") landb[all_text[q].number - 1].text = all_text[q].text.Replace("\r\n", "\n");
                            else if(fileDestination[j].Extension != ".txt" && landb[all_text[q].number - 1].text.Contains("\\n")) landb[all_text[q].number - 1].text = all_text[q].text.Replace("\\n", "\n");

                            if((versionOfGame == "TFTB") && (MainMenu.settings.unicodeSettings == 0))
                            {
                                if (landb[all_text[q].number - 1].text.IndexOf("(ANSI)") > 0)
                                {

                                    landb[all_text[q].number - 1].text = landb[all_text[q].number - 1].text.Replace("(ANSI)", "\0");
                                    landb[all_text[q].number - 1].lenght_of_text = BitConverter.GetBytes(landb[all_text[q].number - 1].text.Length);
                                }
                                else
                                {
                                    byte[] unicode_bin = (byte[])Encoding.UTF8.GetBytes(landb[all_text[q].number - 1].text);
                                    landb[all_text[q].number - 1].lenght_of_text = BitConverter.GetBytes(unicode_bin.Length);
                                }
                            }
                            else landb[all_text[q].number - 1].lenght_of_text = BitConverter.GetBytes(landb[all_text[q].number - 1].text.Length);
                        }
                        Methods.DeleteCurrentFile(pathOutput + "\\" + inputFiles[i].Name);
                        AutoPacker.CreateLandb(header, landb, end_of_file, (pathOutput + "\\" + inputFiles[i].Name), versionOfGame);

                        ReportForWork("File: " + fileDestination[j].Name + " imported in " + inputFiles[i].Name);
                    }
                    else
                    {
                        ReportForWork("Import in file: " + inputFiles[i].Name + " is incorrect! \r\n" + error);
                    }
                }
            }
            catch
            {
                ReportForWork("Import in file: " + inputFiles[i].Name + " is incorrect! Index: " + index);
            }
        }

        public void ImportDDSinD3DTX(FileInfo[] inputFiles, FileInfo[] fileDestination, int i, int j, string pathOutput, ref bool correctWork, string versionOfGame)
        {
            FileStream fs = new FileStream(inputFiles[i].FullName, FileMode.Open);
            byte[] d3dtxContent = Methods.ReadFull(fs);
            fs.Close();

            byte[] new_header = new byte[4];
            Array.Copy(d3dtxContent, 0, new_header, 0, 4);

            int offset = 0;
            byte[] check_ver = new byte[4];

            if (Encoding.ASCII.GetString(new_header) == "5VSM" || Encoding.ASCII.GetString(new_header) == "6VSM")
            {
                byte[] new_ver_check = new byte[4];
                Array.Copy(d3dtxContent, 16, new_ver_check, 0, 4);
                offset = 12 * BitConverter.ToInt32(new_ver_check, 0) + 16 + 4;
                check_ver = new byte[4];
                Array.Copy(d3dtxContent, offset, check_ver, 0, 4);
            }
            else
            {
                byte[] ver_check = new byte[4];
                Array.Copy(d3dtxContent, 4, ver_check, 0, 4);
                if (BitConverter.ToInt32(ver_check, 0) == 6) versionOfGame = "PN2";
                else versionOfGame = " ";
            }

            if ((BitConverter.ToInt32(check_ver, 0) == 6) && (Encoding.ASCII.GetString(new_header)) == "ERTM")
            {
                versionOfGame = "PN2";
            }
            else if ((BitConverter.ToInt32(check_ver, 0) >= 3) && (Encoding.ASCII.GetString(new_header) == "5VSM"))
            {
                switch (BitConverter.ToInt32(check_ver, 0))
                {
                    case 3:
                    case 4:
                        versionOfGame = "WAU";
                        break;
                    case 5:
                        versionOfGame = "TFTB";
                        break;
                    case 7:
                        versionOfGame = "WDM"; //Для Walking Dead Michonne
                        break;
                }
            }
            else if (BitConverter.ToInt32(check_ver, 0) >= 8 && (Encoding.ASCII.GetString(new_header) == "6VSM"))
            {
                switch(BitConverter.ToInt32(check_ver, 0))
                {
                    case 8:
                        versionOfGame = "Batman";
                        break;

                    case 9:
                        versionOfGame = "WDDS";
                        break;
                }
                
            }

            fs = new FileStream(fileDestination[j].FullName, FileMode.Open);
            byte[] ddsContent = Methods.ReadFull(fs);
            fs.Close();

                int razn = 0;
                if (versionOfGame == " ")
                {
                    byte[] check_header = new byte[4];
                    Array.Copy(d3dtxContent, 0, check_header, 0, 4);

                    if ((Encoding.ASCII.GetString(check_header) != "5VSM")) //текстуры старых игр
                    {
                        d3dtxContent = TextureWorker.import_old_textures(d3dtxContent, ddsContent);

                        if (d3dtxContent != null)
                        {
                            byte[] new_d3dtx = d3dtxContent;

                            if (MainMenu.settings.encDDSonly == true && MainMenu.settings.encLangdb == false)
                            {
                                int pos = Methods.FindStartOfStringSomething(new_d3dtx, 8, "DDS");

                                byte[] enc_block = new byte[2048];

                                if (enc_block.Length > new_d3dtx.Length - pos) enc_block = new byte[new_d3dtx.Length - pos]; //Если текстура будет меньше 2048 байт
                                Array.Copy(new_d3dtx, pos, enc_block, 0, enc_block.Length);

                                if (MainMenu.settings.customKey)
                                {
                                    byte[] key = Methods.stringToKey(MainMenu.settings.encCustomKey);

                                    if (key != null)
                                    {
                                        BlowFishCS.BlowFish encBlock = new BlowFishCS.BlowFish(key, AutoPacker.EncVersion);

                                        enc_block = encBlock.Crypt_ECB(enc_block, AutoPacker.EncVersion, false);
                                        Array.Copy(enc_block, 0, new_d3dtx, pos, enc_block.Length);

                                        if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                                        fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                                        fs.Write(new_d3dtx, 0, new_d3dtx.Length);
                                        fs.Close();
                                        ReportForWork("Packed and encrypted DDS-header only: " + inputFiles[i].Name + ". Used custom key: " + Encoding.ASCII.GetString(key));
                                    }
                                    else
                                    {
                                        ReportForWork("Key is incorrect! Trying to just packing.");
                                        if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                                        fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                                        fs.Write(new_d3dtx, 0, new_d3dtx.Length);
                                        fs.Close();
                                        ReportForWork("Packed: " + inputFiles[i].Name);
                                    }
                                }
                                else
                                {
                                    byte[] key = TTG_Tools.MainMenu.gamelist[AutoPacker.numKey].key;
                                    BlowFishCS.BlowFish encBlock = new BlowFishCS.BlowFish(key, AutoPacker.EncVersion);

                                    enc_block = encBlock.Crypt_ECB(enc_block, AutoPacker.EncVersion, false);
                                    Array.Copy(enc_block, 0, new_d3dtx, pos, enc_block.Length);

                                    if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                                    fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                                    fs.Write(new_d3dtx, 0, new_d3dtx.Length);
                                    fs.Close();
                                    ReportForWork("Packed and encrypted DDS-header only: " + inputFiles[i].Name);
                                }
                            }
                            else if (MainMenu.settings.encLangdb)
                            {

                                int pos = Methods.FindStartOfStringSomething(new_d3dtx, 8, "DDS");

                                byte[] enc_block = new byte[2048];

                                if (enc_block.Length > new_d3dtx.Length - pos) enc_block = new byte[new_d3dtx.Length - pos]; //Если текстура будет меньше 2048 байт
                                Array.Copy(new_d3dtx, pos, enc_block, 0, enc_block.Length);

                                if (MainMenu.settings.customKey)
                                {
                                    byte[] key = Methods.stringToKey(MainMenu.settings.encCustomKey);

                                    if (key != null)
                                    {
                                        BlowFishCS.BlowFish encBlock = new BlowFishCS.BlowFish(key, AutoPacker.EncVersion);

                                        enc_block = encBlock.Crypt_ECB(enc_block, AutoPacker.EncVersion, false);
                                        Array.Copy(enc_block, 0, new_d3dtx, pos, enc_block.Length);

                                        Methods.meta_crypt(new_d3dtx, key, AutoPacker.EncVersion, false);

                                        if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                                        fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                                        fs.Write(new_d3dtx, 0, new_d3dtx.Length);
                                        fs.Close();
                                        ReportForWork("Packed and fully encrypted: " + inputFiles[i].Name + ". Used custom key: " + Encoding.ASCII.GetString(key));
                                    }
                                    else
                                    {
                                        ReportForWork("Key is incorrect! Trying to just packing.");

                                        if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                                        fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                                        fs.Write(new_d3dtx, 0, new_d3dtx.Length);
                                        fs.Close();
                                        ReportForWork("Packed: " + inputFiles[i].Name);
                                    }
                                }
                                else
                                {
                                    byte[] key = TTG_Tools.MainMenu.gamelist[AutoPacker.numKey].key;
                                    BlowFishCS.BlowFish encBlock = new BlowFishCS.BlowFish(key, AutoPacker.EncVersion);

                                    enc_block = encBlock.Crypt_ECB(enc_block, AutoPacker.EncVersion, false);
                                    Array.Copy(enc_block, 0, new_d3dtx, pos, enc_block.Length);

                                    Methods.meta_crypt(new_d3dtx, key, AutoPacker.EncVersion, false);

                                    if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                                    fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                                    fs.Write(new_d3dtx, 0, new_d3dtx.Length);
                                    fs.Close();

                                    ReportForWork("Packed and fully encrypted: " + inputFiles[i].Name);
                                }
                            }
                            else
                            {
                                if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                                fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                                fs.Write(new_d3dtx, 0, new_d3dtx.Length);
                                fs.Close();
                                ReportForWork("Packed: " + inputFiles[i].Name);
                            }
                        }
                        else ReportForWork("Something is wrong with file " + inputFiles[i].Name + ". Please, contact with me.");
                        
                    } 
                    
                }
                else if (versionOfGame != " ")
                {
                    bool isPVR = false;
                    d3dtxContent = TextureWorker.import_new_textures(d3dtxContent, ddsContent, versionOfGame, ref isPVR, MainMenu.settings.iOSsupport);
                    
                    if (d3dtxContent != null)
                    {
                        if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                        fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                        fs.Write(d3dtxContent, 0, d3dtxContent.Length);
                        fs.Close();
                        ReportForWork("Packed: " + inputFiles[i].Name);
                    }
                    else ReportForWork("Something wrong with file " + inputFiles[i].Name + ". Please, contact with me.");
                }
        }

        public void ImportTXTinLANGDB(FileInfo[] inputFiles, FileInfo[] fileDestination, int i, int j, string pathOutput, ref bool correctWork, string versionOfGame)
        {
            AutoPacker.langdb[] database = new AutoPacker.langdb[5000];
            FileStream fs = new FileStream(inputFiles[i].FullName, FileMode.Open);
            byte[] binContent = Methods.ReadFull(fs);

            tryRead:

            byte version = 0;
            try
            {
                AutoPacker.ReadLangdb(binContent, database, version);
            }
            catch
            {
                version = 1;
            }
            try
            {
                AutoPacker.ReadLangdb(binContent, database, version);
            }
            catch
            {
                version = 2;
            }
            try
            {
                AutoPacker.ReadLangdb(binContent, database, version);
            }
            catch
            {
                version = 4; //FIX THAT LATER
            }
            try
            {
                AutoPacker.ReadLangdb(binContent, database, version);
            }
            catch
            {
                try
                {
                    string info = Methods.FindingDecrytKey(binContent, "text"); //Пытаемся расшифровать текстовый файл.
                    ReportForWork("File " + inputFiles[i].Name + " decrypted. " + info);
                    goto tryRead;
                }
                catch 
                {
                    System.Windows.Forms.MessageBox.Show("ERROR! Unknown langdb.");
                    return;
                }
                
                
            }


            byte[] header = new byte[0];
            byte[] end_of_file = new byte[0];
            byte[] lenght_of_all_text = new byte[4];
            AutoPacker.ReadLangdb(binContent, database, version);
            fs.Close();

            if (database.Length != 0)
            {
                List<TextCollector.TXT_collection> all_text = new List<TextCollector.TXT_collection>();
                string error = string.Empty;
                string pathForFinalFile = MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name;
                if ((MainMenu.settings.tsvFormat == false) && fileDestination[j].Extension == ".txt") AutoPacker.ImportTXT(fileDestination[j].FullName, ref all_text, false, MainMenu.settings.ASCII_N, "\n", ref error);
                else if ((MainMenu.settings.tsvFormat == true) && (fileDestination[j].Extension == ".tsv")) AutoPacker.ImportTSV(fileDestination[j].FullName, ref all_text, "\\n", ref error);

                if (error == string.Empty)
                {
                    for (int q = 0; q < all_text.Count; q++)
                    {
                            if (MainMenu.settings.importingOfName == true)
                            {
                                database[all_text[q].number - 1].name = all_text[all_text[q].number - 1].name;
                                database[all_text[q].number - 1].lenght_of_name = BitConverter.GetBytes(database[all_text[q].number - 1].name.Length);
                            }

                            if(MainMenu.settings.tsvFormat == false) database[all_text[q].number - 1].text = all_text[q].text.Replace("\r\n", "\n");
                            else database[all_text[q].number - 1].text = all_text[q].text;
                            database[all_text[q].number - 1].lenght_of_text = BitConverter.GetBytes(database[all_text[q].number - 1].text.Length);
                    }
                    Methods.DeleteCurrentFile(pathForFinalFile);
                    AutoPacker.CreateLangdb(database, version, pathForFinalFile);
                    ReportForWork("File: " + fileDestination[j].Name + " imported in " + inputFiles[i].Name);

                    if ((versionOfGame == " ") && MainMenu.settings.encLangdb == true)
                    {
                        byte[] encKey;
                        if (MainMenu.settings.customKey) encKey = Methods.stringToKey(MainMenu.settings.encCustomKey);
                        else encKey = MainMenu.gamelist[TTG_Tools.AutoPacker.numKey].key;

                        fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.Open);
                        byte[] temp_file = new byte[fs.Length];
                        fs.Read(temp_file, 0, temp_file.Length);
                        fs.Close();

                        
                        if (AutoPacker.selected_index == 0) Methods.meta_crypt(temp_file, encKey, 2, false);
                        else Methods.meta_crypt(temp_file, encKey, 7, false);

                        fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.OpenOrCreate);
                        fs.Write(temp_file, 0, temp_file.Length);
                        fs.Close();

                        ReportForWork("File " + inputFiles[i].Name + " encrypted!");
                    }
                }
                else
                {
                    ReportForWork("Import in file: " + inputFiles[i].Name + " is incorrect! \r\n" + error);
                }
            }
            else
            {
                ReportForWork("File " + inputFiles[i].Name + " is EMPTY!");
            }
        }

        //Экспорт
        public void DoExportEncoding(object parametres)
        {
            List<string> param = parametres as List<string>;
            string pathInput = param[0];
            string pathOutput = param[1];
            string versionOfGame = param[2];
            byte[] key = Methods.stringToKey(param[3]);
            int version = Convert.ToInt32(param[4]);

            if (Directory.Exists(pathInput) && Directory.Exists(pathOutput))
            {
                List<string> destinationForExportList = new List<string>();
                destinationForExportList.Add(".langdb");
                destinationForExportList.Add(".d3dtx");

                foreach (string destinationForExport in destinationForExportList)
                {
                    DirectoryInfo dir = new DirectoryInfo(pathInput);
                    FileInfo[] inputFiles = dir.GetFiles('*' + destinationForExport);

                    if (inputFiles.Length > 0)
                    {
                        for (int i = 0; i < inputFiles.Length; i++)
                        {
                            switch (destinationForExport)
                            {
                                case ".langdb":
                                    {
                                        int lenghtOfExtension = inputFiles[i].Extension.Length;
                                        string fileName = inputFiles[i].Name.Remove(inputFiles[i].Name.Length - lenghtOfExtension, lenghtOfExtension) + ".txt";
                                        if (MainMenu.settings.tsvFormat) fileName = inputFiles[i].Name.Remove(inputFiles[i].Name.Length - lenghtOfExtension, lenghtOfExtension) + ".tsv";
                                        ExportTXTfromLANGDB(inputFiles, i, pathOutput, fileName, versionOfGame);
                                        break;
                                    }
                                case ".d3dtx":
                                    {
                                        string message = TextureWorker.ExportTexture(inputFiles, i, AutoPacker.selected_index, key, version, versionOfGame, MainMenu.settings.iOSsupport);
                                        if (message != "") ReportForWork(message);
                                        else ReportForWork("Unknown error. Please send me file.");
                                        break;
                                    }
                                default:
                                    {
                                        System.Windows.Forms.MessageBox.Show("Error in Switch!");
                                        break;
                                    }
                            }

                        }

                        ReportForWork("EXPORT OF ALL ***" + destinationForExport.ToUpper() + " IS COMPLETE!");
                    }
                }
            }

        }
        public void ExportTXTfromLANGDB(FileInfo[] inputFiles, int i, string pathOutput, string fileName, string versionOfGame)
        {
            AutoPacker.langdb[] database = new AutoPacker.langdb[5000];
            //try
            {
                List<AutoPacker.Langdb> landb = new List<AutoPacker.Langdb>();
                FileStream fs = new FileStream(inputFiles[i].FullName, FileMode.Open);
                byte[] binContent = Methods.ReadFull(fs);
                fs.Close();

            tryAgain:
                byte[] header = new byte[0];
                byte[] end_of_file = new byte[0];
                byte[] lenght_of_all_text = new byte[4];

                byte version = 0;
                try
                {
                    AutoPacker.ReadLangdb(binContent, database, version);
                }
                catch
                {
                    version = 1;
                }
                try
                {
                    AutoPacker.ReadLangdb(binContent, database, version);
                }
                catch
                {
                    version = 2;
                }
                try
                {
                    AutoPacker.ReadLangdb(binContent, database, version);
                }
                catch
                {
                    version = 4; //FIX THAT LATER
                }
                try
                {
                    AutoPacker.ReadLangdb(binContent, database, version);
                }
                catch
                {

                    try 
                    {
                        string info = Methods.FindingDecrytKey(binContent, "text");
                        ReportForWork("File " + inputFiles[i].Name + " decrypted. " + info);
                        goto tryAgain;
                    }
                    catch
                    {
                        ReportForWork("ERROR! Unknown langdb.");
                        return;
                    }
                }

                AutoPacker.ReadLangdb(binContent, database, version);


                if (database.Length != 0)
                {
                    List<TextCollector.TXT_collection> all_text = new List<TextCollector.TXT_collection>();

                    Methods.DeleteCurrentFile(pathOutput + "\\" + fileName);

                    List<TextCollector.TXT_collection> allTextForExport = new List<TextCollector.TXT_collection>();

                    for (int q = 0; q < database.Length; q++)
                    {
                        if (database[q].text != null)
                        {
                            UInt32 realID = BitConverter.ToUInt32(database[q].realID, 0);
                            all_text.Add(new TextCollector.TXT_collection((q + 1), realID, database[q].name, database[q].text, false));
                        }
                    }

                    TextCollector.CreateExportingTXTfromOneFile(all_text, ref allTextForExport);

                    //allTextForExport = all_text;
                    FileStream MyExportStream = new FileStream(pathOutput + "\\" + fileName, FileMode.OpenOrCreate);
                    int w = 0;

                    if (!MainMenu.settings.tsvFormat)
                    {
                        while (w < allTextForExport.Count)
                        {
                            try { int u = allTextForExport[w].text.Length; }
                            catch { break; }
                            if (allTextForExport[w].text != null)
                            {
                                if (MainMenu.settings.exportRealID)
                                {
                                    TextCollector.SaveString(MyExportStream, (allTextForExport[w].realId + ") " + allTextForExport[w].name + "\r\n"), MainMenu.settings.ASCII_N);//проверка
                                }
                                else
                                {
                                    TextCollector.SaveString(MyExportStream, (allTextForExport[w].number + ") " + allTextForExport[w].name + "\r\n"), MainMenu.settings.ASCII_N);//проверка
                                }
                                //TextCollector.SaveString(MyExportStream, (allTextForExport[w].number + ") " + allTextForExport[w].name + "\r\n"), MainMenu.settings.ASCII_N);
                                //TextCollector.SaveString(MyExportStream, (BitConverter.ToString(database[all_text_for_export[w].number-1].hz_data)+"\r\n"), MainMenu.settings.ASCII_N);
                                allTextForExport[w].text = allTextForExport[w].text.Replace("\n", "\r\n");
                                TextCollector.SaveString(MyExportStream, (allTextForExport[w].text + "\r\n"), MainMenu.settings.ASCII_N);
                                w++;
                            }
                            else
                            { }
                        }
                    }
                    else
                    {
                        while (w < allTextForExport.Count)
                        {
                            try { int u = allTextForExport[w].text.Length; }
                            catch { break; }

                            string export_tsv;

                            if (allTextForExport[w].text != null)
                            {
                                if (MainMenu.settings.exportRealID)
                                {
                                    export_tsv = allTextForExport[w].realId + "\t" + allTextForExport[w].name + "\t";
                                    //TextCollector.SaveString(MyExportStream, (allTextForExport[w].realId + ") " + allTextForExport[w].name + "\r\n"), MainMenu.settings.ASCII_N);//проверка
                                }
                                else
                                {
                                    export_tsv = allTextForExport[w].number + "\t" + allTextForExport[w].name + "\t";
                                    //TextCollector.SaveString(MyExportStream, (allTextForExport[w].number + ") " + allTextForExport[w].name + "\r\n"), MainMenu.settings.ASCII_N);//проверка
                                }

                                allTextForExport[w].text = allTextForExport[w].text.Replace("\n", "\\n");
                                export_tsv += allTextForExport[w].text + "\r\n";
                                byte[] bin_export = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(export_tsv);
                                bin_export = Encoding.Convert(Encoding.GetEncoding(MainMenu.settings.ASCII_N), Encoding.UTF8, bin_export);
                                export_tsv = Encoding.UTF8.GetString(bin_export);
                                TextCollector.SaveString(MyExportStream, export_tsv, 0);
                                w++;

                                //TextCollector.SaveString(MyExportStream, (allTextForExport[w].number + ") " + allTextForExport[w].name + "\r\n"), MainMenu.settings.ASCII_N);
                                //TextCollector.SaveString(MyExportStream, (BitConverter.ToString(database[all_text_for_export[w].number-1].hz_data)+"\r\n"), MainMenu.settings.ASCII_N);
                                //TextCollector.SaveString(MyExportStream, (allTextForExport[w].text + "\r\n"), MainMenu.settings.ASCII_N);
                            }
                            else
                            { }
                        }
                    }



                    MyExportStream.Close();
                    ReportForWork("File " + inputFiles[i].Name + " exported in " + fileName);
                }
                else
                {
                    ReportForWork("File " + inputFiles[i].Name + " is EMPTY!");
                }
            }
            //catch
            //{
            //    ReportForWork("Import in file: " + inputFiles[i].Name + " is incorrect!");
            //}
        }


        public void DoWork()
        {
            for (int i = 1; i <= 100; ++i)
            {
                Thread.Sleep(100);
                if (Progress != null)
                    Progress(i);
            }
        }
    }
}
