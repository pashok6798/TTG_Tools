﻿using System;
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
            public bool AdditionalSize;
            public string ObjectName;
            public string SubobjectName;
            public byte[] Flags;
            public int Mip;
            public int TextureFormat;
            public int OriginalWidth;
            public int OriginalHeight;
            public int Width;
            public int Height;
            public byte[] UnknownData; //4 bytes
            public byte Zero;
            public FlagsClass TexFlags;
            public byte[] block;
            public int TexSize;
            public int BlockPos; //Need for correct font texture size
            public byte[] Content; //Texture

            public OldT3Texture() { }

            public OldT3Texture(OldT3Texture newClass)
            {
                sizeBlock = newClass.sizeBlock;
                someValue = newClass.someValue;
                AdditionalSize = newClass.AdditionalSize;
                ObjectName = newClass.ObjectName;
                SubobjectName = newClass.SubobjectName;
                Flags = newClass.Flags;
                Mip = newClass.Mip;
                TextureFormat = newClass.TextureFormat;
                OriginalWidth = newClass.OriginalWidth;
                OriginalHeight = newClass.OriginalHeight;
                Width = newClass.Width;
                Height = newClass.Height;
                UnknownData = newClass.UnknownData;
                Zero = newClass.Zero;
                TexFlags = newClass.TexFlags;
                block = newClass.block;
                TexSize = newClass.TexSize;
                BlockPos = newClass.BlockPos;
                Content = newClass.Content;
            }
        }

        public class NewT3Texture
        {
            public struct UnknownFlags
            {
                public int blockSize; //equals 8 bytes
                public uint block; //Something values (used since Hector)
            }

            public struct Platform
            {
                //2 - PC/Mac
                //7 - iOS/Android (With PowerVR graphic chip)
                //9 - PS Vita
                //15 - Nintendo Switch
                public int blockSize; //equals 8 bytes
                public int platform;
            }

            public struct TextureStruct
            {
                public int Zero; //Since value 5
                public int CurrentMip;
                public int One;
                public int MipSize; //Size of one mipmap
                public int BlockSize;
            }

            public struct SubBlock
            {
                public int Size;
                public byte[] Block;
            }

            public struct TextureInfo
            {
                public int MipCount;
                public int SomeData; //For example in Poker Night 2 sometimes were textures with PNG files
                public uint TexSize;
                public TextureStruct[] Textures;
                public SubBlock[] SubBlocks;
                public byte[] Content;
            }


            //For headers 5VSM and 6VSM
            public int headerSize; //Size of header
            public uint textureSize; //Size of texture

            public int SomeValue; //Some value for flags
            public UnknownFlags unknownFlags;
            public Platform platform;
            public string ObjectName;
            public string SubObjectName;
            public int Zero; //Saw in Wolf among Us or Game of Thrones
            public float OneValue;
            public byte OneByte;
            public FlagsClass.SomeClassData UnknownData; //For Walking Dead the New Frontier
            public int Mip;
            public int Width;
            public int Height;
            public int Faces; //For cubmaps
            public int ArrayMembers; //Saw in PVRTexTool
            public int TextureFormat;
            public int Unknown1;
            public byte[] block; //temporary solution
            public TextureInfo Tex;

            public NewT3Texture() { }

            public NewT3Texture(NewT3Texture newClass)
            {
                headerSize = newClass.headerSize;
                textureSize = newClass.textureSize;

                SomeValue = newClass.SomeValue;
                unknownFlags = newClass.unknownFlags;
                platform = newClass.platform;
                ObjectName = newClass.ObjectName;
                SubObjectName = newClass.SubObjectName;
                Zero = newClass.Zero;
                OneValue = newClass.OneValue;
                OneByte = newClass.OneByte;
                UnknownData = newClass.UnknownData;
                Mip = newClass.Mip;
                Width = newClass.Width;
                Height = newClass.Height;
                Faces = newClass.Faces;
                ArrayMembers = newClass.ArrayMembers;
                TextureFormat = newClass.TextureFormat;
                Unknown1 = newClass.Unknown1;
                block = newClass.block;
                Tex = newClass.Tex;
            }
        }
    }
}
