namespace ChaosLib
{
    public enum OBJECT_INFORMATION_CLASS
    {
        ObjectBasicInformation,
        ObjectNameInformation, //OBJECT_NAME_INFORMATION - may hang?
        ObjectTypeInformation, //OBJECT_TYPE_INFORMATION + name buffer
        ObjectAllInformation,
        ObjectDataInformation
    }
}