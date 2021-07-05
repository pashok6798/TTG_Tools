/*************************************************************************************************
 *                                       List of docs:                                           *
 * https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header                         *
 * https://docs.microsoft.com/en-us/windows/win32/api/d3d10/ne-d3d10-d3d10_resource_dimension    *
 * https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format       *
 *************************************************************************************************/

namespace TTG_Tools.Graphics.DDS
{
    class Flags
    {
        uint DDSD_CAPS = 0x1;
        uint DDSD_HEIGHT = 0x2;
        uint DDSD_WIDTH = 0x4;
        uint DDSD_PITCH = 0x8;
        uint DDSD_PIXELFORMAT = 0x1000;
        uint DDSD_MIPMAPCOUNT = 0x20000;
        uint DDSD_LINEARSIZE = 0x80000;
        uint DDSD_DEPTH = 0x800000;
    }
}
