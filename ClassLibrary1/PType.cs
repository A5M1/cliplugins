using System;

namespace PluginSystem {
    public class PType {
        public string TypeName {
            get;
        }
        public Type Type {
            get;
        }
        public bool IsEnum => Type.IsEnum;
        public Array EnumValues => IsEnum ? Enum.GetValues(Type) : Array.Empty<object>();

        public PType(Type type) {
            Type = type;
            TypeName = type.Name;
        }
    }
}
