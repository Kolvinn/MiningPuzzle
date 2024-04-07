using Godot;
using MagicalMountainMinery.Main;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Data.Load
{

    //public struct Vec2
    //{
    //    public float x;
    //    public float y;

    //    [JsonConstructor]
    //    public Vec2(float x, float y)
    //    {
    //        this.x = x;
    //        this.y = y;
    //    }

    //}
    public static class SaveLoader
    {

        static Dictionary<object, object> parsedClasses;
        public static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter>()
            { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };
        public static SaveInstance SaveGame(Node rootNode)
        {
            parsedClasses = new Dictionary<object, object>();
            return CreateSaveInstance(rootNode);
        }

        #region SaveGame
        public static SaveInstance CreateSaveInstance(object o, bool endNode = false, bool isEnum = false)
        {
            Type objectType = o.GetType();

            //parsedClasses.Add(o);
            //////GD.Print("Adding parent object: ", o, " with type: ", o.GetType());
            SaveInstance s = new SaveInstance(objectType);

            if (typeof(Node).IsInstanceOfType(o))
            {
                s.ResPath = ((Node)o).SceneFilePath;
            }

            //this.objectType = o.GetType();
            //this.fieldBook = new Dictionary<string, object>();



            var fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                | BindingFlags.DeclaredOnly)
                .Where(f => !f.Name.ToLower().Contains("backingfield")).ToList();

            var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();

            if (typeof(ISaveable).IsInstanceOfType(o))
            {
                var data = ((ISaveable)o).GetSaveRefs();
                var extra = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                var added = extra.Where(item => data.Contains(item.Name));
                properties = properties.Concat(added).ToList();
                //properties.Union(extra.Where(item => data.Contains(item.Name)));
            }

            //if not endnode, we need to store private and public fields
            //if (!endNode)
            //{
            //    foreach (FieldInfo f in fields)
            //    {

            //        object fieldObject = f.GetValue(o);

            //        if (fieldObject == null)
            //        {
            //            continue;
            //        }

            //        object ret;
            //        if (parsedClasses.Keys.Contains(fieldObject))
            //        {
            //            ////GD.Print("      Already parsed object: ",fieldObject, " for parent", o.GetType());
            //            object savedRef;
            //            parsedClasses.TryGetValue(fieldObject, out savedRef);
            //            ret = new ObjectRef(savedRef);
            //        }
            //        else if (f.GetCustomAttribute(typeof(StoreCollectionAttribute)) != null)
            //        {
            //            var att = (StoreCollectionAttribute)f.GetCustomAttribute(typeof(StoreCollectionAttribute));
            //            if (att.ShouldStore)
            //                ret = StoreCollection(f.Name, fieldObject, o);
            //            else
            //                ret = null;
            //        }
            //        else
            //        {
            //            ret = ParseSaveObject(f.Name, fieldObject, o);

            //            if (ret != null && ret.GetType() == typeof(SaveInstance))
            //                parsedClasses.Add(fieldObject, ret);
            //        }

            //        //make sure that we aren't storing something null that we can't parse!
            //        if (ret != null)
            //        {
            //            //////GD.Print("Adding field object: ", fieldObject, " with type: ", fieldObject.GetType());

            //            s.fieldBook.Add(f.Name, ret);
            //            ////////GD.Print("writing field: ",f.Name,": ", ret, " as ",ret.GetType(), " under Namespace: ",ret.GetType().Namespace);
            //        }
            //        // }
            //    }

            //}

            //always store public properties if you can for this SaveInstance
            foreach (PropertyInfo p in properties)
            {
                //just make sure we aren't storing the same thing twice.
                //or anything weird like globals
                //if (fieldList.FindIndex(item => item.Name == p.Name) >= 0
                //|| p.GetValue(o) == null || p.GetValue(o) == o
                //|| new List<string>() { "Owner", "NativeInstance", "Scale" }.Contains(p.Name)
                //|| p.Name.ToLower().Contains("ptr") || p.Name.ToLower().Contains("memory") || p.Name.ToLower().Contains("global")
                //|| p.Name.Contains("Scale") || p.Name.Contains("Rotation")
                // || p.Name.ToLower().Contains("ptr") || p.Name.ToLower().Contains("memory")
                // )
                //{
                //    continue;
                //}
                //else
                // {
                object fieldObject = p.GetValue(o);
                if (fieldObject == null)
                {
                    continue;
                }
                object ret;
                if (parsedClasses.Keys.Contains(fieldObject))
                {
                    ////GD.Print("      Already parsed object: ",fieldObject, " for parent", o.GetType());
                    object savedRef;
                    parsedClasses.TryGetValue(fieldObject, out savedRef);
                    ret = new ObjectRef(savedRef);
                }
                else if ((StoreCollectionAttribute)p.GetCustomAttribute(typeof(StoreCollectionAttribute)) != null)
                {
                    ret = StoreCollection(p.Name, fieldObject, o);
                }
                else
                {
                    ret = ParseSaveObject(p.Name, fieldObject, o);

                    if (ret != null && ret.GetType() == typeof(SaveInstance))
                        parsedClasses.Add(fieldObject, ret);
                }

                //make sure that we aren't storing something null that we can't parse!
                if (ret != null)
                {
                    s.fieldBook.Add(p.Name, ret);
                }
                //}

            }
            //if(o is Node node)
            //{
            //    var sprites = node.GetChildren().Where(item => item is Mineable).ToList();
            //    foreach (var child in node.GetChildren())
            //    {
            //        object ret;
            //        if (parsedClasses.Keys.Contains(child))
            //        {
            //            ////GD.Print("      Already parsed object: ",fieldObject, " for parent", o.GetType());
            //            object savedRef;
            //            parsedClasses.TryGetValue(child, out savedRef);
            //            ret = new ObjectRef(savedRef);
            //        }
            //        else
            //        {
            //            ret = ParseSaveObject(node.Name, child, o);
            //            if(ret!=null)
            //            {
            //                s.childBook.Add(child.Name,)
            //            }
            //            if (ret != null && ret.GetType() == typeof(SaveInstance))
            //            {
            //                parsedClasses.Add(child, ret);
            //            }
            //        }
            //    }
            //}

            return s;
        }



        public static object ParseSaveObject(string fieldName, object o, object parent)
        {
            object returnValue = null;
            if (o == null)
            {
                return null;

            }
            else
            {
                GD.Print("Not null");
            }
            ////////GD.Print("parsing ",fieldName,": ", o, " as ",o.GetType());
            //need to expand on nodes that are GameObjects, otherwise we can just store a generic Node
            if (typeof(Node).IsInstanceOfType(o) && o.GetType() != typeof(Godot.AnimationPlayer))
            {
                returnValue = !typeof(GameObject).IsInstanceOfType(o) ? CreateSaveInstance(o, endNode: true) : CreateSaveInstance(o);
            }

            else if (!typeof(Node).IsInstanceOfType(o))
            {
                //if (typeof(Enum).IsInstanceOfType(o))
                //{
                //    //don't store mouse states     
                //    // if (((Enum)o).GetType() == typeof(State.MouseEventState))
                //    // return null;
                //    returnValue = o;
                //}
                //else if (typeof(Vector2).IsInstanceOfType(o))
                //{
                //    returnValue = o;
                //}
                //if (o.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>)
                //    || o.GetType().GetGenericTypeDefinition() == typeof(List<>)
                //    || o.GetType().GetGenericTypeDefinition() == typeof(Queue<>)
                //    || o.GetType().GetGenericTypeDefinition() == typeof(Array[,])
                //    || (o.GetType().IsArray))
                //{
                //        returnValue = StoreCollection(fieldName, o, parent);
                //}

                //else if (typeof(Vector2).IsInstanceOfType(o))
                //{
                //    returnValue = new Vec2(((Vector2)o).X, ((Vector2)o).Y);
                //}
                if (o.GetType().IsPrimitive || o.GetType() == typeof(Decimal) || o.GetType() == typeof(String))
                {
                    return o;
                }
                //need to put final exceptions here if we need to parse them at some point
                //currently skip everything under Godot namespace (except for Vector2)
                else 
                {
                    returnValue = TryWriteObjectToJson(o);
                }

            }
            return returnValue;

        }

        private static object TryWriteObjectToJson(object o)
        {
            object returnValue = null;
            try
            {
                // ////////GD.Print("serializing: ",JsonConvert.SerializeObject(o));
                returnValue = JsonConvert.SerializeObject(o, jsonSerializerSettings);
            }
            catch (Exception e)
            {
                GD.Print("EXCEPTION");
                returnValue = o;
            }
            return returnValue;

        }
        #endregion

        #region LoadGame
        /// <summary>
        /// This will fail if the scene that's saved isn't a root scene that can load data in
        /// </summary>
        /// <param name="save"></param>
        /// <param name="root"></param>
        public static Node LoadGame(SaveInstance save, Node root)
        {
            parsedClasses = new Dictionary<object, object>();
            Node n = Runner.LoadScene<Node2D>(save.ResPath);
            if (n == null)
            {
                n = (Node)Activator.CreateInstance(save.objectType);
            }
            Convert.ChangeType(n, save.objectType);
            root.AddChild(n);
            LoadFields(save, n);
            LoadProperties(save, n);
            return n;
        }


        // /// <summary>
        // /// This will fail if the scene that's saved isn't a root scene that can load data in
        // /// </summary>
        // /// <param name="save"></param>
        // /// <param name="root"></param>
        // public static Node LoadSceneRoot(SaveInstance save, Node root){
        //     Node n = Params.LoadScene(save.ResPath);
        //     Convert.ChangeType(n,save.objectType);
        //     root.AddChild(n);        
        //     List<FieldInfo> fields = LoadFields(save,n);   
        //     LoadProperties(save,n,fields);
        //     return n;
        // }

        public static object LoadSaveInstance(SaveInstance save, object loadedObject)
        {
            //Node parentNode = new Node();
            Node saveInstanceNode = new Node();
            //AbstractObjectModel parentModel = new AbstractObjectModel();
            // AbstractObjectModel saveInstanceModel = new AbstractObjectModel();
            object returnobj = null;


            if (typeof(Node).IsAssignableFrom(save.objectType))
            {

                if (loadedObject == null && !String.IsNullOrEmpty(save.ResPath))
                {

                    ////GD.Print("ecks dee      ",save.objectType);

                    //check that this scene has a parent 
                    saveInstanceNode = Runner.LoadScene(save.ResPath);
                    returnobj = saveInstanceNode;
                    Convert.ChangeType(saveInstanceNode, save.objectType);
                    // else
                    // {
                    //     //returnobj
                    // }
                }
                else if (loadedObject != null)
                {
                    returnobj = loadedObject;
                }
                else
                {
                    ////////GD.Print("Attempting to change ",saveInstanceNode, " with type ",saveInstanceNode.GetType(), " to " ,save.objectType);
                    returnobj = Activator.CreateInstance(save.objectType);
                }



                //returnobj = saveInstanceNode;
            }
            // else if (typeof(AbstractObjectModel).IsAssignableFrom(save.objectType))

            // Convert.ChangeType(saveInstanceModel, save.objectType);
            // returnobj = saveInstanceModel;
            // }

            //we now have a non-null object that we can set properties and fields.
            // if(save.objectType == typeof(Player)){
            //     //////GD.Print("starting to parse player with animation state: ", ((Player)returnobj).animationState); 
            // }
            LoadFields(save, returnobj);
            LoadProperties(save, returnobj);
            //LoadFields(save,returnobj);
            return returnobj;


        }

        private static object TryReadObjectFromJson(string json)
        {
            object returnValue = null;
            try
            {
                //////GD.Print("attempting to deserialize: ",json);
                returnValue = JsonConvert.DeserializeObject(json, jsonSerializerSettings);
                //////GD.Print("deserialized: ",json, " to: ",returnValue);
            }
            catch (Exception e)
            {
                GD.Print("EXCEPTION: ");
                return json;
            }
            return returnValue;

        }

        /// <summary>
        /// Compares a field object to it's saved value and populates it with the neccessary data
        /// </summary>
        /// <param name="fieldObject"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object LoadSingleProperty(object fieldObject, object value)
        {
            ////////GD.Print("Loading single property: ",value, " of type: ",value.GetType());
            object o = null;
            if (value == null)
                return null;
            // if(fieldObject !=null){
            //     //this means we've loaded in a gameobject via _Ready()
            //     if(typeof(GameObject).IsInstanceOfType(fieldObject))
            //     {
            //         o = LoadSaveInstance(value,(GameObject)fieldObject,false);
            //     }
            // }
            if (typeof(SaveInstance).IsInstanceOfType(value))
            {
                o = LoadSaveInstance((SaveInstance)value, fieldObject);
            }
            else if (typeof(ObjectRef).IsInstanceOfType(value))
            {

                parsedClasses.TryGetValue(((ObjectRef)value).storedReference, out o);
                ////GD.Print("found the class for field, "  ,fieldObject, ", let's see if it returns anything: ",o);
            }
            else if (typeof(Enum).IsInstanceOfType(value))
            {
                o = ResourceStore.GetEnumType(value.ToString());
            }
            //else if (typeof(Vec2).IsInstanceOfType(value))
            //{
            //    //////GD.Print("Loading single property: ",value, " of type: ",value.GetType());
            //    o = new Vector2(((Vec2)value).x, ((Vec2)value).y);

            //}
            else if (value.GetType() == typeof(string))
            {
                try
                {
                    o = TryReadObjectFromJson((string)value);
                }
                catch (Exception e)
                {
                    GD.Print(e);
                    //////GD.Print(fieldObject,"   ",value);
                }
            }

            else if (value.GetType().IsPrimitive || value.GetType() == typeof(decimal))
            {
                return value;
            }
            return o;
        }

        public static object ParseToken(Type origin, JToken token)
        {
            var str = JToken.Parse(token.ToString()).ToString();
            var vec = new Vector2().GetType();
            vec.GetType();
            if (origin.Name == "Vector2")
            {
                return JsonConvert.DeserializeObject<Vector2>(str);
            }
            else if (origin == typeof(GameResource))
            {
                return JsonConvert.DeserializeObject<GameResource>(str);
            }
            else if (origin == typeof(IndexPos))
            {
                return JsonConvert.DeserializeObject<IndexPos>(str);
            }
            else if(origin == typeof(Condition))
            {
                return JsonConvert.DeserializeObject<Condition>(str);
            }
            return null;
        }

        private static void LoadProperties(SaveInstance save, object saveInstanceObject)
        {
            var properties = saveInstanceObject.GetType().GetProperties(BindingFlags.NonPublic
                | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(p => save.fieldBook.Keys.Contains(p.Name)).ToList();

            foreach (PropertyInfo p in properties)
            {
                object value = null;

                object classField = p.GetValue(saveInstanceObject);

                if (p.Name.Contains("Ptr") || p.Name == "Tracks1")
                    continue;

                //if the field exists in our save instance object, but not already parsed by from our fields
                if (save.fieldBook.TryGetValue(p.Name, out value))
                {


                    GD.Print("Loading: '", p.Name, " in parent: ", saveInstanceObject);
                    var type = value?.GetType();
                    var namey = value?.ToString();
                    object o = null;
                    if(p.GetCustomAttribute(typeof(StoreCollectionAttribute)) != null)
                    {
                        var att = (StoreCollectionAttribute)p.GetCustomAttribute(typeof(StoreCollectionAttribute));
                        if (att.ShouldStore)
                            o = LoadGenericCollection(p.Name, (JArray)value, saveInstanceObject);
                        else
                            o = null;
                    }
                    else if (value is JToken)
                    {
                        o = ParseToken(p.PropertyType, (JToken)value);
                    }
                    else
                    {
                        try
                        {
                            o = LoadSingleProperty(classField, value);
                        }
                        catch (Exception e)
                        {
                            GD.PrintErr(e);
                        }
                        //add the saved instances as opposed to the loaded one 
                        //because we need to check against object refs

                        if (o != null && value.GetType() == typeof(SaveInstance))
                        {
                            parsedClasses.Add(value, o);
                            //////GD.Print("Adding into parsed classes: ",value, " with object: ",o);
                        }
                    }

                    if (o != null && p.CanWrite)
                    {
                        if (o is string)
                        {
                            var obj = ResourceStore.GetEnumType(o as string);
                            if (obj != null)
                                o = obj;
                        }
                        if (o.GetType().IsPrimitive && o is not string)//o is Int64 || o is Int32 || o is Int16)
                            p.SetValue(saveInstanceObject, ParsePrimitive(p.PropertyType.FullName, o));

                        else
                        {
                            try
                            {
                                p.SetValue(saveInstanceObject, o);
                            }
                            catch(Exception e) 
                            { 
                                GD.PrintErr(e.ToString()); 
                            }
                        }
                    }

                }
            }
        }

        private static void LoadFields(SaveInstance save, object saveInstanceObject)
        {
            //GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var fields = saveInstanceObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                | BindingFlags.DeclaredOnly | BindingFlags.Static)
                .Where(f => save.fieldBook.Keys.Contains(f.Name)).ToList();

            //FieldInfo[] fields = saveInstanceObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //List<FieldInfo> fieldList = fields.OfType<FieldInfo>().Where.ToList();

            foreach (FieldInfo f in fields)
            {

                object value = null;
                object classField = f.GetValue(saveInstanceObject);
                //if the field exists in our save instance object
                if (save.fieldBook.TryGetValue(f.Name, out value))
                {
                    if (f.Name.Contains("Ptr"))
                        continue;

                    //use field object
                    if ((PostLoad)f.GetCustomAttribute(typeof(PostLoad)) != null)
                    {

                    }

                    object o = null;
                    if (value.GetType().IsGenericType || value.GetType().IsArray)
                    {
                        o = LoadGenericCollection(f.Name, value, saveInstanceObject);
                    }
                    else if (value is JToken)
                    {
                        o = ParseToken(f.FieldType, (JToken)value);

                        //var token = (JToken)value;
                        // string rootValue = token.Value<string>();
                        //var obj = token.Value<Vec2?>();

                    }
                    else if (f.GetType().IsEnum)
                    {
                        f.SetValue(saveInstanceObject, (Enum)o);
                    }
                    else
                    {
                        try
                        {
                            o = LoadSingleProperty(classField, value);
                        }
                        catch (Exception e)
                        {

                        }
                        //ad   d the saved instances as opposed to the loaded one 
                        //because we need to check against object refs

                        if (o != null && value.GetType() == typeof(SaveInstance))
                        {
                            parsedClasses.Add(value, o);
                            //////GD.Print("Adding into parsed classes: ",value, " with object: ",o);
                        }
                    }

                    if (o != null)
                    {
                        if (o.GetType().IsPrimitive && o is not string)//o is Int64 || o is Int32 || o is Int16)
                            f.SetValue(saveInstanceObject, ParsePrimitive(f.FieldType.FullName, o));
                        else
                        {
                            GD.Print("Setting: '", f.Name, "' to object: '", o, " in parent: ", saveInstanceObject);
                            f.SetValue(saveInstanceObject, o);
                        }
                    }
                }
            }
        }

        public static object ParsePrimitive(string baseType,object obj)
        {
            var temp = Convert.ToInt64(obj);
            object value;
            if (temp >= Int16.MinValue && temp <= Int16.MaxValue && baseType == "System.Int16")
                value = Convert.ToInt16(obj);
            else if (temp >= Int32.MinValue && temp <= Int32.MaxValue && baseType == "System.Int32")
                value = Convert.ToInt32(obj);
            else if (baseType == "System.Single")
                value = Convert.ToSingle(obj);
            else if (baseType == "System.Double")
                value = Convert.ToDouble(obj);
            else if (baseType == "System.Boolean")
                value = Convert.ToBoolean(obj);
            else if (baseType == "System.Int64")
                value = Convert.ToInt64(obj);
            else if (baseType == "System.UInt64")
                value = Convert.ToUInt64(obj);
            else if (baseType == "System.UInt32")
                value = Convert.ToUInt32(obj);
            else
                value = temp;


            return value;
        }
        public static void ParseInt(FieldInfo f, object saveInstanceObject, object obj)
        {
            if (obj is Double)
            {
                f.SetValue(saveInstanceObject, obj);
                return;
            }
            var baseType = f.FieldType.FullName;
            object value;
            var temp = Convert.ToInt64(obj);
            //var tt = instance;
            //if (tt is Int64)
            //{
            //    GD.Print("ss");
            //}
            //else if(tt is Int32)
            //{
            //    GD.Print("ss");
            //}
            //else if(tt is Int16) 
            //{
            //    GD.Print("ss");
            //} 

            if (temp >= Int16.MinValue && temp <= Int16.MaxValue && baseType == "System.Int16")
                value = Convert.ToInt16(obj);
            else if (temp >= Int32.MinValue && temp <= Int32.MaxValue && baseType == "System.Int32")
                value = Convert.ToInt32(obj);
            else
                value = temp;

            f.SetValue(saveInstanceObject, value);
        }
        #endregion

        #region CollectionStorage
        public static object StoreCollection(String fieldName, object o, object parent)
        {
            object returnValue = null;
            List<object> objList = new List<object>();
            Dictionary<object, object> objDict = new Dictionary<object, object>();
            Queue<object> objQueue = new Queue<object>();
            IInteractable[,] MapObjects;
            Track[,] Tracks;
            object[,] doubleObj;
            if (parent.GetType() == typeof(MapLevel))
            {
                if (fieldName == "MapObjects")
                {
                    var array = ((IInteractable[,])o);
                    int w = ((IInteractable[,])o).GetLength(0);
                    int h = ((IInteractable[,])o).GetLength(1);
                    doubleObj = new object[w, h];
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            doubleObj[i, j] = ParseSaveObject(null, array[i, j], null);
                        }
                    }
                    return doubleObj;
                }
                else if (fieldName == "StartPositions" || fieldName == "EndPositions" || fieldName == "Blocked")
                {
                    objList = new List<object>();
                    var list = (List<IndexPos>)o;
                    foreach (var c in list)
                    {
                        objList.Add(ParseSaveObject(null, c, null));
                    }
                    return objList;
                }
            }
            if(parent.GetType() == typeof(LevelTarget))
            {
                if (fieldName == "Conditions" || fieldName == "BonusConditions")
                {
                    var list = (List<Condition>)o;
                    foreach (var c in list)
                    {
                        objList.Add(ParseSaveObject(null, c, null));
                    }
                    return objList;
                }
                else if (fieldName == "Batches")
                {
                    var list = (List<int>)o;
                    foreach (var c in list)
                    {
                        objList.Add(ParseSaveObject(null, c, null));
                    }
                    return objList;
                }

            }

            return null;

        }

        public static object LoadGenericCollection(String fieldName, object o, object parent, bool postLoad = false)
        {
            object returnValue = null;

            if (parent.GetType() == typeof(MapLevel))
            {
                if (fieldName == "MapObjects")
                {
                    var jArray = (JArray)o;
                    //var array = ((object[,])o);
                    var str = JArray.Parse(jArray.ToString()).ToString();
                    var mapObjects = JsonConvert.DeserializeObject<SaveInstance[,]>(str);

                    var rocks = new IInteractable[mapObjects.GetLength(0), mapObjects.GetLength(1)];
                    for (int i = 0; i < mapObjects.GetLength(0); i++)
                    {
                        for (int j = 0; j < mapObjects.GetLength(1); j++)
                        {
                            rocks[i, j] = (IInteractable)LoadSingleProperty(null, mapObjects[i, j]);
                        }

                    }
                    return rocks;
                }
                //list of indexpos
                else if(fieldName == "StartPositions" || fieldName == "EndPositions" || fieldName == "Blocked")
                {
                    var jArray = (JArray)o;
                    //var array = ((object[,])o);
                    var str = JArray.Parse(jArray.ToString()).ToString();
                    var list = JsonConvert.DeserializeObject<List<string>>(str);
                    var objList = new List<IndexPos>();
                    foreach (var c in list)
                    {
                        objList.Add((IndexPos)LoadSingleProperty(null, c));
                    }
                    return objList;
                }
            }
            if (parent.GetType() == typeof(LevelTarget))
            {
                var jArray = (JArray)o;
                var str = JArray.Parse(jArray.ToString()).ToString();
                if (fieldName == "Conditions" || fieldName == "BonusConditions")
                {
                    var list = JsonConvert.DeserializeObject<List<string>>(str);
                    var objList = new List<Condition>();
                    foreach (var c in list)
                    {
                        objList.Add((Condition)LoadSingleProperty(null, c));
                    }
                    return objList;
                }
                else if( fieldName == "Batches")
                {
                    var list = JsonConvert.DeserializeObject<List<string>>(str);
                    var objList = new List<int>();
                    foreach (var c in list)
                    {
                        objList.Add((int)ParsePrimitive("System.Int32", c));
                    }
                    return objList;
                }
               
                //var list = (List<Condition>)o;
                
                //return objList;
            }

            return null;

        }
        #endregion
    }

}
