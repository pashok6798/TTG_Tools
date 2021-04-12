using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TTG_Tools
{
    public partial class TexView : Form
    {
        public TexView()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        bool pvr;

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "D3DTX files (*.d3dtx) | *.d3dtx";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                string versionOfGame = "";
                //try
                //{
                //старые добрые времена с текстурами без извращений
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                byte[] d3dtx = Methods.ReadFull(fs);
                fs.Close();

                byte[] check_header = new byte[4];
                Array.Copy(d3dtx, 0, check_header, 0, 4);

                int offset = 0;

                byte[] check_ver = new byte[4];

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
                    byte[] BinContent = TextureWorker.extract_old_textures(d3dtx, null, 2, ref result, ref pvr);
                    if (BinContent != null)
                    {
                        //string message = "File " + fi[i].Name + " exported in dds file. " + result;
                        DDSReader.DDSImage im = new DDSReader.DDSImage(BinContent, true);
                        System.Drawing.Bitmap bm = im.BitmapImage;

                        pictureBox1.Image = bm;

                        //listBox1.Items.Add(message);
                    }
                    else MessageBox.Show("Unknown error in file " + ofd.FileName + ". Please write me about it.");
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
                    switch (BitConverter.ToInt32(check_ver, 0))
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
                            DDSReader.DDSImage im = new DDSReader.DDSImage(Content, true);
                            System.Drawing.Bitmap bm = im.BitmapImage;

                            //pictureBox1.Image = bm;
                        }
                        else
                        {
                            MessageBox.Show("Unknown error in file " + ofd.FileName + ". Code of Texture: " + tex_code + ". Please write me about it.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        //MessageBox.Show("Something is wrong. Please, contact with me.");
                    }
                }

                //return ""; //return nothing
            }
        }

        private void extractTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "D3DTX files (*.d3dtx) | *.d3dtx";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string versionOfGame = "";
                //try
                //{
                //старые добрые времена с текстурами без извращений
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                byte[] d3dtx = Methods.ReadFull(fs);
                fs.Close();

                byte[] check_header = new byte[4];
                Array.Copy(d3dtx, 0, check_header, 0, 4);

                int offset = 0;

                byte[] check_ver = new byte[4];

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
                    byte[] BinContent = TextureWorker.extract_old_textures(d3dtx, null, 2, ref result, ref pvr);
                    if (BinContent != null)
                    {
                        //string message = "File " + fi[i].Name + " exported in dds file. " + result;
                        DDSReader.DDSImage im = new DDSReader.DDSImage(BinContent, true);
                        System.Drawing.Bitmap bm = im.BitmapImage;

                        pictureBox1.Image = bm;

                        //listBox1.Items.Add(message);
                    }
                    else MessageBox.Show("Unknown error in file " + ofd.FileName + ". Please write me about it.");
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
                    switch (BitConverter.ToInt32(check_ver, 0))
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
                            DDSReader.DDSImage im = new DDSReader.DDSImage(Content, true);
                            System.Drawing.Bitmap bm = im.BitmapImage;

                            bm.Save(AppDomain.CurrentDomain.BaseDirectory + "test.png", System.Drawing.Imaging.ImageFormat.Png);

                            //pictureBox1.Image = bm;
                        }
                        else
                        {
                            MessageBox.Show("Unknown error in file " + ofd.FileName + ". Code of Texture: " + tex_code + ". Please write me about it.");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Something is wrong. Please, contact with me.");
                    }
                }

                //return ""; //return nothing
            }
        }
    }
}
