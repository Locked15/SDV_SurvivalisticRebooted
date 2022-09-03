using System;
using Microsoft.Xna.Framework;

namespace Survive_Net5.Framework.Bars
{
    public class BarsInformations
    {
        /// <summary>
        /// Значение голода умноженное на шаблон высоты. Используется для отрисовки шкалы голода.
        /// </summary>
        public static float hunger_percentage;

        /// <summary>
        /// Значение жажды умноженное на шаблон высоты. Используется для отрисовки шкалы жажды.
        /// </summary>
        public static float thirst_percentage;

        public static Color hunger_color = new Color(207, 98, 7);
        public static Color thirst_color = new Color(13, 151, 151);

        public static void ResetStatus()
        {
            ModEntry.data.actual_hunger = ModEntry.data.max_hunger;
            ModEntry.data.actual_thirst = ModEntry.data.max_thirst;

            BarsUpdate.CalculatePercentage();
        }

        public static void NormalizeStatus()
        {
            if (ModEntry.data.actual_hunger < 0) ModEntry.data.actual_hunger = 0;
            if (ModEntry.data.actual_thirst < 0) ModEntry.data.actual_thirst = 0;

            if (ModEntry.data.actual_hunger > ModEntry.data.max_hunger) ModEntry.data.actual_hunger = ModEntry.data.max_hunger;
            if (ModEntry.data.actual_thirst > ModEntry.data.max_thirst) ModEntry.data.actual_thirst = ModEntry.data.max_thirst;

            BarsUpdate.CalculatePercentage();
        }

        public static Color GetOffsetHungerColor()
        {
            double maxHunger = ModEntry.data.max_hunger * 1.0;
            double currentHunger = ModEntry.data.actual_hunger * 1.0;
            double offset = currentHunger / maxHunger;

            Color color = hunger_color;
            color.R = Convert.ToByte(Math.Abs(offset - 1) * byte.MaxValue);
            color.G = Convert.ToByte(offset * color.G);
            color.B = Convert.ToByte(offset * color.B);

            return color;
        }

        public static Color GetOffsetThirstyColor()
        {
            double maxThirsty = ModEntry.data.max_thirst * 1.0;
            double currentThirsty = ModEntry.data.actual_thirst * 1.0;
            double offset = currentThirsty / maxThirsty;

            Color color = thirst_color;
            color.R = Convert.ToByte(Math.Abs(offset - 1) * byte.MaxValue);
            color.G = Convert.ToByte(offset * color.G);
            color.B = Convert.ToByte(offset * color.B);

            return color;
        }
    }
}