﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace TTG_Tools
{
    class Methods
    {
        public static bool IsNumeric(string str)
        {
            try
            {
                Int64 z = Convert.ToInt64(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void get_kratnost_and_size(int width, int height, int code, ref int ddsContentLength, ref int kratnost)
        {
            kratnost = width * height;
            ddsContentLength = width * height;

            switch (code)
            {
                case 0x00: //8888 RGBA
                    ddsContentLength *= 4;
                    kratnost = kratnost * 4 / height;
                    break;

                case 0x02: //PVRTC 2bpp
                case 0x50:
                    ddsContentLength /= 4;
                    kratnost = kratnost * 2 / height;
                    break;

                case 0x04: //4444
                    ddsContentLength *= 2;
                    kratnost = kratnost * 2 / height;
                    break;

                case 0x10: //Alpha 8 bit
                    kratnost /= height;
                    break;

                case 0x25: //32f.32f.32f.32f
                    ddsContentLength *= 4 * 4;
                    kratnost = kratnost * 4 * 4 / height;
                    break;

                case 0x53: //pvrtc 4bpp
                case 0x51: //pvrtc 4bpp
                case 0x40: //DXT1
                case 0x43: //BC4
                case 0x70: //ETC1
                    ddsContentLength /= 2;
                    kratnost = kratnost * 2 / height;
                    break;

                case 0x42: //DXT5
                case 0x44: //BC5
                    kratnost = kratnost * 4 / height;
                    break;
            }
        }

        public static string GetNameOfFileOnly(string name, string del)
        {
            return name.Replace(del, string.Empty);
        }

        public static byte[] stringToKey(string key) //Конвертация строки с hex-значениями в байты
        {
            byte[] result = null;

            if((key.Length % 2) == 0) //Проверка на чётность строки
            {
                for (int i = 0; i < key.Length; i++) //Проверка на наличие пробелов
                {
                    if (key[i] == ' ')
                    {
                        return null;
                    }
                }

                result = new byte[key.Length / 2];

                for (int i = 0; i < key.Length; i += 2) //Попытки преобразовать строку в массив байт
                {
                    bool remake = byte.TryParse(key.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null as IFormatProvider, out result[i / 2]);

                    if (remake == false) //Если что-то пошло не так, то очистим массив байт и вернём null
                    {
                        return null;
                    }
                }

            }
            
            return result;
        }

        public static string FindingDecrytKey(byte[] bytes, string TypeFile, ref byte[] KeyEnc) //Ищем ключ расшифровки для файлов langdb, dlog и d3dtx
        {
            string result = null;
            byte[] decKey;

            byte[] CheckVersion = new byte[4];
            Array.Copy(bytes, 4, CheckVersion, 0, 4);

            if ((BitConverter.ToInt32(CheckVersion, 0) < 0) || (BitConverter.ToInt32(CheckVersion, 0) > 6))
            {
                for (int a = 0; a < MainMenu.gamelist.Count; a++)
                {
                    byte[] CheckVerOld = CheckVersion; //Старый метод шифрования (Версии 2-6)
                    byte[] tempFileOld = new byte[bytes.Length]; //Временный файл для расшифровки (старый метод шифрования)
                    byte[] CheckVerNew = CheckVersion; //Поновее метод шифрования (Версии 7-9)
                    byte[] tempFileNew = new byte[bytes.Length];

                    decKey = MainMenu.gamelist[a].key;

                    Array.Copy(bytes, 0, tempFileOld, 0, bytes.Length);
                    Array.Copy(bytes, 0, tempFileNew, 0, bytes.Length);

                    if (((BitConverter.ToInt32(CheckVerOld, 0) < 0) || BitConverter.ToInt32(CheckVerOld, 0) > 6)
                        || (BitConverter.ToInt32(CheckVerNew, 0) < 0) || (BitConverter.ToInt32(CheckVerNew, 0) > 6))
                    {
                        Methods.meta_crypt(tempFileOld, decKey, 2, true);
                        CheckVerOld = new byte[4];
                        Array.Copy(tempFileOld, 4, CheckVerOld, 0, 4);
                        Methods.meta_crypt(tempFileNew, decKey, 7, true);
                        CheckVerNew = new byte[4];
                        Array.Copy(tempFileNew, 4, CheckVerNew, 0, 4);
                    }

                    if ((BitConverter.ToInt32(CheckVerOld, 0) > 0) && (BitConverter.ToInt32(CheckVerOld, 0) < 6))
                    {
                        Array.Copy(tempFileOld, 0, bytes, 0, bytes.Length);

                        if (TypeFile == "texture" || TypeFile == "font")
                        {
                            int TexturePosition = -1;
                            if (FindStartOfStringSomething(bytes, 4, "DDS ") > bytes.Length - 100)
                            {
                                if (TypeFile == "texture") TexturePosition = FindStartOfStringSomething(bytes, 4, ".d3dtx") + 6;
                                else TexturePosition = FindStartOfStringSomething(bytes, 4, ".tga") + 4;


                                int DDSPos = meta_find_encrypted(bytes, "DDS ", TexturePosition, decKey, 2);
                                byte[] tempHeader = new byte[2048];
                                if (tempHeader.Length > bytes.Length - DDSPos) tempHeader = new byte[bytes.Length - DDSPos];

                                Array.Copy(bytes, DDSPos, tempHeader, 0, tempHeader.Length);
                                BlowFishCS.BlowFish decHeader = new BlowFishCS.BlowFish(decKey, 2);
                                tempHeader = decHeader.Crypt_ECB(tempHeader, 2, true);
                                Array.Copy(tempHeader, 0, bytes, DDSPos, tempHeader.Length);
                            }
                        }

                        result = "Decryption key: " + MainMenu.gamelist[a].gamename + ". Blowfish type: old (versions 2-6)";
                        KeyEnc = MainMenu.gamelist[a].key;
                        break;
                    }
                    else if ((BitConverter.ToInt32(CheckVerNew, 0) > 0) && (BitConverter.ToInt32(CheckVerNew, 0) < 6))
                    {
                        Array.Copy(tempFileNew, 0, bytes, 0, bytes.Length);

                        if (TypeFile == "texture" || TypeFile == "font")
                        {
                            int TexturePosition = -1;
                            if (FindStartOfStringSomething(bytes, 4, "DDS ") > bytes.Length - 100)
                            {
                                if (TypeFile == "texture") TexturePosition = FindStartOfStringSomething(bytes, 4, ".d3dtx") + 6;
                                else TexturePosition = FindStartOfStringSomething(bytes, 4, ".tga") + 4;

                                int DDSPos = meta_find_encrypted(bytes, "DDS ", TexturePosition, decKey, 7);
                                byte[] tempHeader = new byte[2048];
                                if (tempHeader.Length > bytes.Length - DDSPos) tempHeader = new byte[bytes.Length - DDSPos];

                                Array.Copy(bytes, DDSPos, tempHeader, 0, tempHeader.Length);
                                BlowFishCS.BlowFish decHeader = new BlowFishCS.BlowFish(decKey, 7);
                                tempHeader = decHeader.Crypt_ECB(tempHeader, 7, true);
                                Array.Copy(tempHeader, 0, bytes, DDSPos, tempHeader.Length);
                            }
                        }

                        result = "Decryption key: " + MainMenu.gamelist[a].gamename + ". Blowfish type: new (versions 7-9)";
                        KeyEnc = MainMenu.gamelist[a].key;
                        break;
                    }
                }
            }
            else //Проверка файлов, в которых зашифрован только заголовок DDS-текстуры
            {
                try
                {
                    if (TypeFile == "texture" || TypeFile == "font")
                    {
                        int DDSstart = -1;
                        //Пока что придумал сделать авторасшифровку для одиночных текстур. Да и вроде шрифты были зашифрованы с 1 текстурой.
                        if (TypeFile == "texture") DDSstart = FindStartOfStringSomething(bytes, 4, ".d3dtx") + 6;
                        else DDSstart = FindStartOfStringSomething(bytes, 4, ".tga") + 4;

                        for (int i = 0; i < MainMenu.gamelist.Count; i++)
                        {
                            int DDSPos = meta_find_encrypted(bytes, "DDS ", DDSstart, MainMenu.gamelist[i].key, 2);

                            if ((DDSPos != -1) && (DDSPos < (bytes.Length - 100)))
                            {
                                byte[] tempHeader = new byte[2048];
                                if (tempHeader.Length > bytes.Length - DDSPos) tempHeader = new byte[bytes.Length - DDSPos];

                                Array.Copy(bytes, DDSPos, tempHeader, 0, tempHeader.Length);
                                BlowFishCS.BlowFish decHeader = new BlowFishCS.BlowFish(MainMenu.gamelist[i].key, 2);
                                tempHeader = decHeader.Crypt_ECB(tempHeader, 2, true);
                                Array.Copy(tempHeader, 0, bytes, DDSPos, tempHeader.Length);
                                DDSstart = DDSPos;

                                result = "Decryption key: " + MainMenu.gamelist[i].gamename + ". Blowfish type: old (versions 2-6)";
                                KeyEnc = MainMenu.gamelist[i].key;
                            }

                        }

                        if (DDSstart == -1)
                        {
                            for (int i = 0; i < MainMenu.gamelist.Count; i++)
                            {
                                int DDSPos = meta_find_encrypted(bytes, "DDS ", DDSstart, MainMenu.gamelist[i].key, 7);

                                if ((DDSPos != -1) && (DDSPos < (bytes.Length - 100)))
                                {
                                    byte[] tempHeader = new byte[2048];
                                    if (tempHeader.Length > bytes.Length - DDSPos) tempHeader = new byte[bytes.Length - DDSPos];

                                    Array.Copy(bytes, DDSPos, tempHeader, 0, tempHeader.Length);
                                    BlowFishCS.BlowFish decHeader = new BlowFishCS.BlowFish(MainMenu.gamelist[i].key, 7);
                                    tempHeader = decHeader.Crypt_ECB(tempHeader, 7, true);
                                    Array.Copy(tempHeader, 0, bytes, DDSPos, tempHeader.Length);
                                    DDSstart = DDSPos;

                                    result = "Decryption key: " + MainMenu.gamelist[i].gamename + ". Blowfish type: new (versions 7-9)";
                                    KeyEnc = MainMenu.gamelist[i].key;
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    result = "Error " + ex.Message;
                }
            }

            return result;
        }

        public static string GetChaptersOfDDS(byte[] d3dtx, int poz, List<byte[]> head, List<TTG_Tools.AutoPacker.chapterOfDDS> chaptersOfDDS, string version)
        {
            //теперь разбираем 
                                string s_temp = "";
            //Волк среди нас и Ходячие 2: 23 шт.
            //Покер 2: 22 шт.
            //Борда и ИП: 27 шт.
                                int countHeadByte = 23;
                                if (version == "TFTB" || version == "WDM")
                                {
                                    countHeadByte = 27;
                                }
                                else if (version == "PN2")
                                {
                                    countHeadByte = 22;
                                }
                                else if (version == "Batman")
                                {
                                    countHeadByte = 32;
                                }

                                for (int q = 0; q < countHeadByte; q++)//25
                                {
                                    byte[] temp = new byte[4];
                                    Array.Copy(d3dtx, poz, temp, 0, 4);
                                    head.Add(temp);
                                    poz += 4;
                                    s_temp += BitConverter.ToInt32(temp, 0).ToString() + "\r\n";
                                }

                                int kolvo;
                                if (version == "TFTB" || version == "WDM")
                                {
                                   kolvo  = BitConverter.ToInt32(head[24], 0);
                                }
                                else if (version == "PN2")
                                {
                                    kolvo = BitConverter.ToInt32(head[19], 0);
                                }
                                else if (version == "Batman")
                                {
                                    kolvo = BitConverter.ToInt32(head[29], 0);
                                }
                                else kolvo = BitConverter.ToInt32(head[20],0);//найти смещение для количества текстур и подумать насчет 27

                                //System.Windows.Forms.MessageBox.Show(Convert.ToString(kolvo));
                                //Смещения для количества текстур в играх:
                                //Покер 2 - 19
                                //Волк среди нас - 20
                                //Борда - 24

                                for (int k = 0; k < kolvo; k++)
                                {
                                    List<byte[]> bTemp = new List<byte[]>();
                                    int countHeadByteChapterDDS = 4; //Волк среди нас
                                    if (version == "TFTB" || version == "WDM")
                                    {
                                        countHeadByteChapterDDS = 5; //Борда
                                    }
                                    else if (version == "PN2")
                                    {
                                        countHeadByteChapterDDS = 3; //Покер 2
                                    }
                                    else if (version == "Batman")
                                    {
                                        countHeadByteChapterDDS = 6; //Бэтмен
                                    }


                                    for (int b = 0; b < countHeadByteChapterDDS; b++)
                                    {
                                        byte[] t = new byte[4];
                                        Array.Copy(d3dtx,poz,t,0,4);
                                        bTemp.Add(t);
                                        poz+=4;
                                    }
                                    
                                    if (version == "TFTB" || version == "WDM" )
                                    {
                                        chaptersOfDDS.Add(new TTG_Tools.AutoPacker.chapterOfDDS(bTemp[0], bTemp[1], bTemp[3], bTemp[2], null, bTemp[4]));
                                    }
                                    else if (version == "PN2")
                                    {
                                        chaptersOfDDS.Add(new TTG_Tools.AutoPacker.chapterOfDDS(bTemp[0], null, bTemp[1], bTemp[2], null, null));
                                    }
                                    else if (version == "Batman")
                                    {
                                        chaptersOfDDS.Add(new TTG_Tools.AutoPacker.chapterOfDDS(bTemp[1], bTemp[2], bTemp[3], bTemp[2], null, bTemp[5]));
                                    }
                                    else
                                    {
                                        chaptersOfDDS.Add(new TTG_Tools.AutoPacker.chapterOfDDS(bTemp[0], bTemp[1], bTemp[2], bTemp[3], null, null));
                                    }
                                }
                                
                                for (int k = 0; k < kolvo; k++)
                                {
                                    int len = BitConverter.ToInt32(chaptersOfDDS[k].lenght_of_chapter, 0);
                                    chaptersOfDDS[k].content_chapter = new byte[len];
                                    Array.Copy(d3dtx, poz, chaptersOfDDS[k].content_chapter, 0, len);
                                    poz += len;
                                }
                                return "\r\n" + s_temp + "                                                        ";
        }

        public static byte[] ReadFull(Stream stream)
        {
            byte[] buffer = new byte[3207];

            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
       
        public static string ConvertHexToString(byte[] array, int poz, int len_string, int ASCII_N, int UnicodeNum)
        {
            try
            {
                byte[] temp_hex_string = new byte[len_string];
                Array.Copy(array, poz, temp_hex_string, 0, len_string);

                for (int i = 0; i < temp_hex_string.Length; i++)
                {
                    if (temp_hex_string[i] == 0x00 && i == temp_hex_string.Length - 1)
                    {
                        UnicodeNum = 1;//Unicode = false;
                    }
                    else if (temp_hex_string[i] == 0x00 && i <= temp_hex_string.Length - 1)
                    {
                        UnicodeNum = MainMenu.settings.unicodeSettings;
                        break;
                    }
                }

                string result;
                if (MainMenu.settings.unicodeSettings != 1) result = UnicodeEncoding.UTF8.GetString(temp_hex_string);
                else result = ASCIIEncoding.GetEncoding(ASCII_N).GetString(temp_hex_string);
                return result;
            }
            catch
            { return "error"; }
        }

        public static void DeleteCurrentFile(string path)
        {
            try
            {
                System.IO.File.Delete(path);
            }
            catch { }
        }

      

        public static bool MakePause()
        {
            int start = GetTime() + 2500;
            int i = 0;
            while (start > GetTime())
            {
                i++;
            }
            return true;
        }

        static int GetTime()
        {
            DateTime time = DateTime.Now;
            return (((time.Hour * 60 + time.Minute) * 60 + time.Second) * 1000 + time.Millisecond);
        }

        public static int FindStartOfBinarySomething(byte[] array, int offset, byte[] something)
        {
            int poz = offset;
            byte[] tmp = new byte[something.Length];

            while(BitConverter.ToString(tmp) != BitConverter.ToString(something))
            {
                tmp = new byte[something.Length];
                Array.Copy(array, poz, tmp, 0, tmp.Length);
                poz++;

                if(poz + something.Length + 1 > array.Length)
                {
                    break;
                }
            }

            poz--;

            return poz;
        }

        public static int FindStartOfStringSomething(byte[] array, int offset, string string_something)
        {
            int poz = offset;
            while (Methods.ConvertHexToString(array, poz, string_something.Length, MainMenu.settings.ASCII_N, 1) != string_something)
            {
                poz++;
                if (Methods.ConvertHexToString(array, poz, string_something.Length, MainMenu.settings.ASCII_N, 1) == string_something)
                {
                    return poz;
                }
                if ((poz + string_something.Length + 1) > array.Length)
                {
                    break;
                }
            }
            return poz;
        }

        public static byte[] decryptLua(byte[] luaContent, byte[] key, int version)
        {
            byte[] headerCheck = new byte[4];
            Array.Copy(luaContent, 0, headerCheck, 0, 4);
            BlowFishCS.BlowFish decLuaNew = new BlowFishCS.BlowFish(key, 7);
            byte[] tempLua;


            switch (Encoding.ASCII.GetString(headerCheck))
            {
                case "\x1bLEn":
                    tempLua = new byte[luaContent.Length - 4];
                    Array.Copy(luaContent, 4, tempLua, 0, luaContent.Length - 4);
                    byte[] luaHeader = { 0x1B, 0x4C, 0x75, 0x61 }; //.Lua - начало заголовка

                    tempLua = decLuaNew.Crypt_ECB(tempLua, 7, true);
                    Array.Copy(luaHeader, 0, luaContent, 0, 4);
                    Array.Copy(tempLua, 0, luaContent, 4, tempLua.Length);
                    break;
                case "\x1bLEo":
                    tempLua = new byte[luaContent.Length - 4];
                    Array.Copy(luaContent, 4, tempLua, 0, luaContent.Length - 4);

                    tempLua = decLuaNew.Crypt_ECB(tempLua, 7, true);
                    luaContent = new byte[tempLua.Length];
                    Array.Copy(tempLua, 0, luaContent, 0, luaContent.Length);
                    break;
                default:
                    BlowFishCS.BlowFish decLua = new BlowFishCS.BlowFish(key, version);
                    luaContent = decLua.Crypt_ECB(luaContent, version, true);
                    break;
            }

            return luaContent;
        }

        public static int meta_find_encrypted(byte[] binContent, string NeedData, int pos, byte[] DecKey, int version) //Для поиска в зашифрованных данных (обычно для поиска текстур)
        {
            int bufsz = 128; //Размер буфера по умолчанию
            int result = 0;
            int Max_scan_size = 2048; //Проверка только определённого участка

            bool IsFinding = true;
            
            BlowFishCS.BlowFish decBuf = new BlowFishCS.BlowFish(DecKey, version);


            if (pos > binContent.Length - 4) pos = 4; //Начинать поиск после заголовка файла

            while (IsFinding)
            {
                byte[] buffer = new byte[bufsz];
                buffer = new byte[bufsz];
                if (buffer.Length > binContent.Length - pos)
                {
                    bufsz = binContent.Length - pos;
                    buffer = new byte[bufsz];
                }

                Array.Copy(binContent, pos, buffer, 0, bufsz);
                pos++;
                Max_scan_size--;

                byte[] checkBuffer = decBuf.Crypt_ECB(buffer, version, true);

                    int bfPos = 0; //Позиция в blowfish
                    while (Methods.ConvertHexToString(checkBuffer, bfPos, NeedData.Length, MainMenu.settings.ASCII_N, 1) != NeedData)
                    {
                        bfPos++;
                        if (Methods.ConvertHexToString(checkBuffer, bfPos, NeedData.Length, MainMenu.settings.ASCII_N, 1) == NeedData)
                        {
                            result = bfPos + pos - 1;
                            IsFinding = false;
                        }
                        if ((bfPos + NeedData.Length + 1) > checkBuffer.Length)
                        {
                            break;
                        }
                    }

                

                    if ((pos >= binContent.Length) || (Max_scan_size < 0))
                    {
                        result = -1;
                        IsFinding = false;
                    }


            }

            return result;
        }

        public static int meta_crypt(byte[] file, byte[] key, int version_archive, bool decrypt)
        {
            uint file_type = 0;
            long i,
                block_size = 0,
                block_crypt = 0,
                block_clean = 0,
                blocks;

            int meta = 1;

            if (file.Length < 4) return (int)file_type;
            byte[] check_type = new byte[4];
            Array.Copy(file, 0, check_type, 0, 4);

            file_type = BitConverter.ToUInt32(check_type, 0);

            uint p = (uint)file.Length;
            uint l = p + (uint)file.Length;

            /*
            block_size,
          * block_crypt
          * blocks_clean
            */
            switch (file_type)
            {
                case 0x4D545245: meta = 0; break; //ERTM
                case 0x4D42494E: meta = 0; break; //NIBM
                case 0xFB4A1764: block_size = 0x80; block_crypt = 0x20; block_clean = 0x50; break;
                case 0xEB794091: block_size = 0x80; block_crypt = 0x20; block_clean = 0x50; break;
                case 0x64AFDEFB: block_size = 0x80; block_crypt = 0x20; block_clean = 0x50; break;
                case 0x64AFDEAA: block_size = 0x100; block_crypt = 0x8; block_clean = 0x18; break;
                case 0x4D424553: block_size = 0x40; block_crypt = 0x40; block_clean = 0x64; break; //SEBM
                default: meta = 0; break;
            }

            if (block_size != 0)
            {
                blocks = (file.Length - 4) / block_size;
                long poz = 0;
                byte[] temp_file = new byte[file.Length - 4];
                Array.Copy(file, 4, temp_file, 0, temp_file.Length);
                

                for (i = 0; i < blocks; i++)
                {

                    byte[] block = new byte[block_size];
                    Array.Copy(temp_file, poz, block, 0, block_size);

                    if (p >= l) break;
                    if (i % block_crypt == 0)
                    {
                        BlowFishCS.BlowFish enc = new BlowFishCS.BlowFish(key, version_archive);
                        //block = enc.Crypt_ECB(block, version_archive, false);
                        block = enc.Crypt_ECB(block, version_archive, decrypt);
                        Array.Copy(block, 0, temp_file, poz, block.Length);
                    }
                    else if ((i % block_clean == 0) && (i > 0))
                    {
                        Array.Copy(block, 0, temp_file, poz, block.Length);
                    }
                    else
                    {
                        XorBlock(ref block, 0xff);
                        Array.Copy(block, 0, temp_file, poz, block.Length);
                    }

                    p += (uint)block_size;
                    poz += block_size;
                }

                Array.Copy(temp_file, 0, file, 4, temp_file.Length);
            }

            return meta;
        }

        private static void XorBlock(ref byte[] block, byte xor)
        {
            for (int i = 0; i < block.Length; i++)
            {
                block[i] ^= xor;
            }
        }

        public static byte[] encryptLua(byte[] luaContent, byte[] key, bool newEngine, int version)
        {   //newEngine - игры, выпущенные с Tales From the Borderlands и переизданные на новом движке
        
            BlowFishCS.BlowFish DoEncLua = new BlowFishCS.BlowFish(key, version);
            byte[] header = new byte[4];

            byte[] checkHeader = new byte[4];
            Array.Copy(luaContent, 0, checkHeader, 0, 4);

            if (Encoding.ASCII.GetString(checkHeader) == "\x1bLua")
            {
                if(newEngine == true)
                {
                        header = Encoding.ASCII.GetBytes("\x1bLEn");
                        byte[] tempLua = new byte[luaContent.Length - 4];
                        Array.Copy(luaContent, 4, tempLua, 0, luaContent.Length - 4);
                        tempLua = DoEncLua.Crypt_ECB(tempLua, 7, false);
                        Array.Copy(header, 0, luaContent, 0, 4);
                        Array.Copy(tempLua, 0, luaContent, 4, tempLua.Length);                 
                }
                else luaContent = DoEncLua.Crypt_ECB(luaContent, version, false);
            }
            else if ((Encoding.ASCII.GetString(checkHeader) != "\x1bLEn") && (Encoding.ASCII.GetString(checkHeader) != "\x1bLEo")
                && (Encoding.ASCII.GetString(checkHeader) != "\x1bLua"))
            {
                if(newEngine == true)
                {
                        header = Encoding.ASCII.GetBytes("\x1bLEo");
                        byte[] tempLua2 = new byte[luaContent.Length];
                        Array.Copy(luaContent, 0, tempLua2, 0, luaContent.Length);
                        tempLua2 = DoEncLua.Crypt_ECB(tempLua2, 7, false);

                        luaContent = new byte[tempLua2.Length + 4];
                        Array.Copy(header, 0, luaContent, 0, 4);
                        Array.Copy(tempLua2, 0, luaContent, 4, luaContent.Length - 4);
                }
                else luaContent = DoEncLua.Crypt_ECB(luaContent, version, false);
            }

            return luaContent;
        }

        public static string DeleteCommentary(string str, string start, string end)
        {
            int start_poz = str.IndexOf(start);
            if (start_poz > -1)
            {
                int end_poz = str.IndexOf(end);
                if ((end_poz > -1 && start_poz > -1) && (end_poz > start_poz))
                {
                    str = str.Remove(start_poz, (end_poz - start_poz + end.Length));
                }
            }
            return str;
        }
    }
}