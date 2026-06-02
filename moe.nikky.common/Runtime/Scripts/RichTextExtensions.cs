using UnityEngine;

namespace moe.nikky.common
{
    public static class RichTextExtensions
    {
        public static string ToHex(this Color clr) =>
            $"#{(int)(clr.r * 0xff):X2}{(int)(clr.g * 0xff):X2}{(int)(clr.b * 0xff):X2}";

        public static string Color(this string message, Color color) => $"<color={color.ToHex()}>{message}</color>";

        public static string Color(this string message, RichTextColor color)
        {
            return $"<color={color}>" + message + "</color>";
        }

        public static string Colored(this string message, RichTextColor color)
        {
            return $"<color={color}>" + message + "</color>";
        }


        public static string Bold(this string message)
        {
            return "<b>" + message + "</b>";
        }

        public static string Italics(this string message)
        {
            return "<i>" + message + "</i>";
        }
    }

    public enum RichTextColor
    {
        aqua, // (same as cyan) 	#00ffffff
        black, // 	#000000ff
        blue, // 	#0000ffff
        brown, // 	#a52a2aff
        cyan, // (same as aqua) 	#00ffffff
        darkblue, // 	#0000a0ff
        fuchsia, // (same as magenta) 	#ff00ffff
        green, // 	#008000ff
        grey, // 	#808080ff
        lightblue, // 	#add8e6ff
        lime, // 	#00ff00ff
        magenta, // (same as fuchsia) 	#ff00ffff
        maroon, // 	#800000ff
        navy, // 	#000080ff
        olive, // 	#808000ff
        orange, // 	#ffa500ff
        purple, // 	#800080ff
        red, // 	#ff0000ff
        silver, // 	#c0c0c0ff
        teal, // 	#008080ff
        white, // 	#ffffffff
        yellow //   #ffff00ff
    }
}