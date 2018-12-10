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
                        if (TextCollector.IsStringsSame(allText[i].orText, allText[j].orText, false) && allText[j].exported == false)
                        {
                            allText[j].exported = true;
                            allText.Insert(i + 1, allText[j]);
                            allText[i + 1].exported = true;
                            allText.RemoveAt(j + 1);
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
            //System.Windows.Forms.MessageBox.Show("Test");
        }

        public void CreateExportingTXTfromAllText2(object inputList)
        {

            List<TextEditor.AllText> allText2 = inputList as List<TextEditor.AllText>;
            CreateExportingTXTfromAllTextN(ref allText2);
            BackAllText2(allText2);
            //System.Windows.Forms.MessageBox.Show("Test");
        }

        public void CreateGlossaryFromFirstAndSecondAllText(object TwoList)
        {
            //потом передавать мега-класс, содержащий всё это 
            List<List<TextEditor.AllText>> twoAllText = TwoList as List<List<TextEditor.AllText>>;
            List<TextEditor.AllText> firstText = twoAllText[0];
            List<TextEditor.AllText> secondText = twoAllText[1];
            //string path = "glossary.txt";
            //Methods.DeleteCurrentFile(path);
            //FileStream ExportStream = new FileStream(path, FileMode.OpenOrCreate);
            List<TextEditor.Glossary> glossary = new List<TextEditor.Glossary>();

            for (int i = 0; i < firstText.Count; i++)
            {
                for (int j = 0; j < secondText.Count; j++)
                {
                    if ((firstText[i].orText != "" || firstText[i].orText != string.Empty) && firstText[i].orText == secondText[j].orText)
                    {
                        //Glossary(firstText[i].orText, firstText[i].trText);
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
            //SaveFileDialog sfd = new SaveFileDialog();
            //if (sfd.ShowDialog() == DialogResult.OK)
            //{
            //    FileStream ExportStream = new FileStream("txt.txt", FileMode.OpenOrCreate);
            //    for (int i = 0; i < secondText.Count; i++)
            //    {
            //        TextCollector.SaveString(ExportStream, ((secondText[i].number + 1) + ") " + secondText[i].orName + "\r\n"), MainMenu.settings.ASCII_N);
            //        TextCollector.SaveString(ExportStream, (secondText[i].orText + "\r\n"), MainMenu.settings.ASCII_N);
            //        TextCollector.SaveString(ExportStream, ((secondText[i].number + 1) + ") " + secondText[i].trName + "\r\n"), MainMenu.settings.ASCII_N);
            //        TextCollector.SaveString(ExportStream, (secondText[i].trText + "\r\n"), MainMenu.settings.ASCII_N);
            //    }
            //    ExportStream.Close();
            //}
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

            for (int d = 0; d < destination.Count; d++)
            {
                destinationForExport = destination[d];
                whatImport = extention[d];
                if (Directory.Exists(pathInput) && Directory.Exists(pathOutput))
                {
                    DirectoryInfo dir = new DirectoryInfo(pathInput);
                    FileInfo[] inputFiles = dir.GetFiles('*' + destinationForExport);
                    
                        for (int i = 0; i < inputFiles.Length; i++)
                        {
                            if (destinationForExport == ".font" && versionOfGame != " " && versionOfGame != "WAU" && versionOfGame != "PN2")
                            {
                                //Шифрование шрифта для СиМ 20х
                                FileStream fs = new FileStream(inputFiles[i].FullName, FileMode.Open);
                                byte[] font = Methods.ReadFull(fs);
                                fs.Close();
                                int poz_of_encrypt = Methods.FindStartOfStringSomething(font, 0, "DDS");
                                //byte[] font_encrypted = Methods.EncryptFile(MainMenu.settings.pathForInputFolder, MainMenu.settings.pathForTempFolder, inputFiles[i].Name, font, versionOfGame, poz_of_encrypt, MainMenu.settings.pathForTtarchext);
                                ReportForWork("File " + inputFiles[i].Name + " encrypted");
                                //Methods.PackFile(MainMenu.settings.pathForOutputFolder, MainMenu.settings.pathForTempFolder, inputFiles[i].Name, font_encrypted, versionOfGame, MainMenu.settings.pathForTtarchext);
                                ReportForWork("File " + inputFiles[i].Name + " packed");
                            }
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
                                                break;
                                            }
                                        case ".landb":
                                            {
                                                ImportTXTinLANDB(inputFiles, fileDestination, i, j, pathOutput, ref correct_work, versionOfGame);
                                                break;
                                            }
                                        case ".langdb":
                                            {
                                                ImportTXTinLANGDB(inputFiles, fileDestination, i, j, pathOutput, ref correct_work, versionOfGame);                                                
                                                break;
                                            }
                                        case ".prop":
                                            {
                                                ImportTXTinPROP(inputFiles, fileDestination, i, j, pathOutput, ref correct_work);
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
                ReportForWork("IMPORT OF ALL ***" + destinationForExport.ToUpper() + " IS COMPLETE!");
            }
        }
        public void ImportTXTinPROP(FileInfo[] inputFiles, FileInfo[] fileDestination, int i, int j, string pathOutput, ref bool correctWork)
        {
            try
            {
                List<AutoPacker.Prop> prop = new List<AutoPacker.Prop>();
                FileStream fs = new FileStream(inputFiles[i].FullName, FileMode.Open);
                byte[] binContent = Methods.ReadFull(fs);
                byte[] header = new byte[0];
                byte[] header2 = new byte[0];
                byte[] countOfBlock = new byte[0];
                byte[] end_of_file = new byte[0];
                byte[] lenght_of_all_text = new byte[4];
                int c = 0;
                AutoPacker.ReadProp(binContent, prop, ref header, ref countOfBlock, ref header2, ref lenght_of_all_text, ref end_of_file);
                fs.Close();
                if (prop.Count != 0)
                {
                    List<TextCollector.TXT_collection> all_text = new List<TextCollector.TXT_collection>();
                    string error = string.Empty;
                    AutoPacker.ImportTXT(fileDestination[j].FullName, ref all_text, false, MainMenu.settings.ASCII_N, "\r\n", ref error);
                    if (error == string.Empty)
                    {
                        for (int q = 0; q < prop.Count; q++)
                        {
                            for (int w = 0; w < prop[q].textInProp.Count; w++)
                            {
                                c++;
                                int id = -1;
                                findStringByID(all_text, c, ref id);
                                if (id > 0)
                                {
                                    if (MainMenu.settings.importingOfName)
                                    {
                                        prop[q].textInProp[w].name = all_text[id].name;
                                        prop[q].textInProp[w].lenght_of_name = BitConverter.GetBytes(prop[q].textInProp[w].name.Length);
                                    }
                                    prop[q].textInProp[w].text = all_text[id].text.Replace("\r\n", "\n");
                                    prop[q].textInProp[w].lenght_of_text = BitConverter.GetBytes(prop[q].textInProp[w].text.Length);
                                }
                            }
                        }
                        Methods.DeleteCurrentFile(pathOutput + "\\" + inputFiles[i].Name);
                        AutoPacker.CreateProp(header, countOfBlock, header2, prop, end_of_file, (pathOutput + "\\" + inputFiles[i].Name));

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
                ReportForWork("Import in file: " + inputFiles[i].Name + " is incorrect!");
            }
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
                    if (fileDestination[j].Extension == ".tsv" && AutoPacker.tsvFile == true) AutoPacker.ImportTSV(fileDestination[j].FullName, ref all_text, "\\n", ref error);
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
                                        goto skip_circle;
                                    }


                                    string alphabet = MainMenu.settings.additionalChar;

                                    for (int a = 0; a < alphabet.Length; a++)
                                    {
                                        all_text[q].text = all_text[q].text.Replace(alphabet[a].ToString(), ("Г" + alphabet[a]));
                                    }

                                skip_circle:
                                    int end = 0;
                                }
                            }

                            if (fileDestination[j].Extension == ".txt") landb[all_text[q].number - 1].text = all_text[q].text.Replace("\r\n", "\n");
                            else landb[all_text[q].number - 1].text = all_text[q].text.Replace("\\n", "\n");

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
                ReportForWork("Import in file: " + inputFiles[i].Name + " is incorrect!");
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
                /*if (BitConverter.ToInt32(new_ver_check, 0) < 7)
                {
                    new_ver_check = new byte[4];
                    Array.Copy(d3dtxContent, 92, new_ver_check, 0, 4);
                }
                else
                {
                    new_ver_check = new byte[4];
                    Array.Copy(d3dtxContent, 104, new_ver_check, 0, 4);
                }*/

                /*switch (BitConverter.ToInt32(new_ver_check, 0))
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
                    case 8:
                    case 9:
                        versionOfGame = "Batman";
                        break;
                }*/
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
                versionOfGame = "Batman";
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
                       /* byte[] checkVer = new byte[4];
                        Array.Copy(d3dtxContent, 4, checkVer, 0, 4); //Провека зашифрованных файлов

                        //new_d3dtx = new byte[d3dtxContent.Length];
                        if ((BitConverter.ToInt32(checkVer, 0) < 0) || (BitConverter.ToInt32(checkVer, 0) > 5))
                        {
                            Methods.FindingDecrytKey(d3dtxContent, "texture");
                            if(Methods.FindStartOfStringSomething(d3dtxContent, 0, "DDS") > d3dtxContent.Length - 100)
                            {
                                Methods.FindingDecrytKey(d3dtxContent, "texture");
                            }
                        }
                        else if(((BitConverter.ToInt32(checkVer, 0) > 0) && (BitConverter.ToInt32(checkVer, 0) < 6))
                            && Methods.FindStartOfStringSomething(d3dtxContent, 0, "DDS") > d3dtxContent.Length - 100)
                        {
                            Methods.FindingDecrytKey(d3dtxContent, "texture");
                        }
                        */

                        d3dtxContent = TextureWorker.import_old_textures(d3dtxContent, ddsContent);

                        if (d3dtxContent != null)
                        {
                            byte[] new_d3dtx = d3dtxContent;

                            if (AutoPacker.encDDSonly == true && AutoPacker.encLangdb == false)
                            {
                                int pos = Methods.FindStartOfStringSomething(new_d3dtx, 8, "DDS");

                                byte[] enc_block = new byte[2048];

                                if (enc_block.Length > new_d3dtx.Length - pos) enc_block = new byte[new_d3dtx.Length - pos]; //Если текстура будет меньше 2048 байт
                                Array.Copy(new_d3dtx, pos, enc_block, 0, enc_block.Length);

                                if (AutoPacker.custKey)
                                {
                                    byte[] key = Methods.stringToKey(AutoPacker.customKey);

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
                            else if (AutoPacker.encLangdb == true)
                            {

                                int pos = Methods.FindStartOfStringSomething(new_d3dtx, 8, "DDS");

                                byte[] enc_block = new byte[2048];

                                if (enc_block.Length > new_d3dtx.Length - pos) enc_block = new byte[new_d3dtx.Length - pos]; //Если текстура будет меньше 2048 байт
                                Array.Copy(new_d3dtx, pos, enc_block, 0, enc_block.Length);

                                if (AutoPacker.custKey)
                                {
                                    byte[] key = Methods.stringToKey(AutoPacker.customKey);

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
                        /*else if (Methods.FindStartOfStringSomething(d3dtxContent, 0, "PVR!") < (d3dtxContent.Length - 100))
                        {
                            int start = Methods.FindStartOfStringSomething(d3dtxContent, 0, "PVR!");
                            byte[] newD3dtx = new byte[start + 8];
                            Array.Copy(d3dtxContent, 0, newD3dtx, 0, start + 8);
                            byte[] mipmap = new byte[4];
                            byte[] width = new byte[4];
                            byte[] height = new byte[4];
                            byte[] texType = new byte[8];
                            byte[] metaLength = new byte[4];

                            Array.Copy(ddsContent, 8, texType, 0, 8);
                            Array.Copy(ddsContent, 24, height, 0, 4);
                            Array.Copy(ddsContent, 28, width, 0, 4);
                            Array.Copy(ddsContent, 44, mipmap, 0, 4);
                            Array.Copy(ddsContent, 48, metaLength, 0, 4);

                            int dataLength = ddsContent.Length - BitConverter.ToInt32(metaLength, 0) - 52; //Длина контента текстуры
                            byte[] binDataLength = new byte[4];
                            binDataLength = BitConverter.GetBytes(dataLength);

                            byte[] texContent = new byte[dataLength + 1];
                            Array.Copy(ddsContent, 52 + BitConverter.ToInt32(metaLength, 0), texContent, 0, dataLength);

                            byte[] commonSize = new byte[4];
                            commonSize = BitConverter.GetBytes(texContent.Length + 52); //Размер блока (заголовок + данные контента)

                            mipmap = BitConverter.GetBytes(BitConverter.ToInt32(mipmap, 0) - 1);

                            Array.Copy(commonSize, 0, newD3dtx, start - 48, commonSize.Length);
                            Array.Copy(height, 0, newD3dtx, start - 40, height.Length);
                            Array.Copy(width, 0, newD3dtx, start - 36, width.Length);
                            Array.Copy(mipmap, 0, newD3dtx, start - 32, mipmap.Length);
                            Array.Copy(binDataLength, 0, newD3dtx, start - 24, binDataLength.Length);

                            switch (BitConverter.ToUInt64(texType, 0))
                            {
                                case 0x404040461626772:
                                    texType = new byte[4];
                                    texType = BitConverter.GetBytes(0x8010);
                                    break;
                                case 2:
                                    texType = new byte[4];
                                    texType = BitConverter.GetBytes(0x30d);
                                    break;
                                case 3:
                                    texType = new byte[4];
                                    texType = BitConverter.GetBytes(0x830d);
                                    break;
                                case 0x5060500626772:
                                    texType = new byte[4];
                                    texType = BitConverter.GetBytes(0);
                                    byte[] texformat = { 0x13 };
                                    Array.Copy(texformat, 0, texType, 0, texformat.Length);
                                    break;
                            }

                            Array.Copy(texType, 0, newD3dtx, start - 28, texType.Length);

                            fs.Write(newD3dtx, 0, newD3dtx.Length);
                            fs.Write(texContent, 0, texContent.Length);
                            fs.Close();

                            ReportForWork("File " + inputFiles[i].Name + " remade.");
                        }
                        /*razn = d3dtxContent.Length - ddsContent.Length;
                        fs.Write(d3dtxContent, 0, razn);
                        fs.Write(ddsContent, 0, ddsContent.Length);
                        ReportForWork("Packed: " + inputFiles[i].Name);
                        fs.Close();*/
                    //}
               // }
                else if (versionOfGame != " ")
                {
                    bool isPVR = false;
                    d3dtxContent = TextureWorker.import_new_textures(d3dtxContent, ddsContent, versionOfGame, ref isPVR, TTG_Tools.AutoPacker.isIOS);
                    
                    if (d3dtxContent != null)
                    {
                        if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name);
                        fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + inputFiles[i].Name, FileMode.CreateNew);
                        fs.Write(d3dtxContent, 0, d3dtxContent.Length);
                        fs.Close();
                        ReportForWork("Packed: " + inputFiles[i].Name);
                    }
                    else ReportForWork("Something wrong with file " + inputFiles[i].Name + ". Please, contact with me.");
                    /*List<TTG_Tools.AutoPacker.chapterOfDDS> chaptersOfDDS = new List<TTG_Tools.AutoPacker.chapterOfDDS>();
                    byte[] bLenghtOfHeaderDDS = new byte[4];
                    Array.Copy(d3dtxContent, 4, bLenghtOfHeaderDDS, 0, 4);
                    int lenghtOfHeaderDDS = BitConverter.ToInt32(bLenghtOfHeaderDDS, 0);
                    int start = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 5 + 2;
                    int poz = start;
                    List<byte[]> head = new List<byte[]>();


                    string res = Methods.GetChaptersOfDDS(d3dtxContent, poz, head, chaptersOfDDS, versionOfGame);
                    razn = d3dtxContent.Length;
                    int lenghtInD3dtx = 0;

                    for (int k = chaptersOfDDS.Count - 1; k >= 0; k--)
                    {
                        razn -= chaptersOfDDS[k].content_chapter.Length;
                        lenghtInD3dtx += chaptersOfDDS[k].content_chapter.Length;
                    }

                    byte[] width = new byte[4];
                    byte[] height = new byte[4];
                    byte[] mipmap = new byte[4];
                    Array.Copy(ddsContent, 12, height, 0, 4);
                    Array.Copy(ddsContent, 16, width, 0, 4);
                    byte[] tex_type = new byte[4];
                    int kratnost = 0;

                    /*for (int c = 0; c < TTG_Tools.AutoPacker.tex_format.Count; c++) //определение типа текстуры
                    {
                        byte[] sample_header = new byte[TTG_Tools.AutoPacker.tex_format[c].tex_header.Length];
                        byte[] check_header = new byte[TTG_Tools.AutoPacker.tex_format[c].tex_header.Length];

                        Array.Copy(ddsContent, 32, sample_header, 0, sample_header.Length);
                        Array.Copy(TTG_Tools.AutoPacker.tex_format[c].tex_header, 0, check_header, 0, check_header.Length);
                        int d = 0;
                        bool checked_header = false;

                        while ((d < check_header.Length) && (check_header[d] == sample_header[d])) d++;
                        checked_header = d == check_header.Length;


                        if (checked_header) //Устроен этот маразм из-за разных заголовков в PVR текстурах и текстурах Photoshop
                        {
                            tex_type = BitConverter.GetBytes(TTG_Tools.AutoPacker.tex_format[c].type_num);
                            if (BitConverter.ToInt32(tex_type, 0) == 4)
                            {
                                byte[] checkKratnost = new byte[4]; //Проверка на 4444 или ETC1
                                Array.Copy(ddsContent, 20, checkKratnost, 0, 4);

                                kratnost = BitConverter.ToInt32(width, 0) * BitConverter.ToInt32(height, 0) * 2 / BitConverter.ToInt32(height, 0);
                                if (kratnost != BitConverter.ToInt32(checkKratnost, 0))
                                {
                                    c += 5;
                                    tex_type = BitConverter.GetBytes(TTG_Tools.AutoPacker.tex_format[c].type_num);
                                }
                            }
                            break;
                        }
                    }

                    int ddsContentLength = 0;
                    if ((BitConverter.ToInt32(tex_type, 0) == 0x00) || (BitConverter.ToInt32(tex_type, 0) == 0x04) ||
                        (BitConverter.ToInt32(tex_type, 0) == 0x70)) ddsContentLength = ddsContent.Length - 148;
                    else ddsContentLength = ddsContent.Length - 128;

                    if (lenghtInD3dtx != ddsContentLength) 
                    { //Если длины текстур отличаются
                            #region

                                poz = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 19; //задаётся позиция для начала изменений
                                if (versionOfGame == "TFTB") poz = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 23;

                                Array.Copy(ddsContent, 28, mipmap, 0, 4); //копируем количество мип-мапов из dds текстуры
                                if (BitConverter.ToInt32(mipmap, 0) == 0) mipmap = BitConverter.GetBytes(1);
                                Array.Copy(mipmap, 0, d3dtxContent, poz, mipmap.Length); //и заменяем тут
                                poz += 4;
                                Array.Copy(width, 0, d3dtxContent, poz, 4); //тут заменяем ширину текстуры
                                poz += 4;
                                Array.Copy(height, 0, d3dtxContent, poz, 4); //тут высоту
                                poz += 4;
                                Array.Copy(tex_type, 0, d3dtxContent, poz, 4); //также меняем тип текстуры, если он отличается.

                                //делаем дальше смещение
                                if (versionOfGame == "PN2") poz += 52;
                                else if (versionOfGame == "WAU") poz += 56;
                                else if (versionOfGame == "TFTB") poz += 68;
                                else if (versionOfGame == "WDM") poz += 72;

                                Array.Copy(mipmap, 0, d3dtxContent, poz, 4);
                                poz += 8;

                                byte[] ddsContentLengthBin = new byte[4];
                                int ddsKratnost = 0;
                                byte[] ddsKratnostBin = new byte[4];

                                ddsContentLengthBin = BitConverter.GetBytes(ddsContentLength);
                                Array.Copy(ddsContentLengthBin, 0, d3dtxContent, poz, 4);
                                if (versionOfGame != "PN2") Array.Copy(ddsContentLengthBin, 0, d3dtxContent, 12, 4); //Если не покер 2, меняем ещё и в начале размер данных текстуры
                                poz += 4;

                                if (versionOfGame == "TFTB" || versionOfGame == "WDM") poz += 4;
                                int mipmapTable = 0;

                                //задаём размер блока с данными о мип-мапах и их размерах
                                if (versionOfGame == "PN2") mipmapTable = 12 * BitConverter.ToInt32(mipmap, 0);
                                else if (versionOfGame == "WAU") mipmapTable = 16 * BitConverter.ToInt32(mipmap, 0);
                                else if (versionOfGame == "TFTB" || versionOfGame == "WDM") mipmapTable = 20 * BitConverter.ToInt32(mipmap, 0) - 4;

                                byte[] newD3dtxHeader = new byte[poz + mipmapTable];
                                Array.Copy(d3dtxContent, 0, newD3dtxHeader, 0, poz); 

                                //Задаются новые параметры для размера текстур и их кратностей
                                int[] contentChpater = new int[BitConverter.ToInt32(mipmap, 0)];
                                int[] kratnostChapter = new int[BitConverter.ToInt32(mipmap, 0)];

                                //Далее идёт расчёт
                                ddsContentLength = BitConverter.ToInt32(width, 0) * BitConverter.ToInt32(height, 0);
                                ddsKratnost = BitConverter.ToInt32(width, 0) * BitConverter.ToInt32(height, 0);

                                switch (BitConverter.ToInt32(tex_type, 0)) //И в случае типа текстур меняются дальнейшие условия расчёта
                                {
                                    case 0x00: //8888 RGBA
                                        ddsContentLength *= 4;
                                        ddsKratnost = ddsKratnost * 4 / BitConverter.ToInt32(height, 0);
                                        break;

                                    case 0x02: //PVRTC 2bpp
                                    case 0x50:
                                        ddsContentLength /= 4;
                                        ddsKratnost = ddsKratnost * 2 / BitConverter.ToInt32(height, 0);
                                        break;

                                    case 0x04: //4444
                                        ddsContentLength *= 2;
                                        ddsKratnost = ddsKratnost * 2 / BitConverter.ToInt32(height, 0);
                                        break;

                                    case 0x10: //Alpha 8 bit
                                        ddsKratnost /= BitConverter.ToInt32(height, 0);
                                        break;

                                    case 0x25: //32f.32f.32f.32f
                                        ddsContentLength *= 4 * 4;
                                        ddsKratnost = ddsKratnost * 4 * 4 / BitConverter.ToInt32(height, 0);
                                        break;

                                    case 0x51: //pvrtc 4bpp
                                    case 0x40: //DXT1
                                    case 0x70: //ETC1
                                        ddsContentLength /= 2;
                                        ddsKratnost = ddsKratnost * 2 / BitConverter.ToInt32(height, 0);
                                        break;

                                    case 0x42: //DXT5
                                        ddsKratnost = ddsKratnost * 4 / BitConverter.ToInt32(height, 0);
                                        break;
                                }

                                for (int v = 0; v < BitConverter.ToInt32(mipmap, 0); v++)
                                {
                                    contentChpater[v] = ddsContentLength;
                                    kratnostChapter[v] = ddsKratnost;
                                    ddsContentLength = ddsContentLength / 4;
                                    ddsKratnost = ddsKratnost / 2;
                                }

                                //Идёт запись в блок данных о мип-мапах и размерах текстурах с кратностями
                                for (int x = BitConverter.ToInt32(mipmap, 0) - 1; x >= 0; x--)
                                {
                                    Array.Copy(BitConverter.GetBytes(x), 0, newD3dtxHeader, poz, 4);
                                    poz += 4;

                                    if (versionOfGame != "PN2")
                                    {
                                        Array.Copy(BitConverter.GetBytes(1), 0, newD3dtxHeader, poz, 4);
                                        poz += 4;
                                    }

                                    ddsContentLengthBin = BitConverter.GetBytes(contentChpater[x]);
                                    Array.Copy(ddsContentLengthBin, 0, newD3dtxHeader, poz, 4);
                                    poz += 4;

                                    ddsKratnostBin = BitConverter.GetBytes(kratnostChapter[x]);
                                    Array.Copy(ddsKratnostBin, 0, newD3dtxHeader, poz, 4);
                                    poz += 4;

                                    if (versionOfGame == "TFTB" || versionOfGame == "WDM")
                                    { //Странный метод в борде. Идут сначала какие-то 00 00 00 00 байта, а в конце тупо кратностью заканчивается
                                        if (x != 0)
                                        {
                                            poz += 4;
                                        }
                                    }
                                }

                                if (versionOfGame != "PN2")
                                {
                                    ulong blockSize = (ulong)newD3dtxHeader.Length - 92;
                                    if (versionOfGame == "WDM") blockSize -= 12;


                                    byte[] binBlockSize = new byte[4];
                                    binBlockSize = BitConverter.GetBytes(blockSize);
                                    Array.Copy(binBlockSize, 0, newD3dtxHeader, 4, 4);
                                }

                                fs.Write(newD3dtxHeader, 0, newD3dtxHeader.Length); //Записываем получившийся заголовок.

                                newD3dtxHeader = new byte[ddsContentLength]; //и подготавливаем для записи данных текстуры

                                int pozFromEnd = ddsContent.Length; //начинаем считывать снизу

                                for (int y = BitConverter.ToInt32(mipmap, 0) - 1; y >= 0; y--) //далее идёт запись по твоему методу
                                {
                                    pozFromEnd -= contentChpater[y];
                                    int contentSize = contentChpater[y];
                                    byte[] contentChapterBin = new byte[contentSize];
                                    Array.Copy(ddsContent, pozFromEnd, contentChapterBin, 0, contentSize);
                                    fs.Write(contentChapterBin, 0, contentChapterBin.Length);
                                }
                                ReportForWork("File " + inputFiles[i].Name + " has some differents, but it repacked!"); //Сообщаем об успешной переделке текстуры
                                fs.Close();

                            
                        #endregion
                        /*else ReportForWork("File " + inputFiles[i].Name + " didn't pack. Check DDS texture!");
                        //Иначе сообщат об ошибке и создатся пустой файл
                            fs.Close();
                            
                    }
                    else //Если схожи, программа тупо по параметрам d3dtx текстуры копирует данные из DDS текстуры
                    {
                        //Так как размеры текстур разного типа могут совпадать, меняем тут тип текстуры
                        poz = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 19 + 4 + 4 + 4;
                        if (versionOfGame == "TFTB") poz = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 23 + 4 + 4 + 4;

                        Array.Copy(tex_type, 0, d3dtxContent, poz, 4);

                        fs.Write(d3dtxContent, 0, razn);

                        int pozFromEnd = ddsContent.Length;
                        for (int k = 0; k < chaptersOfDDS.Count; k++)
                        {
                            pozFromEnd -= chaptersOfDDS[k].content_chapter.Length;
                            Array.Copy(ddsContent, pozFromEnd, chaptersOfDDS[k].content_chapter, 0, chaptersOfDDS[k].content_chapter.Length);
                            fs.Write(chaptersOfDDS[k].content_chapter, 0, chaptersOfDDS[k].content_chapter.Length);
                        }
                        ReportForWork("Packed: " + inputFiles[i].Name);
                        fs.Close();
                    }*/
                }


                /*if (versionOfGame != " " && versionOfGame != "WAU" && versionOfGame != "PN2" && versionOfGame != "TFTB")
                {
                    fs = new FileStream(pathOutpuFile, FileMode.Open);
                    byte[] new_d3dtx = Methods.ReadFull(fs);
                    fs.Close();
                    //byte[] encrypted_file = Methods.EncryptFile(MainMenu.settings.pathForInputFolder, MainMenu.settings.pathForTempFolder, inputFiles[i].Name, new_d3dtx, versionOfGame, razn, MainMenu.settings.pathForTtarchext);
                    ReportForWork("File " + inputFiles[i].Name + " encrypted");
                    //Methods.PackFile(MainMenu.settings.pathForOutputFolder, MainMenu.settings.pathForTempFolder, inputFiles[i].Name, encrypted_file, versionOfGame, MainMenu.settings.pathForTtarchext);
                    ReportForWork("File " + inputFiles[i].Name + " packed");
                }*/

           /* else //Если текстура больше d3dtx файла...
            {
                correctWork = false;
                //сначала идёт проверка на версию d3dtx файла
                if (versionOfGame == "WAU" || versionOfGame == "TFTB" || versionOfGame == "PN2") //Если эта текстура борда, волк среди нас или покер 2, то...
                { //и выполняется всё то же самое, что и с условием "DDS текстура < D3DTX текстуры"
                    #region
                    string pathOutpuFile = pathOutput + "\\" + inputFiles[i].Name;
                    Methods.DeleteCurrentFile(pathOutpuFile);
                    fs = new FileStream(pathOutpuFile, FileMode.OpenOrCreate);
                    List<TTG_Tools.AutoPacker.chapterOfDDS> chaptersOfDDS = new List<TTG_Tools.AutoPacker.chapterOfDDS>();
                    byte[] bLenghtOfHeaderDDS = new byte[4];
                    Array.Copy(d3dtxContent, 4, bLenghtOfHeaderDDS, 0, 4);
                    int lenghtOfHeaderDDS = BitConverter.ToInt32(bLenghtOfHeaderDDS, 0);
                    int start = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 5 + 2;
                    int poz = start;
                    List<byte[]> head = new List<byte[]>();
                    int kratnost = 0;

                    string res = Methods.GetChaptersOfDDS(d3dtxContent, poz, head, chaptersOfDDS, versionOfGame);
                    int razn = d3dtxContent.Length;
                    int lenghtInD3dtx = 0;
                    for (int k = chaptersOfDDS.Count - 1; k >= 0; k--)
                    {
                        razn -= chaptersOfDDS[k].content_chapter.Length;
                        lenghtInD3dtx += chaptersOfDDS[k].content_chapter.Length;
                    }


                    byte[] tex_type = new byte[4];

                    byte[] width = new byte[4];
                    byte[] height = new byte[4];
                    byte[] mipmap = new byte[4];
                    Array.Copy(ddsContent, 12, height, 0, 4);
                    Array.Copy(ddsContent, 16, width, 0, 4);
                    Array.Copy(ddsContent, 28, mipmap, 0, 4);

                    for (int c = 0; c < TTG_Tools.AutoPacker.tex_format.Count; c++)
                    {
                        byte[] sample_header = new byte[TTG_Tools.AutoPacker.tex_format[c].tex_header.Length];
                        byte[] check_header = new byte[TTG_Tools.AutoPacker.tex_format[c].tex_header.Length];

                        Array.Copy(ddsContent, 32, sample_header, 0, sample_header.Length);
                        Array.Copy(TTG_Tools.AutoPacker.tex_format[c].tex_header, 0, check_header, 0, check_header.Length);
                        int d = 0;
                        bool checked_header = false;

                        while ((d < check_header.Length) && (check_header[d] == sample_header[d])) d++;
                        checked_header = d == check_header.Length;


                        if (checked_header)
                        {
                            tex_type = BitConverter.GetBytes(TTG_Tools.AutoPacker.tex_format[c].type_num);
                            if (BitConverter.ToInt32(tex_type, 0) == 4)
                            {
                                byte[] checkKratnost = new byte[4]; //Проверка на 4444 или ETC1
                                Array.Copy(ddsContent, 20, checkKratnost, 0, 4);

                                kratnost = BitConverter.ToInt32(width, 0) * BitConverter.ToInt32(height, 0) * 2 / BitConverter.ToInt32(height, 0);
                                if (kratnost != BitConverter.ToInt32(checkKratnost, 0))
                                {
                                    c += 5;
                                    tex_type = BitConverter.GetBytes(TTG_Tools.AutoPacker.tex_format[c].type_num);
                                }
                            }
                            break;
                        }
                    }

                    int ddsContentLength = 0;
                    if ((BitConverter.ToInt32(tex_type, 0) == 0x00) || (BitConverter.ToInt32(tex_type, 0) == 0x04) ||
                        (BitConverter.ToInt32(tex_type, 0) == 0x70)) ddsContentLength = ddsContent.Length - 148;
                    else ddsContentLength = ddsContent.Length - 128;

                    poz = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 19;
                    if (versionOfGame == "TFTB") poz = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 23;

                    Array.Copy(ddsContent, 28, mipmap, 0, 4);
                    Array.Copy(mipmap, 0, d3dtxContent, poz, 4);
                    poz += 4;
                    Array.Copy(width, 0, d3dtxContent, poz, 4);
                    poz += 4;
                    Array.Copy(height, 0, d3dtxContent, poz, 4);
                    poz += 4;
                    Array.Copy(tex_type, 0, d3dtxContent, poz, 4);

                    if (versionOfGame == "PN2") poz += 52;
                    if (versionOfGame == "WAU") poz += 56;
                    else if (versionOfGame == "TFTB") poz += 68;

                    Array.Copy(mipmap, 0, d3dtxContent, poz, 4);
                    poz += 8;

                    byte[] ddsContentLengthBin = new byte[4];
                    int ddsKratnost = 0;
                    byte[] ddsKratnostBin = new byte[4];

                    ddsContentLengthBin = BitConverter.GetBytes(ddsContentLength);
                    Array.Copy(ddsContentLengthBin, 0, d3dtxContent, poz, 4);
                    if (versionOfGame != "PN2") Array.Copy(ddsContentLengthBin, 0, d3dtxContent, 12, 4);
                    poz += 4;

                    if (versionOfGame == "TFTB") poz += 4;
                    int mipmapTable = 0;

                    if (versionOfGame == "PN2") mipmapTable = 12 * BitConverter.ToInt32(mipmap, 0);
                    else if (versionOfGame == "WAU") mipmapTable = 16 * BitConverter.ToInt32(mipmap, 0);
                    else if (versionOfGame == "TFTB") mipmapTable = 20 * BitConverter.ToInt32(mipmap, 0) - 4;

                    byte[] newD3dtxHeader = new byte[poz + mipmapTable];
                    Array.Copy(d3dtxContent, 0, newD3dtxHeader, 0, poz);

                    int[] contentChpater = new int[BitConverter.ToInt32(mipmap, 0)];
                    int[] kratnostChapter = new int[BitConverter.ToInt32(mipmap, 0)];

                    ddsContentLength = BitConverter.ToInt32(width, 0) * BitConverter.ToInt32(height, 0);
                    ddsKratnost = BitConverter.ToInt32(width, 0) * BitConverter.ToInt32(height, 0);

                    switch (BitConverter.ToInt32(tex_type, 0))
                    {
                        case 0x00:
                            ddsContentLength *= 4;
                            ddsKratnost = ddsKratnost * 4 / BitConverter.ToInt32(height, 0);
                            break;

                        case 0x02:
                        case 0x50:
                            ddsContentLength /= 4;
                            ddsKratnost = ddsKratnost * 2 / BitConverter.ToInt32(height, 0);
                            break;

                        case 0x04:
                            ddsContentLength *= 2;
                            ddsKratnost = ddsKratnost * 2 / BitConverter.ToInt32(height, 0);
                            break;

                        case 0x10:
                            ddsKratnost /= BitConverter.ToInt32(height, 0);
                            break;

                        case 0x25:
                            ddsContentLength *= 4 * 4;
                            ddsKratnost = ddsKratnost * 4 * 4 / BitConverter.ToInt32(height, 0);
                            break;

                        case 0x51:
                        case 0x40:
                        case 0x70:
                            ddsContentLength /= 2;
                            ddsKratnost = ddsKratnost * 2 / BitConverter.ToInt32(height, 0);
                            break;

                        case 0x42:
                            ddsKratnost = ddsKratnost * 4 / BitConverter.ToInt32(height, 0);
                            break;
                    }

                    for (int v = 0; v < BitConverter.ToInt32(mipmap, 0); v++)
                    {
                        contentChpater[v] = ddsContentLength;
                        kratnostChapter[v] = ddsKratnost;
                        ddsContentLength = ddsContentLength / 4;
                        ddsKratnost = ddsKratnost / 2;
                    }

                    for (int x = BitConverter.ToInt32(mipmap, 0) - 1; x >= 0; x--)
                    {
                        Array.Copy(BitConverter.GetBytes(x), 0, newD3dtxHeader, poz, 4);
                        poz += 4;

                        if (versionOfGame == "WAU" || versionOfGame == "TFTB")
                        {
                            Array.Copy(BitConverter.GetBytes(1), 0, newD3dtxHeader, poz, 4);
                            poz += 4;
                        }

                        ddsContentLengthBin = BitConverter.GetBytes(contentChpater[x]);
                        Array.Copy(ddsContentLengthBin, 0, newD3dtxHeader, poz, 4);
                        poz += 4;

                        ddsKratnostBin = BitConverter.GetBytes(kratnostChapter[x]);
                        Array.Copy(ddsKratnostBin, 0, newD3dtxHeader, poz, 4);
                        poz += 4;

                        if (versionOfGame == "TFTB")
                        {
                            if (x != 0)
                            {
                                poz += 4;
                            }
                        }
                    }

                    if (versionOfGame != "PN2")
                    {
                        ulong blockSize = (ulong)newD3dtxHeader.Length - 92;
                        byte[] binBlockSize = new byte[4];
                        binBlockSize = BitConverter.GetBytes(blockSize);
                        Array.Copy(binBlockSize, 0, newD3dtxHeader, 4, 4);
                    }

                    fs.Write(newD3dtxHeader, 0, newD3dtxHeader.Length);

                    newD3dtxHeader = new byte[ddsContentLength];

                    int pozFromEnd = ddsContent.Length;

                    for (int y = BitConverter.ToInt32(mipmap, 0) - 1; y >= 0; y--)
                    {
                        pozFromEnd -= contentChpater[y];
                        int contentSize = contentChpater[y];
                        byte[] contentChapterBin = new byte[contentSize];
                        Array.Copy(ddsContent, pozFromEnd, contentChapterBin, 0, contentSize);
                        fs.Write(contentChapterBin, 0, contentChapterBin.Length);
                    }
                    ReportForWork("File " + inputFiles[i].Name + " has some differents, but it repacked!");
                    fs.Close();
                    #endregion
                }
                else
                {
                    
                    //ReportForWork("File: " + inputFiles[i].Name + " CAN'T BE PACKED! CHECK DDS-TEXTURE!"); //Иначе выйдет сообщение об ошибке
                }
            }*/
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
                    goto lonec;
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
                if ((AutoPacker.tsvFile == false) && fileDestination[j].Extension == ".txt") AutoPacker.ImportTXT(fileDestination[j].FullName, ref all_text, false, MainMenu.settings.ASCII_N, "\n", ref error);
                else if ((AutoPacker.tsvFile == true) && (fileDestination[j].Extension == ".tsv")) AutoPacker.ImportTSV(fileDestination[j].FullName, ref all_text, "\\n", ref error);

                if (error == string.Empty)
                {
                    for (int q = 0; q < all_text.Count; q++)
                    {
                       /* int number = -1;
                        if (MainMenu.settings.exportRealID)
                        {
                            for (int w = 0; w < database.Count(); w++)
                            {
                                if (database[w].hz_data != null)
                                {
                                    Int64 b = BitConverter.ToInt64(database[w].hz_data, 0);
                                    if (b == all_text[q].realId)
                                    {
                                        number = w;
                                        break;
                                    }
                                }
                                else 
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            number = all_text[q].number - 1;
                        }
                        if (number > -1)
                        {*/
                            if (MainMenu.settings.importingOfName == true)
                            {
                                database[all_text[q].number - 1].name = all_text[all_text[q].number - 1].name;
                                database[all_text[q].number - 1].lenght_of_name = BitConverter.GetBytes(database[all_text[q].number - 1].name.Length);
                            }

                            //MessageBox.Show("database: " + database[all_text[q].number - 1].text);
                            if(AutoPacker.tsvFile == false) database[all_text[q].number - 1].text = all_text[q].text.Replace("\r\n", "\n");
                            else database[all_text[q].number - 1].text = all_text[q].text;
                            database[all_text[q].number - 1].lenght_of_text = BitConverter.GetBytes(database[all_text[q].number - 1].text.Length);
                        //}

                    }
                    Methods.DeleteCurrentFile(pathForFinalFile);
                    AutoPacker.CreateLangdb(database, version, pathForFinalFile);
                    ReportForWork("File: " + fileDestination[j].Name + " imported in " + inputFiles[i].Name);

                    if ((versionOfGame == " ") && AutoPacker.encLangdb == true)
                    {
                        byte[] encKey;
                        if (AutoPacker.custKey) encKey = Methods.stringToKey(AutoPacker.customKey);
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

                    /*if (versionOfGame != " ")
                    {
                        fs = new FileStream(pathForFinalFile, FileMode.Open);
                        byte[] langdb = Methods.ReadFull(fs);
                        fs.Close();
                        Methods.PackFile(MainMenu.settings.pathForOutputFolder, MainMenu.settings.pathForTempFolder, inputFiles[i].Name, langdb, versionOfGame, MainMenu.settings.pathForTtarchext);
                        ReportForWork("File " + inputFiles[i].Name + " packed");
                    }*/
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

        lonec:
            int konec = 0;
        }
        //Экспорт
        public void DoExportEncoding(object parametres)
        {
            List<string> param = parametres as List<string>;
            string destinationForExport = param[0];
            string pathInput = param[1];
            string pathOutput = param[2];
            //string pathTemp = param[3];
            string versionOfGame = param[3];

            if (Directory.Exists(pathInput) && Directory.Exists(pathOutput))
            {
                DirectoryInfo dir = new DirectoryInfo(pathInput);
                FileInfo[] inputFiles = dir.GetFiles('*' + destinationForExport);
                for (int i = 0; i < inputFiles.Length; i++)
                {
                    int lenghtOfExtension = inputFiles[i].Extension.Length;
                    string fileName = inputFiles[i].Name.Remove(inputFiles[i].Name.Length - lenghtOfExtension, lenghtOfExtension) + ".txt";
                    if (AutoPacker.tsvFile) fileName = inputFiles[i].Name.Remove(inputFiles[i].Name.Length - lenghtOfExtension, lenghtOfExtension) + ".tsv";

                    switch (destinationForExport)
                    {
                        case ".langdb":
                            {
                                ExportTXTfromLANGDB(inputFiles, i, pathOutput, fileName, versionOfGame);
                                break;
                            }
                        case ".d3dtx":
                            {
                                AutoPacker.ExportDDSfromD3DTX(inputFiles, i, pathOutput, fileName);
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
                        goto error;
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
                            Int32 realID = BitConverter.ToInt32(database[q].realID, 0);
                            all_text.Add(new TextCollector.TXT_collection((q + 1), realID, database[q].name, database[q].text, false));
                        }
                    }

                    TextCollector.CreateExportingTXTfromOneFile(all_text, ref allTextForExport);

                    //allTextForExport = all_text;
                    FileStream MyExportStream = new FileStream(pathOutput + "\\" + fileName, FileMode.OpenOrCreate);
                    int w = 0;

                    if (AutoPacker.tsvFile == false)
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

            error:
            int konez = 0;
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
