namespace SpinRallyBot;

public static class TypeExtensions {
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType) {
        Type[] interfaceTypes = givenType.GetInterfaces();

        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType)) {
            return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType) {
            return true;
        }

        Type? baseType = givenType.BaseType;
        return baseType != null && IsAssignableToGenericType(baseType, genericType);
    }
}
