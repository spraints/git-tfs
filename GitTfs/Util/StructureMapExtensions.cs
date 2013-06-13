using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using StructureMap.Query;

namespace Sep.Git.Tfs.Util
{
    public static class StructureMapExtensions
    {
        public static IEnumerable<InstanceRef> GetPlugins<BaseType>(this IContainer container)
        {
            return container.Model
                .PluginTypes
                .Single(p => p.PluginType == typeof(BaseType))
                .Instances
                .Where(i => i != null);
        }
    }
}
