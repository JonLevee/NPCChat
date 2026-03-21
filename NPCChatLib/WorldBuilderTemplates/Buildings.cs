using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPCChatLib.Attributes;
using NPCChatLib.WorldClasses;
using Windows.Media.Devices;

namespace NPCChatLib.WorldBuilderTemplates
{

    public static class Templates
    {
        public static class Buildings
        {
            public static class Shops
            {
                public static WorldObject SmallShop()
                {
                    var shop = new WorldObject
                    {
                        Type = WorldObjectType.Building,
                        Size = new(5, 3)
                    };
                    return shop;
                }
            }
        }
    }
}
