using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTG_Tools.ClassesStructs
{
    public class FontClass
    {
        public class OldFontClass
        {
            public class TRect
            {
                public int TexNum; //CurrentTexture
                public float XStart;
                public float XEnd;
                public float YStart;
                public float YEnd;

                public TRect() { }
            }
            public struct GlyphInfo
            {
                public int BlockCoordSize; //Size of block class TRect
                public int CharCount; //Count characters (before Poker Night 2 default was 256)
                public TRect[] chars; //Table of characters
            }

            public string FontName;
            public byte One;
            public float BaseSize; //Common char size in text line
            public int BlockTexSize; //Size of block Textures
            public int TexCount; //Count textures

            public GlyphInfo glyph;
            public TextureClass.OldT3Texture[] tex;
            
            public OldFontClass() { }
        }
    }
}
