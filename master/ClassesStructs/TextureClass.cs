using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTG_Tools.ClassesStructs
{
    //Experiments with oldest format textures
    public class TextureClass
    {
        public class OldT3Texture
        {
            public int sizeBlock; //For games since Hector
            public int someValue; //For games since Hector
            public string ObjectName;
            public string SubobjectName;
            public byte[] Flags;
            public int Mip;
            public int TextureFormat;
            public int OriginalWidth;
            public int OriginalHeight;
            public byte[] UnknownData; //4 bytes
            public byte Zero;
            public FlagsClass TexFlags;
            public int TexSize;
            public byte[] Content; //Texture

            public OldT3Texture() { }
        }
    }
}
