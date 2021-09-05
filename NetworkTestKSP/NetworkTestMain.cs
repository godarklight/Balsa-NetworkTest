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
            //hander.RegisterReceive(MAVLink.MAVLINK_MSG_ID.SET_RATE, protocol.ReceiveSetRate);
            hander.RegisterSend(MAVLink.MAVLINK_MSG_ID.HEARTBEAT, protocol.SendHeartbeat);
            hander.RegisterSend(MAVLink.MAVLINK_MSG_ID.ATTITUDE, protocol.SendAttitude);
            //hander.RegisterSend(MAVLink.MAVLINK_MSG_ID.POSITION, protocol.SendPosition);
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
            //Balsa is YUp
            //Mavlink is degE7, 1° = 111 km 1E7/111000 = ~90
            data.latitude = (int)(FloatingOrigin.GetAbsoluteWPos(v.transform.position).x / 90d);
            data.longitude = (int)(FloatingOrigin.GetAbsoluteWPos(v.transform.position).z / 90d);
            //Metres -> mm
            data.altitude = (int)(FloatingOrigin.GetAbsoluteWPos(v.transform.position).y * 1000d);
        }
    }
}
