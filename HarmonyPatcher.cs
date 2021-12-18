using ExitGames.Client.Photon;
using Harmony;
using MelonLoader;
using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using VRC.Networking;

namespace PhotonLogFull
{
    internal class HarmonyPatcher
    {
        internal void Patch()
        {
            MelonLogger.Msg("[PhotonLogFull] Patching EnqueueOperation");

            //harmonyInstance.Patch(typeof(EnetPeer).GetMethod("EnqueueOperation"), null, null, new HarmonyMethod(typeof(HarmonyPatcher).GetMethod("EnqueueOperationPatcher", BindingFlags.NonPublic | BindingFlags.Static)));

            Type loadBalancingClient = typeof(VRCFlowManager).Assembly.GetTypes().FirstOrDefault(t => !t.IsInterface && t.GetMethod("OnOperationResponse") != null);

            PhotonLogFull.Instance.Harmony.Patch(
                loadBalancingClient.GetMethod("OnEvent"),
                new HarmonyMethod(typeof(HarmonyPatcher).GetMethod("OnEventPrefix", BindingFlags.NonPublic | BindingFlags.Static)), null, null);
            PhotonLogFull.Instance.Harmony.Patch(
                loadBalancingClient.GetMethod("OnOperationResponse"),
                new HarmonyMethod(typeof(HarmonyPatcher).GetMethod("OnOperationResponsePrefix", BindingFlags.NonPublic | BindingFlags.Static)), null, null);

            //harmonyInstance.Patch(typeof(VRCPlayer).GetMethod("EMHEJMDAIBO", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(HarmonyPatcher).GetMethod("VRCPlayerPropertiesPrefix", BindingFlags.NonPublic | BindingFlags.Static)), null, null);

            PhotonLogFull.Instance.Harmony.Patch(
                typeof(PhotonPeer).GetMethod("SendOperation"),
                new HarmonyMethod(typeof(HarmonyPatcher).GetMethod("SendOperationPrefix", BindingFlags.NonPublic | BindingFlags.Static)), null, null);

            MelonLogger.Msg("[PhotonLogFull] Done");
        }

        /*
        private static bool VRCPlayerPropertiesPrefix(Hashtable __0)
        {
            MelonLogger.Msg("[VRCPlayer] Contains Key \"user\": " + __0.ContainsKey("user"));
            if (__0.ContainsKey("user"))
            {
                MelonLogger.Msg("[VRCPlayer] \"user\" is Dictionary<string, object>: " + (__0["user"] is Dictionary<string, object>) + "(type: " + __0["user"].GetType() + ")");
                if (__0["user"] is Dictionary<string, object>)
                {
                    MelonLogger.Msg("[VRCPlayer] \"user\" contains key \"id\": " + (__0["user"] as Dictionary<string, object>).ContainsKey("id"));
                }
            }
            return true;
        }
        */

        private static bool SendOperationPrefix(byte operationCode, Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object> operationParameters, SendOptions sendOptions)
        {
            try
            {
                byte eventCode = operationCode == (byte)OperationCode.RaiseEvent ? operationParameters[244 /*ParameterCode.Code*/].Unbox<byte>() : (byte)0;

                //  1: USpeak - unverified
                //  2: ??? - Kick?
                //  3: RequestPastEvents - Only sent to master when joining
                //  4: VRCEventReplicator SyncQueue Flush ???
                //  5: VRCEventReplicator SyncQueue Flush ???
                //  6: BroadcastEvent
                //  7: FloatBufferNetworkSerializer (unreliable)
                //  8: FloatBufferNetworkSerializer (interest list) - sent every 'seconds ?'
                //  9: FloatBufferNetworkSerializer (reliable)
                // 30: ExecutiveModeration
                // 40: SetUserIconDelayed

                if (/*eventCode == 1 / * USpeak * / ||*/ eventCode == 7 /* FloatBufferNetworkSerializer */)
                    return true;
                else if (eventCode == 8)
                {
                    byte[] data = Il2CppArrayBase<byte>.WrapNativeGenericArrayPointer(operationParameters[245].Pointer);
                    int count = data.Length / 6;

                    string res = count > 0 ? $"[{sendOptions.Channel}] Queuing Event 8 with the following objects ({count}):" : $"[{sendOptions.Channel}] Queuing Event 8 with no objects";
                    for (int i = 0; i < count; ++i)
                        res += $"\n - {BitConverter.ToInt32(data, i * 6)}: {BitConverter.ToString(data, i * 6 + 4, 2)}";

                    //HashSet<FlatBufferNetworkSerializer> hashset = new HashSet<FlatBufferNetworkSerializer>();
                    //foreach (var fbns in FlatBufferNetworkSerializer.field_Internal_Static_HashSet_1_FlatBufferNetworkSerializer_0 /*field_Private_Static_HashSet_1_FlatBufferNetworkSerializer_1*/)
                    //    hashset.Add(fbns);
                    /*
                    MelonLogger.Msg("FlatBufferNetworkSerializer.field_Private_Static_HashSet_1_FlatBufferNetworkSerializer_0: " + FlatBufferNetworkSerializer.field_Private_Static_HashSet_1_FlatBufferNetworkSerializer_0.Count);
                    MelonLogger.Msg("FlatBufferNetworkSerializer.field_Private_Static_HashSet_1_FlatBufferNetworkSerializer_1: " + FlatBufferNetworkSerializer.field_Private_Static_HashSet_1_FlatBufferNetworkSerializer_1.Count);
                    MelonLogger.Msg("FlatBufferNetworkSerializer.field_Internal_Static_HashSet_1_FlatBufferNetworkSerializer_0: " + FlatBufferNetworkSerializer.field_Internal_Static_HashSet_1_FlatBufferNetworkSerializer_0.Count);
                    
                    FlatBufferNetworkSerializer.ObjectNPrivateInByFlByUnique[] interestRecords = hashset.Where(fbns => fbns != null && fbns.field_Private_PhotonView_0 != null && fbns.field_Private_PhotonView_0.viewIdField > 0)
                    .Select(fbns =>
                    {
                        PhotonView photonView = fbns.field_Private_PhotonView_0;

                        int photonViewId = photonView.viewIdField;
                        byte updateFrequency = fbns.field_Private_Byte_1;
                        float voiceQuality = fbns.field_Internal_Single_0;

                        FlatBufferNetworkSerializer.ObjectNPrivateInByFlByUnique ir = new FlatBufferNetworkSerializer.ObjectNPrivateInByFlByUnique(photonViewId, updateFrequency, fbns, (byte)voiceQuality);

                        return ir;
                    })
                    .OrderByDescending(ir => ir.field_Public_Byte_0)
                    //.DistinctBy(ir => ir.field_Public_Int32_0) // Method not found
                    .ToArray();
                    MelonLogger.Msg("interestRecords count: " + interestRecords.Length);
                    
                    foreach (FlatBufferNetworkSerializer.ObjectNPrivateInByFlByUnique interestRecord in interestRecords)
                        res += "\n = " + interestRecord.field_Public_Int32_0 + " " + interestRecord.field_Public_Byte_0.ToString("X2") + " " + interestRecord.field_Public_Byte_1.ToString("X2");
                    */
                    MelonLogger.Msg(res);
                }
                else
                    MelonLogger.Msg("[" + sendOptions.Channel + "] Queuing Operation " + (Enum.GetName(typeof(OperationCode), operationCode) ?? operationCode.ToString()) + " : " + DictionaryToString(operationParameters));

            }
            catch (Exception e)
            {
                MelonLogger.Error("Exception in SendOperationPrefix:\n" + e);
            }
            return true;
        }

        private static bool OnEventPrefix(object __instance, ref EventData __0)
        {
            try
            {
                if (__0.Code == 7 /* FloatBufferNetworkSerializer */ || __0.Code == 1 /* USpeak */)
                    return true;

                MelonLogger.Msg("Received Event " + ToStringFull(__0));
            }
            catch (Exception e)
            {
                MelonLogger.Error("[PhotonLogFull] " + e);
            }
            return true;
        }

        private static bool OnOperationResponsePrefix(object __instance, ref OperationResponse __0)
        {
            try
            {
                MelonLogger.Msg("Received OperationResponse " + __0.OperationCode + ": " + __0.ToStringFull());
            }
            catch (Exception e)
            {
                MelonLogger.Error("[PhotonLogFull] " + e);
            }
            return true;
        }

        private static string ToStringFull(EventData eventData)
        {
            //MelonLogger.Msg("ToStringFull for eventData.Parameters " + eventData.Parameters.GetIl2CppType().ToString());
            /*
            MelonLogger.Msg("Iterating over eventData.Parameters.Keys");
            foreach (byte obj in eventData.Parameters.Keys)
                MelonLogger.Msg(" > " + obj + ": " + eventData.Parameters[obj].Unbox<int>());

            MelonLogger.Msg("Iterating over eventData.Parameters.TryCast<Il2CppSystem.Collections.IDictionary>().TryCast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>().Keys");
            foreach (object obj in eventData.Parameters.TryCast<Il2CppSystem.Collections.IDictionary>().TryCast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>().Keys)
                MelonLogger.Msg(" > " + obj);
            */
            return string.Format("{0}: {1}", eventData.Code, DictionaryToString(
                eventData.Parameters ));
        }

        private static string DictionaryToString<T>(Il2CppSystem.Collections.Generic.Dictionary<T, Il2CppSystem.Object> dictionary, int depth = 0, bool includeTypes = true)
        {
            string pre = new string(' ', depth * 4);
            //MelonLogger.Msg("DictionaryToString " + dictionary);
            string result;
            if (dictionary == null)
                result = "null";
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("{");
                foreach (T obj in dictionary.Keys)
                {
                    //MelonLogger.Msg(" > " + obj);
                    if (stringBuilder.Length > 1)
                        stringBuilder.Append(", ");

                    Il2CppSystem.Type type;
                    string text;

                    if (dictionary[obj] == null)
                    {
                        type = Il2CppType.Of<Il2CppSystem.Object>();
                        text = "null";
                    }
                    else
                    {
                        type = dictionary[obj].GetIl2CppType();
                        //MelonLogger.Msg("type: " + type.ToString());
                        if (type == Il2CppType.Of<byte>())
                            text = dictionary[obj].Unbox<byte>().ToString();
                        else if (type == Il2CppType.Of<ushort>())
                            text = dictionary[obj].Unbox<ushort>().ToString();
                        else if (type == Il2CppType.Of<short>())
                            text = dictionary[obj].Unbox<short>().ToString();
                        else if (type == Il2CppType.Of<uint>())
                            text = dictionary[obj].Unbox<uint>().ToString();
                        else if (type == Il2CppType.Of<int>())
                            text = dictionary[obj].Unbox<int>().ToString();
                        else if (type == Il2CppType.Of<float>())
                            text = dictionary[obj].Unbox<float>().ToString();
                        else if (type == Il2CppType.Of<long>())
                            text = dictionary[obj].Unbox<long>().ToString();
                        else if (type == Il2CppType.Of<double>())
                            text = dictionary[obj].Unbox<double>().ToString();
                        else if (type == Il2CppType.Of<string>())
                            text = $"\"{dictionary[obj].ToString()}\"";
                        else
                            text = dictionary[obj].ToString();
                    }

                    if (type == Il2CppType.Of<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>())
                        text = DictionaryToString(dictionary[obj].TryCast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>(), depth + 1, includeTypes);

                    if (type == Il2CppType.Of<Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Object>>())
                        text = DictionaryToString(dictionary[obj].TryCast<Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Object>>(), depth + 1, includeTypes);

                    if (type == Il2CppType.Of<Il2CppSystem.Collections.Hashtable>())
                        text = HashtableToString(dictionary[obj].Cast<Il2CppSystem.Collections.Hashtable>(), depth + 1, includeTypes);

                    if (type.IsArray && type.GetElementType() == Il2CppType.Of<string>())
                    {
                        string[] array = Il2CppStringArray.WrapNativeGenericArrayPointer(dictionary[obj].Pointer);
                        text = array.Length > 0 ? $"[{array.Length}]{{\n{pre}        \"{string.Join($"\",\n{pre}        \"", array)}\"\n{pre}    }}" : "[0]{}";
                    }

                    if (type.IsArray && type.GetElementType().IsArray && type.GetElementType().GetElementType() == Il2CppType.Of<string>())
                    {
                        Il2CppStringArray[] arrayOut = Il2CppArrayBase<Il2CppStringArray>.WrapNativeGenericArrayPointer(dictionary[obj].Pointer);
                        if (arrayOut.Length > 0)
                        {
                            text = $"[{arrayOut.Length}]{{";
                            foreach (string[] array in arrayOut)
                                text += array.Length > 0 ? $"\n{pre}        [{array.Length}]{{\n{pre}            \"{string.Join($"\",\n{pre}            \"", array)}\"\n{pre}        }}" : $"\n{pre}        [0]{{}}";
                            text += $"\n{pre}    }}";
                        }
                        else
                            text = "[0]{}";
                    }

                    if (type.IsArray && type.GetElementType() == Il2CppType.Of<byte>())
                    {
                        byte[] array = Il2CppArrayBase<byte>.WrapNativeGenericArrayPointer(dictionary[obj].Pointer);
                        text = $"[{array.Length}]{BitConverter.ToString(array)}";
                    }

                    if (type.IsArray && type.GetElementType() == Il2CppType.Of<int>())
                    {
                        int[] array = Il2CppArrayBase<int>.WrapNativeGenericArrayPointer(dictionary[obj].Pointer);
                        text = array.Length > 0 ? $"[{array.Length}]{{\n{pre}        {string.Join($",\n{pre}        ", array)}\n{pre}    }}" : "[0]{}";
                    }

                    if (includeTypes)
                        stringBuilder.Append($"\n{pre}    ({obj.GetType().Name}){obj} = ({type.Name}){text}");
                    else
                        stringBuilder.Append($"\n{pre}    {obj} = {text}");
                }
                stringBuilder.Append("\n" + pre + "}");
                result = stringBuilder.ToString();
            }
            return result;
        }

        private static string HashtableToString(Il2CppSystem.Collections.Hashtable dictionary, int depth = 0, bool includeTypes = true)
        {
            string pre = new string(' ', depth * 4);
            //MelonLogger.Msg("HashtableToString " + dictionary);
            string result;
            if (dictionary == null)
                result = "null";
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("{");
                foreach (Il2CppSystem.Object entry in dictionary.Cast<Il2CppSystem.Collections.IEnumerable>())
                {
                    //MelonLogger.Msg(" > " + entry + " (" + entry.GetIl2CppType().FullName + ")");

                    Il2CppSystem.Object obj = entry.Cast<Il2CppSystem.Collections.DictionaryEntry>().Key;
                    //MelonLogger.Msg(" => " + dictionary[obj]);
                    if (stringBuilder.Length > 1)
                        stringBuilder.Append(", ");

                    Il2CppSystem.Type type;
                    string text;
                    string objectText;

                    Il2CppSystem.Type objectType = obj.GetIl2CppType();
                    //MelonLogger.Msg("objectType: " + objectType.ToString());
                    if (objectType == Il2CppType.Of<byte>())
                        objectText = obj.Unbox<byte>().ToString();
                    else if (objectType == Il2CppType.Of<ushort>())
                        objectText = obj.Unbox<ushort>().ToString();
                    else if (objectType == Il2CppType.Of<short>())
                        objectText = obj.Unbox<short>().ToString();
                    else if (objectType == Il2CppType.Of<uint>())
                        objectText = obj.Unbox<uint>().ToString();
                    else if (objectType == Il2CppType.Of<int>())
                        objectText = obj.Unbox<int>().ToString();
                    else if (objectType == Il2CppType.Of<float>())
                        objectText = obj.Unbox<float>().ToString();
                    else if (objectType == Il2CppType.Of<long>())
                        objectText = obj.Unbox<long>().ToString();
                    else if (objectType == Il2CppType.Of<double>())
                        objectText = obj.Unbox<double>().ToString();
                    else
                        objectText = obj.ToString();

                    if (dictionary[obj] == null)
                    {
                        type = Il2CppType.Of<Il2CppSystem.Object>();
                        text = "null";
                    }
                    else
                    {
                        type = dictionary[obj].GetIl2CppType();
                        //MelonLogger.Msg("type: " + type.ToString());
                        if (type == Il2CppType.Of<byte>())
                            text = dictionary[obj].Unbox<byte>().ToString();
                        else if (type == Il2CppType.Of<ushort>())
                            text = dictionary[obj].Unbox<ushort>().ToString();
                        else if (type == Il2CppType.Of<short>())
                            text = dictionary[obj].Unbox<short>().ToString();
                        else if (type == Il2CppType.Of<uint>())
                            text = dictionary[obj].Unbox<uint>().ToString();
                        else if (type == Il2CppType.Of<int>())
                            text = dictionary[obj].Unbox<int>().ToString();
                        else if (type == Il2CppType.Of<float>())
                            text = dictionary[obj].Unbox<float>().ToString();
                        else if (type == Il2CppType.Of<long>())
                            text = dictionary[obj].Unbox<long>().ToString();
                        else if (type == Il2CppType.Of<double>())
                            text = dictionary[obj].Unbox<double>().ToString();
                        else
                            text = dictionary[obj].ToString();
                    }

                    if(type == Il2CppType.Of<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>())
                        text = DictionaryToString(dictionary[obj].TryCast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>(), depth + 1, includeTypes);

                    if (type == Il2CppType.Of<Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Object>>())
                        text = DictionaryToString(dictionary[obj].TryCast<Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Object>>(), depth + 1, includeTypes);

                    if (type == Il2CppType.Of<Il2CppSystem.Collections.Hashtable>())
                        text = HashtableToString(dictionary[obj].Cast<Il2CppSystem.Collections.Hashtable>(), depth + 1, includeTypes);

                    if (type.IsArray && type.GetElementType() == Il2CppType.Of<string>())
                    {
                        string[] array = Il2CppStringArray.WrapNativeGenericArrayPointer(dictionary[obj].Pointer);
                        text = array.Length > 0 ? $"[{array.Length}]{{\n{pre}        \"{string.Join($"\",\n{pre}        \"", array)}\"\n{pre}    }}" : "[0]{}";
                    }

                    if (type.IsArray && type.GetElementType().IsArray && type.GetElementType().GetElementType() == Il2CppType.Of<string>())
                    {
                        Il2CppStringArray[] arrayOut = Il2CppArrayBase<Il2CppStringArray>.WrapNativeGenericArrayPointer(dictionary[obj].Pointer);
                        if (arrayOut.Length > 0)
                        {
                            text = $"[{arrayOut.Length}]{{";
                            foreach (string[] array in arrayOut)
                                text += array.Length > 0 ? $"\n{pre}        [{array.Length}]{{\n{pre}            \"{string.Join($"\",\n{pre}            \"", array)}\"\n{pre}        }}" : $"\n{pre}        [0]{{}}";
                            text += $"\n{pre}    }}";
                        }
                        else
                            text = "[0]{}";
                    }

                    if (type.IsArray && type.GetElementType() == Il2CppType.Of<byte>())
                    {
                        byte[] array = Il2CppArrayBase<byte>.WrapNativeGenericArrayPointer(dictionary[obj].Pointer);
                        text = $"[{array.Length}]{BitConverter.ToString(array)}";
                    }

                    if (type.IsArray && type.GetElementType() == Il2CppType.Of<int>())
                    {
                        int[] array = Il2CppArrayBase<int>.WrapNativeGenericArrayPointer(dictionary[obj].Pointer);
                        text = array.Length > 0 ? $"[{array.Length}]{{\n{pre}        {string.Join($",\n{pre}        ", array)}\n{pre}    }}" : "[0]{}";
                    }

                    if (includeTypes)
                        stringBuilder.Append($"\n{pre}    ({obj.GetIl2CppType().Name}){objectText} = ({type.Name}){text}");
                    else
                        stringBuilder.Append($"\n{pre}    {objectText} = {text}");
                }
                stringBuilder.Append("\n" + pre + "}");
                result = stringBuilder.ToString();
            }
            return result;
        }

        public enum OperationCode : byte
        {
            ExchangeKeysForEncryption = 250,
            Join = 255,
            AuthenticateOnce = 231,
            Authenticate = 230,
            JoinLobby = 229,
            LeaveLobby = 228,
            CreateGame = 227,
            JoinGame = 226,
            JoinRandomGame = 225,
            Leave = (byte)254,
            RaiseEvent = (byte)253,
            SetProperties = (byte)252,
            GetProperties = (byte)251,
            ChangeGroups = (byte)248,
            FindFriends = 222,
            GetLobbyStats = 221,
            GetRegions = 220,
            WebRpc = 219,
            ServerSettings = 218,
            GetGameList = 217
        }

        /*
        private static IEnumerable<CodeInstruction> EnqueueOperationPatcher(ILGenerator ilg, IEnumerable<CodeInstruction> instructions)
        {

            if (instructions.Count() != 174)
            {
                Console.WriteLine($"Instructions hash incorrect ( { instructions.Count() } ) !");
                return instructions;
            }

            List<CodeInstruction> instructionList = instructions.ToList();
            int insertionIndex = -1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Nop)
                {
                    insertionIndex = i;
                    break;
                }
            }

            var pathedInstructions = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatcher), "EnqueueOperation")),
                new CodeInstruction(OpCodes.Brtrue_S, 7),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ret),
            };

            instructionList.InsertRange(insertionIndex, pathedInstructions);

            return instructionList;
        }

        public static bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, SendOptions sendParams)
        {
            try
            {
                if (sendParams.DeliveryMode != DeliveryMode.ReliableUnsequenced)
                    return true;
                JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.Formatting = Formatting.Indented;
                jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                MelonLogger.Msg("send OP " + opCode + ": " + JsonConvert.SerializeObject(parameters, jss));
            }
            catch(Exception e)
            {
                MelonLogger.Error("[PhotonLogFull] " + e);
            }
            return true;
        }
        */
    }
}
