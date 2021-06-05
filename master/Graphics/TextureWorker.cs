using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace TTG_Tools
{
    public class TextureWorker
    {
        public class TexData //класс для формата текстур
        {
            public byte[] sample; //Небольшой фрагмент из заголовка
            public int code; //код формата для шрифта
            public string tex_info;
            public int platform;
            public bool Is_iOS; //Проверка на конкретную платформу (Android на PVR или iOS)
            public bool PVR_header; //Пригодится для определения заголовков

            public TexData() { }
            public TexData(byte[] _sample, int _code, string _tex_info, int _platform, bool _PVR_header, bool _Is_iOS)
            {
                this.sample = _sample;
                this.code = _code;
                this.tex_info = _tex_info;
                this.platform = _platform;
                this.Is_iOS = _Is_iOS;
                this.PVR_header = _PVR_header;
            }
        }

        public static string ExtractTextures(string InputFile, string OutputDir)
        {
            string result = null;
            string additionalMessage = null;
            bool NewFormat = false;

            FileInfo fi = new FileInfo(InputFile);

            byte[] binContent = File.ReadAllBytes(InputFile);
            byte[] check_header = new byte[4];
            Array.Copy(binContent, 0, check_header, 0, check_header.Length);
            int poz = 4;

            if ((Encoding.ASCII.GetString(check_header) != "5VSM") && (Encoding.ASCII.GetString(check_header) != "ERTM")
            && (Encoding.ASCII.GetString(check_header) != "6VSM")) //Supposed this texture encrypted
            {
                //First trying decrypt probably encrypted font
                try
                {
                    string info = Methods.FindingDecrytKey(binContent, "texture");
                    if (info != null)
                    {
                        additionalMessage = "D3dtx file was encrypted, but I decrypted. " + info;
                    }
                }
                catch
                {
                    result = "Maybe that D3DTX file encrypted. Try to decrypt first: " + fi.Name;

                    return result;
                }
            }
            if ((Encoding.ASCII.GetString(check_header) == "5VSM") || (Encoding.ASCII.GetString(check_header) == "6VSM"))
            {
                poz = 16;
                NewFormat = true;
            }

            try
            {
                byte[] tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                int countElements = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[8];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);

                bool flags = false;
                bool someData = false;
                bool PossibleNewFlags = false;

                if (BitConverter.ToString(tmp) == "E2-CC-38-6F-7E-9E-24-3E")
                {
                    byte[][] elements = new byte[countElements][];

                    for(int i = 0; i < countElements; i++)
                    {
                        elements[i] = new byte[8];
                        Array.Copy(binContent, poz, elements[i], 0, elements[i].Length);
                        poz += 12;

                        if(BitConverter.ToString(elements[i]) == "41-16-D7-79-B9-3C-28-84")
                        {
                            flags = true;
                        }
                        
                        if(BitConverter.ToString(elements[i]) == "E3-88-09-7A-48-5D-7F-93")
                        {
                            someData = true;
                        }

                        if(BitConverter.ToString(elements[i]) == "0F-F4-20-E6-20-BA-A1-EF")
                        {
                            NewFormat = true;
                        }
                        if(BitConverter.ToString(elements[i]) == "D2-15-9F-6B-4F-DC-75-CD")
                        {
                            PossibleNewFlags = true;
                        }
                    }
                }
                else
                {
                    string[] elements = new string[countElements];
                    int lenStr;

                    for (int i = 0; i < countElements; i++)
                    {
                        tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        poz += 4;
                        lenStr = BitConverter.ToInt32(tmp, 0);
                        tmp = new byte[lenStr];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        poz += lenStr + 4; //Length element's name and 4 bytes data for Telltale Tool
                        elements[i] = Encoding.ASCII.GetString(tmp);

                        if (elements[i] == "class Flags")
                        {
                            flags = true;
                        }
                    }
                }

                if (!NewFormat)
                {
                    ClassesStructs.TextureClass.OldT3Texture oldTex = GetOldTextures(binContent, ref poz, flags, someData);

                    result = "File " + fi.Name + " successfully extracted. ";

                    if (oldTex == null)
                    {
                        result = "Something wrong with this file: " + fi.Name;
                        return result;
                    }

                    if (File.Exists(OutputDir + "\\" + fi.Name.Replace(".d3dtx", ".dds"))) File.Delete(OutputDir + "\\" + fi.Name.Replace(".d3dtx", ".dds"));
                    File.WriteAllBytes(OutputDir + "\\" + fi.Name.Replace(".d3dtx", ".dds"), oldTex.Content);

                    if (additionalMessage != null) result += additionalMessage;

                    return result;
                }

                uint tmpPoz = 0;
                
                ClassesStructs.TextureClass.NewT3Texture tex = GetNewTextures(binContent, ref poz, ref tmpPoz, flags, someData, false, ref additionalMessage);

                result = "File " + fi.Name + " successfully extracted. ";

                if (tex == null)
                {
                    result = "Something wrong with this file: " + fi.Name;
                    return result;
                }

                if (File.Exists(OutputDir + "\\" + fi.Name.Replace(".d3dtx", ".dds"))) File.Delete(OutputDir + "\\" + fi.Name.Replace(".d3dtx", ".dds"));
                File.WriteAllBytes(OutputDir + "\\" + fi.Name.Replace(".d3dtx", ".dds"), tex.Tex.Content);

                if (additionalMessage != null) result += additionalMessage;

                return result;
            }
            catch
            {
                return fi.Name + ": Unsupported format";
            }
        }

        public static ClassesStructs.TextureClass.OldT3Texture GetOldTextures(byte[] binContent, ref int poz, bool flags, bool someData)
        {
            //Try read begin of file
            try
            {
                ClassesStructs.TextureClass.OldT3Texture tex = new ClassesStructs.TextureClass.OldT3Texture();
                tex.TexFlags = null;

                if (flags) tex.TexFlags = new ClassesStructs.FlagsClass();

                if (someData)
                {
                    byte[] someBinData = new byte[4];
                    Array.Copy(binContent, poz, someBinData, 0, someBinData.Length);
                    tex.sizeBlock = BitConverter.ToInt32(someBinData, 0);
                    poz += 4;

                    someBinData = new byte[4];
                    Array.Copy(binContent, poz, someBinData, 0, someBinData.Length);
                    tex.someValue = BitConverter.ToInt32(someBinData, 0);
                    poz += 4;

                    someBinData = null;
                }

                byte[] tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                int nameLen = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                if (nameLen - BitConverter.ToInt32(tmp, 0) == 8)
                {
                    nameLen = BitConverter.ToInt32(tmp, 0);
                    poz += 4;
                }

                tmp = new byte[nameLen];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                poz += nameLen;
                tex.ObjectName = Encoding.ASCII.GetString(tmp);

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                nameLen = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                if (nameLen - BitConverter.ToInt32(tmp, 0) == 8)
                {
                    nameLen = BitConverter.ToInt32(tmp, 0);
                    poz += 4;
                }

                tmp = new byte[nameLen];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                poz += nameLen;
                tex.SubobjectName = Encoding.ASCII.GetString(tmp);

                tmp = new byte[8];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);

                int counter = 0;

                for (int k = 0; k < tmp.Length; k++)
                {
                    if ((tmp[k] == 0x30) || (tmp[k] == 0x31))
                    {
                        counter++;
                    }
                }

                tex.Flags = new byte[counter];
                Array.Copy(binContent, poz, tex.Flags, 0, tex.Flags.Length);
                poz += tex.Flags.Length;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.Mip = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                poz += 4;
                tex.TextureFormat = BitConverter.ToInt32(tmp, 0);

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.OriginalWidth = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.OriginalHeight = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                int lastPos = poz;

                //And just ignore some flags and etc. Just search DDS header
                poz = Methods.FindStartOfStringSomething(binContent, poz, "DDS ") - 4; //Get position for a size

                tex.block = new byte[poz - lastPos];
                Array.Copy(binContent, lastPos, tex.block, 0, tex.block.Length);

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.TexSize = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tex.Content = new byte[tex.TexSize];
                Array.Copy(binContent, poz, tex.Content, 0, tex.Content.Length);
                poz += tex.Content.Length;

                tmp = new byte[4];
                Array.Copy(tex.Content, 16, tmp, 0, tmp.Length);
                tex.Width = BitConverter.ToInt32(tmp, 0);

                tmp = new byte[4];
                Array.Copy(tex.Content, 12, tmp, 0, tmp.Length);
                tex.Height = BitConverter.ToInt32(tmp, 0);

                if ((tex.TexFlags != null) && (tex.TexFlags.TexSizes != null)) 
                {
                    for (int j = 0; j < tex.TexFlags.TexSizes.Length; j++)
                    {
                        tex.TexFlags.SubTexContent[j] = new byte[tex.TexFlags.TexSizes[j]];
                        Array.Copy(binContent, poz, tex.TexFlags.SubTexContent[j], 0, tex.TexFlags.SubTexContent[j].Length);
                        poz += tex.TexFlags.TexSizes[j];
                    }
                }

                return tex;
            }
            catch
            {
                return null;
            }
        }

        //Extract new format textures (Since Poker Night 2)
        public static ClassesStructs.TextureClass.NewT3Texture GetNewTextures(byte[] binContent, ref int poz, ref uint texFontPoz, bool flags, bool someData, bool IsFont, ref string AdditionalInfo)
        {
            ClassesStructs.TextureClass.NewT3Texture tex = new ClassesStructs.TextureClass.NewT3Texture();
            byte[] tmp = new byte[4];
            Array.Copy(binContent, 0, tmp, 0, tmp.Length);
            string checkHeader = Encoding.ASCII.GetString(tmp);

            if (checkHeader != "ERTM")
            {
                if (!IsFont)
                {
                    tmp = new byte[4];
                    Array.Copy(binContent, 4, tmp, 0, tmp.Length);
                    tex.headerSize = BitConverter.ToInt32(tmp, 0);

                    tmp = new byte[4];
                    Array.Copy(binContent, 12, tmp, 0, tmp.Length);
                    tex.textureSize = BitConverter.ToUInt32(tmp, 0);
                }
            }

            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.SomeValue = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.unknownFlags.blockSize = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.unknownFlags.block = BitConverter.ToUInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.platform.blockSize = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.platform.platform = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            int tmpInt = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tmpInt = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[tmpInt];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.ObjectName = Encoding.ASCII.GetString(tmp);
            poz += tmpInt;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tmpInt = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tmpInt = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[tmpInt];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.SubObjectName = Encoding.ASCII.GetString(tmp);
            poz += tmpInt;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.OneValue = BitConverter.ToSingle(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);

            if (checkHeader == "5VSM" && tex.SomeValue > 4)
            {
                tex.Zero = BitConverter.ToInt32(tmp, 0);
                poz += 4;
                tmp = new byte[1];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            }

            tex.OneByte = tmp[0];
            poz++;

            if(tex.OneByte == 0x31)
            {
                tex.UnknownData = new ClassesStructs.FlagsClass.SomeClassData();

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.UnknownData.count = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.UnknownData.Unknown1 = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.UnknownData.len = BitConverter.ToInt32(tmp, 0);

                tex.UnknownData.someData = new byte[tex.UnknownData.len];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);

                poz += tex.UnknownData.len;
            }

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.Mip = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.Width = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.Height = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            if(checkHeader == "6VSM" && tex.SomeValue == 9)
            {
                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.Faces = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.ArrayMembers = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                //if (tex.Surfaces > 1 || tex.Faces > 1) return null; //Need think about it!
            }

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.TextureFormat = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.Unknown1 = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            int blSize = 0x2c;

            switch (tex.SomeValue)
            {
                case 4:
                    blSize = 0x30;
                    break;

                case 5:
                    blSize = 0x3c;
                    break;
                case 7:
                    blSize = 0x40;
                    break;
                case 9:
                    blSize = 0x4c;
                    break;
            }

            tex.block = new byte[blSize];
            Array.Copy(binContent, poz, tex.block, 0, tex.block.Length);
            poz += tex.block.Length;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.Tex.MipCount = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.Tex.SomeData = BitConverter.ToInt32(tmp, 0);
            poz += 4;

            tmp = new byte[4];
            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
            tex.Tex.TexSize = BitConverter.ToUInt32(tmp, 0);
            poz += 4;

            tex.Tex.Textures = new ClassesStructs.TextureClass.NewT3Texture.TextureStruct[tex.Tex.MipCount];

            for(int i = tex.Tex.MipCount - 1; i >= 0; i--)
            {
                if(tex.SomeValue >= 5)
                {
                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    tex.Tex.Textures[i].Zero = BitConverter.ToInt32(tmp, 0);
                    poz += 4;
                }

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.Tex.Textures[i].CurrentMip = BitConverter.ToInt32(tmp, 0);
                poz += 4;


                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                if (BitConverter.ToInt32(tmp, 0) == 1)
                {
                    tex.Tex.Textures[i].One = BitConverter.ToInt32(tmp, 0);
                    poz += 4;

                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                }

                tex.Tex.Textures[i].MipSize = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                tmp = new byte[4];
                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                tex.Tex.Textures[i].BlockSize = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                if(checkHeader == "6VSM" && tex.SomeValue == 9)
                {
                    poz += 4; //Skip Mip size
                }
            }

            if(tex.Tex.SomeData > 0)
            {
                tex.Tex.SubBlocks = new ClassesStructs.TextureClass.NewT3Texture.SubBlock[tex.Tex.SomeData];

                for(int i = 0; i < tex.Tex.SomeData; i++)
                {
                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    tex.Tex.SubBlocks[i].Size = BitConverter.ToInt32(tmp, 0);

                    tex.Tex.SubBlocks[i].Block = new byte[tex.Tex.SubBlocks[i].Size];
                    Array.Copy(binContent, poz, tex.Tex.SubBlocks[i].Block, 0, tex.Tex.SubBlocks[i].Block.Length);
                    poz += tex.Tex.SubBlocks[i].Block.Length;
                }
            }

            string format = "";
            bool pvr = false;

            byte[] header = genHeader(tex.Width, tex.Height, tex.Mip, tex.TextureFormat, tex.platform.platform, ref pvr, ref format);

            if(header == null)
            {
                return null;
            }

            AdditionalInfo = "Texture format: " + format + ". Mip count: " + Convert.ToString(tex.Mip);

            if(checkHeader == "6VSM" && tex.SomeValue == 9)
            {
                AdditionalInfo += ". Faces: " + Convert.ToString(tex.Faces) + ". Array members: " + Convert.ToString(tex.ArrayMembers);
            }

            tex.Tex.Content = new byte[tex.Tex.TexSize + header.Length];
            Array.Copy(header, 0, tex.Tex.Content, 0, header.Length);
            int texPoz = tex.Tex.Content.Length;

            uint tmpPoz = (uint)poz;
            if (texFontPoz != 0) tmpPoz = texFontPoz;
            
            for(int i = tex.Mip - 1; i >= 0; i--)
            {
                texPoz -= tex.Tex.Textures[i].MipSize;
                tmp = new byte[tex.Tex.Textures[i].MipSize];
                Array.Copy(binContent, tmpPoz, tmp, 0, tmp.Length);
                tmpPoz += (uint)tex.Tex.Textures[i].MipSize;

                Array.Copy(tmp, 0, tex.Tex.Content, texPoz, tmp.Length);
            }

            tmp = new byte[4];
            Array.Copy(binContent, 0, tmp, 0, tmp.Length);
            if (Encoding.ASCII.GetString(tmp) == "ERTM") poz = (int)tmpPoz;
            else texFontPoz = tmpPoz;

            return tex;
        }

        //Всё для экспорта текстур
        public static byte[] extract_old_textures(byte[] binContent, byte[] key, int version, ref string result, ref bool pvr) //Разбор ресурсов древних версий движков Telltale Tool
        {
            byte[] check_header = new byte[4];
            Array.Copy(binContent, 0, check_header, 0, 4);
            byte[] GetVers = new byte[4]; //Пытаемся проверить версию
            Array.Copy(binContent, 4, GetVers, 0, 4);

            if (((BitConverter.ToInt32(GetVers, 0) > 6) || (BitConverter.ToInt32(GetVers, 0) < 0)) && 
               ((Encoding.ASCII.GetString(check_header) != "5VSM") && (Encoding.ASCII.GetString(check_header) != "ERTM")
               && (Encoding.ASCII.GetString(check_header) != "NIBM"))) //Предполагается, что файл зашифрован
            {
                int index = Methods.meta_find_encrypted(binContent, "DDS |", 0, key, version);

                if (index == -1)
                {
                    result = Methods.FindingDecrytKey(binContent, "texture");
                    if (result != null)
                    {
                        string temp = "File was decrypted! " + result;
                        result = temp;
                        pvr = false;
                    }
                }
                else
                {
                    Methods.meta_crypt(binContent, key, version, true);
                }
            }
            else if(((BitConverter.ToInt32(GetVers, 0) > 0) && (BitConverter.ToInt32(GetVers, 0) < 6)) //Если файл не зашифрован, но зашифрован заголовок текстуры
                && (Methods.FindStartOfStringSomething(binContent, 0, "DDS") > binContent.Length - 100
                && Methods.FindStartOfStringSomething(binContent, 0, "PVR!") > binContent.Length - 100))
            {
                int index = Methods.meta_find_encrypted(binContent, "DDS |", 0, key, version);

                if (index == -1)
                {
                    result = Methods.FindingDecrytKey(binContent, "texture");
                    if (result != null)
                    {
                        string temp = "File was decrypted! " + result;
                        result = temp;
                        pvr = false;
                    }
                }
                else
                {
                    byte[] tempHeader = new byte[2048];
                    if (tempHeader.Length > binContent.Length - index) tempHeader = new byte[binContent.Length - index];

                    Array.Copy(binContent, index, tempHeader, 0, tempHeader.Length);

                    BlowFishCS.BlowFish dec = new BlowFishCS.BlowFish(key, version);
                    tempHeader = dec.Crypt_ECB(tempHeader, version, true);
                    Array.Copy(tempHeader, 0, binContent, index, tempHeader.Length);
                }
            }

            //Проверки, которые были выше, нужны для проверки на зашифрованные файлы. Ниже как раз работа с текстурами

            if (Methods.FindStartOfStringSomething(binContent, 8, "DDS") < binContent.Length - 100) //Тупо копируем DDS текстуру и возвращаем её обратно для записи
            {
                int pos = Methods.FindStartOfStringSomething(binContent, 8, "DDS");
                byte[] tempContent = new byte[binContent.Length - pos];
                Array.Copy(binContent, pos, tempContent, 0, tempContent.Length);
                binContent = tempContent;
                pvr = false;

                return binContent;
            }
            else if (Methods.FindStartOfStringSomething(binContent, 8, "PVR!") < binContent.Length - 100) //Повозимся с разбором заголовка, а потом запишем всё в binContent
            {
                pvr = true;
                byte[] PVR_header = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x03,
                                      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                      0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                                      0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
                                      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; //Шаблон заголовка pvr-текстур

                byte[] width = new byte[4]; //Ширина текстуры
                byte[] height = new byte[4]; //Высота текстуры
                byte[] mip_map = new byte[2]; //Тупо, зато скопирует нужное количество мип-мапов
                byte[] tex_format = new byte[4]; //формат текстуры
                byte[] tex_length = new byte[4]; //Длина текстуры
                byte[] tex_header_format = new byte[8];

                int pos = Methods.FindStartOfStringSomething(binContent, 8, "PVR!") - 0x28; //Находим позицию для копирования данных

                Array.Copy(binContent, pos, height, 0, height.Length);
                Array.Copy(binContent, pos + 4, width, 0, width.Length);
                Array.Copy(binContent, pos + 8, mip_map, 0, mip_map.Length);
                Array.Copy(binContent, pos + 12, tex_format, 0, tex_format.Length);
                Array.Copy(binContent, pos + 16, tex_length, 0, tex_length.Length);
                
                Array.Copy(height, 0, PVR_header, 24, height.Length);
                Array.Copy(width, 0, PVR_header, 28, width.Length);
                mip_map = BitConverter.GetBytes(BitConverter.ToInt16(mip_map, 0) + 1);
                Array.Copy(mip_map, 0, PVR_header, 44, mip_map.Length);

                result = null;

                switch (BitConverter.ToInt32(tex_format, 0))
                {
                    case 0x8010: //4444 RGBA
                    case 0x8110: //4444 RGBA
                        result = "Texture Format: 4444 RGBA. ";
                        tex_header_format = BitConverter.GetBytes(0x404040461626772);
                        Array.Copy(tex_header_format, 0, PVR_header, 8, tex_header_format.Length);
                        break;
                    case 0x830d: //PVRTC 4bpp RGBA
                    case 0x820d: //PVRTC 4bpp RGBA
                    case 0x20d: //PVRTC 4bpp RGBA
                    case 0x8a0d: //PVRTC 4bpp RGBA
                    case 0x8b0d: //PVRTC 4bpp RGBA
                        result = "PVRTC 4bpp RGBA. ";
                        tex_header_format = BitConverter.GetBytes(3);
                        Array.Copy(tex_header_format, 0, PVR_header, 8, tex_header_format.Length);
                        break;
                    case 0xa0d: //PVRTC 4bpp RGB
                    case 0x30d: //PVRTC 4bpp RGB
                        result = "PVRTC 4bpp RGB. ";
                        tex_header_format = BitConverter.GetBytes(2);
                        Array.Copy(tex_header_format, 0, PVR_header, 8, tex_header_format.Length);
                        break;
                    case 0x13: //565 RGB
                    case 0x113: //565 RGB
                        result = "565 RGB. ";
                        tex_header_format = BitConverter.GetBytes(0x5060500626772);
                        Array.Copy(tex_header_format, 0, PVR_header, 8, tex_header_format.Length);
                        break;
                    case 0x8011:
                        result = "8888 RGBA. ";
                        tex_header_format = BitConverter.GetBytes(0x808080861626772);
                        Array.Copy(tex_header_format, 0, PVR_header, 8, tex_header_format.Length);
                        break;
                    default:
                        result = "Unknown format. Its code is " + BitConverter.ToInt32(tex_format, 0).ToString() + " ";
                        break;
                }

                if (BitConverter.ToInt32(mip_map, 0) <= 1) result += "No mip-maps.";
                else result += "Mip-maps count: " + BitConverter.ToInt32(mip_map, 0);

                pos += 0x30; //Смещаемся к самой текстуре

                byte[] tempContent = new byte[BitConverter.ToInt32(tex_length, 0)];
                Array.Copy(binContent, pos, tempContent, 0, tempContent.Length);

                binContent = new byte[PVR_header.Length + tempContent.Length]; //Копируем текстуру и возвращаем её AutoPacker'у
                Array.Copy(PVR_header, 0, binContent, 0, PVR_header.Length);
                Array.Copy(tempContent, 0, binContent, PVR_header.Length, tempContent.Length);

                return binContent;
            }
            else return null; //Иначе отправим ничего
        }

        public static byte[] getFontHeader(byte[] binContent, ref byte[] code, ref byte[] width, ref byte[] height, ref byte[] tex_size, ref byte[] tex_kratnost)
        {
            byte[] checkHeader = new byte[4];
            Array.Copy(binContent, 0, checkHeader, 0, checkHeader.Length);

            byte[] t_height = new byte[4];
            byte[] t_width = new byte[4];
            byte[] t_mip = new byte[4]; //Если кто-то засунет с несколькими мип-мапами текстуру, прога выдернет только 1

            bool pvr = false;
            int height_pos = 12;
            int width_pos = 16;
            int mip_pos = 28;
            int header_length = 128;
            int check_header_length = 28;
            int check_header_pos = 80;

            if (Encoding.ASCII.GetString(checkHeader) == "PVR\x03")
            {
                height_pos = 24;
                width_pos = 28;
                mip_pos = 44;
                header_length = 52;
                check_header_length = 8;
                check_header_pos = 8;
                pvr = true;
            }

            Array.Copy(binContent, height_pos, t_height, 0, t_height.Length);
            Array.Copy(binContent, width_pos, t_width, 0, t_width.Length);
            Array.Copy(binContent, mip_pos, t_mip, 0, t_mip.Length);

            int index = -1;

            for (int i = 0; i < MainMenu.texture_header.Count; i++)
            {
                if (header_length == MainMenu.texture_header[i].sample.Length)
                {
                    byte[] texHeader = new byte[check_header_length];
                    byte[] SampleHeader = new byte[check_header_length];

                    Array.Copy(binContent, check_header_pos, texHeader, 0, check_header_length);
                    Array.Copy(MainMenu.texture_header[i].sample, check_header_pos, SampleHeader, 0, check_header_length);

                    if (BitConverter.ToString(texHeader) == BitConverter.ToString(SampleHeader) && (TTG_Tools.MainMenu.texture_header[i].PVR_header == pvr))
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index != -1)
            {
                height = t_height;
                width = t_width;
                int tex_length = 0;
                int tex_krat = 0;
                Methods.get_kratnost_and_size(BitConverter.ToInt32(width, 0), BitConverter.ToInt32(height, 0), TTG_Tools.MainMenu.texture_header[index].code, ref tex_length, ref tex_krat);

                int tex_pos = 128; //Для DDS текстур
                
                if (pvr)
                {
                    byte[] GetMeta = new byte[4];
                    Array.Copy(binContent, 48, GetMeta, 0, GetMeta.Length);
                    tex_pos = 52 + BitConverter.ToInt32(GetMeta, 0);
                }

                code = BitConverter.GetBytes(TTG_Tools.MainMenu.texture_header[index].code);
                tex_size = BitConverter.GetBytes(tex_length);
                tex_kratnost = BitConverter.GetBytes(tex_krat);

                byte[] texContent = new byte[tex_length];
                Array.Copy(binContent, tex_pos, texContent, 0, texContent.Length);
                return texContent;
            }
            else return null;
        }

        //Generate header for PVR and DDS files (need destroy it and use from DDS directory!)
        public static byte[] genHeader(int width, int height, int mip, int code, int platform, ref bool pvr, ref string format)
        {
            byte[] binContent = null;

            int index = -1;
            byte[] tmp = null;

            for (int i = 0; i < TTG_Tools.MainMenu.texture_header.Count; i++)
            {
                if ((code == TTG_Tools.MainMenu.texture_header[i].code))
                {
                    if ((code == TTG_Tools.MainMenu.texture_header[i].code) &&
                        (platform == TTG_Tools.MainMenu.texture_header[i].platform) 
                        && (platform == 7 || platform == 9))
                    {
                        index = i;
                        pvr = true;
                    }
                    else if ((code == TTG_Tools.MainMenu.texture_header[i].code) &&
                        (platform == TTG_Tools.MainMenu.texture_header[i].platform)
                        && (platform != 7 && platform != 9))
                    {
                        index = i;
                        pvr = false;
                    }
                }
            }

            if (index != -1)
            {
                if (pvr)
                {
                    binContent = TTG_Tools.MainMenu.texture_header[index].sample;
                    tmp = BitConverter.GetBytes(height);
                    Array.Copy(tmp, 0, binContent, 24, tmp.Length);

                    tmp = BitConverter.GetBytes(width);
                    Array.Copy(tmp, 0, binContent, 28, tmp.Length);
                    format = TTG_Tools.MainMenu.texture_header[index].tex_info;

                    tmp = BitConverter.GetBytes(mip);
                    Array.Copy(tmp, 0, binContent, 44, tmp.Length);
                }
                else
                {
                    binContent = TTG_Tools.MainMenu.texture_header[index].sample;
                    int length = 0;
                    int kratnost = 0;
                    Methods.get_kratnost_and_size(width, height, TTG_Tools.MainMenu.texture_header[index].code, ref length, ref kratnost);
                    byte[] bLength = new byte[4];
                    bLength = BitConverter.GetBytes(length);

                    tmp = BitConverter.GetBytes(height);
                    Array.Copy(tmp, 0, binContent, 12, 4);

                    tmp = BitConverter.GetBytes(width);
                    Array.Copy(tmp, 0, binContent, 16, 4);
                    Array.Copy(bLength, 0, binContent, 20, bLength.Length);

                    tmp = BitConverter.GetBytes(mip);
                    Array.Copy(tmp, 0, binContent, 28, 1);
                    format = TTG_Tools.MainMenu.texture_header[index].tex_info;
                }
            }
            else
            {
                pvr = false;
                index = 0;
                int length = 0;
                int kratnost = 0;
                Methods.get_kratnost_and_size(width, height, TTG_Tools.MainMenu.texture_header[index].code, ref length, ref kratnost);
                binContent = TTG_Tools.MainMenu.texture_header[index].sample;
                byte[] bLength = new byte[4];
                bLength = BitConverter.GetBytes(length);
                tmp = BitConverter.GetBytes(height);
                Array.Copy(tmp, 0, binContent, 12, 4);

                tmp = BitConverter.GetBytes(height);
                Array.Copy(tmp, 0, binContent, 16, 4);

                Array.Copy(bLength, 0, binContent, 20, bLength.Length);

                tmp = BitConverter.GetBytes(mip);
                Array.Copy(tmp, 0, binContent, 28, 1);
                format = "Unknown format. Set default: " + TTG_Tools.MainMenu.texture_header[index].tex_info;
            }

            return binContent;
        }

        public static string ExportTexture(FileInfo[] fi, int i, int selected_index, byte[] key, int version_arc, string versionOfGame, bool pvr)
        {
            //try
            //{
            //старые добрые времена с текстурами без извращений
            FileStream fs = new FileStream(fi[i].FullName, FileMode.Open);
            byte[] d3dtx = Methods.ReadFull(fs);
            fs.Close();

            byte[] check_header = new byte[4];
            Array.Copy(d3dtx, 0, check_header, 0, 4);
            int version = 2;
            if (selected_index == 1) version = 7;

            int offset = 0;

            byte[] check_ver = new byte[4];


            //For very old textures
            if(Encoding.ASCII.GetString(check_header) != "5VSM" && Encoding.ASCII.GetString(check_header) != "6VSM" && Encoding.ASCII.GetString(check_header) != "ERTM")
            {
                string res = null;
                byte[] BinContent = TextureWorker.extract_old_textures(d3dtx, key, version_arc, ref res, ref pvr);

                if (BinContent != null)
                {
                    string message = "File " + fi[i].Name + " exported in dds file. " + res;
                    string path = TTG_Tools.MainMenu.settings.pathForOutputFolder + "\\" + Methods.GetNameOfFileOnly(fi[i].Name, ".d3dtx") + ".dds";
                    if (pvr)
                    {
                        message = "File " + fi[i].Name + " exported in pvr file. " + res;
                        path = TTG_Tools.MainMenu.settings.pathForOutputFolder + "\\" + Methods.GetNameOfFileOnly(fi[i].Name, ".d3dtx") + ".pvr";
                    }

                    fs = new FileStream(path, FileMode.OpenOrCreate);
                    fs.Write(BinContent, 0, BinContent.Length);
                    fs.Close();

                    return message;
                    //listBox1.Items.Add(message);
                }
                else return "Unknown error in file " + fi[i].Name + ". Please write me about it.";
            }

            if (Encoding.ASCII.GetString(check_header) == "5VSM" || Encoding.ASCII.GetString(check_header) == "6VSM")
            {
                Array.Copy(d3dtx, 16, check_ver, 0, 4);

                offset = 12 * BitConverter.ToInt32(check_ver, 0) + 16 + 4;
                check_ver = new byte[4];
                Array.Copy(d3dtx, offset, check_ver, 0, 4);
            }
            else Array.Copy(d3dtx, 4, check_ver, 0, 4);

            string result = null;

            if ((((Encoding.ASCII.GetString(check_header) != "5VSM") && (Encoding.ASCII.GetString(check_header) != "6VSM")) && BitConverter.ToInt32(check_ver, 0) < 6))
            {
                pvr = false;
                byte[] BinContent = TextureWorker.extract_old_textures(d3dtx, key, version_arc, ref result, ref pvr);
                if (BinContent != null)
                {
                    string message = "File " + fi[i].Name + " exported in dds file. " + result;
                    string path = TTG_Tools.MainMenu.settings.pathForOutputFolder + "\\" + Methods.GetNameOfFileOnly(fi[i].Name, ".d3dtx") + ".dds";
                    if (pvr)
                    {
                        message = "File " + fi[i].Name + " exported in pvr file. " + result;
                        path = TTG_Tools.MainMenu.settings.pathForOutputFolder + "\\" + Methods.GetNameOfFileOnly(fi[i].Name, ".d3dtx") + ".pvr";
                    }

                    fs = new FileStream(path, FileMode.OpenOrCreate);
                    fs.Write(BinContent, 0, BinContent.Length);
                    fs.Close();

                    return message;
                    //listBox1.Items.Add(message);
                }
                else return "Unknown error in file " + fi[i].Name + ". Please write me about it.";
            }

            if ((BitConverter.ToInt32(check_ver, 0) == 6) && (Encoding.ASCII.GetString(check_header)) == "ERTM")
            {
                versionOfGame = "PN2";
            }
            else if ((BitConverter.ToInt32(check_ver, 0) >= 4) && (Encoding.ASCII.GetString(check_header) == "5VSM"))
            {
                switch (BitConverter.ToInt32(check_ver, 0))
                {
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
            else if (BitConverter.ToInt32(check_ver, 0) >= 7 && (Encoding.ASCII.GetString(check_header) == "6VSM"))
            {
                switch(BitConverter.ToInt32(check_ver, 0))
                {
                    case 7:
                        versionOfGame = "TftBR"; //New version Tales from the Borderlands
                        break;

                    case 8:
                        versionOfGame = "Batman";
                        break;

                    case 9:
                        versionOfGame = "WDDS"; //Walking Dead: The Definitive Series
                        break;
                }
                
            }

            if (versionOfGame != " ")
            {
                try
                {
                    int platform_pos = 0; //Позиция данных о платформе (сделано для долбаного PVR формата!)
                    int code_pos = 0; //Позиция кода
                    int mips_pos = 0; //Позиция мип-текстур
                    int mips_pos2 = 0;
                    int add_mips = 0;

                    switch (versionOfGame)
                    {
                        case "PN2":
                            platform_pos = 0x60;
                            code_pos = 4;
                            mips_pos = 0x44;
                            mips_pos2 = 8;
                            add_mips = 4;
                            break;
                        case "WAU":
                            platform_pos = 0x6C;
                            code_pos = 4;
                            mips_pos = 0x44;
                            mips_pos2 = 8;
                            add_mips = 0;
                            break;
                        case "TFTB":
                            platform_pos = 0x6C;
                            code_pos = 4;
                            mips_pos = 0x54;
                            mips_pos2 = 8;
                            add_mips = 4;
                            break;
                        case "WDM":
                            platform_pos = 0x78;
                            code_pos = 4;
                            mips_pos = 0x64;
                            mips_pos2 = 8;
                            add_mips = 8;
                            break;
                        case "Batman":
                        case "WDDS":
                            platform_pos = offset + 16;//0x78;
                                                       //num = 8;
                                                       //num_width = 5;
                                                       // num_height = 4;
                                                       // num_mipmaps = 3;
                            code_pos = 12;
                            mips_pos = 0x64;
                            mips_pos2 = 8;
                            add_mips = 8;
                            break;

                        case "TftBR":
                            platform_pos = 0x78;
                            code_pos = 4;
                            mips_pos = 0x58;
                            mips_pos2 = 8;
                            add_mips = 4;
                            break;
                    }

                    List<AutoPacker.chapterOfDDS> chaptersOfDDS = new List<AutoPacker.chapterOfDDS>();
                    int start = Methods.FindStartOfStringSomething(d3dtx, 0, ".d3dtx") + 6;
                    int poz = start;
                    //List<byte[]> head = new List<byte[]>();
                    byte[] getPlatform = new byte[4];
                    Array.Copy(d3dtx, platform_pos, getPlatform, 0, 4);

                    //string res = Methods.GetChaptersOfDDS(d3dtx, poz, head, chaptersOfDDS, versionOfGame);
                    byte[] mips = new byte[4];
                    byte[] width = new byte[4];
                    byte[] height = new byte[4];
                    int tex_code = 0;


                    byte[] block_size = new byte[4];
                    Array.Copy(d3dtx, poz, block_size, 0, block_size.Length);
                    poz += 4;
                    byte[] content_size = new byte[4];
                    Array.Copy(d3dtx, poz, content_size, 0, content_size.Length);
                    poz += 4;
                    int size = BitConverter.ToInt32(content_size, 0);
                    poz += size;
                    //if (size == 0) poz += content_size - 4;
                    poz += 4;

                    if (versionOfGame == "TFTB") poz += 4;

                    byte[] check = new byte[1];
                    Array.Copy(d3dtx, poz, check, 0, check.Length);
                    poz += 1;
                    byte ch = check[0];

                    if (ch == 0x31)
                    {
                        poz += 8;
                        byte[] temp_sz = new byte[4];
                        Array.Copy(d3dtx, poz, temp_sz, 0, temp_sz.Length);
                        poz += BitConverter.ToInt32(temp_sz, 0);
                    }

                    Array.Copy(d3dtx, poz, mips, 0, mips.Length);

                    poz += 4;
                    Array.Copy(d3dtx, poz, width, 0, width.Length);
                    poz += 4;
                    Array.Copy(d3dtx, poz, height, 0, height.Length);
                    poz += code_pos;
                    byte[] temp = new byte[4];
                    Array.Copy(d3dtx, poz, temp, 0, temp.Length);
                    tex_code = BitConverter.ToInt32(temp, 0);
                    poz += mips_pos;

                    /*byte[] tmp = new byte[4];
                    Array.Copy(width, 0, tmp, 0, width.Length);
                    Array.Copy(height, 0, width, 0, height.Length);
                    Array.Copy(tmp, 0, height, 0, tmp.Length);*/


                    List<AutoPacker.chapterOfDDS> chDDS = new List<AutoPacker.chapterOfDDS>();
                    List<byte[]> temp_mas = new List<byte[]>();

                    int check_mips = -1;
                    bool need_skip = false;
                    //for (int t = 0; t < BitConverter.ToInt32(mips, 0); t++)
                    int t = 0;
                    while (t < BitConverter.ToInt32(mips, 0))
                    {
                        byte[] some_shit = new byte[4];
                        poz += 8;
                        //if (versionOfGame == "PN2" || versionOfGame == "Batman" || versionOfGame == "WDDS") poz -= 8; //было -4

                        switch (versionOfGame)
                        {
                            case "PN2":
                            case "Batman":
                            case "WDDS":
                                poz -= 8;
                                break;
                        }

                        Array.Copy(d3dtx, poz, some_shit, 0, some_shit.Length);

                        if (versionOfGame == "Batman" || versionOfGame == "WDDS") //Terrible way to fix that problem
                        {
                            if (BitConverter.ToInt32(some_shit, 0) == check_mips)
                            {
                                poz += 0x78;
                                need_skip = true;
                            }
                            else
                            {
                                check_mips = BitConverter.ToInt32(some_shit, 0);
                                poz += 8;
                                some_shit = new byte[4];
                                Array.Copy(d3dtx, poz, some_shit, 0, some_shit.Length);

                                temp_mas.Add(some_shit);
                                poz += mips_pos2;


                                if (t != BitConverter.ToInt32(mips, 0) - 1)
                                {
                                    poz += add_mips;
                                }

                                t++;
                            }
                        }
                        else
                        {
                            temp_mas.Add(some_shit);
                            //chDDS.Add(new chapterOfDDS(nul, nul, nul, nul, some_shit, nul));
                            poz += mips_pos2;


                            if (t != BitConverter.ToInt32(mips, 0) - 1)
                            {
                                poz += add_mips;
                            }

                            t++;
                        }
                    }

                    if ((versionOfGame == "Batman" || versionOfGame == "WDDS") && need_skip)
                    {
                        poz += 0x78; //I'll see later
                    }

                    if (versionOfGame == "WDM" || versionOfGame == "Batman" || versionOfGame == "WDDS") poz += 4;

                    for (t = 0; t < temp_mas.Count; t++)
                    {
                        int size_tex = BitConverter.ToInt32(temp_mas[t], 0);
                        byte[] nul = new byte[4];
                        byte[] some_shit = new byte[size_tex];

                        Array.Copy(d3dtx, poz, some_shit, 0, some_shit.Length);
                        poz += some_shit.Length;
                        chDDS.Add(new AutoPacker.chapterOfDDS(nul, nul, nul, nul, some_shit, nul));
                    }

                    string tex_info = "MIP-map count: ";

                    if (BitConverter.ToInt32(mips, 0) <= 1) tex_info += "no mip-maps";
                    else tex_info += BitConverter.ToInt32(mips, 0); //Информация о текстурах


                    string AdditionalInfo = null;
                    
                    int platform = BitConverter.ToInt32(getPlatform, 0);


                    byte[] Content = TextureWorker.extract_new_textures(tex_code, width, height, mips, platform, ref pvr, chDDS, ref AdditionalInfo);

                    AdditionalInfo += " " + tex_info;


                    if (Content != null)
                    {
                        string FilePath = MainMenu.settings.pathForOutputFolder + "\\" + Methods.GetNameOfFileOnly(fi[i].Name, ".d3dtx") + ".dds";
                        if (pvr) FilePath = MainMenu.settings.pathForOutputFolder + "\\" + Methods.GetNameOfFileOnly(fi[i].Name, ".d3dtx") + ".pvr";
                        fs = new FileStream(FilePath, FileMode.OpenOrCreate);
                        fs.Write(Content, 0, Content.Length);
                        fs.Close();

                        if (pvr) return "Exported pvr from: " + fi[i].Name + ", " + AdditionalInfo;
                        else return "Exported dds from: " + fi[i].Name + ", " + AdditionalInfo;
                    }
                    else
                    {
                        return "Unknown error in file " + fi[i].Name + ". Code of Texture: " + tex_code + ". Please write me about it.";
                    }
                }
                catch
                {
                    return "Something is wrong. Please, contact with me.";
                }
            }

            return ""; //return nothing
        }

        public static byte[] extract_new_textures(int code, byte[] width, byte[] height, byte[] mips, int platform, ref bool pvr, List<AutoPacker.chapterOfDDS> DDSs, ref string result)
        {
            byte[] binContent;
            string format = null;
            //int code = BitConverter.ToInt32(chDDS[num], 0);
            //byte[] header = genHeader(chDDS[num_width], chDDS[num_height], chDDS[num_mip], code, platform, ref pvr, ref format);
            byte[] header = genHeader(BitConverter.ToInt32(width, 0), BitConverter.ToInt32(height, 0), BitConverter.ToInt32(mips, 0), code, platform, ref pvr, ref format);

            if (header != null)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                int dds_width = BitConverter.ToInt32(width, 0);
                int dds_height = BitConverter.ToInt32(height, 0);

                if (platform == 15)
                {
                    for (int i = 0; i < DDSs.Count; i++)
                    {
                        //DDSs[i].content_chapter = SwizzleNintendo(DDSs[i].content_chapter, dds_width, dds_height, tex_indx);
                        DDSs[i].content_chapter = Swizzle.NintendoSwizzle(DDSs[i].content_chapter, dds_width, dds_height, code, true);
                        ms.Write(DDSs[i].content_chapter, 0, DDSs[i].content_chapter.Length);
                        if (dds_width % 2 == 0) dds_width /= 2;
                        if (dds_height % 2 == 0) dds_height /= 2;
                    }
                }
                else
                {
                    for (int i = DDSs.Count - 1; i >= 0; i--)
                    {
                        ms.Write(DDSs[i].content_chapter, 0, DDSs[i].content_chapter.Length);
                    }
                }

                byte[] tempContent = ms.ToArray();
                ms.Close();

                binContent = new byte[header.Length + tempContent.Length];
                Array.Copy(header, 0, binContent, 0, header.Length);
                Array.Copy(tempContent, 0, binContent, header.Length, tempContent.Length);
                result = "Texture format: " + format + ". ";
                tempContent = null;
                header = null;

                return binContent;
            }
            else return null;
        }

        //Всё для импорта текстур

        public static byte[] import_old_textures(byte[] d3dtxContent, byte[] binContent)
        {
            byte[] checkHeader = new byte[4];
            byte[] checkVer = new byte[4];

            Array.Copy(d3dtxContent, 0, checkHeader, 0, 4);
            Array.Copy(d3dtxContent, 4, checkVer, 0, 4);

            if((Encoding.ASCII.GetString(checkHeader) != "5VSM" && Encoding.ASCII.GetString(checkHeader) != "ERTM"
                && Encoding.ASCII.GetString(checkHeader) != "NIBM") && (BitConverter.ToInt32(checkVer, 0) < 0 || BitConverter.ToInt32(checkVer, 0) > 6))
            {
                string result = Methods.FindingDecrytKey(d3dtxContent, "texture");

                if (result != null)
                {
                    string temp = "File was decrypted! " + result;
                    result = temp;
                }
            }
            else if ((Encoding.ASCII.GetString(checkHeader) != "5VSM" && Encoding.ASCII.GetString(checkHeader) != "ERTM"
                && Encoding.ASCII.GetString(checkHeader) != "NIBM") && (BitConverter.ToInt32(checkVer, 0) > 0 && BitConverter.ToInt32(checkVer, 0) < 6)
                && (Methods.FindStartOfStringSomething(d3dtxContent, 8, "DDS") > d3dtxContent.Length - 100
                && Methods.FindStartOfStringSomething(d3dtxContent, 8, "PVR!") > d3dtxContent.Length - 100))
            {
                string result = Methods.FindingDecrytKey(d3dtxContent, "texture");

                if (result != null)
                {
                    string temp = "File was decrypted! " + result;
                    result = temp;
                }
            }

            if ((Methods.FindStartOfStringSomething(d3dtxContent, 8, "DDS") < d3dtxContent.Length - 100) && (Methods.FindStartOfStringSomething(binContent, 0, "DDS |") == 0))
            {
                uint dds_length = (uint)binContent.Length;
                int pos = Methods.FindStartOfStringSomething(d3dtxContent, 8, "DDS ") - 4;
                byte[] bin_length = new byte[4];
                Array.Copy(d3dtxContent, pos, bin_length, 0, 4);

                if (dds_length != BitConverter.ToInt32(bin_length, 0)) //Если длина текстуры не соответсвует
                {
                    bin_length = BitConverter.GetBytes(dds_length); //Меняем длину текстуры на длину новой DDS-текстуры
                    Array.Copy(bin_length, 0, d3dtxContent, pos, bin_length.Length);
                    pos += 4;

                    byte[] tempContent = new byte[pos + binContent.Length]; //И копируем изменённые данные с d3dtx текстуры,
                                                                            //заменив старую текстуру на новую


                    Array.Copy(d3dtxContent, 0, tempContent, 0, pos);
                    Array.Copy(binContent, 0, tempContent, pos, binContent.Length);
                    d3dtxContent = tempContent;
                    tempContent = null; //удаляем временное содержимое текстуры
                }
                else //Иначе просто скопируем содержимое
                {
                    pos += 4;
                    Array.Copy(binContent, 0, d3dtxContent, pos, binContent.Length);
                }

                return d3dtxContent;
            }
            else if ((Methods.FindStartOfStringSomething(d3dtxContent, 8, "PVR!") < d3dtxContent.Length - 100) && (Methods.FindStartOfStringSomething(binContent, 0, "PVR\x03") == 0))
            {
                byte[] bin_meta = new byte[4];
                Array.Copy(binContent, 0x30, bin_meta, 0, 4);
                
                int pos = 52; //Позиция данных текстур в PVR файле (если длина мета-данных равна 0)
                int meta_length = BitConverter.ToInt32(bin_meta, 0);
                if (meta_length > 0) pos += meta_length - 4;

                byte[] tempContent = new byte[binContent.Length - pos];

                Array.Copy(binContent, pos, tempContent, 0, tempContent.Length);

                byte[] width = new byte[4];
                byte[] height = new byte[4];
                byte[] mip = new byte[2];
                byte[] texType = new byte[8];

                Array.Copy(binContent, 12, width, 0, 4);
                Array.Copy(binContent, 16, height, 0, 4);
                Array.Copy(binContent, 44, mip, 0, 1);
                Array.Copy(binContent, 8, texType, 0, 8);

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

                mip = BitConverter.GetBytes(BitConverter.ToInt16(mip, 0) - 1);

                int d3dtxPos = Methods.FindStartOfStringSomething(d3dtxContent, 8, "PVR!") - 0x30;
                int data_length = tempContent.Length + 1 + 0x34;
                byte[] binLength = new byte[4];
                binLength = BitConverter.GetBytes(data_length);
                Array.Copy(binLength, 0, d3dtxContent, d3dtxPos, binLength.Length);
                d3dtxPos += 8;
                Array.Copy(d3dtxContent, d3dtxPos, height, 0, height.Length);
                d3dtxPos += 4;
                Array.Copy(d3dtxContent, d3dtxPos, width, 0, width.Length);
                d3dtxPos += 4;
                Array.Copy(d3dtxContent, d3dtxPos, mip, 0, mip.Length);
                d3dtxPos += 4;
                Array.Copy(d3dtxContent, d3dtxPos, texType, 0, texType.Length);
                d3dtxPos += 4;
                binLength = new byte[4];
                binLength = BitConverter.GetBytes(tempContent.Length);
                Array.Copy(binLength, 0, d3dtxContent, d3dtxPos, binLength.Length);
                d3dtxPos += 32;

                byte[] tempd3dtx = new byte[d3dtxPos + tempContent.Length + 1];
                Array.Copy(d3dtxContent, 0, tempd3dtx, 0, d3dtxPos);
                Array.Copy(tempContent, 0, tempd3dtx, d3dtxPos, tempContent.Length);

                d3dtxContent = tempd3dtx;
                tempContent = null;
                tempd3dtx = null;

                return d3dtxContent;
            }
            else return null;
            
        }

        private static void WriteMipHeader(string VersionOfGame, ref byte[] newD3dtxHeader, ref int poz, int[] contentChpater, int[] kratnostChapter, int x, int lastMip)
        {
            Array.Copy(BitConverter.GetBytes(x), 0, newD3dtxHeader, poz, 4);
            poz += 4;

            if (VersionOfGame != "PN2")
            {
                Array.Copy(BitConverter.GetBytes(1), 0, newD3dtxHeader, poz, 4);
                poz += 4;
            }

            byte[] ddsContentLengthBin = BitConverter.GetBytes(contentChpater[x]);
            Array.Copy(ddsContentLengthBin, 0, newD3dtxHeader, poz, 4);
            poz += 4;

            byte[] ddsKratnostBin = BitConverter.GetBytes(kratnostChapter[x]);
            Array.Copy(ddsKratnostBin, 0, newD3dtxHeader, poz, 4);
            poz += 4;

            if (VersionOfGame == "Batman" || VersionOfGame == "WDDS")
            {
                Array.Copy(ddsContentLengthBin, 0, newD3dtxHeader, poz, 4);
                poz += 4;
            }

            if (VersionOfGame == "TFTB" || VersionOfGame == "WDM" || VersionOfGame == "Batman" || VersionOfGame == "WDDS" || VersionOfGame == "TftBR")
            { //Странный метод в борде. Идут сначала какие-то 00 00 00 00 байта, а в конце тупо кратностью заканчивается
                if (x != lastMip)
                {
                    poz += 4;
                }
            }
        }

        public static byte[] import_new_textures(byte[] d3dtxContent, byte[] binContent, string Version, ref bool pvr, bool iOS)
        {
            string VersionOfGame = Version;

            int start = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 18; //задаётся позиция для начала изменений
            if (VersionOfGame == "TFTB") start = Methods.FindStartOfStringSomething(d3dtxContent, 0, ".d3dtx") + 22;

            byte[] check = new byte[1];
            Array.Copy(d3dtxContent, start, check, 0, check.Length);
            byte ch = check[0];
            start += 1;

            if (ch == 0x31)
            {
                start += 8;
                byte[] temp = new byte[4];
                Array.Copy(d3dtxContent, start, temp, 0, temp.Length);
                start += BitConverter.ToInt32(temp, 0);
            }

            int poz = start;

            byte[] checkHeader = new byte[4];
            Array.Copy(binContent, 0, checkHeader, 0, checkHeader.Length);

            int mip_pos = 0, width_pos = 0, height_pos = 0, check_header_length = 0, check_header_pos = 0, header_length = 0;

            byte[] width = new byte[4];
            byte[] height = new byte[4];
            byte[] mip = new byte[4];
            byte[] texType = new byte[1];

            if (Encoding.ASCII.GetString(checkHeader) == "DDS ")
            {
                width_pos = 16;
                height_pos = 12;
                mip_pos = 28;
                check_header_length = 28;
                check_header_pos = 80;
                header_length = 128;
                pvr = false;
            }
            else if(Encoding.ASCII.GetString(checkHeader) == "PVR\x03")
            {
                width_pos = 28;
                height_pos = 24;
                mip_pos = 44;
                check_header_length = 8;
                check_header_pos = 8;
                header_length = 52;
                pvr = true;
            }

            Array.Copy(binContent, width_pos, width, 0, width.Length);
            Array.Copy(binContent, height_pos, height, 0, height.Length);
            Array.Copy(binContent, mip_pos, mip, 0, mip.Length);

            int index = -1;

            for (int i = 0; i < MainMenu.texture_header.Count; i++)
            {
                if (header_length == MainMenu.texture_header[i].sample.Length)
                {
                    byte[] texHeader = new byte[check_header_length];
                    byte[] SampleHeader = new byte[check_header_length];

                    Array.Copy(binContent, check_header_pos, texHeader, 0, check_header_length);
                    Array.Copy(MainMenu.texture_header[i].sample, check_header_pos, SampleHeader, 0, check_header_length);

                    if ((BitConverter.ToString(texHeader) == BitConverter.ToString(SampleHeader)) && (MainMenu.texture_header[i].Is_iOS == iOS))
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index != -1)
            {
                if (pvr)
                {
                    byte[] bMetaLength = new byte[4];
                    Array.Copy(binContent, header_length - 4, bMetaLength, 0, bMetaLength.Length);
                    header_length += BitConverter.ToInt32(bMetaLength, 0);
                }

                byte[] tex_type = new byte[4];
                tex_type = BitConverter.GetBytes(TTG_Tools.MainMenu.texture_header[index].code);

                if (BitConverter.ToInt32(mip, 0) == 0) mip = BitConverter.GetBytes(1);

                Array.Copy(mip, 0, d3dtxContent, poz, mip.Length); //и заменяем тут
                poz += 4;
                Array.Copy(width, 0, d3dtxContent, poz, 4); //тут заменяем ширину текстуры
                poz += 4;
                Array.Copy(height, 0, d3dtxContent, poz, 4); //тут высоту
                poz += 4;
                if (VersionOfGame == "Batman" || VersionOfGame == "WDDS") poz += 8;
                Array.Copy(tex_type, 0, d3dtxContent, poz, 4); //также меняем тип текстуры, если он отличается.

                //делаем дальше смещение
                switch (VersionOfGame)
                {
                    case "PN2":
                        poz += 52;
                        break;

                    case "WAU":
                        poz += 56;
                        break;

                    case "TFTB":
                        poz += 68;
                        break;

                    case "WDM":
                    case "TftBR": //New version Tales from the Borderlands
                        poz += 72;
                        break;

                    case "Batman":
                    case "WDDS":
                        poz += 84;
                        break;
                }


                Array.Copy(mip, 0, d3dtxContent, poz, 4);
                poz += 8;

                byte[] ddsContentLengthBin = new byte[4];
                byte[] ddsKratnostBin = new byte[4];

                ddsContentLengthBin = BitConverter.GetBytes(binContent.Length - header_length);
                Array.Copy(ddsContentLengthBin, 0, d3dtxContent, poz, 4);
                if (VersionOfGame != "PN2") Array.Copy(ddsContentLengthBin, 0, d3dtxContent, 12, 4); //Если не покер 2, меняем ещё и в начале размер данных текстуры
                poz += 4;

                switch (VersionOfGame)
                {
                    case "TFTB":
                    case "TftBR":
                    case "WDM":
                    case "Batman":
                    case "WDDS":
                        poz += 4;
                        break;
                }


                int mipmapTable = 0;

                //set a block size for mip maps and texture sizes
                if (VersionOfGame == "PN2") mipmapTable = 12 * BitConverter.ToInt32(mip, 0);
                else if (VersionOfGame == "WAU") mipmapTable = 16 * BitConverter.ToInt32(mip, 0);
                else if (VersionOfGame == "TFTB" || VersionOfGame == "WDM" || VersionOfGame == "TftBR") mipmapTable = (20 * BitConverter.ToInt32(mip, 0)) - 4;
                else if (VersionOfGame == "Batman" || VersionOfGame == "WDDS") mipmapTable = (24 * BitConverter.ToInt32(mip, 0)) - 4;

                byte[] newD3dtxHeader = new byte[poz + mipmapTable];
                Array.Copy(d3dtxContent, 0, newD3dtxHeader, 0, poz);

                //Задаются новые параметры для размера текстур и их кратностей
                int[] contentChpater = new int[BitConverter.ToInt32(mip, 0)];
                int[] kratnostChapter = new int[BitConverter.ToInt32(mip, 0)];


                int int_width = BitConverter.ToInt32(width, 0);
                int int_height = BitConverter.ToInt32(height, 0);

                /*
                  To do:
                  Need fix calculating correctly texture size
                  */

                for (int w = 0; w < contentChpater.Count(); w++)
                {
                    Methods.get_kratnost_and_size(int_width, int_height, MainMenu.texture_header[index].code, ref contentChpater[w], ref kratnostChapter[w]);
                    if(int_height % 2 == 0) int_height /= 2;
                    if(int_width % 2 == 0) int_width /= 2;
                }

                //Write data block about mip maps
                if (AutoPacker.sizzleNintendo)
                {
                    int int_mip = BitConverter.ToInt32(mip, 0);
                    for (int x = 0; x < int_mip; x++)
                    {
                        WriteMipHeader(VersionOfGame, ref newD3dtxHeader, ref poz, contentChpater, kratnostChapter, x, int_mip);
                    }
                }
                else
                {
                    for (int x = BitConverter.ToInt32(mip, 0) - 1; x >= 0; x--)
                    {
                        WriteMipHeader(VersionOfGame, ref newD3dtxHeader, ref poz, contentChpater, kratnostChapter, x, 0);
                    }
                }

                    int plat_pos = 0;

                    switch (VersionOfGame)
                    {
                        case "PN2":
                            plat_pos = 0x60;
                            break;
                        case "WAU":
                        case "TFTB":
                            plat_pos = 0x6C;
                            break;
                        case "WDM":
                        case "Batman":
                        case "WDDS":
                        case "TftBR":
                            plat_pos = 0x78;
                            break;
                    }

                    byte[] bPlat = new byte[4];
                    bPlat = BitConverter.GetBytes(TTG_Tools.MainMenu.texture_header[index].platform);
                    
                    if (AutoPacker.sizzleNintendo) bPlat = BitConverter.GetBytes(15);
                    Array.Copy(bPlat, 0, newD3dtxHeader, plat_pos, bPlat.Length);

                    if (VersionOfGame != "PN2")
                {
                    ulong blockSize = (ulong)newD3dtxHeader.Length - 92;
                    if (VersionOfGame == "WDM" || VersionOfGame == "Batman" || VersionOfGame == "WDDS" || VersionOfGame == "TftBR") blockSize -= 12;

                    byte[] binBlockSize = new byte[4];
                    binBlockSize = BitConverter.GetBytes(blockSize);
                    Array.Copy(binBlockSize, 0, newD3dtxHeader, 4, 4);
                }

                d3dtxContent = new byte[newD3dtxHeader.Length + (binContent.Length - header_length)];
                Array.Copy(newD3dtxHeader, 0, d3dtxContent, 0, newD3dtxHeader.Length);
                
                poz = newD3dtxHeader.Length;

                int pozFromEnd = binContent.Length; //begin reading from end of file

                if (AutoPacker.sizzleNintendo)
                {
                    pozFromEnd = header_length;
                    int_width = BitConverter.ToInt32(width, 0);
                    int_height = BitConverter.ToInt32(height, 0);

                    for (int y = 0; y < BitConverter.ToInt32(mip, 0); y++)
                    {
                        int contentSize = contentChpater[y];
                        byte[] contentChapterBin = new byte[contentSize];
                        Array.Copy(binContent, pozFromEnd, contentChapterBin, 0, contentSize);
                        contentChapterBin = Swizzle.NintendoSwizzle(contentChapterBin, int_width, int_height, MainMenu.texture_header[index].code, false);
                        Array.Copy(contentChapterBin, 0, d3dtxContent, poz, contentSize);
                        poz += contentSize;
                        pozFromEnd += contentSize;

                        if (int_width % 2 == 0) int_width /= 2;
                        if (int_height % 2 == 0) int_height /= 2;
                        //fs.Write(contentChapterBin, 0, contentChapterBin.Length);
                    }
                }
                else {
                    for (int y = BitConverter.ToInt32(mip, 0) - 1; y >= 0; y--)
                    {
                        pozFromEnd -= contentChpater[y];

                        int contentSize = contentChpater[y];
                        byte[] contentChapterBin = new byte[contentSize];
                        Array.Copy(binContent, pozFromEnd, contentChapterBin, 0, contentSize);
                        Array.Copy(contentChapterBin, 0, d3dtxContent, poz, contentSize);
                        poz += contentSize;
                        //fs.Write(contentChapterBin, 0, contentChapterBin.Length);
                    }
                }
                return d3dtxContent;
            }
            else return null;
        }
    }
}
