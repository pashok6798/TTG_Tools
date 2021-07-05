/*************************************************************************************************
 *                                       List of docs:                                           *
 * https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header                         *
 * https://docs.microsoft.com/en-us/windows/win32/api/d3d10/ne-d3d10-d3d10_resource_dimension    *
 * https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format       *
 *************************************************************************************************/

namespace TTG_Tools.Graphics.DDS
{
    class Caps
    {
        //Caps
        uint DDSCAPS_COMPLEX = 0x8;
        uint DDSCAPS_MIPMAP = 0x400000;
        uint DDSCAPS_TEXTURE = 0x1000;

        //Caps2
        uint DDSCAPS2_CUBEMAP = 0x200;
        uint DDSCAPS2_CUBEMAP_POSITIVEX = 0x400;
        uint DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800;
        uint DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000;
        uint DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000;
        uint DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000;
        uint DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000;
        uint DDSCAPS2_VOLUME = 0x200000;
    }
}
