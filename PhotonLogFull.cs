using ExitGames.Client.Photon;
using Harmony;
using MelonLoader;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using PhotonLogFull;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using VRC;

namespace PhotonLogFull
{
    public class PhotonLogFull : MelonMod
    {
        internal static PhotonLogFull Instance { get; private set; }
        /*
        public void OnUpdate()
        {

            if (Input.GetKeyDown(KeyCode.R))
            {
                int[] targets = PlayerManager.GetAllPlayers().Where(HHJGPFPCIMK => HHJGPFPCIMK != null && HHJGPFPCIMK.GetInstigatorId() != null).Select(HHJGPFPCIMK => HHJGPFPCIMK.GetInstigatorId().Value).ToArray();
                /*
                PhotonNetwork.RaiseEvent(1, new byte[] { 136, 137, 138 }, new RaiseEventOptions() { IBBGGGLPMIK = targets, FGOMBOBCIME = ELKJAHNBIAB.DoNotCache, LNHINHNBFMB = 0, Receivers = ReceiverGroup.Others }, new SendOptions
                {
                    Channel = 1,
                    DeliveryMode = DeliveryMode.ReliableUnsequenced
                });
                * /
                /*
                LNOCIIBBPOA.IPECKLBCJFE(1, new byte[] { 136, 137, 138 }, new BHCJLLEGLDE() { IBBGGGLPMIK = targets, FGOMBOBCIME = ELKJAHNBIAB.DoNotCache, LNHINHNBFMB = 0, HLEFBIJIHBB = FDJOPPEEGKE.Others }, new SendOptions
                {
                    Channel = 1,
                    DeliveryMode = DeliveryMode.Reliable
                    //DeliveryMode = DeliveryMode.ReliableUnsequenced
                });
                * /
            }
        }
        */

        public override void OnUpdate()
        {
            /*
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                Il2CppStructArray<byte> data = new Il2CppStructArray<byte>(9);
                MelonLogger.Msg("VRCPlayer's ViewID: " + VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponent<PhotonView>().viewIdField);
                byte[] photonViewIdBytes = BitConverter.GetBytes(VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponent<PhotonView>().viewIdField);
                data[0] = photonViewIdBytes[0];
                data[1] = photonViewIdBytes[1];
                data[2] = photonViewIdBytes[2];
                data[3] = photonViewIdBytes[3];

                byte[] buffersize = BitConverter.GetBytes(int.MaxValue);
                data[4] = buffersize[0];
                data[5] = buffersize[1];
                data[6] = buffersize[2];
                data[7] = buffersize[3];

                data[8] = Byte.MaxValue;

                //new Il2CppSystem.Boolean() { m_value = false }.BoxIl2CppObject();

                ObjectPublicAbstractSealedStPhStObInSeSiObBoStUnique
                    .field_Public_Static_ObjectPublicIPhotonPeerListenerObStBoStObCoDiBo2ObUnique_0
                    .Method_Public_Virtual_New_Boolean_Byte_Object_ObjectPublicObByObInByObObUnique_SendOptions_0( // OpRaiseEvent
                        7,
                        data.Cast<Il2CppSystem.Object>(),
                        new ObjectPublicObByObInByObObUnique
                        {
                            field_Public_EnumPublicSealedvaOtAlMa4vUnique_0 = EnumPublicSealedvaOtAlMa4vUnique.Others
                        }, SendOptions.SendUnreliable);
            }
            */
        }

        public override void OnApplicationStart()
        {
            Instance = this;

            MelonCoroutines.Start(InitAfterFrame());

            /*
            Harmony.Patch(
                typeof(LogManager).GetMethod("Awake"),
                new HarmonyMethod(typeof(PhotonLogFull).GetMethod("LogManagerAwakePrefix", BindingFlags.NonPublic | BindingFlags.Static)), null, null);
            */
        }

        private IEnumerator InitAfterFrame()
        {
            yield return null;

            //MelonLogger.Msg("PhotonNetwork.NetworkingClient: " + ObjectPublicAbstractSealedStPhStObInSeSiObBoStUnique.field_Public_Static_ObjectPublicIPhotonPeerListenerObStBoStObCoDiBo2ObUnique_0?.ToString());
            //MelonLogger.Msg("PhotonNetwork.NetworkingClient.LoadBalancingPeer: " + ObjectPublicAbstractSealedStPhStObInSeSiObBoStUnique.field_Public_Static_ObjectPublicIPhotonPeerListenerObStBoStObCoDiBo2ObUnique_0?.prop_PhotonPeerPublicTyDi2ByObUnique_0?.ToString());

            MelonLogger.Msg("[PhotonLogFull] Changing photon log level to Full/All");
            PhotonNetwork.field_Public_Static_PunLogLevel_0 = PunLogLevel.Full; //PhotonNetwork.LogLevel
            PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_LoadBalancingPeer_0.DebugOut = DebugLevel.ALL; //PhotonNetwork.NetworkingClient.LoadBalancingPeer.DebugOut
            
            MelonLogger.Msg("[PhotonLogFull] Changing Unity log level to Full");

            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);


            MelonLogger.Msg("[PhotonLogFull] Running HarmonyPatcher");
            new HarmonyPatcher().Patch();

            //Harmony.Patch(
            //    typeof(PlayerNet).GetMethod("Encode"),
            //    new HarmonyMethod(typeof(PhotonLogFull).GetMethod("PlayerNetEncodePrefix", BindingFlags.NonPublic | BindingFlags.Static)), null, null);
        }

        /*
        private static void PlayerNetEncodePrefix(PlayerNet __instance)
        {
            MelonLogger.Msg("PlayerNetEncodePrefix");
            Debug.Log("PlayerNetEncodePrefix");
            Debug.LogException(new Il2CppSystem.Exception("PlayerNetEncodePrefixIl2CppException"));
            Resources.FindObjectsOfTypeAll<LogManager>()[0].vrcLogHandler.LogException(new Il2CppSystem.Exception("PlayerNetEncodePrefixIl2CppException"), __instance);
            //throw new Exception("PlayerNetEncodePrefixException");
        }
        */

        /*
        private static string EventContentToString(string eventContent)
        {
            if (eventContent == null)
                return "null";
        }
        */

        private static bool LogManagerAwakePrefix(LogManager __instance)
        {
            MelonLogger.Msg("LogManagerAwakePrefix called");
            __instance.enabled = false;
            return false;
        }


        /*
        public static bool PhotonOnEvent(EventData photonEvent)
        {
            try
            {
                PrintPacket(photonEvent);
                /*
                if (photonEvent.Code == 200)
                {
                    if (photonEvent.Parameters.TryGetValue(245, out object hashtable))
                    {
                        if (((Hashtable)hashtable).TryGetValue((byte)4, out object dicEntry_))
                        {
                            object[] dicEntry = (object[]) dicEntry_;
                            if (dicEntry[0].GetType() == typeof(VRC_EventLog.DIPHJLDJBCF))
                            {
                                VRC_EventLog.DIPHJLDJBCF eventLogEntry = (VRC_EventLog.DIPHJLDJBCF) dicEntry[0];
                                if (eventLogEntry.GFJCLHMFEJC != 0)
                                {
                                    MelonLogger.Msg("Packet contains VRC_EventLog.EventLogEntry with CombinedNetworkId " + eventLogEntry.GFJCLHMFEJC);
                                    return false;
                                }
                            }
                            else MelonLogger.Msg("Packet 200 doesn't contains VRC_EventLog.EventLogEntry");
                        }
                        else MelonLogger.Msg("Packet 200 doesn't contains hashtable index 4. Content: ");
                    }
                    else MelonLogger.Msg("Packet 200 doesn't contains index 245");
                }
                * /
            }
            catch (Exception e) { VRCModLogger.LogError("[PhotonLogFull] " + e); }
            return true;
        }
        */

        /*
        public static void PrintPacket(EventData photonEvent)
        {
            if (photonEvent.Parameters.TryGetValue(254, out object playerId))
                MelonLogger.Msg("Received packet " + photonEvent.Code + " from playerId " + playerId + " ! Data:");
            else MelonLogger.Msg("Received packet " + photonEvent.Code + " from undefined playerId ! Data:");

            foreach (KeyValuePair<byte, object> obj in photonEvent.Parameters)
            {
                if (obj.Value == null)
                    MelonLogger.Msg(" - " + obj.Key + ": null");
                else if (obj.Value.GetType() == typeof(object[]))
                {
                    MelonLogger.Msg(" - " + obj.Key + ": [" + obj.Value.GetType() + "]:");
                    PrintObjectArray((object[])obj.Value, " -");
                }
                else if (obj.Value.GetType() == typeof(int[]))
                {
                    MelonLogger.Msg(" - " + obj.Key + ": [" + obj.Value.GetType() + "]:");
                    PrintIntArray((int[])obj.Value, " -");
                }
                else if (obj.Value.GetType() == typeof(byte[]))
                {
                    MelonLogger.Msg(" - " + obj.Key + ": [" + obj.Value.GetType() + "]:");
                    PrintByteArray((byte[])obj.Value, " -");
                }
                else if (obj.Value.GetType() == typeof(Hashtable))
                {
                    MelonLogger.Msg(" - " + obj.Key + ": [" + obj.Value.GetType() + "]:");
                    PrintHashtable((Hashtable)obj.Value, " -");
                }
                else if (obj.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    MelonLogger.Msg(" - " + obj.Key + ": [" + obj.Value.GetType() + "]:");
                    ParseDictionary((Dictionary<string, object>)obj.Value, " -");
                }
                else MelonLogger.Msg(" - " + obj.Key + ": [" + obj.Value.GetType() + "] " + obj.Value);
            }
        }

        public static void PrintObjectArray(object[] array, string prefix)
        {
            foreach (object obj in array)
            {
                if (obj == null)
                    MelonLogger.Msg(prefix + " | null");
                else if (obj.GetType() == typeof(object[]))
                {
                    MelonLogger.Msg(prefix + " | [" + obj.GetType().Name + "]:");
                    PrintObjectArray((object[])obj, prefix + " | ");
                }
                else if (obj.GetType() == typeof(int[]))
                {
                    MelonLogger.Msg(prefix + " | [" + obj.GetType().Name + "]:");
                    PrintIntArray((int[])obj, prefix + " | ");
                }
                else if (obj.GetType() == typeof(byte[]))
                {
                    MelonLogger.Msg(prefix + " | [" + obj.GetType().Name + "]:");
                    PrintByteArray((byte[])obj, prefix + " | ");
                }
                else if (obj.GetType() == typeof(Hashtable))
                {
                    MelonLogger.Msg(prefix + " | [" + obj.GetType().Name + "]:");
                    PrintHashtable((Hashtable)obj, prefix + " | ");
                }
                else if (obj.GetType() == typeof(Dictionary<string, object>))
                {
                    MelonLogger.Msg(prefix + " | [" + obj.GetType().Name + "]:");
                    ParseDictionary((Dictionary<string, object>)obj, prefix + " | ");
                }
                else MelonLogger.Msg(prefix + " | [" + obj.GetType().Name + "]: " + obj);
            }
        }

        public static void PrintIntArray(int[] array, string prefix)
        {
            foreach (int obj in array)
            {
                MelonLogger.Msg(prefix + " | [" + obj.GetType().Name + "]: " + obj);
            }
        }

        public static void PrintByteArray(byte[] array, string prefix)
        {
            foreach (byte obj in array)
            {
                MelonLogger.Msg(prefix + " | [" + obj.GetType().Name + "]: " + obj);
            }
        }

        public static void PrintHashtable(Hashtable hashtable, string prefix)
        {
            foreach (System.Collections.DictionaryEntry obj in hashtable)
            {
                if (obj.Value == null)
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": null");
                else if (obj.Value.GetType() == typeof(object[]))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.GetType().Name + "]:");
                    PrintObjectArray((object[])obj.Value, prefix + " | ");
                }
                else if (obj.Value.GetType() == typeof(int[]))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.GetType().Name + "]:");
                    PrintIntArray((int[])obj.Value, prefix + " | ");
                }
                else if (obj.Value.GetType() == typeof(byte[]))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.GetType().Name + "]:");
                    PrintByteArray((byte[])obj.Value, prefix + " | ");
                }
                else if (obj.Value.GetType() == typeof(Hashtable))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.Value.GetType().Name + "]:");
                    PrintHashtable((Hashtable)obj.Value, prefix + " | ");
                }
                else if (obj.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.Value.GetType().Name + "]:");
                    ParseDictionary((Dictionary<string, object>)obj.Value, prefix + " | ");
                }
                else MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.Value.GetType().Name + "]: " + obj.Value);
            }
        }

        public static void ParseDictionary(Dictionary<string, object> dictionary, string prefix)
        {
            foreach (KeyValuePair<string, object> obj in dictionary)
            {
                if (obj.Value == null)
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": null");
                else if (obj.Value.GetType() == typeof(object[]))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.GetType().Name + "]:");
                    PrintObjectArray((object[])obj.Value, prefix + " | ");
                }
                else if (obj.Value.GetType() == typeof(int[]))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.GetType().Name + "]:");
                    PrintIntArray((int[])obj.Value, prefix + " | ");
                }
                else if (obj.Value.GetType() == typeof(byte[]))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.GetType().Name + "]:");
                    PrintByteArray((byte[])obj.Value, prefix + " | ");
                }
                else if (obj.Value.GetType() == typeof(Hashtable))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.Value.GetType().Name + "]:");
                    PrintHashtable((Hashtable)obj.Value, prefix + " | ");
                }
                else if (obj.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.Value.GetType().Name + "]:");
                    ParseDictionary((Dictionary<string, object>)obj.Value, prefix + " | ");
                }
                else MelonLogger.Msg(prefix + " | - " + obj.Key + ": [" + obj.Value.GetType().Name + "]: " + obj.Value);
            }
        }
        */
    }
}
