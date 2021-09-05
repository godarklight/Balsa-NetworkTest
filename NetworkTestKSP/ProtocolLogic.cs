using System;
using NetworkTest;
using DarkLog;

namespace NetworkTestKSP
{
    public class ProtocolLogic
    {
        private long startTime;
        private const byte systemID = 1;
        private const byte componentID = 1;
        private static MAVLink.MavlinkParse parser = new MAVLink.MavlinkParse();
        private DataStore data;
        private ModLog log;

        public ProtocolLogic(DataStore data, ModLog log)
        {
            startTime = DateTime.UtcNow.Ticks;
            this.data = data;
            this.log = log;
        }

        public void ConnectEvent(ClientObject client)
        {
            client.requestedRates[MAVLink.MAVLINK_MSG_ID.HEARTBEAT] = 1f;
            log.Log("Client connected");
        }

        public void DisconnectEvent(ClientObject client)
        {
            log.Log("Client disconnected");
        }

        public void ReceiveSetRate(ClientObject client, MAVLink.MAVLinkMessage rawMessage)
        {
            //client.requestedRates[message.messageType] = message.rate;
        }

        public void SendHeartbeat(ClientObject client)
        {
            MAVLink.mavlink_heartbeat_t message = new MAVLink.mavlink_heartbeat_t();
            message.custom_mode = 0;
            message.type = (byte)MAVLink.MAV_TYPE.FIXED_WING;
            message.autopilot = (byte)MAVLink.MAV_AUTOPILOT.ARDUPILOTMEGA;
            message.base_mode = (byte)MAVLink.MAV_MODE.AUTO_ARMED;
            message.system_status = (byte)MAVLink.MAV_STATE.ACTIVE;
            message.mavlink_version = (byte)MAVLink.MAVLINK_VERSION;
            client.SendMessage(message);
        }

        public void SendAttitude(ClientObject client)
        {
            MAVLink.mavlink_attitude_t message = new MAVLink.mavlink_attitude_t();
            message.pitch = data.pitch;
            message.roll = data.roll;
            message.yaw = data.yaw;
            message.pitchspeed = 0;
            message.rollspeed = 0;
            message.yawspeed = 0;
            message.time_boot_ms = GetUptime();
            client.SendMessage(message);
        }

        private void SendPosition(ClientObject client)
        {
            MAVLink.mavlink_global_position_int_t message = new MAVLink.mavlink_global_position_int_t();
            message.lat = data.latitude;
            message.lon = data.longitude;
            message.alt = data.altitude;
            message.relative_alt = data.altitude;
            message.hdg = 0;
            message.vx = 0;
            message.vy = 0;
            message.vz = 0;
            message.time_boot_ms = GetUptime();
            client.SendMessage(message);
        }

        private uint GetUptime()
        {
            long timeMS = (DateTime.UtcNow.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            return (uint)timeMS;
        }
    }
}
