using System;

namespace MagicalMountainMinery.Data.Load
{


    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class PersistAttribute : Attribute
    {
        public bool IsPersist
        {
            get;
            set;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class StoreCollectionAttribute : Attribute
    {
        public bool ShouldStore
        {
            get;
            set;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class PostLoad : Attribute
    {

    }

    [Serializable]
    public class ObjectRef
    {

        public object storedReference;
        public ObjectRef(object reff)
        {
            this.storedReference = reff;
        }
    }
}
