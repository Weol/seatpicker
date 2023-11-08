using System.Text;
using System.Text.RegularExpressions;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class SvgUtils
{
    public static bool IsSvg(byte[] svgImage)
    {
        var utf8 = new UTF8Encoding();
        var svgString = utf8.GetString(svgImage);

        return Regex.IsMatch(svgString, @"^\n*(<[!?].+>\n*)+\n*<svg.*>[.\n]*");
    }
}