using System;
using NetworkTest;
using DarkLog;
using UnityEngine;
using FSControl;

namespace NetworkTestKSP
{
    public class NetworkTestMain : MonoBehaviour
    {
        ModLog log;
        DataStore data;
        ProtocolLogic protocol;
        NetworkHandler hander;

        public void Start()
        {
            log = new ModLog("NetworkTest");
            log.Log("Start!");
            data = new DataStore();
            protocol = new ProtocolLogic(data, log);
            hander = new NetworkHandler();
            hander.RegisterConnect(protocol.ConnectEvent);
            hander.RegisterReceive(MessageType.SET_RATE, protocol.ReceiveSetRate);
            hander.RegisterSend(MessageType.HEARTBEAT, protocol.SendHeartbeat);
            hander.RegisterSend(MessageType.ATTITUDE, protocol.SendAttitude);
            hander.RegisterSend(MessageType.POSITION, protocol.SendPosition);
            hander.RegisterDisconnect(protocol.DisconnectEvent);
            hander.StartServer(log.Log);
            DontDestroyOnLoad(this);
        }

        public void FixedUpdate()
        {
            if (!GameLogic.inGame || !GameLogic.SceneryLoaded || GameLogic.LocalPlayerVehicle == null || !GameLogic.LocalPlayerVehicle.InitComplete)
            {
                data.pitch = 0;
                data.roll = 0;
                data.yaw = 0;
                data.latitude = 0;
                data.longitude = 0;
                data.altitude = 0;
                return;
            }
            Vehicle v = GameLogic.LocalPlayerVehicle;
            data.pitch = FSControlUtil.GetVehiclePitch(v) * Mathf.Rad2Deg;
            data.roll = FSControlUtil.GetVehicleRoll(v) * Mathf.Rad2Deg;
            data.yaw = v.Physics.HeadingDegs;
            //YUp
            data.latitude = (float)FloatingOrigin.GetAbsoluteWPos(v.transform.position).x;
            data.longitude = (float)FloatingOrigin.GetAbsoluteWPos(v.transform.position).z;
            data.altitude = (float)FloatingOrigin.GetAbsoluteWPos(v.transform.position).y;
        }
    }
}
