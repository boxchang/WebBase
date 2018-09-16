using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

[DebuggerNonUserCode, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0"), CompilerGenerated]
public class Message
{
    // Fields
    private static CultureInfo resourceCulture;
    private static ResourceManager resourceMan;

    // Methods
    public Message()
    {
    }

    // Properties
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static CultureInfo Culture
    {
        get
        {
            return resourceCulture;
        }
        set
        {
            resourceCulture = value;
        }
    }

    public static string NoConnection
    {
        get
        {
            return ResourceManager.GetString("NoConnection", resourceCulture);
        }
    }

    public static string NoRowEffected
    {
        get
        {
            return ResourceManager.GetString("NoRowEffected", resourceCulture);
        }
    }

    public static string NoSupportDatabaseType
    {
        get
        {
            return ResourceManager.GetString("NoSupportDatabaseType", resourceCulture);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static ResourceManager ResourceManager
    {
        get
        {
            if (object.ReferenceEquals(resourceMan, null))
            {
                ResourceManager manager = new ResourceManager("Genesis.Mes.Library.WCF.Database.Message", typeof(Message).Assembly);
                resourceMan = manager;
            }
            return resourceMan;
        }
    }
}

