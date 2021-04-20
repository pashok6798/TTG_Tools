using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTG_Tools.ClassesStructs
{
    //I DON'T KNOW ABOUT FLAGS BUT I HAVE TO DO WITH THAT!
    //In some files I saw class flags and it does something in other classes.
    public class FlagsClass
    {
        //flags for Textures
        public int Unknown1; //Before 0x30 byte

        //after 0x30 byte
        public int Unknown2;
        public int One; //Saw value 1
        public int Unknown3;
        public int Unknown4;
        public byte[] TexFlags; //Something 0x30 and 0x31 values
        public int Unknown5;



        //flags for Fonts
        public float halfVal; //I saw value with 0.5f. What is it?!
    }
}
