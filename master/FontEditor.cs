﻿using System;
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
        FontStructure ffs = new FontStructure();
        bool edited; //Проверка на изменения в шрифте
        bool iOS; //Для поддержки pvr текстур
        bool encripted; //В случае, если шрифт был зашифрован
        int platform = -1; //Номер платформы

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void FontEditor_Load(object sender, EventArgs e)
        {
            edited = false; //Даем программе знать, что шрифт не изменен.
            iOS = false;

            #region//Шаблоны некоторых шрифтов.
            byte[] i = {
                           0xAC, 0xC7, 0x49, 0x03, 0x2C, 0x29, 0xC2, 0x04, 0x23, 0xFA, 0x4B, 0xAB, 0x95, 0x3E, 0xA6, 0xA9,
                           0x95, 0x38, 0x98, 0x86, 0xAA, 0xB3, 0xA0, 0x53, 0xEC, 0xF4, 0x33, 0x77, 0xE2, 0xCC, 0x38, 0x6F,
                           0x7E, 0x9E, 0x24, 0x3E, 0x3D, 0xD5, 0x38, 0x76, 0xE3, 0x88, 0x09, 0x7A, 0x48, 0x5D, 0x7F, 0x93,
                           0xEA, 0x82, 0x11, 0xD6, 0x8C, 0x59, 0x05, 0x84, 0xB7, 0xFB, 0x88, 0x8E, 0x4D, 0x79, 0xAB, 0x26,
                           0x07, 0x1A, 0x1F, 0xE6, 0x44, 0xA2, 0xBC, 0x7B, 0xAC, 0xD6, 0x8C, 0x2A, 0x0F, 0xF4, 0x20, 0xE6,
                           0x20, 0xBA, 0xA1, 0xEF, 0xF5, 0x7A, 0x6B, 0xFF, 0xEA, 0x0E, 0x30, 0xAE, 0xF1, 0x19, 0x46, 0x58,
                           0x07, 0x26, 0x57, 0xC2
                       };
            version_of_font.Add(new Version_of_font(i, "Wolf Among Us"));//6

            byte[] i2 =
            {
                           0x6B, 0x8C, 0x53, 0xFB, 0x2C, 0x29, 0xC2, 0x04,
                           0x23, 0xFA, 0x4B, 0xAB, 0x15, 0xA7, 0x5E, 0x33, 0x95, 0x38, 0x98, 0x86, 0xAA, 0xB3, 0xA0, 0x53,
                           0xEC, 0xF4, 0x33, 0x77, 0xE2, 0xCC, 0x38, 0x6F, 0x7E, 0x9E, 0x24, 0x3E, 0xD9, 0xC9, 0x14, 0xAA,
                           0xE3, 0x88, 0x09, 0x7A, 0x48, 0x5D, 0x7F, 0x93, 0xA8, 0xB2, 0x29, 0xD3
            };
            version_of_font.Add(new Version_of_font(i2, "Wolf Among Us PS4"));

            byte[] j = {
                           0xE1, 0xD1, 0xC0, 0x2B, 0x2C, 0x29, 0xC2, 0x04, 0x23, 0xFA, 0x4B, 0xAB, 0x15, 0xA7, 0x5E, 0x33
                       };
            version_of_font.Add(new Version_of_font(j, "Tales From the Borderlands"));//7
            version_used = -1; //шрифты, не вошедшие в шаблоны
            #endregion

        }

        public List<Version_of_font> version_of_font = new List<Version_of_font>();
        public List<byte[]> head = new List<byte[]>();
        public ClassesStructs.FlagsClass fontFlags;
        FontClass.ClassFont font = null;

        byte[] start_version = { 0x81, 0x53, 0x37, 0x63, 0x9E, 0x4A, 0x3A, 0x9A }; //указывается начало заголовка. Не знаю, как можно было бы позицию по байтам сделать. Сделал по строке.

        public class FontStructure
        {
            public byte[] header_of_file;
            public byte[] dds_lenght;
            public byte[] count_dds;
            public byte[] coord_lenght;
            public byte[] coord_count;
            public List<Coordinates> coord = new List<Coordinates>();
            public List<Dds> dds = new List<Dds>();

            public FontStructure() { }
            
            public void AddCoord(byte[] _h_start, byte[] _h_end, byte[] _w_start, byte[] _w_end, byte[] _n_texture, byte[] _widht, byte[] _height, byte[] _HZ, byte[] _kern_left_side, byte[] _to_top, byte[] _widht_with_kern, byte[] _symbol)
            {
                coord.Add(new Coordinates(_h_start, _h_end, _w_start, _w_end, _n_texture, _widht, _height, _HZ, _kern_left_side, _to_top, _widht_with_kern, _symbol));
            }
            public void AddDds(byte[] _header, byte[] _height_in_font, byte[] _widht_in_font,
                byte[] _size_in_font, byte[] _hz_data_after_sizes, byte[] _dds_content,
                byte[] _height_in_dds, byte[] _widht_in_dds, byte[] _size_of_dds, List<byte[]> _pn2dds_head)
            {
                dds.Add(new Dds(_header, _height_in_font, _widht_in_font, _size_in_font, _hz_data_after_sizes,
                    _dds_content, _height_in_dds, _widht_in_dds, _size_of_dds, _pn2dds_head));
            }
        }

        public class Dds
        {
            public byte[] header;
            public byte[] height_in_font;//4 байта
            public byte[] widht_in_font;//4 байта
            public byte[] size_in_font;//4 байта
            public byte[] hz_data_after_sizes; //47
            public byte[] dds_content;
            //потом пропустить 12 байт и получить это
            public byte[] height_in_dds;//4 байта
            public byte[] widht_in_dds;//4 байта
            //это самому посчитать
            public byte[] size_of_dds;//4 байта
            public List<byte[]> pn2dds_head;//для Покера

            public Dds() { }

            public Dds(byte[] _header, byte[] _height_in_font, byte[] _widht_in_font,
                byte[] _size_in_font, byte[] _hz_data_after_sizes, byte[] _dds_content,
                byte[] _height_in_dds, byte[] _widht_in_dds, byte[] _size_of_dds, List<byte[]> _pn2dds_head)
            {
                this.header = _header;
                this.height_in_dds = _height_in_dds;
                this.height_in_font = _height_in_font;
                this.hz_data_after_sizes = _hz_data_after_sizes;
                this.size_in_font = _size_in_font;
                this.widht_in_dds = _widht_in_dds;
                this.widht_in_font = _widht_in_font;
                this.size_of_dds = _size_of_dds;
                this.pn2dds_head = _pn2dds_head;
            }
        }

        public class Coordinates
        {
            public byte[] h_start;
            public byte[] h_end;
            public byte[] w_start;
            public byte[] w_end;
            public byte[] n_texture;
            public byte[] widht;
            public byte[] height;
            public byte[] HZ;
            public byte[] kern_left_side;
            public byte[] to_top;
            public byte[] widht_with_kern;
            public byte[] symbol;

            public Coordinates() { }

            public Coordinates(byte[] _h_start, byte[] _h_end, byte[] _w_start, byte[] _w_end,
                byte[] _n_texture, byte[] _widht, byte[] _height, byte[] _HZ, byte[] _kern_left_side, byte[] _to_top, byte[] _widht_with_kern, byte[] _symbol)
            {
                this.h_end = _h_end;
                this.h_start = _h_start;
                this.n_texture = _n_texture;
                this.w_end = _w_end;
                this.w_start = _w_start;
                this.widht = _widht;
                this.height = _height;
                this.HZ = _HZ;
                this.kern_left_side = _kern_left_side;
                this.to_top = _to_top;
                this.widht_with_kern = _widht_with_kern;
                this.symbol = _symbol;
            }
        }

        public class Version_of_font
        {
            public string games;
            public byte[] header;

            public Version_of_font() { }
            public Version_of_font(byte[] _header, string _games)
            {
                this.games = _games;
                this.header = _header;
            }
        }
        public static int version_used;

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

        private void fillTableOfCoordinates()
        {
            dataGridViewWithCoord.RowCount = ffs.coord.Count;
            //byte[] b = new byte[1];
            //b[0] = 0;

            if (version_used < 6)
            {
                dataGridViewWithCoord.ColumnCount = 7;
            }
            else if (version_used >= 6 && version_used < 9)
            {
                dataGridViewWithCoord.ColumnCount = 9;
            }

            //выводим в дата грид всю инфу
            for (int i = 0; i < ffs.coord.Count; i++)
            {
                int z = 0;
                if (ffs.coord[i].n_texture != null)
                {
                    z = BitConverter.ToInt32(ffs.coord[i].n_texture, 0);
                }

                int max_w = BitConverter.ToInt32(ffs.dds[z].widht_in_font, 0);
                int max_h = BitConverter.ToInt32(ffs.dds[z].height_in_font, 0);

                dataGridViewWithCoord[0, i].Value = i.ToString();

                UnicodeEncoding unicode = new UnicodeEncoding();
                dataGridViewWithCoord[1, i].Value = unicode.GetString(ffs.coord[i].symbol);


                //dataGridViewWithCoord[1, i].Value = unicode.GetString(b);
                // ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(b);
                double w_start = Math.Round(BitConverter.ToSingle(ffs.coord[i].w_start, 0) * max_w);
                dataGridViewWithCoord[2, i].Value = w_start;
                double w_end = Math.Round(BitConverter.ToSingle(ffs.coord[i].w_end, 0) * max_w);
                dataGridViewWithCoord[3, i].Value = w_end;
                if (w_end - w_start < 0)
                {
                    dataGridViewWithCoord[0, i].Style.BackColor = System.Drawing.Color.Red;
                }
                dataGridViewWithCoord[4, i].Value = Math.Round(BitConverter.ToSingle(ffs.coord[i].h_start, 0) * max_h);
                dataGridViewWithCoord[5, i].Value = Math.Round(BitConverter.ToSingle(ffs.coord[i].h_end, 0) * max_h);
                dataGridViewWithCoord[6, i].Value = BitConverter.ToInt32(ffs.coord[i].n_texture, 0);

                if (version_used < 9) //Если пользователь грузит шрифты до покера 2, то лишние столбцы скрываются (зачем они ему?)
                {
                    byte[] ch = BitConverter.GetBytes(i);
                    dataGridViewWithCoord[1, i].Value = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(ch).ToString();
                    /*dataGridViewWithCoord.Columns[7].Visible = false;
                    dataGridViewWithCoord.Columns[8].Visible = false;
                    dataGridViewWithCoord.Columns[9].Visible = false;
                    dataGridViewWithCoord.Columns[10].Visible = false;
                    dataGridViewWithCoord.Columns[11].Visible = false;
                    dataGridViewWithCoord.Columns[12].Visible = false;*/
                }
                /*else //А иначе они будут грузиться
                {
                    dataGridViewWithCoord.Columns[7].Visible = true;
                    dataGridViewWithCoord.Columns[8].Visible = true;
                    dataGridViewWithCoord.Columns[9].Visible = true;
                    dataGridViewWithCoord.Columns[10].Visible = true;
                    dataGridViewWithCoord.Columns[11].Visible = true;
                    dataGridViewWithCoord.Columns[12].Visible = true;
                }*/

                if (version_used != -1 && version_used >= 6)
                {
                    //dataGridViewWithCoord.Columns[7].Visible = true;
                    //dataGridViewWithCoord.Columns[8].Visible = true;
                    dataGridViewWithCoord[7, i].Value = Math.Round(BitConverter.ToSingle(ffs.coord[i].widht, 0));
                    dataGridViewWithCoord[8, i].Value = Math.Round(BitConverter.ToSingle(ffs.coord[i].height, 0));
                }
                /*else
                {
                    dataGridViewWithCoord.Columns[7].Visible = false;
                    dataGridViewWithCoord.Columns[8].Visible = false;
                }*/

                if (version_used >= 9)
                {
                    dataGridViewWithCoord.Columns[7].Visible = true;
                    dataGridViewWithCoord.Columns[8].Visible = true;
                    dataGridViewWithCoord[0, i].Value = BitConverter.ToInt32(ffs.coord[i].symbol, 0);

                    if (MainMenu.settings.unicodeSettings > 0)
                    {
                        dataGridViewWithCoord[1, i].Value = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(ffs.coord[i].symbol).ToString();//Methods.ConvertHexToString(ffs.coord[i].symbol, 0, 4, MainMenu.settings.ASCII_N, false);
                    }
                    else
                    {
                        dataGridViewWithCoord[1, i].Value = UnicodeEncoding.Unicode.GetString(ffs.coord[i].symbol, 0, 4);
                    }

                    dataGridViewWithCoord[2, i].Value = BitConverter.ToSingle(ffs.coord[i].w_start, 0) * max_w;
                    dataGridViewWithCoord[3, i].Value = BitConverter.ToSingle(ffs.coord[i].w_end, 0) * max_w;
                    dataGridViewWithCoord[4, i].Value = BitConverter.ToSingle(ffs.coord[i].h_start, 0) * max_h;
                    dataGridViewWithCoord[5, i].Value = BitConverter.ToSingle(ffs.coord[i].h_end, 0) * max_h;
                    dataGridViewWithCoord[6, i].Value = BitConverter.ToInt32(ffs.coord[i].n_texture, 0);
                    dataGridViewWithCoord[7, i].Value = Math.Round(BitConverter.ToSingle(ffs.coord[i].widht, 0));
                    dataGridViewWithCoord[8, i].Value = Math.Round(BitConverter.ToSingle(ffs.coord[i].height, 0));

                    dataGridViewWithCoord[9, i].Value = BitConverter.ToInt32(ffs.coord[i].HZ, 0);
                    dataGridViewWithCoord[10, i].Value = BitConverter.ToSingle(ffs.coord[i].kern_left_side, 0);
                    dataGridViewWithCoord[11, i].Value = BitConverter.ToSingle(ffs.coord[i].to_top, 0);
                    dataGridViewWithCoord[12, i].Value = BitConverter.ToSingle(ffs.coord[i].widht_with_kern, 0);
                }

                //b[0]++;
                for (int j = 0; j < dataGridViewWithCoord.ColumnCount; j++)
                {
                    dataGridViewWithCoord[j, i].Style.BackColor = System.Drawing.Color.White;
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

        private void fillTableOfTextures()
        {
            dataGridViewWithTextures.RowCount = ffs.dds.Count();
            for (int i = 0; i < ffs.dds.Count(); i++)
            {
                dataGridViewWithTextures[0, i].Value = i.ToString();
                dataGridViewWithTextures[1, i].Value = BitConverter.ToInt32(ffs.dds[i].height_in_dds, 0);
                dataGridViewWithTextures[2, i].Value = BitConverter.ToInt32(ffs.dds[i].widht_in_dds, 0);
                dataGridViewWithTextures[3, i].Value = BitConverter.ToInt32(ffs.dds[i].size_in_font, 0);
            }
        }

        public bool CompareArray(byte[] arr0, byte[] arr1)
        {
            int i = 0;
            while ((i < arr0.Length) && (arr0[i] == arr1[i])) i++;
            return (i == arr0.Length);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ffs.coord.Clear();
            ffs.dds.Clear();

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
                encripted = false;
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
                    version_used = -1;
                    int DecKey = -1;

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

                    byte[] check_header = new byte[4];
                    Array.Copy(binContent, 0, check_header, 0, check_header.Length);

                    if ((Encoding.ASCII.GetString(check_header) != "5VSM") && (Encoding.ASCII.GetString(check_header) != "ERTM")
                    && (Encoding.ASCII.GetString(check_header) != "6VSM") && (Encoding.ASCII.GetString(check_header) != "NIBM")) //Supposed this font encrypted
                    {
                        //First trying decrypt probably encrypted font
                        try
                        {
                            string info = Methods.FindingDecrytKey(binContent, "font");
                            if (info != null)
                            {
                                MessageBox.Show("Font was encrypted, but I decrypted.\r\n" + info);
                                encripted = true;
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
                    bool someTexData = false;

                    version_used = countElements;

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


        private void encFunc(string path) //Функция по шифрованию шрифтов
        {
            if (encripted == true) //Проверка на зашифрованность файла
            {
                if (MessageBox.Show("We found out that font was encrypted. Do you want to encrypt back?", "About encrypted  font...",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    FileStream fs = new FileStream(path, FileMode.Open);
                    byte[] fontContent = Methods.ReadFull(fs);
                    fs.Close();

                    byte[] checkHeader = new byte[4];
                    byte[] checkVer = new byte[4];

                    Array.Copy(fontContent, 0, checkHeader, 0, 4);
                    Array.Copy(fontContent, 4, checkVer, 0, 4);

                    if (Encoding.ASCII.GetString(checkHeader) != "ERTM" && Encoding.ASCII.GetString(checkHeader) != "5VSM"
                        && (BitConverter.ToInt32(checkVer, 0) > 0 && BitConverter.ToInt32(checkVer, 0) < 6))
                    {
                        int selKey = -1;
                        bool encMode = false; //true - старый режим, false - новый
                        bool encFull = false; //Зашифровать полностью файл или только заголовок текстуры (для сборки архивов)
                        bool customKey = false;


                        EncryptionVariants f = new EncryptionVariants();
                        f.ShowDialog();
                        selKey = f.keyEnc;
                        encMode = f.OldMode;
                        encFull = f.fullEnc;
                        customKey = f.customKey;

                        if (selKey == -1) selKey = 0; //Поставлю по умолчанию Telltale's texas hold'em

                        byte[] key = MainMenu.gamelist[selKey].key;

                        if (customKey) key = Methods.stringToKey(f.key);
                        if (key == null)
                        {
                            MessageBox.Show("Custom key wasn't correct. Set key from chosen list: " + MainMenu.gamelist[selKey].gamename + "\r\nYou'll need key like this: 96CA99A085CF988AE4DBE2CDA6968388C08B99E39ED89BB6D790DCBEAD9D9165B6A69EBBC2C69EB3E7E3E5D5AB6382A09CC4929FD1D5A4");
                            key = MainMenu.gamelist[selKey].key;
                        }

                        int version = 2;
                        if (encMode == false) version = 7;

                        int TexPos = Methods.FindStartOfStringSomething(fontContent, 4, "DDS |");
                        byte[] tempBytes = new byte[2048];
                        if (tempBytes.Length > fontContent.Length - TexPos) tempBytes = new byte[fontContent.Length - TexPos];
                        Array.Copy(fontContent, TexPos, tempBytes, 0, tempBytes.Length);

                        BlowFishCS.BlowFish encTexHeader = new BlowFishCS.BlowFish(key, version);
                        tempBytes = encTexHeader.Crypt_ECB(tempBytes, version, false);
                        Array.Copy(tempBytes, 0, fontContent, TexPos, tempBytes.Length);

                        if (encFull == true) Methods.meta_crypt(fontContent, key, version, false);

                        if (File.Exists(path)) File.Delete(path);
                        fs = new FileStream(path, FileMode.Create);
                        fs.Write(fontContent, 0, fontContent.Length);
                        fs.Close();
                    }
                }

            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Methods.DeleteCurrentFile((ofd.FileName));
            FileStream fs = new FileStream((ofd.FileName), FileMode.OpenOrCreate);
            SaveFont(fs, ffs);
            fs.Close();

            encFunc(ofd.FileName);

            edited = false; //Файл сохранили, так что вернули флаг на ЛОЖЬ
        }

        private void SaveFont(FileStream fs, FontStructure ffs)
        {
            //сохраняем из дата грид в прогу
            for (int i = 0; i < ffs.coord.Count; i++)
            {
                int z = Convert.ToInt32(dataGridViewWithCoord[6, i].Value);
                //int z = BitConverter.ToInt32(ffs.coord[i].n_texture, 0);
                int max_x = BitConverter.ToInt32(ffs.dds[z].widht_in_dds, 0);
                int max_y = BitConverter.ToInt32(ffs.dds[z].height_in_dds, 0);
                ffs.coord[i].w_start = BitConverter.GetBytes((float)Convert.ToDouble(dataGridViewWithCoord[2, i].Value) / max_x);
                ffs.coord[i].w_end = BitConverter.GetBytes((float)Convert.ToDouble(dataGridViewWithCoord[3, i].Value) / max_x);
                ffs.coord[i].h_start = BitConverter.GetBytes((float)Convert.ToDouble(dataGridViewWithCoord[4, i].Value) / max_y);
                ffs.coord[i].h_end = BitConverter.GetBytes((float)Convert.ToDouble(dataGridViewWithCoord[5, i].Value) / max_y);
                ffs.coord[i].n_texture = BitConverter.GetBytes((int)Convert.ToInt32(dataGridViewWithCoord[6, i].Value.ToString()));

                if (version_used != -1 && version_used >= 6)
                {
                    byte[] widht = BitConverter.GetBytes((float)(Convert.ToInt32(dataGridViewWithCoord[7, i].Value)));//-2 тут
                    byte[] height = BitConverter.GetBytes((float)(Convert.ToInt32(dataGridViewWithCoord[8, i].Value)));
                    ffs.coord[i].widht = widht;
                    ffs.coord[i].height = height;
                }

                if (version_used >= 9)
                {
                    //dataGridViewWithCoord[1, i].Value = Methods.ConvertHexToString(ffs.coord[i].symbol, 0, 4, MainMenu.settings.ASCII_N, false);
                    ffs.coord[i].HZ = BitConverter.GetBytes((int)Convert.ToInt32(dataGridViewWithCoord[9, i].Value.ToString()));
                    ffs.coord[i].kern_left_side = BitConverter.GetBytes((float)Convert.ToDouble(dataGridViewWithCoord[10, i].Value.ToString()));
                    ffs.coord[i].to_top = BitConverter.GetBytes((float)Convert.ToDouble(dataGridViewWithCoord[11, i].Value.ToString()));
                    ffs.coord[i].widht_with_kern = BitConverter.GetBytes((float)Convert.ToDouble(dataGridViewWithCoord[12, i].Value.ToString()));
                }
            }


            if (version_used < 9)
            {
                //записываем всё в файл
                fs.Write(ffs.header_of_file, 0, ffs.header_of_file.Length);
                fs.Write(ffs.coord_lenght, 0, ffs.coord_lenght.Length);
                fs.Write(ffs.coord_count, 0, ffs.coord_count.Length);

                for (int i = 0; i < ffs.coord.Count; i++)
                {
                    fs.Write(ffs.coord[i].n_texture, 0, ffs.coord[i].n_texture.Length);
                    fs.Write(ffs.coord[i].w_start, 0, ffs.coord[i].w_start.Length);
                    fs.Write(ffs.coord[i].w_end, 0, ffs.coord[i].w_end.Length);
                    fs.Write(ffs.coord[i].h_start, 0, ffs.coord[i].h_start.Length);
                    fs.Write(ffs.coord[i].h_end, 0, ffs.coord[i].h_end.Length);

                    if (version_used != -1 && version_used >= 6)
                    {
                        fs.Write(ffs.coord[i].widht, 0, ffs.coord[i].widht.Length);
                        fs.Write(ffs.coord[i].height, 0, ffs.coord[i].height.Length);
                    }
                }

                for (int i = 0; i < ffs.dds.Count; i++)
                {
                    fs.Write(ffs.dds[i].header, 0, ffs.dds[i].header.Length);
                    fs.Write(ffs.dds[i].widht_in_dds, 0, ffs.dds[i].widht_in_dds.Length);
                    fs.Write(ffs.dds[i].height_in_dds, 0, ffs.dds[i].height_in_dds.Length);

                    if (iOS == false)
                    {
                        fs.Write(ffs.dds[i].hz_data_after_sizes, 0, ffs.dds[i].hz_data_after_sizes.Length);
                        fs.Write(ffs.dds[i].size_in_font, 0, ffs.dds[i].size_in_font.Length);
                        fs.Write(ffs.dds[i].dds_content, 0, ffs.dds[i].dds_content.Length);
                    }
                    else
                    {
                        int position = ffs.dds[i].hz_data_after_sizes.Length;
                        Array.Copy(ffs.dds[i].size_in_font, 0, ffs.dds[i].hz_data_after_sizes, position - 48, ffs.dds[i].size_in_font.Length);
                        Array.Copy(ffs.dds[i].widht_in_dds, 0, ffs.dds[i].hz_data_after_sizes, position - 36, ffs.dds[i].widht_in_dds.Length);
                        Array.Copy(ffs.dds[i].height_in_dds, 0, ffs.dds[i].hz_data_after_sizes, position - 40, ffs.dds[i].height_in_dds.Length);
                        byte[] Binmeta = new byte[4];
                        Array.Copy(ffs.dds[i].dds_content, 48, Binmeta, 0, 4);
                        int meta_size = BitConverter.ToInt32(Binmeta, 0);

                        byte[] texLength = new byte[4];
                        texLength = BitConverter.GetBytes(ffs.dds[i].dds_content.Length - 52 - meta_size);
                        Array.Copy(texLength, 0, ffs.dds[i].hz_data_after_sizes, position - 24, texLength.Length);

                        fs.Write(ffs.dds[i].hz_data_after_sizes, 0, ffs.dds[i].hz_data_after_sizes.Length);

                        byte[] pvrHead = { 0x50, 0x56, 0x52, 0x21, 0x01, 0x00, 0x00, 0x00 };
                        fs.Write(pvrHead, 0, pvrHead.Length);

                        byte[] temp = new byte[ffs.dds[i].dds_content.Length - 52 - meta_size];
                        Array.Copy(ffs.dds[i].dds_content, 52 + meta_size, temp, 0, temp.Length);
                        fs.Write(temp, 0, temp.Length);
                    }
                }
            }
            else if (version_used >= 9)//покер найт2
            {
                //записываем всё в файл
                if (version_used >= 10)//Волк, Ходячие 2 и Ходячие 1 Remastered (Android, PS Vita, PS4 и Xbox One)
                {
                    byte[] ddsOffset, ddsLenght = new byte[4];
                    int offset = ffs.header_of_file.Length - 128 + dataGridViewWithCoord.Rows.Count * 12 * 4 + 8 + 8;
                    if (version_used == 14 || version_used == 15)
                    {
                        byte[] tmp = new byte[4];
                        Array.Copy(ffs.header_of_file, 16, tmp, 0, tmp.Length);
                        if (BitConverter.ToInt32(tmp, 0) == 13) offset -= 36; //Guardians of the Galaxy
                        offset -= (24 + 11); //Batman 1, WD3
                    }

                    if (version_used >= 11)
                    {
                        offset++;
                    }

                    int lenght = 0;
                    for (int i = 0; i < ffs.dds.Count; i++)
                    {
                        offset += ffs.dds[i].header.Length;
                        lenght += ffs.dds[i].dds_content.Length;

                        for (int j = 0; j < ffs.dds[i].pn2dds_head.Count; j++)
                        {
                            offset += ffs.dds[i].pn2dds_head[j].Length;
                        }
                    }

                   

                    if (version_used == 14 || version_used == 15) offset += 4 + 8 + 8 + 3;
                    if (version_used == 16) offset -= 12; //Tales from the Borderlands Redux

                    ddsOffset = BitConverter.GetBytes(offset);

                    Array.Copy(ddsOffset, 0, ffs.header_of_file, 4, 4);
                    for (int i = 0; i < ffs.dds.Count; i++)
                    {
                        ddsLenght = BitConverter.GetBytes(lenght);
                    }
                    Array.Copy(ddsLenght, 0, ffs.header_of_file, 12, 4);
                }

                fs.Write(ffs.header_of_file, 0, ffs.header_of_file.Length);
                byte[] temp = new byte[4];
                ffs.coord_count = BitConverter.GetBytes(dataGridViewWithCoord.Rows.Count);
                ffs.coord_lenght = BitConverter.GetBytes(dataGridViewWithCoord.Rows.Count * 12 * 4 + 8);
                fs.Write(ffs.coord_lenght, 0, 4);
                fs.Write(ffs.coord_count, 0, 4);

                for (int i = 0; i < ffs.coord.Count; i++)
                {
                    fs.Write(ffs.coord[i].symbol, 0, 4);
                    fs.Write(ffs.coord[i].n_texture, 0, 4);
                    fs.Write(ffs.coord[i].HZ, 0, 4);
                    fs.Write(ffs.coord[i].w_start, 0, 4);
                    fs.Write(ffs.coord[i].w_end, 0, 4);
                    fs.Write(ffs.coord[i].h_start, 0, 4);
                    fs.Write(ffs.coord[i].h_end, 0, 4);
                    fs.Write(ffs.coord[i].widht, 0, 4);
                    fs.Write(ffs.coord[i].height, 0, 4);
                    fs.Write(ffs.coord[i].kern_left_side, 0, 4);
                    fs.Write(ffs.coord[i].to_top, 0, 4);
                    fs.Write(ffs.coord[i].widht_with_kern, 0, 4);
                }
                int lenghtDds = 8;//сперва 8 байт

                for (int i = 0; i < ffs.dds.Count; i++)
                {
                    if (version_used == 9)
                    {
                        lenghtDds += ffs.dds[i].dds_content.Length;
                    }
                    
                    lenghtDds += ffs.dds[i].header.Length + ffs.dds[i].pn2dds_head.Count * 4;
                }

                if (version_used == 14 || version_used == 15) lenghtDds += ffs.dds[ffs.dds.Count - 1].pn2dds_head.Count * 4;

                byte[] lenghtDdsByte = new byte[4];
                lenghtDdsByte = BitConverter.GetBytes(lenghtDds);
                fs.Write(lenghtDdsByte, 0, 4);
                fs.Write(ffs.count_dds, 0, 4);
                if (version_used == 9)
                {
                    for (int i = 0; i < ffs.dds.Count; i++)
                    {
                        fs.Write(ffs.dds[i].header, 0, ffs.dds[i].header.Length);
                        for (int j = 0; j < ffs.dds[i].pn2dds_head.Count; j++)
                        {
                            fs.Write(ffs.dds[i].pn2dds_head[j], 0, 4);
                        }

                        fs.Write(ffs.dds[i].dds_content, 0, ffs.dds[i].dds_content.Length);
                    }
                }
                else if (version_used == 10)
                {
                    for (int i = 0; i < ffs.dds.Count; i++)
                    {
                        fs.Write(ffs.dds[i].header, 0, ffs.dds[i].header.Length);
                        for (int j = 0; j < ffs.dds[i].pn2dds_head.Count; j++)
                        {
                            fs.Write(ffs.dds[i].pn2dds_head[j], 0, 4);
                        }
                    }
                    for (int i = 0; i < ffs.dds.Count; i++)
                    {
                        fs.Write(ffs.dds[i].dds_content, 0, ffs.dds[i].dds_content.Length);
                    }
                }
                else if (version_used >= 11)
                {
                    for (int i = 0; i < ffs.dds.Count; i++)
                    {
                        fs.Write(ffs.dds[i].header, 0, ffs.dds[i].header.Length);

                        for (int j = 0; j < ffs.dds[i].pn2dds_head.Count; j++)
                        { 
                           fs.Write(ffs.dds[i].pn2dds_head[j], 0, 4);
                        }
                    }

                    byte[] z = { 0x30 };

                    fs.Write(z, 0, z.Length);

                    for (int i = 0; i < ffs.dds.Count; i++)
                    {
                        fs.Write(ffs.dds[i].dds_content, 0, ffs.dds[i].dds_content.Length);
                    }
                }


            }
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
                if (version_used < 9)
                {
                    //fs.Write(ffs.dds[file_n].dds_content, 0, ffs.dds[file_n].dds_content.Length);
                    fs.Write(font.tex[file_n].Content, 0, font.tex[file_n].Content.Length);
                }
                else if (version_used >= 9)
                {
                    fs.Write(font.NewTex[file_n].Tex.Content, 0, font.NewTex[file_n].Tex.Content.Length);
                    /*int num = 4;

                    if (version_used == 11 || version_used == 12) num = 5;
                    else if (version_used == 13) num = 6;
                    else if (version_used == 14) num = 4;
                    else if (version_used == 16) num = 3; //Tales from the borderlands redux

                    byte[] mip = new byte[4];
                    mip = BitConverter.GetBytes(1);

                    bool pvr = false;
                    string result = null;

                    //byte[] header = TextureWorker.genHeader(ffs.dds[file_n].widht_in_dds, ffs.dds[file_n].height_in_dds, mip, BitConverter.ToInt32(ffs.dds[file_n].pn2dds_head[num], 0), platform, ref pvr, ref result);
                    //fs.Write(header, 0, header.Length);

                    if(platform == 15)
                    {
                        int int_width = BitConverter.ToInt32(ffs.dds[file_n].widht_in_dds, 0);
                        int int_height = BitConverter.ToInt32(ffs.dds[file_n].height_in_dds, 0);

                        byte[] tmp_dds = Swizzle.NintendoSwizzle(ffs.dds[file_n].dds_content, int_width, int_height, BitConverter.ToInt32(ffs.dds[file_n].pn2dds_head[num], 0), true);
                        fs.Write(tmp_dds, 0, tmp_dds.Length);
                    }
                    else fs.Write(ffs.dds[file_n].dds_content, 0, ffs.dds[file_n].dds_content.Length);*/
                }
                fs.Close();
            }
        }

        private void importDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int file_n = dataGridViewWithTextures.SelectedCells[0].RowIndex;
            OpenFileDialog openFD = new OpenFileDialog();

            if (iOS == true && version_used < 9)
            {
                openFD.Filter = "PVR files (*.pvr)|*.pvr";
            }
            else if (iOS == false && version_used < 9)
            {
                openFD.Filter = "dds files (*.dds)|*.dds";
            }
            else openFD.Filter = "DDS-files (*.dds)|*.dds|PVR-files (*.pvr)|*.pvr";


            if (openFD.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(openFD.FileName, FileMode.Open);
                byte[] temp = Methods.ReadFull(fs);
                fs.Close();


                int meta_size = 0;

                //Решил сначала проверить заголовок, а потом приниматься за копирование данных о ширине и высоте текстуры,
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
                }
                fillTableOfTextures();
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
                SaveFont(fs, ffs);
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
                    SaveFont(fs, ffs);
                    //После соханения чистим списки
                    version_of_font.Clear();
                }
                else //А иначе просто закрываем программу и чистим списки
                {
                    version_of_font.Clear();
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
                            if (temp >= ffs.dds.Count)
                            {
                                dataGridViewWithCoord[end_edit_column, end_edit_row].Value = old_data;
                            }
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
            OpenFileDialog openFD = new OpenFileDialog();
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
            }
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
            OpenFileDialog openFD = new OpenFileDialog();
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
            }
        }

        public void importCoordinatesFromFontStudioxmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFD = new OpenFileDialog();
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
            }
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
                }
                else
                {
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
                }

                fillTableofCoordinates(font, true);
            }

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void changeCoordinatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
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
            }
        }

        private void removeDuplicatesCharsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ffs != null && ffs.coord.Count > 0)
            {
                var tmp_array = ffs.coord.ToArray();
                Array.Sort(tmp_array, (tmp_ar1, tmp_ar2) => BitConverter.ToInt32(tmp_ar1.symbol, 0).CompareTo(BitConverter.ToInt32(tmp_ar2.symbol, 0)));
                ffs.coord = tmp_array.OfType<Coordinates>().ToList();

                ffs.coord = ffs.coord.GroupBy(i => BitConverter.ToInt32(i.symbol, 0)).Select(g => g.Last()).ToList();
                tmp_array = null;
                GC.Collect();
                fillTableOfCoordinates();
                if(!edited) edited = true;
            }
        }
    }
}
