using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Verse;
using RimWorld;

namespace CaravanAdventures
{
    public static class Extensions
    {
        public static IEnumerable<Pawn> ContainedMechs(this Room room, bool onlyAsleep = false)
        {
            // likely very slow, better find mechs over spawnedPawns
            foreach (var region in room.Regions)
            {
                foreach (var thing in region.ListerThings.AllThings)
                {
                    if (thing is Pawn mech && mech?.Faction == Faction.OfMechanoids)
                    {
                        if (onlyAsleep && mech.Awake()) continue;
                        yield return mech;
                    }
                }
            }
        }

        public static T CastTo<T>(this object myobj)
        {
            Type objectType = myobj.GetType();
            Type target = typeof(T);
            var x = Activator.CreateInstance(target, false);
            var z = from source in objectType.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            var d = from source in target.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
               .ToList().Contains(memberInfo.Name)).ToList();
            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                value = myobj.GetType().GetProperty(memberInfo.Name).GetValue(myobj, null);

                propertyInfo.SetValue(x, value, null);
            }
            return (T)x;
        }

        public static T DeepClone<T>(this T a)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

                serializer.Serialize(stream, a);
                stream.Position = 0;
                return (T)serializer.Deserialize(stream);
            }
        }
       
    }
}
