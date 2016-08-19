using System.Collections.Generic;
using UnityEngine;
using System;

namespace SoundtrackEditor
{
    public static class GuiUtils
    {
        public static void label(string text, object obj)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            GUILayout.Label(obj == null ? "null" : obj.ToString(), GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }

        public static float editFloat(string text, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            bool maxVal = value == float.MaxValue;
            bool minVal = value == float.MinValue;
            string displayVal = maxVal || minVal ? "" : value.ToString();
            string v = GUILayout.TextField(displayVal, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            float f = value;
            if (v == string.Empty)
            {
                if (maxVal)
                    f = float.MaxValue;
                else if (minVal)
                    f = float.MinValue;
                else
                    f = 0;
            }
            else
                float.TryParse(v, out f);
            return f;
        }

        public static string editString(string text, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            string v = GUILayout.TextField(value, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            return v;
        }

        public static T editEnum<T>(string text, T value) where T : struct, IConvertible
        {
            Type genericType = typeof(T);
            if (!genericType.IsEnum)
                throw new ArgumentException("Type 'T' must be an enum");

            GUILayout.BeginHorizontal();
            GUILayout.Label(text);
            //GUILayout.FlexibleSpace();
            GUILayout.Label(" ");
            foreach (var e in Enum.GetValues(typeof(T)))
            {
                string name = e.ToString();
                GUILayout.Toggle(false, ""); // Text parameter causes overlap.
                GUILayout.Label(name + " ");
            }
            GUILayout.EndHorizontal();

            return value;
        }

        public static void slider(string label, ref float variable, float from, float to)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label + ": " + variable.ToString());
            GUILayout.FlexibleSpace();
            variable = GUILayout.HorizontalSlider(variable, from, to, GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }

        public static Color rgbaSlider(string label, ref float r, ref float g, ref float b, ref float a, float from, float to)
        {
            GUILayout.Label(label);
            slider("r", ref r, from, to);
            slider("g", ref g, from, to);
            slider("b", ref b, from, to);
            slider("a", ref a, from, to);
            return new Color(r, g, b, a);

        }

        static float x = 0;
        static float y = 0;
        public static KeyValuePair<float, float> GetSliderXY()
        {
            return new KeyValuePair<float, float>(x, y);
        }
    }
}
