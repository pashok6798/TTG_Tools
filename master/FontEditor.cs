using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TTG_Tools.ClassesStructs;

namespace TTG_Tools
{
    public partial class FontEditor : Form
    {
        [DllImport("kernel32.dll")]
        public static extern void SetProcessWorkingSetSize(IntPtr hWnd, int i, int j);

        public FontEditor()
        {
            InitializeComponent();
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        }

        OpenFileDialog ofd = new OpenFileDialog();
        bool edited; //Проверка на изменения в шрифте
        bool iOS; //Для поддержки pvr текстур
        bool encrypted; //В случае, если шрифт был зашифрован
        byte[] encKey;
        int version;
        byte[] tmpHeader;
        byte[] check_header;
        bool someTexData;

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void FontEditor_Load(object sender, EventArgs e)
        {
            edited = false; //Tell a program about first launch window form so font is not modified.
            iOS = false;
        }

        public List<byte[]> head = new List<byte[]>();
        public ClassesStructs.FlagsClass fontFlags;
        FontClass.ClassFont font = null;

        byte[] start_version = { 0x81, 0x53, 0x37, 0x63, 0x9E, 0x4A, 0x3A, 0x9A }; //указывается начало заголовка. Не знаю, как можно было бы позицию по байтам сделать. Сделал по строке.

        private void ReplaceTexture(string DdsFile, int file_n, ClassesStructs.FontClass.ClassFont font)
        {
            FileStream fs = new FileStream(DdsFile, FileMode.Open);
            byte[] temp = Methods.ReadFull(fs);
            fs.Close();

            byte[] tmp = null;

            if (!font.NewFormat)
            {
                font.tex[file_n].Content = new byte[temp.Length];
                Array.Copy(temp, 0, font.tex[file_n].Content, 0, temp.Length);

                tmp = new byte[4];
                Array.Copy(temp, 12, tmp, 0, tmp.Length);
                font.tex[file_n].OriginalHeight = BitConverter.ToInt32(tmp, 0);
                font.tex[file_n].Height = BitConverter.ToInt32(tmp, 0);

                tmp = new byte[4];
                Array.Copy(temp, 16, tmp, 0, tmp.Length);
                font.tex[file_n].OriginalWidth = BitConverter.ToInt32(tmp, 0);
                font.tex[file_n].Width = BitConverter.ToInt32(tmp, 0);

                tmp = new byte[4];
                Array.Copy(temp, 28, tmp, 0, tmp.Length);
                font.tex[file_n].Mip = BitConverter.ToInt32(tmp, 0);

                font.BlockTexSize += font.tex[file_n].Content.Length - font.tex[file_n].TexSize;

                font.tex[file_n].TexSize = font.tex[file_n].Content.Length;
            }
            else
            {

            }
        }

        private void fillTableofCoordinates(FontClass.ClassFont font, bool Modified)
        {
            if (!font.NewFormat)
            {
                dataGridViewWithCoord.RowCount = font.glyph.CharCount;
                dataGridViewWithCoord.ColumnCount = 7;
                if (font.hasScaleValue)
                {
                    dataGridViewWithCoord.ColumnCount = 9;
                    dataGridViewWithCoord.Columns[7].HeaderText = "Width";
                    dataGridViewWithCoord.Columns[8].HeaderText = "Height";
                }

                for (int i = 0; i < font.glyph.CharCount; i++)
                {
                    dataGridViewWithCoord.Rows[i].HeaderCell.Value = Convert.ToString(i + 1);
                    dataGridViewWithCoord[0, i].Value = i;
                    dataGridViewWithCoord[1, i].Value = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(BitConverter.GetBytes(i));
                    dataGridViewWithCoord[2, i].Value = Math.Round(font.glyph.chars[i].XStart);
                    dataGridViewWithCoord[3, i].Value = Math.Round(font.glyph.chars[i].XEnd);
                    dataGridViewWithCoord[4, i].Value = Math.Round(font.glyph.chars[i].YStart);
                    dataGridViewWithCoord[5, i].Value = Math.Round(font.glyph.chars[i].YEnd);
                    dataGridViewWithCoord[6, i].Value = font.glyph.chars[i].TexNum;

                    if (font.hasScaleValue)
                    {
                        dataGridViewWithCoord[7, i].Value = Math.Round(font.glyph.chars[i].CharWidth);
                        dataGridViewWithCoord[8, i].Value = Math.Round(font.glyph.chars[i].CharHeight);
                    }
                }
            }
            else
            {
                dataGridViewWithCoord.RowCount = font.glyph.CharCount;
                dataGridViewWithCoord.ColumnCount = 13;
                dataGridViewWithCoord.Columns[7].HeaderText = "Width";
                dataGridViewWithCoord.Columns[8].HeaderText = "Height";
                dataGridViewWithCoord.Columns[9].HeaderText = "Offset by X";
                dataGridViewWithCoord.Columns[10].HeaderText = "Offset by Y";
                dataGridViewWithCoord.Columns[11].HeaderText = "X advance";
                dataGridViewWithCoord.Columns[12].HeaderText = "Channel";

                for (int i = 0; i < font.glyph.CharCount; i++)
                {
                    dataGridViewWithCoord.Rows[i].HeaderCell.Value = Convert.ToString(i + 1);
                    dataGridViewWithCoord[0, i].Value = font.glyph.charsNew[i].charId;
                    dataGridViewWithCoord[1, i].Value = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(BitConverter.GetBytes(font.glyph.charsNew[i].charId));
                    
                    if(MainMenu.settings.unicodeSettings == 0)
                    {
                        dataGridViewWithCoord[1, i].Value = Encoding.Unicode.GetString(BitConverter.GetBytes(font.glyph.charsNew[i].charId));
                    }

                    dataGridViewWithCoord[2, i].Value = Math.Round(font.glyph.charsNew[i].XStart);
                    dataGridViewWithCoord[3, i].Value = Math.Round(font.glyph.charsNew[i].XEnd);
                    dataGridViewWithCoord[4, i].Value = Math.Round(font.glyph.charsNew[i].YStart);
                    dataGridViewWithCoord[5, i].Value = Math.Round(font.glyph.charsNew[i].YEnd);
                    dataGridViewWithCoord[6, i].Value = font.glyph.charsNew[i].TexNum;
                    dataGridViewWithCoord[7, i].Value = Math.Round(font.glyph.charsNew[i].CharWidth);
                    dataGridViewWithCoord[8, i].Value = Math.Round(font.glyph.charsNew[i].CharHeight);
                    dataGridViewWithCoord[9, i].Value = Math.Round(font.glyph.charsNew[i].XOffset);
                    dataGridViewWithCoord[10, i].Value = Math.Round(font.glyph.charsNew[i].YOffset);
                    dataGridViewWithCoord[11, i].Value = Math.Round(font.glyph.charsNew[i].XAdvance);
                    dataGridViewWithCoord[12, i].Value = font.glyph.charsNew[i].Channel;
                }
            }

            for(int k = 0; k < dataGridViewWithCoord.RowCount; k++)
            {
                for(int l = 0; l < dataGridViewWithCoord.ColumnCount; l++)
                {
                    switch (Modified)
                    {
                        case true:
                            dataGridViewWithCoord[l, k].Style.BackColor = Color.GreenYellow;
                            break;

                        default:
                            dataGridViewWithCoord[l, k].Style.BackColor = Color.White;
                            break;
                    }
                }
            }
        }

        private void fillTableofTextures(FontClass.ClassFont font)
        {
            dataGridViewWithTextures.RowCount = font.TexCount;

            if (!font.NewFormat)
            {
                for (int i = 0; i < font.TexCount; i++)
                {
                    dataGridViewWithTextures[0, i].Value = i;
                    dataGridViewWithTextures[1, i].Value = font.tex[i].Height;
                    dataGridViewWithTextures[2, i].Value = font.tex[i].Width;
                    dataGridViewWithTextures[3, i].Value = font.tex[i].TexSize;
                }
            }
            else
            {
                for (int i = 0; i < font.TexCount; i++)
                {
                    dataGridViewWithTextures[0, i].Value = i;
                    dataGridViewWithTextures[1, i].Value = font.NewTex[i].Height;
                    dataGridViewWithTextures[2, i].Value = font.NewTex[i].Width;
                    dataGridViewWithTextures[3, i].Value = font.NewTex[i].Tex.TexSize;
                }
            }
        }

        private string ConvertToString(byte[] mas)
        {
            string str = "";
            foreach (byte b in mas)
            { str += b.ToString("x") + " "; }

            return str;
        }

        public bool CompareArray(byte[] arr0, byte[] arr1)
        {
            int i = 0;
            while ((i < arr0.Length) && (arr0[i] == arr1[i])) i++;
            return (i == arr0.Length);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            ofd.Filter = "Font files (*.font)|*.font";
            ofd.RestoreDirectory = true;
            ofd.Title = "Open font file";
            ofd.DereferenceLinks = false;
            byte[] binContent = new byte[0];
            string FileName = "";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                encrypted = false;
                bool read = false;

                FileStream fs;
                try
                {
                    FileName = ofd.FileName;
                    fs = new FileStream(ofd.FileName, FileMode.Open);
                    binContent = Methods.ReadFull(fs);
                    fs.Close();
                    read = true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error!");
                    saveToolStripMenuItem.Enabled = false;
                    saveAsToolStripMenuItem.Enabled = false;
                    exportCoordinatesToolStripMenuItem1.Enabled = false;
                    importCoordinatesToolStripMenuItem1.Enabled = false;
                    Form.ActiveForm.Text = "Font Editor";
                }
                if (read)
                {
                    fontFlags = null;

                    byte[] header = new byte[4];
                    Array.Copy(binContent, 0, header, 0, 4);

                    int poz = 0;

                    //Experiments with too old fonts
                    font = new FontClass.ClassFont();
                    font.blockSize = false;
                    font.hasScaleValue = false;

                    font.headerSize = 0;
                    font.texSize = 0;

                    poz = 4; //Begin position

                    check_header = new byte[4];
                    Array.Copy(binContent, 0, check_header, 0, check_header.Length);
                    encKey = null;
                    version = 2;

                    if ((Encoding.ASCII.GetString(check_header) != "5VSM") && (Encoding.ASCII.GetString(check_header) != "ERTM")
                    && (Encoding.ASCII.GetString(check_header) != "6VSM") && (Encoding.ASCII.GetString(check_header) != "NIBM")) //Supposed this font encrypted
                    {
                        //First trying decrypt probably encrypted font
                        try
                        {
                            string info = Methods.FindingDecrytKey(binContent, "font", ref encKey, ref version);
                            if (info != null)
                            {
                                MessageBox.Show("Font was encrypted, but I decrypted.\r\n" + info);
                                encrypted = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Maybe that font encrypted. Try to decrypt first.", "Error " + ex.Message);
                            poz = -1;
                            return;
                        }
                    }

                    if ((Encoding.ASCII.GetString(check_header) == "5VSM") || (Encoding.ASCII.GetString(check_header) == "6VSM"))
                    {
                        byte[] tmpBytes = new byte[4];
                        Array.Copy(binContent, 4, tmpBytes, 0, tmpBytes.Length);
                        font.NewFormat = true;
                        font.headerSize = BitConverter.ToUInt32(tmpBytes, 0);

                        tmpBytes = new byte[4];
                        Array.Copy(binContent, 12, tmpBytes, 0, tmpBytes.Length);
                        font.texSize = BitConverter.ToUInt32(tmpBytes, 0);

                        poz = 16;
                    }

                    byte[] tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    poz += 4;
                    int countElements = BitConverter.ToInt32(tmp, 0);
                    font.elements = new string[countElements];
                    font.binElements = new byte[countElements][];
                    int lenStr;
                    someTexData = false;

                    //version_used = countElements;

                    tmp = new byte[8];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);

                    if(BitConverter.ToString(tmp) == "81-53-37-63-9E-4A-3A-9A")
                    {
                        for (int i = 0; i < countElements; i++)
                        {
                            font.binElements[i] = new byte[8];
                            Array.Copy(binContent, poz, font.binElements[i], 0, font.binElements[i].Length);
                            poz += 12;

                            if (BitConverter.ToString(font.binElements[i]) == "41-16-D7-79-B9-3C-28-84")
                            {
                                fontFlags = new FlagsClass();
                            }

                            if (BitConverter.ToString(font.binElements[i]) == "E3-88-09-7A-48-5D-7F-93")
                            {
                                someTexData = true;
                            }

                            if (BitConverter.ToString(font.binElements[i]) == "0F-F4-20-E6-20-BA-A1-EF")
                            {
                                font.NewFormat = true;
                            }
                        }
                    }
                    else {
                        for (int i = 0; i < countElements; i++)
                        {
                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            poz += 4;
                            lenStr = BitConverter.ToInt32(tmp, 0);
                            tmp = new byte[lenStr];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            poz += lenStr + 4; //Length element's name and 4 bytes data for Telltale Tool
                            font.elements[i] = Encoding.ASCII.GetString(tmp);

                            if (font.elements[i] == "class Flags")
                            {
                                fontFlags = new FlagsClass();
                            }
                        }
                    }

                    tmpHeader = new byte[poz];
                    Array.Copy(binContent, 0, tmpHeader, 0, tmpHeader.Length);

                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    int nameLen = BitConverter.ToInt32(tmp, 0);
                    poz += 4;

                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    if (nameLen - BitConverter.ToInt32(tmp, 0) == 8)
                    {
                        nameLen = BitConverter.ToInt32(tmp, 0);
                        poz += 4;
                        font.blockSize = true;
                    }

                    tmp = new byte[nameLen];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    font.FontName = Encoding.ASCII.GetString(tmp);
                    poz += nameLen;
                    
                    font.One = binContent[poz];
                    poz++;

                    //Temporary solution
                    if((font.One == 0x31 && (Encoding.ASCII.GetString(check_header) == "5VSM"))
                        || (Encoding.ASCII.GetString(check_header) == "6VSM"))
                    {
                        tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        poz += 4;

                        font.NewSomeValue = BitConverter.ToSingle(tmp, 0);
                    }

                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    poz += 4;
                    font.BaseSize = BitConverter.ToSingle(tmp, 0);

                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    font.halfValue = 0.0f;

                    if ((BitConverter.ToSingle(tmp, 0) == 0.5)
                        || (BitConverter.ToSingle(tmp, 0) == 1.0))
                    {
                        font.halfValue = BitConverter.ToSingle(tmp, 0);
                        poz += 4;
                    }

                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);

                    if (BitConverter.ToSingle(tmp, 0) == 1.0)
                    {
                        font.oneValue = BitConverter.ToSingle(tmp, 0);
                        font.hasScaleValue = true;
                        poz += 4;
                    }

                    font.glyph.BlockCoordSize = 0;

                    if (font.blockSize)
                    {
                        tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        font.glyph.BlockCoordSize = BitConverter.ToInt32(tmp, 0);
                        poz += 4;
                    }

                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    font.glyph.CharCount = BitConverter.ToInt32(tmp, 0);
                    poz += 4;

                    if (!font.NewFormat)
                    {
                        font.glyph.chars = new FontClass.ClassFont.TRect[font.glyph.CharCount];
                        font.glyph.charsNew = null;

                        for (int i = 0; i < font.glyph.CharCount; i++)
                        {
                            font.glyph.chars[i] = new FontClass.ClassFont.TRect();

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.chars[i].TexNum = BitConverter.ToInt32(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.chars[i].XStart = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.chars[i].XEnd = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.chars[i].YStart = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.chars[i].YEnd = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            if (font.hasScaleValue)
                            {
                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.chars[i].CharWidth = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.chars[i].CharHeight = BitConverter.ToSingle(tmp, 0);
                                poz += 4;
                            }
                        }
                    }
                    else
                    {
                        font.glyph.chars = null;
                        font.glyph.charsNew = new ClassesStructs.FontClass.ClassFont.TRectNew[font.glyph.CharCount];

                        for(int i = 0; i < font.glyph.CharCount; i++)
                        {
                            font.glyph.charsNew[i] = new FontClass.ClassFont.TRectNew();

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].charId = BitConverter.ToUInt32(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].TexNum = BitConverter.ToInt32(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].Channel = BitConverter.ToInt32(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].XStart = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].XEnd = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].YStart = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].YEnd = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].CharWidth = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].CharHeight = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].XOffset = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].YOffset = BitConverter.ToSingle(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.charsNew[i].XAdvance = BitConverter.ToSingle(tmp, 0);
                            poz += 4;
                        }
                    }

                    if (font.blockSize)
                    {
                        tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        font.BlockTexSize = BitConverter.ToInt32(tmp, 0);
                        poz += 4;
                    }

                    tmp = new byte[4];
                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                    font.TexCount = BitConverter.ToInt32(tmp, 0);
                    poz += 4;

                    if (!font.NewFormat)
                    {
                        font.tex = new TextureClass.OldT3Texture[font.TexCount];
                        font.NewTex = null;

                        for (int i = 0; i < font.TexCount; i++)
                        {
                            font.tex[i] = TextureWorker.GetOldTextures(binContent, ref poz, fontFlags != null, someTexData);
                            if (font.tex[i] == null)
                            {
                                MessageBox.Show("Maybe unsupported font.", "Error");
                                return;
                            }
                        }

                        for (int k = 0; k < font.glyph.CharCount; k++)
                        {
                            font.glyph.chars[k].XStart *= font.tex[font.glyph.chars[k].TexNum].Width;
                            font.glyph.chars[k].XEnd *= font.tex[font.glyph.chars[k].TexNum].Width;

                            font.glyph.chars[k].YStart *= font.tex[font.glyph.chars[k].TexNum].Width;
                            font.glyph.chars[k].YEnd *= font.tex[font.glyph.chars[k].TexNum].Height;
                        }
                    }
                    else
                    {
                        font.tex = null;
                        font.NewTex = new TextureClass.NewT3Texture[font.TexCount];
                        string format = "";
                        uint tmpPosition = 0;

                        if(font.headerSize != 0)
                        {
                            tmpPosition = font.headerSize + 16 + ((uint)countElements * 12) + 4;
                        }

                        for(int i = 0; i < font.TexCount; i++)
                        {
                            font.NewTex[i] = TextureWorker.GetNewTextures(binContent, ref poz, ref tmpPosition, fontFlags != null, someTexData, true, ref format);

                            if(font.NewTex[i] == null)
                            {
                                MessageBox.Show("Maybe unsupported font.", "Error");
                                return;
                            }
                        }

                        for(int k = 0; k < font.glyph.CharCount; k++)
                        {
                            font.glyph.charsNew[k].XStart *= font.NewTex[font.glyph.charsNew[k].TexNum].Width;
                            font.glyph.charsNew[k].XEnd *= font.NewTex[font.glyph.charsNew[k].TexNum].Width;

                            font.glyph.charsNew[k].YStart *= font.NewTex[font.glyph.charsNew[k].TexNum].Height;
                            font.glyph.charsNew[k].YEnd *= font.NewTex[font.glyph.charsNew[k].TexNum].Height;
                        }
                    }

                    fillTableofCoordinates(font, false);
                    fillTableofTextures(font);

                    saveToolStripMenuItem.Enabled = true;
                    saveAsToolStripMenuItem.Enabled = true;
                    exportCoordinatesToolStripMenuItem1.Enabled = true;
                    rbKerning.Enabled = font.NewFormat;
                    rbNoKerning.Enabled = font.NewFormat;
                    edited = false; //Открыли новый неизмененный файл
                    Form.ActiveForm.Text = "Font Editor. Opened file " + FileName;
                }
            }
        stop_it:
            binContent = null;
            GC.Collect();
            //}
            //catch { MessageBox.Show("Maybe you forget decrypt font?", "Error!"); }
}

        public int FindStartOfStringSomething(byte[] array, int offset, string string_something)
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


        private void encFunc(string path) //Encrypts full font
        {
            if (encrypted == true) //Ask about a full enryption if you don't want build archive
            {
                if (MessageBox.Show("Do you want to make a full encryption?", "About encrypted font...",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    FileStream fs = new FileStream(path, FileMode.Open);
                    byte[] fontContent = Methods.ReadFull(fs);
                    fs.Close();

                    Methods.meta_crypt(fontContent, encKey, version, false);

                    if (File.Exists(path)) File.Delete(path);
                    fs = new FileStream(path, FileMode.Create);
                    fs.Write(fontContent, 0, fontContent.Length);
                    fs.Close();
                }

            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Methods.DeleteCurrentFile(ofd.FileName);

            FileStream fs = new FileStream(ofd.FileName, FileMode.OpenOrCreate);
            SaveFont(fs, font);
            fs.Close();

            encFunc(ofd.FileName);

            edited = false; //After saving return trigger to FALSE
        }

        private void SaveFont(Stream fs, ClassesStructs.FontClass.ClassFont font)
        {
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(tmpHeader);
            
            //First need check textures import
            font.texSize = 0;
            font.headerSize = 0;

            int len = Encoding.ASCII.GetBytes(font.FontName).Length;
            font.headerSize += 4;

            if (font.blockSize)
            {
                int subLen = len + 8;
                font.headerSize += 4;
                bw.Write(subLen);
            }

            bw.Write(len);
            bw.Write(Encoding.ASCII.GetBytes(font.FontName));
            font.headerSize += (uint)len;

            bw.Write(font.One);
            font.headerSize++;

            if ((font.One == 0x31 && (Encoding.ASCII.GetString(check_header) == "5VSM"))
                        || (Encoding.ASCII.GetString(check_header) == "6VSM"))
            {
                bw.Write(font.NewSomeValue);
                font.headerSize += 4;
            }

            bw.Write(font.BaseSize);
            font.headerSize += 4;

            if(font.halfValue == 0.5f || font.halfValue == 1.0f)
            {
                bw.Write(font.halfValue);
                font.headerSize += 4;
            }

            if (font.hasScaleValue)
            {
                bw.Write(font.oneValue);
                font.headerSize += 4;
            }

            if (font.blockSize)
            {
                if (!font.NewFormat)
                {
                    font.glyph.BlockCoordSize = font.glyph.CharCount * (5 * 4);

                    if (font.hasScaleValue) font.glyph.BlockCoordSize = font.glyph.CharCount * (7 * 4);

                    font.glyph.BlockCoordSize += 4; //Includes char count block
                }
                else
                {
                    font.glyph.BlockCoordSize = font.glyph.CharCount * (12 * 4);
                    font.glyph.BlockCoordSize += 4; //Includes char count block
                }

                font.glyph.BlockCoordSize += 4; //And block size itself

                bw.Write(font.glyph.BlockCoordSize);
                font.headerSize += 4;
            }

            bw.Write(font.glyph.CharCount);
            font.headerSize += 4;

            if (!font.NewFormat)
            {
                for(int i = 0; i < font.glyph.CharCount; i++)
                {
                    bw.Write(font.glyph.chars[i].TexNum);
                    bw.Write(font.glyph.chars[i].XStart / font.tex[font.glyph.chars[i].TexNum].OriginalWidth);
                    bw.Write(font.glyph.chars[i].XEnd / font.tex[font.glyph.chars[i].TexNum].OriginalWidth);
                    bw.Write(font.glyph.chars[i].YStart / font.tex[font.glyph.chars[i].TexNum].OriginalHeight);
                    bw.Write(font.glyph.chars[i].YEnd / font.tex[font.glyph.chars[i].TexNum].OriginalHeight);

                    if (font.hasScaleValue)
                    {
                        bw.Write(font.glyph.chars[i].CharWidth);
                        bw.Write(font.glyph.chars[i].CharHeight);
                    }
                }

                if (font.blockSize)
                {
                    //bw.Write(font.BlockTexSize);
                    font.BlockTexSize = 0;

                    for (int j = 0; j < font.TexCount; j++)
                    {
                        font.BlockTexSize += font.tex[j].BlockPos + font.tex[j].TexSize;
                    }

                    font.BlockTexSize += 8; //4 bytes of block size and 4 bytes of block (if it empty)

                    bw.Write(font.BlockTexSize);
                }

                bw.Write(font.TexCount);

                for (int i = 0; i < font.TexCount; i++)
                {
                    TextureWorker.ReplaceOldTextures(fs, font.tex[i], someTexData, encrypted, encKey, version);
                }
            }
            else
            {

            }

            bw.Close();
            fs.Close();
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            label1.Text = "(" + textBox8.Text.Length.ToString() + ")";
        }
        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            label2.Text = "(" + textBox9.Text.Length.ToString() + ")";
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox8.Text = "";
            textBox9.Text = "";
            label1.Text = "(0)";
            label2.Text = "(0)";
            checkBox1.Checked = true;
            checkBox2.Checked = true;

        }

        private void buttonCopyCoordinates_Click(object sender, EventArgs e)
        {
            string ch1 = textBox8.Text;
            string ch2 = textBox9.Text;
            if (ch1.Length == ch2.Length)
            {
                for (int i = 0; i < ch1.Length; i++)
                {
                    int f = Convert.ToInt32(ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(ch1[i].ToString())[0]);
                    int s = Convert.ToInt32(ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(ch2[i].ToString())[0]);
                    int first = 0;
                    int second = 0;
                    for (int j = 0; j < dataGridViewWithCoord.RowCount; j++)
                    {
                        if (Convert.ToInt32(dataGridViewWithCoord[0, j].Value) == f)
                        {
                            first = j;
                        }
                        if (Convert.ToInt32(dataGridViewWithCoord[0, j].Value) == s)
                        {
                            second = j;
                        }
                    }


                    CopyDataIndataGridViewWithCoord(6, first, second);
                    CopyDataIndataGridViewWithCoord(7, first, second);
                    CopyDataIndataGridViewWithCoord(8, first, second);
                    CopyDataIndataGridViewWithCoord(9, first, second);
                    CopyDataIndataGridViewWithCoord(10, first, second);
                    CopyDataIndataGridViewWithCoord(11, first, second);
                    CopyDataIndataGridViewWithCoord(12, first, second);

                    if (checkBox1.Checked == true)
                    {
                        CopyDataIndataGridViewWithCoord(2, first, second);
                        CopyDataIndataGridViewWithCoord(3, first, second);
                    }
                    if (checkBox2.Checked == true)
                    {
                        CopyDataIndataGridViewWithCoord(4, first, second);
                        CopyDataIndataGridViewWithCoord(5, first, second);
                    }
                }
            }
            else if (ch1.Length == 1)
            {
                for (int i = 0; i < ch2.Length; i++)
                {
                    int f = Convert.ToInt32(ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(ch1[i].ToString())[0]);
                    int s = Convert.ToInt32(ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(ch2[i].ToString())[0]);
                    int first = 0;
                    int second = 0;
                    for (int j = 0; j < dataGridViewWithCoord.RowCount; j++)
                    {
                        if (Convert.ToInt32(dataGridViewWithCoord[0, j].Value) == f)
                        {
                            first = j;
                        }
                        if (Convert.ToInt32(dataGridViewWithCoord[0, j].Value) == s)
                        {
                            second = j;
                        }
                    }

                    CopyDataIndataGridViewWithCoord(6, first, second);
                    CopyDataIndataGridViewWithCoord(7, first, second);
                    CopyDataIndataGridViewWithCoord(8, first, second);
                    CopyDataIndataGridViewWithCoord(9, first, second);
                    CopyDataIndataGridViewWithCoord(10, first, second);
                    CopyDataIndataGridViewWithCoord(11, first, second);
                    CopyDataIndataGridViewWithCoord(12, first, second);

                    if (checkBox1.Checked == true)
                    {
                        CopyDataIndataGridViewWithCoord(2, first, second);
                        CopyDataIndataGridViewWithCoord(3, first, second);
                    }
                    if (checkBox2.Checked == true)
                    {
                        CopyDataIndataGridViewWithCoord(4, first, second);
                        CopyDataIndataGridViewWithCoord(5, first, second);
                    }
                }
            }
        }

        private void CopyDataIndataGridViewWithCoord(int column, int first, int second)
        {
            dataGridViewWithCoord[column, second].Value = dataGridViewWithCoord[column, first].Value;
            dataGridViewWithCoord[column, second].Style.BackColor = System.Drawing.Color.Green;
        }

        private void contextMenuStripExport_Import_Opening(object sender, CancelEventArgs e)
        {
            if (dataGridViewWithTextures.Rows.Count > 0)
            {
                if (dataGridViewWithTextures.SelectedCells[0].RowIndex >= 0)
                {
                    exportToolStripMenuItem.Enabled = true;
                    importDDSToolStripMenuItem.Enabled = true;
                    //exportCoordinatesToolStripMenuItem1.Enabled = true;
                    importCoordinatesToolStripMenuItem1.Enabled = true;
                }
                else
                {
                    exportToolStripMenuItem.Enabled = false;
                    importDDSToolStripMenuItem.Enabled = false;
                    exportCoordinatesToolStripMenuItem1.Enabled = false;
                    importCoordinatesToolStripMenuItem1.Enabled = false;
                    importCoordinatesFromFontStudioxmlToolStripMenuItem.Enabled = false;
                    toolStripImportFNT.Enabled = false;
                }
            }
        }

        private void dataGridViewWithTextures_RowContextMenuStripNeeded(object sender, DataGridViewRowContextMenuStripNeededEventArgs e)
        {
            dataGridViewWithTextures.Rows[e.RowIndex].Selected = true;
            MessageBox.Show(dataGridViewWithTextures.Rows[e.RowIndex].Selected.ToString());
        }

        private void dataGridViewWithTextures_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewWithTextures.Rows[e.RowIndex].Selected = true;
            }
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // получаем координаты
                Point pntCell = dataGridViewWithTextures.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                pntCell.X += e.Location.X;
                pntCell.Y += e.Location.Y;

                // вызываем менюшку
                contextMenuStripExport_Import.Show(dataGridViewWithTextures, pntCell);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int file_n = dataGridViewWithTextures.SelectedCells[0].RowIndex;
            SaveFileDialog saveFD = new SaveFileDialog();
            if (iOS == false)
            {
                saveFD.Filter = "dds files (*.dds)|*.dds";
                saveFD.FileName = Methods.GetNameOfFileOnly(ofd.SafeFileName.ToString(), ".font") + "_" + file_n.ToString() + ".dds";
            }
            else
            {
                saveFD.Filter = "PVR files (*.pvr)|*.pvr";
                saveFD.FileName = Methods.GetNameOfFileOnly(ofd.SafeFileName.ToString(), ".font") + "(" + file_n.ToString() + ").pvr";
            }

            if (saveFD.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(saveFD.FileName, FileMode.Create);
                Methods.DeleteCurrentFile(saveFD.FileName);

                switch (font.NewFormat)
                {
                    case true:
                        fs.Write(font.NewTex[file_n].Tex.Content, 0, font.NewTex[file_n].Tex.Content.Length);
                        break;

                    default:
                        fs.Write(font.tex[file_n].Content, 0, font.tex[file_n].Content.Length);
                        break;
                }

                fs.Close();
            }
        }

        private void importDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int file_n = dataGridViewWithTextures.SelectedCells[0].RowIndex;
            OpenFileDialog openFD = new OpenFileDialog();

            openFD.Filter = "dds files (*.dds)|*.dds";

            /*if (iOS == true && version_used < 9)
            {
                openFD.Filter = "PVR files (*.pvr)|*.pvr";
            }
            else if (iOS == false && version_used < 9)
            {
                openFD.Filter = "dds files (*.dds)|*.dds";
            }
            else openFD.Filter = "DDS-files (*.dds)|*.dds|PVR-files (*.pvr)|*.pvr";*/


            if (openFD.ShowDialog() == DialogResult.OK)
            {
                int meta_size = 0;

                #region Old code
                /*//Решил сначала проверить заголовок, а потом приниматься за копирование данных о ширине и высоте текстуры,
                //ну и длине самой текстуры. А то может выйти непонятно что.

                if (version_used < 9)
                {
                    if (iOS)
                    {
                        ffs.dds[file_n].height_in_dds = new byte[4];
                        Array.Copy(temp, 24, ffs.dds[file_n].height_in_dds, 0, 4);
                        ffs.dds[file_n].widht_in_dds = new byte[4];
                        Array.Copy(temp, 28, ffs.dds[file_n].widht_in_dds, 0, 4);

                        byte[] Binmeta_size = new byte[4];
                        Array.Copy(temp, 48, Binmeta_size, 0, 4);
                        meta_size = BitConverter.ToInt32(Binmeta_size, 0);
                    }
                    else
                    {
                        ffs.dds[file_n].height_in_dds = new byte[4];
                        Array.Copy(temp, 12, ffs.dds[file_n].height_in_dds, 0, 4);
                        ffs.dds[file_n].widht_in_dds = new byte[4];
                        Array.Copy(temp, 16, ffs.dds[file_n].widht_in_dds, 0, 4);
                    }

                    byte[] checkHeader = new byte[8];
                    Array.Copy(temp, 8, checkHeader, 0, 8);

                    if (iOS == true)
                    {
                        switch (BitConverter.ToUInt64(checkHeader, 0)) //Зачем-то делал и забросил. Потом посмотрю. Точно эта проверка для iOS версий.
                        {
                            case 0x404040461626772:
                                byte[] texType = new byte[4];
                                texType = BitConverter.GetBytes(0x8010);

                                break;
                            default:
                                MessageBox.Show("I don't know. Please send me a file if you see this error.");
                                break;
                        }
                    }
                    
                    

                    ffs.dds[file_n].dds_content = temp;
                    if (iOS == false) ffs.dds[file_n].size_in_font = BitConverter.GetBytes(temp.Length);
                }
                else if (version_used >= 9)
                {
                    int num = 4; //Номер массива head под код формата текстуры
                    int height_num = 3; //для массива с высотой
                    int width_num = 2; //с шириной
                    int size_in_font1 = 19; //размер текстуры 1
                    int size_in_font2 = 21; //размер текстуры 2. Зачем это сделали теллтейлы, известно только им.

                    switch (version_used) //Подправка для каждой версии. Не придумал ничего лучше.
                    {
                        case 10:
                            size_in_font1 = 20;
                            size_in_font2 = 23;
                            break;
                        case 11:
                            num = 5;
                            height_num = 4;
                            width_num = 3;
                            size_in_font1 = 24;
                            size_in_font2 = 28;
                            break;
                        case 12:
                            num = 5;
                            height_num = 4;
                            width_num = 3;
                            size_in_font1 = 24;
                            size_in_font2 = 28;
                            break;
                        case 13:
                            num = 6;
                            size_in_font1 = 29;
                            size_in_font2 = 33;
                            break;
                        case 14:
                        case 15:
                            width_num = 0;
                            height_num = 1;
                            size_in_font1 = 27;
                            size_in_font2 = 31;
                            break;
                        case 16:
                            width_num = 1;
                            height_num = 2;
                            num = 3;
                            size_in_font1 = 23;
                            size_in_font2 = 27;
                            break;
                    }

                    byte[] code = new byte[4];
                    byte[] dds_size = new byte[4];
                    byte[] dds_krat = new byte[4];
                    int kratnostPos = size_in_font2 + 1;
                    byte[] width = new byte[4];
                    byte[] height = new byte[4];


                    byte[] tempContent = TextureWorker.getFontHeader(temp, ref code, ref width, ref height, ref dds_size, ref dds_krat);

                    if (tempContent != null)
                    {
                        if(platform == 15)
                        {
                            tempContent = Swizzle.NintendoSwizzle(tempContent, BitConverter.ToInt32(width, 0), BitConverter.ToInt32(height, 0), BitConverter.ToInt32(code, 0), false);
                        }

                        ffs.dds[file_n].widht_in_dds = width;
                        ffs.dds[file_n].height_in_dds = height;
                        ffs.dds[file_n].pn2dds_head[num] = code;
                        ffs.dds[file_n].pn2dds_head[width_num] = ffs.dds[file_n].widht_in_dds;
                        ffs.dds[file_n].pn2dds_head[height_num] = ffs.dds[file_n].height_in_dds;
                        ffs.dds[file_n].size_in_font = dds_size;
                        ffs.dds[file_n].pn2dds_head[size_in_font1] = ffs.dds[file_n].size_in_font;
                        ffs.dds[file_n].pn2dds_head[size_in_font2] = ffs.dds[file_n].size_in_font;
                        if (version_used == 13) ffs.dds[file_n].pn2dds_head[size_in_font2 + 2] = ffs.dds[file_n].size_in_font; //Больше... БОЛЬШЕ КОСТЫЛЕЙ! УАХАХААХАХАХ!!
                        ffs.dds[file_n].size_of_dds = ffs.dds[file_n].size_in_font;
                        ffs.dds[file_n].height_in_font = ffs.dds[file_n].height_in_dds;
                        ffs.dds[file_n].widht_in_font = ffs.dds[file_n].widht_in_font;
                        ffs.dds[file_n].pn2dds_head[kratnostPos] = dds_krat;
                        ffs.dds[file_n].dds_content = tempContent;
                    }
                    else
                    {
                        MessageBox.Show("Unknown texture format. Please contact me.", "Error");
                        return;
                    }
                }*/
                #endregion

                ReplaceTexture(openFD.FileName, file_n, font);

                //fillTableOfTextures();
                fillTableofTextures(font);
                edited = true; //Отмечаем, что шрифт изменился
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFD = new SaveFileDialog();
            saveFD.Filter = "font files (*.font)|*.font";
            saveFD.FileName = ofd.SafeFileName.ToString();
            if (saveFD.ShowDialog() == DialogResult.OK)
            {
                Methods.DeleteCurrentFile((saveFD.FileName));
                FileStream fs = new FileStream((saveFD.FileName), FileMode.OpenOrCreate);
                SaveFont(fs, font);
                fs.Close();

                encFunc(saveFD.FileName);

                edited = false; //Файл сохранили, так что вернули флаг на ЛОЖЬ
            }
        }

        private void FontEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (edited == true)
            {
                DialogResult status = MessageBox.Show("Save font before closing Font Editor?", "Exit", MessageBoxButtons.YesNoCancel);
                if (status == DialogResult.Cancel)
                // если (состояние == DialogResult.Отмена) 
                {
                    e.Cancel = true; // Отмена = истина 
                }
                else if (status == DialogResult.Yes) //Если (состояние == DialogResult.Да)
                {
                    FileStream fs = new FileStream(ofd.SafeFileName, FileMode.Create); //Сохраняем в открытый файл.
                    SaveFont(fs, font);
                    //После соханения чистим списки
                }
                else //А иначе просто закрываем программу и чистим списки
                {
                }
            }
        }

        private void dataGridViewWithCoord_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int end_edit_column = e.ColumnIndex;
            int end_edit_row = e.RowIndex;
            if (old_data != "")
            {
                if ((end_edit_column >= 2 && end_edit_column <= dataGridViewWithCoord.ColumnCount) && Methods.IsNumeric(dataGridViewWithCoord[end_edit_column, end_edit_row].Value.ToString()))
                {
                    if (dataGridViewWithCoord[end_edit_column, end_edit_row].Value.ToString() != old_data)
                    {
                        if (end_edit_column == 2 || end_edit_column == 3)
                        {
                            dataGridViewWithCoord[7, end_edit_row].Value = (Convert.ToInt32(dataGridViewWithCoord[3, end_edit_row].Value) - Convert.ToInt32(dataGridViewWithCoord[2, end_edit_row].Value));
                        }
                        else if (end_edit_column == 4 || end_edit_column >= 5)
                        {
                            dataGridViewWithCoord[8, end_edit_row].Value = (Convert.ToInt32(dataGridViewWithCoord[5, end_edit_row].Value) - Convert.ToInt32(dataGridViewWithCoord[4, end_edit_row].Value));
                        }
                        else if (end_edit_column == 6)
                        {
                            int temp = Convert.ToInt32(dataGridViewWithCoord[end_edit_column, end_edit_row].Value);
                            /*if (temp >= ffs.dds.Count)
                            {
                                dataGridViewWithCoord[end_edit_column, end_edit_row].Value = old_data;
                            }*/
                        }
                    }
                }
                else
                {
                    dataGridViewWithCoord[end_edit_column, end_edit_row].Value = old_data;
                }
            }
        }
        public static string old_data;

        private void dataGridViewWithCoord_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            int now_edit_column = e.ColumnIndex;
            int now_edit_row = e.RowIndex;
            old_data = dataGridViewWithCoord[now_edit_column, now_edit_row].Value.ToString();
        }

        private void dataGridViewWithCoord_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewWithCoord.Rows[e.RowIndex].Selected = true;
            }
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // получаем координаты
                Point pntCell = dataGridViewWithCoord.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                pntCell.X += e.Location.X;
                pntCell.Y += e.Location.Y;

                // вызываем менюшку
                contextMenuStripExp_imp_Coord.Show(dataGridViewWithCoord, pntCell);
            }
        }

        private void exportCoordinatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportCoordinatesToolStripMenuItem1_Click(sender, e);
        }

        private void importCoordinatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Old code
            /*OpenFileDialog openFD = new OpenFileDialog();
            openFD.Filter = "txt files (*.txt)|*.txt";
            openFD.FileName = Methods.GetNameOfFileOnly(ofd.SafeFileName, ".font") + ".txt";

            if (openFD.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFD.FileName, System.Text.ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N));
                string curLine = null;
                while ((curLine = sr.ReadLine()) != null)
                {
                    string[] ch = curLine.Split(' ');
                    if (Methods.IsNumeric(ch[0]) && ch.Length >= 6)//8)
                    {
                        //int i = Convert.ToInt32(ch[0]);
                        int k;
                        for (int i = 0; i < dataGridViewWithCoord.RowCount; i++) //Чтобы не было вылетов при импорте, я сделал цикл (по сути, костыль)
                        {
                            if (Convert.ToInt32(dataGridViewWithCoord[0, i].Value) == Convert.ToInt32(ch[0]))//И проверку на наличие id символа (всё равно этот метод нужен будет только
                            {
                                k = Convert.ToInt32(ch[0]);
                                if (k >= dataGridViewWithCoord.RowCount) k = i;
                                dataGridViewWithCoord[0, k].Value = ch[0];      //для импорта координат со старых шрифтов
                                dataGridViewWithCoord[2, k].Value = ch[1];
                                dataGridViewWithCoord[3, k].Value = ch[2];
                                dataGridViewWithCoord[4, k].Value = ch[3];
                                dataGridViewWithCoord[5, k].Value = ch[4];

                                if (dataGridViewWithCoord.ColumnCount >= 7)
                                {
                                    dataGridViewWithCoord[6, k].Value = ch[5];

                                    if (version_used >= 9)
                                    {
                                        dataGridViewWithCoord[10, k].Value = 0;
                                        dataGridViewWithCoord[11, k].Value = 0;
                                        dataGridViewWithCoord[12, k].Value = Convert.ToInt32(ch[3]) - Convert.ToInt32(ch[2]);
                                    }

                                    if (version_used != -1 && dataGridViewWithCoord.ColumnCount > 7)
                                    {
                                        dataGridViewWithCoord[7, k].Value = Convert.ToInt32(ch[3]) - Convert.ToInt32(ch[2]);
                                        dataGridViewWithCoord[8, k].Value = Convert.ToInt32(ch[5]) - Convert.ToInt32(ch[4]);
                                    }
                                }

                                for (int j = 0; j < dataGridViewWithCoord.ColumnCount; j++)
                                {
                                    dataGridViewWithCoord[j, k].Style.BackColor = System.Drawing.Color.Beige;
                                }
                            }
                        }
                        //dataGridViewWithCoord[1, i].Value = ch[1];

                    }
                }
                sr.Close();
                edited = true; //Измененный шрифт
            }*/
            #endregion
        }

        private void exportCoordinatesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "FNT file (*.fnt) | *.fnt";
            sfd.FileName = font.FontName + ".fnt";

            if(sfd.ShowDialog() == DialogResult.OK)
            {
                string info = "info face=\"" + font.FontName + "\" size=" + font.BaseSize + " bold=0 italic=0 charset=\"\" unicode=";
                switch (font.NewFormat)
                {
                    case true:
                        info += "1\r\n";
                        break;

                    default:
                        info += "0\r\n";
                        break;
                }
                
                info += "common lineHeight=" + font.BaseSize + " base=" + font.BaseSize + " pages=" + font.TexCount + "\r\n";

                if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                FileStream fs = new FileStream(sfd.FileName, FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(info);
                info = "";

                for(int i = 0; i < font.TexCount; i++)
                {
                    info = "page id=" + i + " file=\"" + font.FontName + "_" + i + ".dds\"\r\n";
                    sw.Write(info);
                }

                info = "chars count=" + font.glyph.CharCount + "\r\n";
                sw.Write(info);

                if (!font.NewFormat)
                {
                    for(int i = 0; i < font.glyph.CharCount; i++)
                    {
                        info = "char id=" + i + " x=" + font.glyph.chars[i].XStart + " y=" + font.glyph.chars[i].YStart;
                        info += " width=";

                        if (font.hasScaleValue)
                        {
                            info += font.glyph.chars[i].CharWidth;
                        }
                        else
                        {
                            info += font.glyph.chars[i].XEnd - font.glyph.chars[i].XStart;
                        }

                        info += " height=";

                        if (font.hasScaleValue)
                        {
                            info += font.glyph.chars[i].CharHeight;
                        }
                        else
                        {
                            info += font.glyph.chars[i].YEnd - font.glyph.chars[i].YStart;
                        }

                        info += " xoffset=0 yoffset=0 xadvance=";

                        if (font.hasScaleValue)
                        {
                            info += font.glyph.chars[i].CharWidth;
                        }
                        else
                        {
                            info += font.glyph.chars[i].XEnd - font.glyph.chars[i].XStart;
                        }

                        info += " page=" + font.glyph.chars[i].TexNum + " chnl=15\r\n";

                        sw.Write(info);
                    }
                }
                else
                {
                    for (int i = 0; i < font.glyph.CharCount; i++)
                    {
                        info = "char id=" + font.glyph.charsNew[i].charId + " x=" + font.glyph.charsNew[i].XStart + " y=" + font.glyph.charsNew[i].YStart;
                        info += " width=" + font.glyph.charsNew[i].CharWidth + " height=" + font.glyph.charsNew[i].CharHeight;
                        info += " xoffset=" + font.glyph.charsNew[i].XOffset + " yoffset=" + font.glyph.charsNew[i].YOffset + " xadvance=";
                        info += font.glyph.charsNew[i].XAdvance + " page=" + font.glyph.charsNew[i].TexNum + " chnl=" + font.glyph.charsNew[i].Channel + "\r\n";

                        sw.Write(info);
                    }
                }

                sw.Close();
                fs.Close();
            }
        }

        private void importCoordinatesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            /*OpenFileDialog openFD = new OpenFileDialog();
            openFD.Filter = "txt files (*.txt)|*.txt";
            //openFD.FileName = Methods.GetNameOfFileOnly(ofd.SafeFileName, ".font") + ".txt";

            if (openFD.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFD.FileName, System.Text.ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N));
                string curLine = null;
                while ((curLine = sr.ReadLine()) != null)
                {
                    string[] str = curLine.Split('"');
                    int xStart = Convert.ToInt32(str[3]);
                    int yStart = Convert.ToInt32(str[5]);
                    int widht = Convert.ToInt32(str[7]);
                    int height = Convert.ToInt32(str[9]);

                    int i = Convert.ToInt32(str[1]);
                    dataGridViewWithCoord[2, i].Value = xStart;
                    dataGridViewWithCoord[3, i].Value = xStart + widht;
                    dataGridViewWithCoord[4, i].Value = yStart;
                    dataGridViewWithCoord[5, i].Value = yStart + height;
                    dataGridViewWithCoord[6, i].Value = dataGridViewWithTextures.SelectedCells[0].RowIndex.ToString();

                    if (version_used != -1)
                    {
                        dataGridViewWithCoord[7, i].Value = widht;
                        dataGridViewWithCoord[8, i].Value = height;
                    }
                    for (int j = 0; j < dataGridViewWithCoord.ColumnCount; j++)
                    {
                        dataGridViewWithCoord[j, i].Style.BackColor = System.Drawing.Color.Beige;
                    }
                }
                sr.Close();
                edited = true; //Шрифт изменился
            }*/
        }

        public void importCoordinatesFromFontStudioxmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region OldCode
            /*OpenFileDialog openFD = new OpenFileDialog();
            openFD.Filter = "xml files (*.xml)|*.xml";

            if (openFD.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFD.FileName, System.Text.ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N));

                string curLine = null;
                while ((curLine = sr.ReadLine()) != null)
                {
                    if (curLine.IndexOf("chardata char") > 0)
                    {
                        if (version_used > 9)//этот блок намеренно не задействован, тут импортируются ВСЕ буквы, которых нет в шрифте. Т.е. кириллица встает отдельно.
                        {
                            string[] str = curLine.Split('"');
                            Int32 charN = System.Convert.ToInt32(str[1]);
                            byte[] q = new byte[4];
                            char s = (char)charN;
                            q = BitConverter.GetBytes(charN);
                            Single xStart = System.Convert.ToSingle(str[17]) * BitConverter.ToInt32(ffs.dds[dataGridViewWithTextures.SelectedCells[0].RowIndex].widht_in_dds, 0);
                            Single yStart = System.Convert.ToSingle(str[19]) * BitConverter.ToInt32(ffs.dds[dataGridViewWithTextures.SelectedCells[0].RowIndex].height_in_dds, 0);
                            Single xEnd = System.Convert.ToSingle(str[21]) * BitConverter.ToInt32(ffs.dds[dataGridViewWithTextures.SelectedCells[0].RowIndex].widht_in_dds, 0);
                            Single yEnd = System.Convert.ToSingle(str[23]) * BitConverter.ToInt32(ffs.dds[dataGridViewWithTextures.SelectedCells[0].RowIndex].height_in_dds, 0);

                            int widht = System.Convert.ToInt32(str[13]);
                            int height = System.Convert.ToInt32(str[15]);

                            //try
                            {
                                int i = -1;
                                for (int o = 0; o < dataGridViewWithCoord.RowCount; o++)
                                {
                                    int t = (int)dataGridViewWithCoord[0, o].Value;//((char)Convert.ToInt32(dataGridViewWithCoord[0, o].Value)).ToString();

                                    if (charN == t)
                                    {
                                        i = o;
                                        break;
                                        //MessageBox.Show(c);
                                    }
                                }

                                if (i == -1)
                                {
                                    dataGridViewWithCoord.Rows.Add();
                                    i = dataGridViewWithCoord.Rows.Count - 1;
                                    dataGridViewWithCoord[0, i].Value = charN;
                                    dataGridViewWithCoord[1, i].Value = s;
                                    dataGridViewWithCoord[9, i].Value = dataGridViewWithCoord[9, 0].Value;
                                    byte[] nil = new byte[4];
                                    ffs.AddCoord(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, nil);
                                    ffs.coord[i].symbol = q;
                                }

                                if (i > -1)
                                {
                                    dataGridViewWithCoord[2, i].Value = xStart;
                                    dataGridViewWithCoord[3, i].Value = xEnd;
                                    dataGridViewWithCoord[4, i].Value = yStart;
                                    dataGridViewWithCoord[5, i].Value = yEnd;
                                    dataGridViewWithCoord[6, i].Value = dataGridViewWithTextures.SelectedCells[0].RowIndex.ToString();
                                    dataGridViewWithCoord[7, i].Value = widht;
                                    dataGridViewWithCoord[8, i].Value = height;
                                    if (rbKerning.Checked)
                                    {
                                        //dataGridViewWithCoord[9, i].Value = 0;
                                        dataGridViewWithCoord[10, i].Value = str[3];
                                        dataGridViewWithCoord[11, i].Value = 0;//до верха буквы
                                        dataGridViewWithCoord[12, i].Value = (Convert.ToInt32(str[5]) + Convert.ToInt32(str[7])).ToString();
                                    }
                                    else
                                    {
                                        dataGridViewWithCoord[10, i].Value = 0;
                                        dataGridViewWithCoord[11, i].Value = 0;//до верха буквы
                                        dataGridViewWithCoord[12, i].Value = widht;
                                    }

                                    for (int j = 0; j < dataGridViewWithCoord.ColumnCount; j++)
                                    {
                                        dataGridViewWithCoord[j, i].Style.BackColor = System.Drawing.Color.Green;
                                    }
                                }
                            }
                        }
                        else//старые версии
                        {
                            string[] str = curLine.Split('"');
                            Int32 charN = System.Convert.ToInt32(str[1]);
                            string c = ((char)charN).ToString();
                            byte[] q = ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(c.ToString());
                            string w = ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(q);

                            byte[] b = BitConverter.GetBytes(charN);

                            Single xStart = System.Convert.ToSingle(str[17]) * BitConverter.ToInt32(ffs.dds[dataGridViewWithTextures.SelectedCells[0].RowIndex].widht_in_dds, 0);
                            Single yStart = System.Convert.ToSingle(str[19]) * BitConverter.ToInt32(ffs.dds[dataGridViewWithTextures.SelectedCells[0].RowIndex].height_in_dds, 0);
                            Single xEnd = System.Convert.ToSingle(str[21]) * BitConverter.ToInt32(ffs.dds[dataGridViewWithTextures.SelectedCells[0].RowIndex].widht_in_dds, 0);
                            Single yEnd = System.Convert.ToSingle(str[23]) * BitConverter.ToInt32(ffs.dds[dataGridViewWithTextures.SelectedCells[0].RowIndex].height_in_dds, 0);

                            int widht = System.Convert.ToInt32(str[13]);
                            int height = System.Convert.ToInt32(str[15]);
                            //string tempCharUnicode = ASCIIEncoding.Unicode.GetString(b);
                            //try
                            {
                                int i = -1;
                                for (int o = 0; o < dataGridViewWithCoord.RowCount; o++)
                                {
                                    string t = dataGridViewWithCoord[1, o].Value.ToString();//((char)Convert.ToInt32(dataGridViewWithCoord[0, o].Value)).ToString();

                                    if (c == t[0].ToString())
                                    {
                                        i = o;
                                        break;
                                    }
                                }
                                if (version_used >= 9)
                                {
                                    if (i == -1)
                                    {
                                        dataGridViewWithCoord.Rows.Add();
                                        i = dataGridViewWithCoord.Rows.Count - 1;
                                        dataGridViewWithCoord[0, i].Value = Convert.ToInt32(q[0]);
                                        dataGridViewWithCoord[1, i].Value = w;
                                        dataGridViewWithCoord[9, i].Value = dataGridViewWithCoord[9, 0].Value;
                                        byte[] nil = new byte[4];
                                        ffs.AddCoord(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, nil);
                                        ffs.coord[i].symbol[0] = q[0];
                                    }
                                }

                                if (i > -1)
                                {
                                    dataGridViewWithCoord[2, i].Value = xStart;
                                    dataGridViewWithCoord[3, i].Value = xEnd;
                                    dataGridViewWithCoord[4, i].Value = yStart;
                                    dataGridViewWithCoord[5, i].Value = yEnd;
                                    dataGridViewWithCoord[6, i].Value = dataGridViewWithTextures.SelectedCells[0].RowIndex.ToString();
                                    if (version_used >= 6)
                                    {
                                        dataGridViewWithCoord[7, i].Value = widht;
                                        dataGridViewWithCoord[8, i].Value = height;
                                    }
                                    if (version_used >= 9)
                                    {
                                        if (rbKerning.Checked)
                                        {
                                            //dataGridViewWithCoord[9, i].Value = 0;
                                            dataGridViewWithCoord[10, i].Value = str[3];
                                            dataGridViewWithCoord[11, i].Value = 0;//до верха буквы
                                            dataGridViewWithCoord[12, i].Value = (Convert.ToInt32(str[5]) + Convert.ToInt32(str[7])).ToString();
                                        }
                                        else
                                        {
                                            dataGridViewWithCoord[10, i].Value = 0;
                                            dataGridViewWithCoord[11, i].Value = 0;//до верха буквы
                                            dataGridViewWithCoord[12, i].Value = widht;
                                        }
                                    }

                                    for (int j = 0; j < dataGridViewWithCoord.ColumnCount; j++)
                                    {
                                        dataGridViewWithCoord[j, i].Style.BackColor = System.Drawing.Color.Green;
                                    }
                                }
                            }
                        }
                    }
                }
                ///
                sr.Close();
                edited = true; //Шрифт изменился
            }*/
            #endregion
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Methods.IsNumeric(textBox1.Text))
            {
                int w = Convert.ToInt32(textBox1.Text);
                for (int i = 0; i < dataGridViewWithCoord.RowCount; i++)
                {
                    if (radioButtonXend.Checked)
                    {
                        dataGridViewWithCoord[3, i].Value = Convert.ToInt32(dataGridViewWithCoord[3, i].Value) + w;
                    }
                    else
                    {
                        dataGridViewWithCoord[2, i].Value = Convert.ToInt32(dataGridViewWithCoord[2, i].Value) + w;
                    }
                    dataGridViewWithCoord[7, i].Value = Convert.ToInt32(dataGridViewWithCoord[7, i].Value) + w;
                    dataGridViewWithCoord[12, i].Value = Convert.ToInt32(dataGridViewWithCoord[12, i].Value) + w;
                }
            }
        }

        private void toolStripImportFNT_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFD = new OpenFileDialog();
            openFD.Filter = "fnt files (*.fnt)|*.fnt";

            if (openFD.ShowDialog() == DialogResult.OK)
            {
                #region old code
                /*StreamReader sr = new StreamReader(openFD.FileName, System.Text.UnicodeEncoding.GetEncoding(MainMenu.settings.ASCII_N));//System.Text.ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N));

                bool unicode = true;

                string curLine = null;
                while ((curLine = sr.ReadLine()) != null)
                {
                    if (curLine.IndexOf("unicode=") > 0)
                    {
                        string[] test = curLine.Split('\"');
                        if (Convert.ToInt32(test[11]) == 0) unicode = false;
                    }
                    if (curLine.IndexOf("char id") > 0)
                    {

                        string[] str = curLine.Split('"');
                        Int32 charN = System.Convert.ToInt32(str[1]);
                        string c = ((char)charN).ToString();
                        byte[] q;
                        string w;

                        if (TTG_Tools.MainMenu.settings.unicodeSettings > 0)
                        {
                            q = ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(c.ToString());
                            w = ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(q);
                        }
                        else
                        {
                            q = new byte[4];
                            q = BitConverter.GetBytes(charN);
                            w = Encoding.Unicode.GetString(q);
                        }

                        byte[] b = BitConverter.GetBytes(charN);

                        Single xStart = System.Convert.ToInt32(str[3]);
                        Single yStart = System.Convert.ToInt32(str[5]);
                        Single xEnd = (System.Convert.ToInt32(str[3]) + System.Convert.ToInt32(str[7]));
                        Single yEnd = (System.Convert.ToInt32(str[5]) + System.Convert.ToInt32(str[9]));
                        int widht = System.Convert.ToInt32(str[7]);
                        int height = System.Convert.ToInt32(str[9]);
                        //string tempCharUnicode = ASCIIEncoding.Unicode.GetString(b);
                        //try
                        {
                            int i = -1;
                            for (int o = 0; o < dataGridViewWithCoord.RowCount; o++)
                            {
                                if (unicode)
                                {
                                    string t = dataGridViewWithCoord[1, o].Value.ToString();//((char)Convert.ToInt32(dataGridViewWithCoord[0, o].Value)).ToString();

                                    //MessageBox.Show(t);


                                    if (c == t[0].ToString())
                                    {
                                        i = o;
                                        break;
                                        //MessageBox.Show(c);
                                    }
                                }
                                else
                                {
                                    int id = Convert.ToInt32(dataGridViewWithCoord[0, o].Value);

                                    if (charN == id)
                                    {
                                        i = o;
                                        break;
                                    }
                                }
                            }
                            if (version_used >= 9)
                            {
                                if (i == -1)
                                {
                                    dataGridViewWithCoord.Rows.Add();
                                    i = dataGridViewWithCoord.Rows.Count - 1;

                                    if (unicode)
                                    {
                                        if ((version_used >= 11) && (MainMenu.settings.unicodeSettings == 0)) dataGridViewWithCoord[0, i].Value = BitConverter.ToInt32(q, 0);
                                        else dataGridViewWithCoord[0, i].Value = Convert.ToInt32(q[0]);
                                    }
                                    else
                                    {
                                        if ((version_used >= 11) && (MainMenu.settings.unicodeSettings == 0)) dataGridViewWithCoord[0, i].Value = BitConverter.ToInt32(q, 0);
                                        else dataGridViewWithCoord[0, i].Value = charN;
                                    }

                                    if (unicode) dataGridViewWithCoord[1, i].Value = w;
                                    else
                                    {
                                        byte[] bch = BitConverter.GetBytes(charN);
                                        string s = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(bch);
                                        dataGridViewWithCoord[1, i].Value = s;
                                    }


                                    dataGridViewWithCoord[9, i].Value = dataGridViewWithCoord[9, 0].Value;
                                    byte[] nil = new byte[4];
                                    ffs.AddCoord(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, nil);

                                    if ((version_used >= 11) && (MainMenu.settings.unicodeSettings == 0)) ffs.coord[i].symbol = q;
                                    else ffs.coord[i].symbol[0] = q[0];
                                }
                            }

                            if (i > -1)
                            {
                                dataGridViewWithCoord[2, i].Value = xStart;
                                dataGridViewWithCoord[3, i].Value = xEnd;
                                dataGridViewWithCoord[4, i].Value = yStart;
                                dataGridViewWithCoord[5, i].Value = yEnd;
                                dataGridViewWithCoord[6, i].Value = str[17];
                                if (version_used >= 6)
                                {
                                    dataGridViewWithCoord[7, i].Value = widht;
                                    dataGridViewWithCoord[8, i].Value = height;
                                }
                                if (version_used >= 9)
                                {
                                    if (rbKerning.Checked)
                                    {
                                        //dataGridViewWithCoord[9, i].Value = 0;
                                        dataGridViewWithCoord[10, i].Value = str[11];
                                        dataGridViewWithCoord[11, i].Value = str[13];//до верха буквы
                                        dataGridViewWithCoord[12, i].Value = str[15];
                                    }
                                    else
                                    {
                                        dataGridViewWithCoord[10, i].Value = 0;
                                        dataGridViewWithCoord[11, i].Value = 0;//до верха буквы
                                        dataGridViewWithCoord[12, i].Value = widht;
                                    }
                                }


                                for (int j = 0; j < dataGridViewWithCoord.ColumnCount; j++)
                                {
                                    dataGridViewWithCoord[j, i].Style.BackColor = System.Drawing.Color.Green;
                                }

                            }
                        }
                    }
                }
                sr.Close();
                edited = true; //Шрифт изменен*/
                #endregion

                FileInfo fi = new FileInfo(openFD.FileName);

                int charsCount = 0;

                string[] strings = File.ReadAllLines(fi.FullName);

                int ch = -1;

                //Check for xml tags and removing it for comfortable searching needed data (useful for xml fnt files)
                for (int n = 0; n < strings.Length; n++)
                {
                    if ((strings[n].IndexOf('<') >= 0) || (strings[n].IndexOf('<') >= 0 && strings[n].IndexOf('/') > 0))
                    {
                        strings[n] = strings[n].Remove(strings[n].IndexOf('<'), 1);
                        if (strings[n].IndexOf('/') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('/'), 1);
                    }
                    if (strings[n].IndexOf('>') >= 0 || (strings[n].IndexOf('/') >= 0 && strings[n + 1].IndexOf('>') > 0))
                    {
                        strings[n] = strings[n].Remove(strings[n].IndexOf('>'), 1);
                        if (strings[n].IndexOf('/') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('/'), 1);
                    }
                    if (strings[n].IndexOf('"') >= 0)
                    {
                        while (strings[n].IndexOf('"') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('"'), 1);
                    }
                }

                if (font.NewFormat)
                {
                    TextureClass.NewT3Texture[] tmpNewTex = null;

                    for (int m = 0; m < strings.Length; m++)
                    {
                        if (strings[m].ToLower().Contains("common lineheight"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "lineheight":
                                        font.BaseSize = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "pages":
                                        tmpNewTex = new TextureClass.NewT3Texture[Convert.ToInt32(splitted[k + 1])];

                                        if(Convert.ToInt32(splitted[k + 1]) > font.TexCount)
                                        {

                                            for(int j = 0; j < tmpNewTex.Length; j++)
                                            {
                                                tmpNewTex[j] = new TextureClass.NewT3Texture(font.NewTex[0]);
                                            }
                                        }
                                        else
                                        {
                                            for(int j = 0; j < tmpNewTex.Length; j++)
                                            {
                                                tmpNewTex[j] = new TextureClass.NewT3Texture(font.NewTex[j]);
                                            }
                                        }
                                        break;
                                }
                            }
                        }

                        if(strings[m].Contains("page id"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            int idNum = 0;

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "id":
                                        idNum = Convert.ToInt32(splitted[k + 1]);
                                        break;

                                    case "file":
                                        if(File.Exists(fi.DirectoryName + "\\" + splitted[k + 1]))
                                        {

                                            tmpNewTex[idNum].Tex.Content = File.ReadAllBytes(fi.DirectoryName + "\\" + splitted[k + 1]);
                                            
                                            tmpNewTex[idNum].textureSize = (uint)tmpNewTex[idNum].Tex.Content.Length - 128;
                                            tmpNewTex[idNum].Tex.TexSize = tmpNewTex[idNum].textureSize;
                                            tmpNewTex[idNum].Tex.Textures[0].MipSize = (int)tmpNewTex[idNum].textureSize;

                                            byte[] tmp = new byte[4];
                                            Array.Copy(tmpNewTex[idNum].Tex.Content, 12, tmp, 0, tmp.Length);
                                            tmpNewTex[idNum].Width = BitConverter.ToInt32(tmp, 0);

                                            tmp = new byte[4];
                                            Array.Copy(tmpNewTex[idNum].Tex.Content, 16, tmp, 0, tmp.Length);
                                            tmpNewTex[idNum].Height = BitConverter.ToInt32(tmp, 0);
                                        }
                                        break;
                                }
                            }
                        }

                        if (strings[m].Contains("chars count"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "count":
                                        font.glyph.CharCount = Convert.ToInt32(splitted[k + 1]);
                                        font.glyph.charsNew = new FontClass.ClassFont.TRectNew[font.glyph.CharCount];
                                        break;
                                }
                            }
                        }

                        if (strings[m].Contains("char id"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "id":
                                        ch++;
                                        font.glyph.charsNew[ch] = new FontClass.ClassFont.TRectNew();

                                        if (Convert.ToInt32(splitted[k + 1]) < 0)
                                        {
                                            font.glyph.charsNew[ch].charId = 0;
                                        }
                                        else
                                        {
                                            font.glyph.charsNew[ch].charId = Convert.ToUInt32(splitted[k + 1]);
                                        }
                                        break;

                                    case "x":
                                        font.glyph.charsNew[ch].XStart = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "y":
                                        font.glyph.charsNew[ch].YStart = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "width":
                                        font.glyph.charsNew[ch].CharWidth = Convert.ToSingle(splitted[k + 1]);
                                        font.glyph.charsNew[ch].XEnd = font.glyph.charsNew[ch].XStart + font.glyph.charsNew[ch].CharWidth;
                                        break;

                                    case "height":
                                        font.glyph.charsNew[ch].CharHeight = Convert.ToSingle(splitted[k + 1]);
                                        font.glyph.charsNew[ch].YEnd = font.glyph.charsNew[ch].YStart + font.glyph.charsNew[ch].CharHeight;
                                        break;

                                    case "xoffset":
                                        font.glyph.charsNew[ch].XOffset = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "yoffset":
                                        font.glyph.charsNew[ch].YOffset = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "xadvance":
                                        font.glyph.charsNew[ch].XAdvance = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "page":
                                        font.glyph.charsNew[ch].TexNum = Convert.ToInt32(splitted[k + 1]);
                                        break;

                                    case "chnl":
                                        font.glyph.charsNew[ch].Channel = Convert.ToInt32(splitted[k + 1]);
                                        break;
                                }
                            }
                        }
                    }

                    if(tmpNewTex != null)
                    {
                        font.NewTex = tmpNewTex;
                        font.TexCount = font.NewTex.Length;
                        fillTableofTextures(font);
                    }
                }
                else
                {
                    TextureClass.OldT3Texture[] tmpOldTex = null;

                    //Make all characters as first texture due bug after saving font if font was with multi textures and saves as font with a 1 texture.
                    for(int i = 0; i < font.glyph.CharCount; i++)
                    {
                        font.glyph.chars[i].TexNum = 0;
                    }

                    for (int m = 0; m < strings.Length; m++)
                    {
                        if (strings[m].ToLower().Contains("common lineheight"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "lineheight":
                                        font.BaseSize = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "pages":
                                        tmpOldTex = new TextureClass.OldT3Texture[Convert.ToInt32(splitted[k + 1])];

                                        if (Convert.ToInt32(splitted[k + 1]) > font.TexCount)
                                        {
                                            for(int c = 0; c < tmpOldTex.Length; c++)
                                            {
                                                tmpOldTex[c] = new TextureClass.OldT3Texture(font.tex[0]);
                                            }
                                        }
                                        else
                                        {
                                            for (int c = 0; c < tmpOldTex.Length; c++)
                                            {
                                                tmpOldTex[c] = new TextureClass.OldT3Texture(font.tex[c]);
                                            }
                                        }

                                        break;
                                }
                            }
                        }

                        if (strings[m].Contains("page id"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            int idNum = 0;

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "id":
                                        idNum = Convert.ToInt32(splitted[k + 1]);
                                        break;

                                    case "file":
                                        if (File.Exists(fi.DirectoryName + "\\" +  splitted[k + 1]))
                                        {
                                            tmpOldTex[idNum].Content = File.ReadAllBytes(fi.DirectoryName + "\\" + splitted[k + 1]);
                                            
                                            byte[] tmp = new byte[4];
                                            Array.Copy(tmpOldTex[idNum].Content, 12, tmp, 0, tmp.Length);
                                            tmpOldTex[idNum].Height = BitConverter.ToInt32(tmp, 0);
                                            tmpOldTex[idNum].OriginalHeight = tmpOldTex[idNum].Height;

                                            tmp = new byte[4];
                                            Array.Copy(tmpOldTex[idNum].Content, 16, tmp, 0, tmp.Length);
                                            tmpOldTex[idNum].Width = BitConverter.ToInt32(tmp, 0);
                                            tmpOldTex[idNum].OriginalWidth = tmpOldTex[idNum].Width;

                                            tmpOldTex[idNum].TexSize = tmpOldTex[idNum].Content.Length;
                                        }
                                        break;
                                }
                            }
                        }

                        if (strings[m].Contains("char id"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "id":
                                        uint tmpChar = 0;

                                        if (Convert.ToInt32(splitted[k + 1]) < 0)
                                        {
                                            tmpChar = 0;
                                        }
                                        else
                                        {
                                            tmpChar = Convert.ToUInt32(splitted[k + 1]);
                                        }

                                        for(int t = 0; t < font.glyph.CharCount; t++)
                                        {
                                            if(Convert.ToUInt32(dataGridViewWithCoord[0, t].Value) == tmpChar)
                                            {
                                                ch = t;
                                                break;
                                            }
                                        }

                                        break;

                                    case "x":
                                        font.glyph.chars[ch].XStart = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "y":
                                        font.glyph.chars[ch].YStart = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "width":
                                        if (font.hasScaleValue)
                                        {
                                            font.glyph.chars[ch].CharWidth = Convert.ToSingle(splitted[k + 1]);
                                            font.glyph.chars[ch].XEnd = font.glyph.chars[ch].XStart + font.glyph.chars[ch].CharWidth;
                                        }
                                        else
                                        {
                                            font.glyph.chars[ch].XEnd = font.glyph.chars[ch].XStart + Convert.ToSingle(splitted[k + 1]);
                                        }
                                        break;

                                    case "height":
                                        if (font.hasScaleValue)
                                        {
                                            font.glyph.chars[ch].CharHeight = Convert.ToSingle(splitted[k + 1]);
                                            font.glyph.chars[ch].YEnd = font.glyph.chars[ch].YStart + font.glyph.chars[ch].CharHeight;
                                        }
                                        else
                                        {
                                            font.glyph.chars[ch].YEnd = font.glyph.chars[ch].YStart + Convert.ToSingle(splitted[k + 1]);
                                        }
                                        break;

                                    case "page":
                                        font.glyph.chars[ch].TexNum = Convert.ToInt32(splitted[k + 1]);
                                        break;
                                }
                            }
                        }
                    }

                    if (tmpOldTex != null)
                    {
                        font.tex = new TextureClass.OldT3Texture[tmpOldTex.Length];

                        for(int i = 0; i < font.tex.Length; i++)
                        {
                            font.tex[i] = new TextureClass.OldT3Texture(tmpOldTex[i]);
                        }

                        tmpOldTex = null;
                        GC.Collect();

                        font.TexCount = font.tex.Length;
                        fillTableofTextures(font);
                    }
                }

                fillTableofCoordinates(font, true);
            }

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void changeCoordinatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Old code
            /*OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Font txt coords (*.txt) | *.txt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                string tmp;
                string[] block;

                int index;

                while (!sr.EndOfStream)
                {
                    tmp = sr.ReadLine();
                    block = tmp.Split(' ');

                    if (block.Length == 6 || block.Length == 10 && Methods.IsNumeric(block[0]))
                    {
                        index = -1;

                        for (int i = 0; i < ffs.coord.Count; i++)
                        {
                            if (BitConverter.ToInt32(ffs.coord[i].symbol, 0) == Convert.ToInt32(block[0]))
                            {
                                index = i;
                                break;
                            }
                        }

                        if (index != -1)
                        {
                            int ch_id = Convert.ToInt32(block[0]);
                            int x = Convert.ToInt32(block[1]);
                            int x_end = Convert.ToInt32(block[2]);
                            int y = Convert.ToInt32(block[3]);
                            int y_end = Convert.ToInt32(block[4]);
                            int tex_num = Convert.ToInt32(block[5]);

                            int height = y_end - y;
                            int width = x_end - x;

                            int chnl = 0;
                            int x_offset = 0;
                            int y_offset = 0;
                            int x_advance = width;

                            if (version_used >= 9 && rbKerning.Checked && block.Length == 10)
                            {
                                chnl = Convert.ToInt32(block[6]);
                                x_offset = Convert.ToInt32(block[7]);
                                y_offset = Convert.ToInt32(block[8]);
                                x_advance = Convert.ToInt32(block[9]);
                            }

                            dataGridViewWithCoord[0, index].Value = Convert.ToInt32(ch_id);
                            dataGridViewWithCoord[1, index].Value = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(BitConverter.GetBytes(ch_id));
                            if (version_used >= 9 && MainMenu.settings.unicodeSettings == 0) dataGridViewWithCoord[1, index].Value = Encoding.Unicode.GetString(BitConverter.GetBytes(ch_id));
                            dataGridViewWithCoord[2, index].Value = x;
                            dataGridViewWithCoord[3, index].Value = x_end;
                            dataGridViewWithCoord[4, index].Value = y;
                            dataGridViewWithCoord[5, index].Value = y_end;
                            dataGridViewWithCoord[6, index].Value = tex_num;

                            if (version_used >= 9)
                            {
                                dataGridViewWithCoord[7, index].Value = width;
                                dataGridViewWithCoord[8, index].Value = height;
                                dataGridViewWithCoord[9, index].Value = chnl;
                                dataGridViewWithCoord[10, index].Value = x_offset;
                                dataGridViewWithCoord[11, index].Value = y_offset;
                                dataGridViewWithCoord[12, index].Value = x_advance;
                            }

                            for (int j = 0; j < dataGridViewWithCoord.ColumnCount; j++)
                            {
                                dataGridViewWithCoord[j, index].Style.BackColor = System.Drawing.Color.Bisque;
                            }
                        }
                        else
                        {
                            if (version_used >= 9)
                            {
                                int ch_id = Convert.ToInt32(block[0]);
                                int x = Convert.ToInt32(block[1]);
                                int x_end = Convert.ToInt32(block[2]);
                                int y = Convert.ToInt32(block[3]);
                                int y_end = Convert.ToInt32(block[4]);
                                int tex_num = Convert.ToInt32(block[5]);

                                int height = y_end - y;
                                int width = x_end - x;

                                int chnl = 0;
                                int x_offset = 0;
                                int y_offset = 0;
                                int x_advance = width;

                                if (rbKerning.Checked && block.Length == 10)
                                {
                                    chnl = Convert.ToInt32(block[6]);
                                    x_offset = Convert.ToInt32(block[7]);
                                    y_offset = Convert.ToInt32(block[8]);
                                    x_advance = Convert.ToInt32(block[9]);
                                }

                                dataGridViewWithCoord.Rows.Add();
                                index = dataGridViewWithCoord.RowCount - 1;

                                dataGridViewWithCoord[0, index].Value = Convert.ToInt32(ch_id);
                                dataGridViewWithCoord[1, index].Value = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(BitConverter.GetBytes(ch_id));
                                if (MainMenu.settings.unicodeSettings == 0) dataGridViewWithCoord[1, index].Value = Encoding.Unicode.GetString(BitConverter.GetBytes(ch_id));
                                dataGridViewWithCoord[2, index].Value = x;
                                dataGridViewWithCoord[3, index].Value = x_end;
                                dataGridViewWithCoord[4, index].Value = y;
                                dataGridViewWithCoord[5, index].Value = y_end;
                                dataGridViewWithCoord[6, index].Value = tex_num;

                                if (version_used >= 9)
                                {
                                    dataGridViewWithCoord[7, index].Value = width;
                                    dataGridViewWithCoord[8, index].Value = height;
                                    dataGridViewWithCoord[9, index].Value = chnl;
                                    dataGridViewWithCoord[10, index].Value = x_offset;
                                    dataGridViewWithCoord[11, index].Value = y_offset;
                                    dataGridViewWithCoord[12, index].Value = x_advance;
                                }

                                byte[] nil = new byte[4];
                                ffs.AddCoord(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, nil);

                                for (int j = 0; j < dataGridViewWithCoord.ColumnCount; j++)
                                {
                                    dataGridViewWithCoord[j, index].Style.BackColor = System.Drawing.Color.Aqua;
                                }
                            }
                        }

                        if (!edited) edited = true;
                    }
                }

                sr.Close();
                fs.Close();
            }*/
            #endregion
        }

        private void removeDuplicatesCharsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if(ffs != null && ffs.coord.Count > 0)
            {
                var tmp_array = ffs.coord.ToArray();
                Array.Sort(tmp_array, (tmp_ar1, tmp_ar2) => BitConverter.ToInt32(tmp_ar1.symbol, 0).CompareTo(BitConverter.ToInt32(tmp_ar2.symbol, 0)));
                ffs.coord = tmp_array.OfType<Coordinates>().ToList();

                ffs.coord = ffs.coord.GroupBy(i => BitConverter.ToInt32(i.symbol, 0)).Select(g => g.Last()).ToList();
                tmp_array = null;
                GC.Collect();
                fillTableOfCoordinates();
                if(!edited) edited = true;
            }*/
        }
    }
}
