namespace Godot.FSharp;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GDClassAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GDGlobalClassAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GDToolAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class GDMethodAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class GDSignalAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class GDPropertyAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class GDExportAttribute : Attribute { }
