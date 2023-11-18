using System;
using System.Collections.Generic;
using System.Linq;
using CreeperX.Profiles;

namespace CreeperX.Utils
{
    public static class ProfileHelper
    {
        public readonly static Dictionary<string, Type> ProfileTypeDictionary =
                GetProfileTypes().ToDictionary(x => x.Name, x => x);

        public static CreeperProfile CreateProfile(string profileDef, string name, string workDir)
        {
            if (!ProfileTypeDictionary.ContainsKey(profileDef))
            {
                throw new Exception($"Unknown profile definition: {profileDef}");
            }

            var profileType = ProfileTypeDictionary[profileDef];
            var profile = Activator.CreateInstance(profileType, workDir) as CreeperProfile;
            profile.Name = name;

            return profile;
        }

        private static List<Type> GetProfileTypes()
        {
            return new List<Type>()
            {
                typeof (TingRoomProfile)
            };
        }
    }
}
