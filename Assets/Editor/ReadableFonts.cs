using System;
using UnityEditor;
using UnityEngine;
namespace BalanceWithMe.Editor
{
    public static class ReadableFonts
    {
        public static Font Regular => Load("AtkinsonHyperlegible-Regular");
        public static Font Bold => Load("AtkinsonHyperlegible-Bold");
        static Font Load(string name)
        {
            var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/"+name+".ttf");
            if(font==null)throw new InvalidOperationException("Missing readable UI font: "+name);
            return font;
        }
    }
}
