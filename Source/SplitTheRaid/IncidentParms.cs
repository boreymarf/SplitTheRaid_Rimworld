using System.Reflection;
using RimWorld;

namespace SplitTheRaid
{
    public class IncidentParmsPatched : IncidentParms
    {
        // Techincally a hack
        // Can't make a separate worker for dublicated raids,
        // because other mods' patches won't work

        // Will break if some idiot will also try to make their own IncidentParms
        public static IncidentParmsPatched ConvertParms(IncidentParms source)
        {
            IncidentParmsPatched split = new IncidentParmsPatched();
            foreach (FieldInfo field in typeof(IncidentParms).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                field.SetValue(split, field.GetValue(source));
            }
            return split;
        }
    }
}