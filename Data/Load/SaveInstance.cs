using Godot;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MagicalMountainMinery.Main;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace MagicalMountainMinery.Data.Load
{
    /// <summary>
    /// There are three types of save instance,
    ///  - Generic Node
    ///  - GameObject
    ///  - AbstractModel
    /// </summary>
    /// <param name="o"></param>
    /// <param name="endNode"></param>
    [Serializable]

    public class SaveInstance
    {

        public Type objectType;

        public string ResPath;
        //lass objectClass;

        public Dictionary<string, object> fieldBook { get; set; }
        public Dictionary<string, object> childBook { get; set; }

        public SaveInstance(object o, bool endNode = false, bool isEnum = false)
        {

            //SaveObjectData(o, endNode, isEnum);

        }

        public SaveInstance(Type type)
        {
            this.objectType = type;
            fieldBook = new Dictionary<string, object>();
            childBook = new Dictionary<string, object>();
        }

        public SaveInstance() { }

        public void PrintHeirarchy(string indent)
        {
            ////GD.Print(indent+"SaveInstance with type: ",this.objectType);
            indent += "     ";
            ////GD.Print(indent+"Resource Path: "+this.ResPath);

            foreach (string s in this.fieldBook?.Keys)
            {
                object o = null;
                this.fieldBook?.TryGetValue(s, out o);

                if (o.GetType() == typeof(SaveInstance))
                {
                    SaveInstance saveInstance = (SaveInstance)o;
                    saveInstance.PrintHeirarchy(indent);
                }
                else
                {
                    ////GD.Print(indent+s+"  "+o);
                }
            }
        }
        ///// <summary>
        ///// Save the object for this SaveInstance
        ///// </summary>
        ///// <param name="o"></param>
        //private void SaveObjectData(object o, bool endNode, bool isEnum)
        //{
        //    this.objectType = o.GetType();
        //    this.fieldBook = new Dictionary<string, object>();

        //    if (typeof(Node).IsInstanceOfType(o))
        //    {
        //        this.ResPath = ((Node)o).SceneFilePath;
        //    }

        //    FieldInfo[] fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //    PropertyInfo[] properties = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        //    List<FieldInfo> fieldList = fields.OfType<FieldInfo>().ToList();
        //    int loopNum = 0;

        //    //if not endnode, we need to store private and public fields
        //    if (!endNode)
        //    {
        //        foreach (FieldInfo f in fieldList)
        //        {
        //            //don't store null fields!
        //            if (f.Name == "ptr" || f.Name == "memoryOwn"
        //            || f.GetValue(o) == null
        //            || (PersistAttribute)f.GetCustomAttribute(typeof(PersistAttribute)) != null)
        //                continue;
        //            else
        //            {

        //                //////GD.Print("      custom attributes: ", f.Attributes,"      ",f.CustomAttributes);
        //                object ret = ParseSaveObject(f.Name, f.GetValue(o), o);
        //                loopNum++;

        //                //make sure that we aren't storing something null that we can't parse!
        //                if (ret != null)
        //                {
        //                    this.fieldBook.Add(f.Name, ParseSaveObject(f.Name, f.GetValue(o), o));
        //                    //////GD.Print("writing field: ",f.Name,": ", ret, " as ",ret.GetType(), " under Namespace: ",ret.GetType().Namespace);
        //                }
        //            }
        //        }

        //    }

        //    //always store public properties if you can for this SaveInstance
        //    foreach (PropertyInfo p in properties)
        //    {
        //        //just make sure we aren't storing the same thing twice.
        //        if (fieldList.FindIndex(item => item.Name == p.Name) >= 0
        //        || p.GetValue(o) == null || p.GetValue(o) == o
        //        || new List<string>() { "Owner", "NativeInstance" }.Contains(p.Name)
        //        || (PersistAttribute)p.GetCustomAttribute(typeof(PersistAttribute)) != null)
        //        {
        //            continue;
        //        }
        //        else
        //        {

        //            object ret = ParseSaveObject(p.Name, p.GetValue(o), o);

        //            //make sure that we aren't storing something null that we can't parse!
        //            if (ret != null)
        //            {
        //                this.fieldBook.Add(p.Name, ParseSaveObject(p.Name, p.GetValue(o), o));
        //                //////GD.Print("writing property: ",p.Name,": ", ret, " as ",ret.GetType(), " under Namespace: ",ret.GetType().Namespace);
        //            }
        //        }

        //    }

        //}

        //private bool ParseAttribute(PersistAttribute att)
        //{
        //    if (att.IsPersist == false)
        //    {
        //        return false;
        //    }
        //    return att.IsPersist;
        //}

        //#region Collections
        //public object StoreCollection(String fieldName, object o, object parent)
        //{
        //    object returnValue = null;
        //    List<object> objList = new List<object>();
        //    Dictionary<object, object> objDict = new Dictionary<object, object>();
        //    Queue<object> objQueue = new Queue<object>();
        //    IGameObject[,] MapObjects;
        //    Track[,] Tracks1;
        //    if(parent.GetType() == typeof(MapLevel))
        //    {
        //        if (fieldName == "MapObjects")
        //        {
        //            for (int i = 0; i < ((MapLevel)o).IndexHeight + 1; i++)
        //            {
        //                for (int j = 0; j < ((MapLevel)o).IndexHeight + 1; j++)
        //                {
        //                    objList.Add(ParseSaveObject(null, ((MapLevel)o).MapObjects[i,j], null));
        //                }
        //            }
        //        }
        //        else if (fieldName == "Tracks1")
        //        {
        //            for (int i = 0; i < ((MapLevel)o).IndexHeight + 1; i++)
        //            {
        //                for (int j = 0; j < ((MapLevel)o).IndexHeight + 1; j++)
        //                {
        //                    objList.Add(ParseSaveObject(null, ((MapLevel)o).Tracks1[i, j], null));
        //                }
        //            }
        //        }
        //    }
        //    return null;

        //}

        //public void ParseCollection(String fieldName, object o, object parent)
        //{

        //    if (parent.GetType() == typeof(Card))
        //    {
        //        if (fieldName == "testStringList")
        //        {
        //            foreach (object thing in (List<object>)o)
        //            {
        //                //((Card)parent).testStringList.Add((string)(LoadProperty(null,thing,null)));
        //            }

        //        }
        //    }

        //}
        //public object LoadGenericCollection(String fieldName, object o, object parent)
        //{
        //    object returnValue = null;
        //    List<object> objList = new List<object>();
        //    Dictionary<object, object> objDict = new Dictionary<object, object>();
        //    Queue<object> objQueue = new Queue<object>();
        //    if (parent.GetType() == typeof(Card))
        //    {
        //        if (fieldName == "testStringList")
        //        {
        //            List<string> list = new List<string>();
        //            foreach (object thing in (List<object>)o)
        //            {
        //                list.Add((string)(LoadSingleProperty(null, thing)));
        //            }
        //            return list;

        //        }
        //    }
        //    else if (parent.GetType() == typeof(CardController))
        //    {
        //        if (fieldName == "cardList")
        //        {
        //            foreach (object thing in (List<object>)o)
        //            {
        //                ((CardController)parent).cardList.Add((Card)(LoadSingleProperty(null, thing)));
        //            }
        //            return objList;
        //        }
        //        else if (fieldName == "spellSlots")
        //        {
        //            foreach (KeyValuePair<object, object> thing in ((Dictionary<object, object>)o))
        //            {
        //                ((CardController)parent).spellSlots.Add((SpellSlot)(LoadSingleProperty(null, thing.Key)), (Card)(LoadSingleProperty(null, thing.Value)));
        //            }
        //            return objList;
        //        }
        //        else if (fieldName == "eventQueue")
        //        {

        //        }
        //    }
        //    else if (parent.GetType() == typeof(HandObject))
        //    {
        //        if (fieldName == "cards")
        //        {

        //            return objList;
        //        }
        //        else if (fieldName == "cardMap")
        //        {

        //            return objList;
        //        }
        //    }
        //    else if (parent.GetType() == typeof(HandView))
        //    {
        //        if (fieldName == "holders")
        //        {

        //            return objList;
        //        }
        //    }

        //    return null;

        //}
        //#endregion


        ///// <summary>
        ///// Determines what object type the given object should be returned as to store as a child for saving.
        ///// Will need to put final exceptions here if we need to parse them at some point
        ///// currently skip everything under Godot namespace (except for Vector2).
        ///// </summary>
        ///// <param name="o">The object to be parsed</param>
        ///// <returns>The parse object</returns>
        //public object ParseSaveObject(string fieldName, object o, object parent)
        //{
        //    object returnValue = null;

        //    //////GD.Print("parsing ",fieldName,": ", o, " as ",o.GetType());
        //    //need to expand on nodes that are GameObjects, otherwise we can just store a generic Node
        //    if (typeof(Node).IsInstanceOfType(o) && o.GetType() != typeof(Godot.AnimationPlayer))
        //    {
        //        returnValue = !typeof(GameObject).IsInstanceOfType(o) ? new SaveInstance(o, endNode: true) : new SaveInstance(o);
        //    }

        //    else if (!typeof(Node).IsInstanceOfType(o))
        //    {
        //        //if we are a model, create a new thing
        //        if (typeof(AbstractObjectModel).IsInstanceOfType(o))
        //        {
        //            returnValue = new SaveInstance(o);
        //        }
        //        else if (typeof(Enum).IsInstanceOfType(o))
        //        {
        //            returnValue = o;
        //        }
        //        else if (typeof(Vector2).IsInstanceOfType(o))
        //        {
        //            returnValue = new Vector2Save((Vector2)o);
        //        }
        //        else if (o.GetType().IsGenericType)
        //        {
        //            if (o.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>)
        //            || o.GetType().GetGenericTypeDefinition() == typeof(List<>)
        //            || o.GetType().GetGenericTypeDefinition() == typeof(Queue<>))
        //            {
        //                returnValue = StoreCollection(fieldName, o, parent);
        //            }
        //        }
        //        else if (o.GetType().IsPrimitive || o.GetType() == typeof(decimal) || o.GetType() == typeof(string))
        //        {
        //            return o;
        //        }
        //        //need to put final exceptions here if we need to parse them at some point
        //        //currently skip everything under Godot namespace (except for Vector2)
        //        else if (o.GetType().Namespace != "Godot")
        //        {
        //            returnValue = TryWriteObjectToJson(o);
        //        }

        //    }
        //    return returnValue;

        //}




        //private string TryWriteObjectToJson(object o)
        //{
        //    string returnValue = null;
        //    try
        //    {
        //        // //////GD.Print("serializing: ",JsonConvert.SerializeObject(o));
        //        returnValue = JsonConvert.SerializeObject(o);
        //    }
        //    catch (Exception e)
        //    {
        //        // //////GD.Print("EXCEPTION");
        //    }
        //    return returnValue;

        //}

        //private object TryReadObjectFromJson(string json)
        //{
        //    object returnValue = null;
        //    try
        //    {
        //        ////GD.Print("attempting to deserialize: ",json);
        //        returnValue = JsonConvert.DeserializeObject(json);
        //        ////GD.Print("deserialized: ",json, " to: ",returnValue);
        //    }
        //    catch (Exception e)
        //    {
        //        ////GD.Print("EXCEPTION: ");
        //    }
        //    return returnValue;

        //}

        //public void PrintHeirarchy(string indent)
        //{
        //    ////GD.Print(indent+"SaveInstance with type: ",this.objectType);
        //    indent += "     ";
        //    ////GD.Print(indent+"Resource Path: "+this.ResPath);

        //    foreach (string s in this.fieldBook?.Keys)
        //    {
        //        object o = null;
        //        this.fieldBook?.TryGetValue(s, out o);

        //        if (o.GetType() == typeof(SaveInstance))
        //        {
        //            SaveInstance saveInstance = (SaveInstance)o;
        //            saveInstance.PrintHeirarchy(indent);
        //        }
        //        else
        //        {
        //            ////GD.Print(indent+s+"  "+o);
        //        }
        //    }
        //}


        //private void LoadProperties(SaveInstance save, object saveInstanceObject, List<FieldInfo> fieldList)
        //{
        //    PropertyInfo[] properties = saveInstanceObject.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        //    foreach (PropertyInfo p in properties)
        //    {
        //        object value = null;
        //        try
        //        {
        //            object classField = p.GetValue(saveInstanceObject);
        //        }
        //        catch (Exception e)
        //        {
        //            ////GD.Print("FOUND THE EXCEPTION");
        //        }//////GD.Print("   p:",p.Name);

        //        //if the field exists in our save instance object, but not already parsed by from our fields
        //        if (save.fieldBook.TryGetValue(p.Name, out value) && !fieldList.Any(item => item.Name == p.Name))
        //        {
        //            ////GD.Print("Properties loader: ",p.Name, "  ", saveInstanceObject, "  ", saveInstanceObject.GetType());
        //            // ////GD.Print("         ---> ",value);
        //            //f.SetValue(saveInstanceObject, LoadProperty(f.Name, classField ,value, saveInstanceObject));

        //            if (saveInstanceObject.GetType() == typeof(Player))
        //            {
        //                //////GD.Print("assinging property ",p.Name,"    ",p.GetValue(saveInstanceObject));
        //            }
        //            object o = null;
        //            if (value.GetType().IsGenericType)
        //            {
        //                o = LoadGenericCollection(p.Name, value, saveInstanceObject);
        //            }
        //            else
        //            {
        //                o = LoadSingleProperty(p.GetValue(saveInstanceObject), value);
        //            }
        //            if (o != null)
        //                p.SetValue(saveInstanceObject, o);
        //        }
        //    }
        //}

        //private List<FieldInfo> LoadFields(SaveInstance save, object saveInstanceObject)
        //{

        //    FieldInfo[] fields = saveInstanceObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //    List<FieldInfo> fieldList = fields.OfType<FieldInfo>().ToList();

        //    foreach (FieldInfo f in fieldList)
        //    {
        //        object value = null;
        //        object classField = f.GetValue(saveInstanceObject);
        //        //if the field exists in our save instance object
        //        if (save.fieldBook.TryGetValue(f.Name, out value))
        //        {
        //            ////GD.Print("Field Loader: ",f.Name,"   ",classField, "  ", saveInstanceObject, "  ", saveInstanceObject.GetType());
        //            // ////GD.Print(f.Name, " ---> ",value);
        //            //f.SetValue(saveInstanceObject, LoadProperty(f.Name, classField ,value, saveInstanceObject));

        //            object o = null;
        //            if (value.GetType().IsGenericType)
        //            {
        //                o = LoadGenericCollection(f.Name, value, saveInstanceObject);
        //            }
        //            else
        //            {
        //                o = LoadSingleProperty(f.GetValue(saveInstanceObject), value);
        //            }

        //            if (o != null)
        //            {
        //                f.SetValue(saveInstanceObject, o);
        //                if (typeof(Player).IsInstanceOfType(f.GetValue(saveInstanceObject)))
        //                {
        //                    Player p = (Player)f.GetValue(saveInstanceObject);

        //                    ////GD.Print("got the player ", p.animationState," ",p.animationPlayer," ",p.animationTree);
        //                }
        //            }
        //        }
        //    }
        //    return fieldList;
        //}
        ///// <summary>
        ///// Compares a field object to it's saved value and populates it with the neccessary data
        ///// </summary>
        ///// <param name="fieldObject"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public object LoadSingleProperty(object fieldObject, object value)
        //{
        //    //////GD.Print("Loading single property: ",value, " of type: ",value.GetType());
        //    object o = null;
        //    // if(fieldObject !=null){
        //    //     //this means we've loaded in a gameobject via _Ready()
        //    //     if(typeof(GameObject).IsInstanceOfType(fieldObject))
        //    //     {
        //    //         o = LoadSaveInstance(value,(GameObject)fieldObject,false);
        //    //     }
        //    // }
        //    if (typeof(SaveInstance).IsInstanceOfType(value))
        //    {
        //        o = LoadSaveInstance((SaveInstance)value, fieldObject);
        //    }
        //    else if (typeof(Enum).IsInstanceOfType(value))
        //    {
        //        o = State.GetEnumType(value.ToString());
        //    }
        //    else if (typeof(Vector2Save).IsInstanceOfType(value))
        //    {
        //        o = ((Vector2Save)value).ToVector();
        //    }
        //    else if (value.GetType().IsPrimitive || value.GetType() == typeof(Decimal) || value.GetType() == typeof(String))
        //    {
        //        return value;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            o = TryReadObjectFromJson((string)value);
        //        }
        //        catch (Exception e)
        //        {
        //            ////GD.Print(e);
        //            ////GD.Print(fieldObject,"   ",value);
        //        }
        //    }
        //    return o;
        //}

        ///// <summary>
        ///// This will fail if the scene that's saved isn't a root scene that can load data in
        ///// </summary>
        ///// <param name="save"></param>
        ///// <param name="root"></param>
        //public Node LoadSceneRoot(SaveInstance save, Node root)
        //{
        //    Node n = Params.LoadScene(save.ResPath);
        //    Convert.ChangeType(n, save.objectType);
        //    root.AddChild(n);
        //    List<FieldInfo> fields = LoadFields(save, n);
        //    LoadProperties(save, n, fields);
        //    return n;
        //}

        //public object LoadSaveInstance(SaveInstance save, object loadedObject)
        //{
        //    //Node parentNode = new Node();
        //    Node saveInstanceNode = new Node();
        //    //AbstractObjectModel parentModel = new AbstractObjectModel();
        //    AbstractObjectModel saveInstanceModel = new AbstractObjectModel();
        //    object returnobj = null;


        //    if (typeof(Node).IsAssignableFrom(save.objectType))
        //    {

        //        if (loadedObject == null && !String.IsNullOrEmpty(save.ResPath))
        //        {
        //            //check that this scene has a parent 
        //            saveInstanceNode = Params.LoadScene(save.ResPath);
        //            returnobj = saveInstanceNode;
        //            Convert.ChangeType(saveInstanceNode, save.objectType);
        //            // else
        //            // {
        //            //     //returnobj
        //            // }
        //        }
        //        else if (loadedObject != null)
        //        {
        //            returnobj = loadedObject;
        //        }
        //        else
        //        {
        //            //////GD.Print("Attempting to change ",saveInstanceNode, " with type ",saveInstanceNode.GetType(), " to " ,save.objectType);
        //            returnobj = Activator.CreateInstance(save.objectType);
        //        }



        //        //returnobj = saveInstanceNode;
        //    }
        //    else if (typeof(AbstractObjectModel).IsAssignableFrom(save.objectType))
        //    {
        //        Convert.ChangeType(saveInstanceModel, save.objectType);
        //        returnobj = saveInstanceModel;
        //    }

        //    //we now have a non-null object that we can set properties and fields.
        //    // if(save.objectType == typeof(Player)){
        //    //     ////GD.Print("starting to parse player with animation state: ", ((Player)returnobj).animationState); 
        //    // }
        //    List<FieldInfo> fields = LoadFields(save, returnobj);
        //    LoadProperties(save, returnobj, fields);
        //    //LoadFields(save,returnobj);
        //    return returnobj;


        //}



        //public object LoadProperty(string fieldName, object fieldObject, object o, object parent)
        //{
        //    ////GD.Print("attemping to load object ", o, " as ",o.GetType(), parent, fieldName);
        //    object returnValue = null;
        //    //need to expand on nodes that are GameObjects, otherwise we can just store a generic Node
        //    if (typeof(SaveInstance).IsInstanceOfType(fieldObject))
        //    {
        //        // returnValue = LoadSaveInstance((SaveInstance)o);
        //    }
        //    else
        //    {
        //        if (typeof(Enum).IsInstanceOfType(fieldObject))
        //        {
        //            returnValue = State.GetEnumType(o.ToString());
        //        }
        //        else if (typeof(Vector2Save).IsInstanceOfType(fieldObject))
        //        {
        //            ////GD.Print("Attempting to desserialize vector2: ",o, ", ",fieldName);
        //            object json = JsonConvert.DeserializeObject((string)o);

        //            ////GD.Print(json, "   ",json.GetType());
        //            JObject conv = (JObject)JsonConvert.DeserializeObject((string)o);

        //            returnValue = conv.ToObject<Vector2Save>().ToVector();
        //        }
        //        else if (o.GetType().IsGenericType)
        //        {
        //            returnValue = LoadGenericCollection(fieldName, o, parent);
        //        }
        //        else
        //        {
        //            try
        //            {
        //                returnValue = TryReadObjectFromJson((string)o);
        //            }
        //            catch (Exception e)
        //            {
        //                ////GD.Print(e);
        //                ////GD.Print();
        //            }
        //        }

        //    }
        //    return returnValue;
        //}


        //public void LoadPropertyForNode(string fieldName, object fieldObject, Node parent)
        //{
        //    //////GD.Print(parent);
        //    var fields = parent.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //    var properties = parent.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //    List<string> fieldNames = new List<string>();

        //    foreach (FieldInfo f in fields)
        //    {
        //        //if it's not null, then use it to set the value
        //        //we assume all scenes have been loaded in at this point and referenced by _Ready or similar

        //        object originObject = f.GetValue(parent);
        //        if (originObject != null && f.Name == fieldName)
        //        {
        //            //unpack Saveinstance for nodes & Collections
        //            if (typeof(SaveInstance) == fieldObject.GetType())
        //            {
        //                //////GD.Print("is type of SaveINstance");
        //                SaveInstance si = ((SaveInstance)fieldObject);

        //                foreach (string name in si.fieldBook?.Keys)
        //                {

        //                    object obj = null;
        //                    si.fieldBook.TryGetValue(name, out obj);

        //                    LoadPropertyForNode(name, obj, (Node)originObject);
        //                }

        //            }
        //            else if (f.GetValue(parent) is Enum)
        //            {
        //                object o = State.GetEnumType(fieldObject.ToString());
        //                //////GD.Print("Found an enum. Setting variable to: ",o, "  from string: ", fieldObject.ToString());
        //                f.SetValue(parent, o);
        //            }
        //            else if (f.GetValue(parent) is Vector2)
        //            {

        //                JObject conv = (JObject)JsonConvert.DeserializeObject((string)fieldObject);

        //                f.SetValue(parent, conv.ToObject<Vector2Save>().ToVector());
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    //////GD.Print("      Attempting to deserialize field: " ,fieldName," with type: ",fieldObject.GetType(), "  on parent", parent);
        //                    string s = (string)fieldObject;
        //                    f.SetValue(parent, JsonConvert.DeserializeObject(s));
        //                }
        //                catch (Exception e)
        //                {
        //                    //////GD.Print(e);
        //                    //////GD.Print("Cannot convert from ", fieldObject, " to ", (string)fieldObject);
        //                }
        //            }
        //        }
        //        else if (originObject == null && f.Name == fieldName)
        //        {
        //            //////GD.Print("Foudn null field: ", fieldName);
        //        }
        //        else
        //        {

        //        }
        //    }

        //    foreach (PropertyInfo p in properties)
        //    {
        //        if (p.Name == fieldName)
        //        {
        //            // ////GD.Print("found name: ", fieldName, " for object: ",parent);
        //        }
        //        //if it's not null, then use it to set the value
        //        //we assume all scenes have been loaded in at this point and referenced by _Ready or similar
        //        object originObject = p.GetValue(parent);
        //        if (originObject != null && p.Name == fieldName)
        //        {
        //            //////GD.Print("Found non null field: ", fieldName, ": ",fieldObject, "  ",f.GetValue(parent).GetType());
        //            //unpack Saveinstance for nodes
        //            if (typeof(SaveInstance) == fieldObject.GetType())
        //            {
        //                //////GD.Print("is type of SaveINstance");
        //                SaveInstance si = ((SaveInstance)fieldObject);

        //                foreach (string name in si.fieldBook?.Keys)
        //                {

        //                    object obj = null;
        //                    si.fieldBook.TryGetValue(name, out obj);

        //                    LoadPropertyForNode(name, obj, (Node)originObject);
        //                }

        //            }
        //            else if (p.GetValue(parent) is Enum)
        //            {
        //                try
        //                {
        //                    object o = State.GetEnumType(fieldObject.ToString());
        //                    //////GD.Print("Found an enum. Setting variable to: ",o, "  from string: ", fieldObject.ToString());
        //                    p.SetValue(parent, o);
        //                }
        //                catch (Exception e)
        //                {

        //                }
        //            }
        //            else if (p.GetValue(parent) is Vector2)
        //            {
        //                if (parent.GetParent() != null)
        //                {
        //                    ////GD.Print("set position relative to parent, not global");
        //                    if (p.Name.Contains("Global"))
        //                    {
        //                        continue;
        //                    }
        //                }
        //                //if(p.Name == "RectGlobalPosition")
        //                //continue;
        //                JObject conv = (JObject)JsonConvert.DeserializeObject((string)fieldObject);

        //                ////GD.Print("Found ",parent.Name, " Vector2 ",p.Name, ": ",conv);

        //                p.SetValue(parent, conv.ToObject<Vector2Save>().ToVector());
        //                if (typeof(Control).IsInstanceOfType(parent))
        //                {
        //                    ////GD.Print("New control positions are: ,",((Control)parent).RectPosition,((Control)parent).RectGlobalPosition);
        //                }
        //            }
        //            else if (p.GetValue(parent) is AnimationPlayer)
        //            {
        //                continue;
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    //////GD.Print("      Attempting to deserialize field: " ,fieldName," with type: ",fieldObject.GetType(), "  on parent", parent);
        //                    string s = (string)fieldObject;
        //                    p.SetValue(parent, JsonConvert.DeserializeObject(s));
        //                }
        //                catch (Exception e)
        //                {
        //                    //////GD.Print(e);
        //                    //////GD.Print("Cannot convert from ", fieldObject, " to ", (string)fieldObject);
        //                }
        //            }
        //        }
        //        else if (originObject == null && p.Name == fieldName)
        //        {
        //            //////GD.Print("Foudn null field: ", fieldName);
        //        }
        //        else
        //        {

        //        }
        //    }


        //}



    }
}
